﻿#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2010 Stephen M. McKamey

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.

\*---------------------------------------------------------------------------------*/
#endregion License

using System;
using System.Linq;

using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;
using JsonFx.Serialization.Resolvers;

namespace JsonFx.Xml.Stax
{
	public class StaxTokenizerTests
	{
		#region Simple Single Element Tests

		[Fact]
		public void GetTokens_SingleOpenCloseTag_ReturnsSequence()
		{
			const string input = @"<root></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleVoidTag_ReturnsSequence()
		{
			const string input = @"<root />";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Simple Single Element Tests

		#region Namespace Tests

		[Fact]
		public void GetTokens_DefaultNamespaceTag_ReturnsSequence()
		{
			const string input = @"<root xmlns=""http://example.com/schema""></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenPrefixBegin("", "http://example.com/schema"),
			        StaxGrammar.TokenElementBegin(new DataName("root", "http://example.com/schema")),
			        StaxGrammar.TokenElementEnd(new DataName("root", "http://example.com/schema")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.com/schema"),
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_NamespacePrefixTag_ReturnsSequence()
		{
			const string input = @"<prefix:root xmlns:prefix=""http://example.com/schema""></prefix:root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenPrefixBegin("prefix", "http://example.com/schema"),
			        StaxGrammar.TokenElementBegin(new DataName("root", "http://example.com/schema")),
			        StaxGrammar.TokenElementEnd(new DataName("root", "http://example.com/schema")),
			        StaxGrammar.TokenPrefixEnd("prefix", "http://example.com/schema"),
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_NamespacedChildTag_ReturnsSequence()
		{
			const string input = @"<foo><child xmlns=""http://example.com/schema"">value</child></foo>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("foo")),
			        StaxGrammar.TokenPrefixBegin("", "http://example.com/schema"),
			        StaxGrammar.TokenElementBegin(new DataName("child", "http://example.com/schema")),
			        StaxGrammar.TokenText("value"),
			        StaxGrammar.TokenElementEnd(new DataName("child", "http://example.com/schema")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.com/schema"),
			        StaxGrammar.TokenElementEnd(new DataName("foo"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ParentAndChildShareDefaultNamespace_ReturnsSequence()
		{
			const string input = @"<foo xmlns=""http://example.org""><child>value</child></foo>";
			var expected = new[]
			    {
			        StaxGrammar.TokenPrefixBegin("", "http://example.org"),
			        StaxGrammar.TokenElementBegin(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenElementBegin(new DataName("child", "http://example.org")),
			        StaxGrammar.TokenText("value"),
			        StaxGrammar.TokenElementEnd(new DataName("child", "http://example.org")),
			        StaxGrammar.TokenElementEnd(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.org")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ParentAndChildSharePrefixedNamespace_ReturnsSequence()
		{
			const string input = @"<bar:foo xmlns:bar=""http://example.org""><bar:child>value</bar:child></bar:foo>";
			var expected = new[]
			    {
			        StaxGrammar.TokenPrefixBegin("bar", "http://example.org"),
			        StaxGrammar.TokenElementBegin(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenElementBegin(new DataName("child", "http://example.org")),
			        StaxGrammar.TokenText("value"),
			        StaxGrammar.TokenElementEnd(new DataName("child", "http://example.org")),
			        StaxGrammar.TokenElementEnd(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenPrefixEnd("bar", "http://example.org")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ParentAndChildDifferentDefaultNamespaces_ReturnsSequence()
		{
			const string input = @"<foo xmlns=""http://json.org""><child xmlns=""http://jsonfx.net"">text value</child></foo>";
			var expected = new[]
			    {
			        StaxGrammar.TokenPrefixBegin("", "http://json.org"),
			        StaxGrammar.TokenElementBegin(new DataName("foo", "http://json.org")),
			        StaxGrammar.TokenPrefixBegin("", "http://jsonfx.net"),
			        StaxGrammar.TokenElementBegin(new DataName("child", "http://jsonfx.net")),
			        StaxGrammar.TokenText("text value"),
			        StaxGrammar.TokenElementEnd(new DataName("child", "http://jsonfx.net")),
			        StaxGrammar.TokenPrefixEnd("", "http://jsonfx.net"),
			        StaxGrammar.TokenElementEnd(new DataName("foo", "http://json.org")),
			        StaxGrammar.TokenPrefixEnd("", "http://json.org")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_DifferentPrefixSameNamespace_ReturnsSequence()
		{
			const string input = @"<foo xmlns=""http://example.org"" xmlns:blah=""http://example.org"" blah:key=""value"" />";
			var expected = new[]
			    {
			        StaxGrammar.TokenPrefixBegin("", "http://example.org"),
			        StaxGrammar.TokenPrefixBegin("blah", "http://example.org"),
			        StaxGrammar.TokenElementBegin(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenAttribute(new DataName("key", "http://example.org")),
			        StaxGrammar.TokenText("value"),
			        StaxGrammar.TokenElementEnd(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.org"),
			        StaxGrammar.TokenPrefixEnd("blah", "http://example.org")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_NestedDefaultNamespaces_ReturnsSequence()
		{
			const string input = @"<outer xmlns=""http://example.org/outer""><middle-1 xmlns=""http://example.org/inner""><inner>this should be inner</inner></middle-1><middle-2>this should be outer</middle-2></outer>";

			var expected = new[]
			    {
			        StaxGrammar.TokenPrefixBegin("", "http://example.org/outer"),
			        StaxGrammar.TokenElementBegin(new DataName("outer", "http://example.org/outer")),
			        StaxGrammar.TokenPrefixBegin("", "http://example.org/inner"),
			        StaxGrammar.TokenElementBegin(new DataName("middle-1", "http://example.org/inner")),
			        StaxGrammar.TokenElementBegin(new DataName("inner", "http://example.org/inner")),
			        StaxGrammar.TokenText("this should be inner"),
			        StaxGrammar.TokenElementEnd(new DataName("inner", "http://example.org/inner")),
			        StaxGrammar.TokenElementEnd(new DataName("middle-1", "http://example.org/inner")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.org/inner"),
			        StaxGrammar.TokenElementBegin(new DataName("middle-2", "http://example.org/outer")),
			        StaxGrammar.TokenText("this should be outer"),
			        StaxGrammar.TokenElementEnd(new DataName("middle-2", "http://example.org/outer")),
			        StaxGrammar.TokenElementEnd(new DataName("outer", "http://example.org/outer")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.org/outer")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Namespace Tests

		#region Simple Attribute Tests

		[Fact]
		public void GetTokens_SingleTagSingleAttribute_ReturnsSequence()
		{
			const string input = @"<root attrName=""attrValue""></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("attrName")),
			        StaxGrammar.TokenText("attrValue"),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagSingleAttributeNoValue_ReturnsSequence()
		{
			const string input = @"<root noValue></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("noValue")),
			        StaxGrammar.TokenText(String.Empty),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagSingleAttributeEmptyValue_ReturnsSequence()
		{
			const string input = @"<root emptyValue=""""></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("emptyValue")),
			        StaxGrammar.TokenText(String.Empty),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagWhitespaceAttributeQuotDelims_ReturnsSequence()
		{
			const string input = @"<root white  =  "" extra whitespace around quote delims "" ></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("white")),
			        StaxGrammar.TokenText(" extra whitespace around quote delims "),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagWhitespaceAttributeAposDelims_ReturnsSequence()
		{
			const string input = @"<root white  =  ' extra whitespace around apostrophe delims ' ></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("white")),
			        StaxGrammar.TokenText(" extra whitespace around apostrophe delims "),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagSingleAttributeWhitespace_ReturnsSequence()
		{
			const string input = @"<root whitespace="" this contains whitespace ""></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("whitespace")),
			        StaxGrammar.TokenText(" this contains whitespace "),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagSingleAttributeSingleQuoted_ReturnsSequence()
		{
			const string input = @"<root singleQuoted='apostrophe'></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("singleQuoted")),
			        StaxGrammar.TokenText("apostrophe"),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagSingleAttributeSingleQuotedWhitespace_ReturnsSequence()
		{
			const string input = @"<root singleQuoted_whitespace=' apostrophe with whitespace '></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("singleQuoted_whitespace")),
			        StaxGrammar.TokenText(" apostrophe with whitespace "),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_SingleTagMultipleAttributes_ReturnsSequence()
		{
			const string input = @"<root no-value whitespace="" this contains whitespace "" anyQuotedText=""/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?""></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("no-value")),
			        StaxGrammar.TokenText(String.Empty),
			        StaxGrammar.TokenAttribute(new DataName("whitespace")),
			        StaxGrammar.TokenText(" this contains whitespace "),
			        StaxGrammar.TokenAttribute(new DataName("anyQuotedText")),
			        StaxGrammar.TokenText(@"/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Simple Attribute Tests

		#region Text Content Tests

		[Fact]
		public void GetTokens_XmlEntityLt_ReturnsSequence()
		{
			const string input = @"&lt;";
			var expected = new[]
			    {
			        StaxGrammar.TokenText("<")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_XmlEntityB_ReturnsSequence()
		{
			const string input = @"&#66;";
			var expected = new[]
			    {
			        StaxGrammar.TokenText("B")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_XmlEntityHexLowerX_ReturnsSequence()
		{
			const string input = @"&#x37;";
			var expected = new[]
			    {
			        StaxGrammar.TokenText("7")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_XmlEntityHexUpperX_ReturnsSequence()
		{
			const string input = @"&#X38;";
			var expected = new[]
			    {
			        StaxGrammar.TokenText("8")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_XmlEntityHexUpperCase_ReturnsSequence()
		{
			const string input = @"&#xABCD;";
			var expected = new[]
			    {
			        StaxGrammar.TokenText("\uABCD")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_XmlEntityHexLowerCase_ReturnsSequence()
		{
			const string input = @"&#xabcd;";
			var expected = new[]
			    {
			        StaxGrammar.TokenText("\uabcd")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_HtmlEntityEuro_ReturnsSequence()
		{
			const string input = @"&euro;";
			var expected = new[]
			    {
			        StaxGrammar.TokenText("€")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_EntityWithLeadingText_ReturnsSequence()
		{
			const string input = @"leading&amp;";
			var expected = new[]
			    {
			        StaxGrammar.TokenText("leading"),
			        StaxGrammar.TokenText("&")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_EntityWithTrailingText_ReturnsSequence()
		{
			const string input = @"&amp;trailing";
			var expected = new[]
			    {
			        StaxGrammar.TokenText("&"),
			        StaxGrammar.TokenText("trailing")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_MixedEntities_ReturnsSequence()
		{
			const string input = @"there should &lt;b&gt;e decoded chars &amp; inside this text";
			var expected = new[]
			    {
			        StaxGrammar.TokenText(@"there should "),
			        StaxGrammar.TokenText(@"<"),
			        StaxGrammar.TokenText(@"b"),
			        StaxGrammar.TokenText(@">"),
			        StaxGrammar.TokenText(@"e decoded chars "),
			        StaxGrammar.TokenText(@"&"),
			        StaxGrammar.TokenText(@" inside this text")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_MixedEntitiesMalformed_ReturnsSequence()
		{
			const string input = @"there should &#xnot &Xltb&#gte decoded chars & inside this text";
			var expected = new[]
			    {
			        StaxGrammar.TokenText(@"there should "),
			        StaxGrammar.TokenText(@"&#x"),
			        StaxGrammar.TokenText(@"not "),
			        StaxGrammar.TokenText(@"&Xltb"),
			        StaxGrammar.TokenText(@"&#"),
			        StaxGrammar.TokenText(@"gte decoded chars "),
			        StaxGrammar.TokenText(@"&"),
			        StaxGrammar.TokenText(@" inside this text")
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Text Content Tests

		#region Mixed Content Tests

		[Fact]
		public void GetTokens_HtmlContent_ReturnsSequence()
		{
			const string input = @"<div class=""content""><p style=""color:red""><strong>Lorem ipsum</strong> dolor sit amet, <i>consectetur</i> adipiscing elit.</p></div>";

			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("div")),
			        StaxGrammar.TokenAttribute(new DataName("class")),
			        StaxGrammar.TokenText("content"),
			        StaxGrammar.TokenElementBegin(new DataName("p")),
			        StaxGrammar.TokenAttribute(new DataName("style")),
			        StaxGrammar.TokenText("color:red"),
			        StaxGrammar.TokenElementBegin(new DataName("strong")),
			        StaxGrammar.TokenText("Lorem ipsum"),
			        StaxGrammar.TokenElementEnd(new DataName("strong")),
			        StaxGrammar.TokenText(" dolor sit amet, "),
			        StaxGrammar.TokenElementBegin(new DataName("i")),
			        StaxGrammar.TokenText("consectetur"),
			        StaxGrammar.TokenElementEnd(new DataName("i")),
			        StaxGrammar.TokenText(" adipiscing elit."),
			        StaxGrammar.TokenElementEnd(new DataName("p")),
					StaxGrammar.TokenElementEnd(new DataName("div")),
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_HtmlContentPrettyPrinted_ReturnsSequence()
		{
			const string input =
@"<div class=""content"">
	<p style=""color:red"">
		<strong>Lorem ipsum</strong> dolor sit amet, <i>consectetur</i> adipiscing elit.
	</p>
</div>";

			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("div")),
			        StaxGrammar.TokenAttribute(new DataName("class")),
			        StaxGrammar.TokenText("content"),
			        StaxGrammar.TokenWhitespace("\r\n\t"),
			        StaxGrammar.TokenElementBegin(new DataName("p")),
			        StaxGrammar.TokenAttribute(new DataName("style")),
			        StaxGrammar.TokenText("color:red"),
			        StaxGrammar.TokenWhitespace("\r\n\t\t"),
			        StaxGrammar.TokenElementBegin(new DataName("strong")),
			        StaxGrammar.TokenText("Lorem ipsum"),
			        StaxGrammar.TokenElementEnd(new DataName("strong")),
			        StaxGrammar.TokenText(" dolor sit amet, "),
			        StaxGrammar.TokenElementBegin(new DataName("i")),
			        StaxGrammar.TokenText("consectetur"),
			        StaxGrammar.TokenElementEnd(new DataName("i")),
			        StaxGrammar.TokenText(" adipiscing elit.\r\n\t"),
			        StaxGrammar.TokenElementEnd(new DataName("p")),
			        StaxGrammar.TokenWhitespace("\r\n"),
					StaxGrammar.TokenElementEnd(new DataName("div")),
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Mixed Content Tests

		#region Error Recovery Tests

		[Fact]
		public void GetTokens_UnclosedOpenTag_ReturnsSequence()
		{
			const string input = @"<root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root"))
			    };

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			// TODO: determine how this should be handled
			Assert.Equal(expected, actual);
		}

		#endregion Error Recovery Tests

		#region Input Edge Case Tests

		[Fact]
		public void GetTokens_NullInput_ReturnsEmptySequence()
		{
			const string input = null;
			var expected = new Token<StaxTokenType>[0];

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_EmptyInput_ReturnsEmptySequence()
		{
			const string input = "";
			var expected = new Token<StaxTokenType>[0];

			var tokenizer = new StaxTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Input Edge Case Tests
	}
}
