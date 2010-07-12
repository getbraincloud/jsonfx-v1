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
using System.Collections.Generic;
using System.Linq;

using JsonFx.Serialization;
using JsonFx.Serialization.GraphCycles;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Json
{
	public class JsonWalkerTests
	{
		#region Test Types

		public class Person
		{
			public string Name { get; set; }
			public Person Father { get; set; }
			public Person Mother { get; set; }
			public Person[] Children { get; set; }
		}

		#endregion Test Types

		#region Array Tests

		[Fact]
		public void GetTokens_ArrayEmpty_ReturnsEmptyArrayTokens()
		{
			var input = new object[0];

			var expected = new[]
				{
					JsonGrammar.TokenArrayBegin,
					JsonGrammar.TokenArrayEnd
				};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ListEmpty_ReturnsEmptyArrayTokens()
		{
			var input = new List<object>(0);

			var expected = new[]
				{
					JsonGrammar.TokenArrayBegin,
					JsonGrammar.TokenArrayEnd
				};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArraySingleItem_ReturnsSingleItemArrayTokens()
		{
			var input = new object[]
				{
					null
				};

			var expected = new[]
				{
					JsonGrammar.TokenArrayBegin,
					JsonGrammar.TokenNull,
					JsonGrammar.TokenArrayEnd
				};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArrayMultiItem_ReturnsArrayTokens()
		{
			var input = new object[]
				{
					false,
					true,
					null,
					'a',
					'b',
					'c',
					1,
					2,
					3
				};

			var expected = new[]
				{
					JsonGrammar.TokenArrayBegin,
					JsonGrammar.TokenFalse,
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenTrue,
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenNull,
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString('a'),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString('b'),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString('c'),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenNumber(1),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenNumber(2),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenNumber(3),
					JsonGrammar.TokenArrayEnd
				};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ArrayNested_ReturnsNestedArrayTokens()
		{
			var input = new object[]
				{
					false,
					true,
					null,
					new []
					{
						'a',
						'b',
						'c'
					},
					new []
					{
						1,
						2,
						3
					}
				};

			var expected = new[]
				{
					JsonGrammar.TokenArrayBegin,
					JsonGrammar.TokenFalse,
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenTrue,
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenNull,
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenArrayBegin,
					JsonGrammar.TokenString('a'),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString('b'),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString('c'),
					JsonGrammar.TokenArrayEnd,
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenArrayBegin,
					JsonGrammar.TokenNumber(1),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenNumber(2),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenNumber(3),
					JsonGrammar.TokenArrayEnd,
					JsonGrammar.TokenArrayEnd
				};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Array Tests

		#region Object Tests

		[Fact]
		public void GetTokens_EmptyObject_ReturnsEmptyObjectTokens()
		{
			var input = new object();

			var expected = new[]
				{
					JsonGrammar.TokenObjectBegin,
					JsonGrammar.TokenObjectEnd
				};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_EmptyDictionary_ReturnsEmptyObjectTokens()
		{
			var input = new Dictionary<string,object>(0);

			var expected = new[]
				{
					JsonGrammar.TokenObjectBegin,
					JsonGrammar.TokenObjectEnd
				};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ObjectAnonymous_ReturnsObjectTokens()
		{
			var input = new
			{
				One = 1,
				Two = 2,
				Three = 3
			};

			var expected = new[]
				{
					JsonGrammar.TokenObjectBegin,
					JsonGrammar.TokenString("One"),
					JsonGrammar.TokenPairDelim,
					JsonGrammar.TokenNumber(1),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString("Two"),
					JsonGrammar.TokenPairDelim,
					JsonGrammar.TokenNumber(2),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString("Three"),
					JsonGrammar.TokenPairDelim,
					JsonGrammar.TokenNumber(3),
					JsonGrammar.TokenObjectEnd
				};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ObjectDynamic_ReturnsObjectTokens()
		{
			dynamic input = new System.Dynamic.ExpandoObject();
			input.One = 1;
			input.Two = 2;
			input.Three = 3;

			var expected = new[]
				{
					JsonGrammar.TokenObjectBegin,
					JsonGrammar.TokenString("One"),
					JsonGrammar.TokenPairDelim,
					JsonGrammar.TokenNumber(1),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString("Two"),
					JsonGrammar.TokenPairDelim,
					JsonGrammar.TokenNumber(2),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString("Three"),
					JsonGrammar.TokenPairDelim,
					JsonGrammar.TokenNumber(3),
					JsonGrammar.TokenObjectEnd
				};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings());
			var actual = walker.GetTokens((object)input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_ObjectDictionary_ReturnsObjectTokens()
		{
			var input = new Dictionary<string, object>
			{
				{ "One", 1 },
				{ "Two", 2 },
				{ "Three", 3 }
			};

			var expected = new[]
				{
					JsonGrammar.TokenObjectBegin,
					JsonGrammar.TokenString("One"),
					JsonGrammar.TokenPairDelim,
					JsonGrammar.TokenNumber(1),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString("Two"),
					JsonGrammar.TokenPairDelim,
					JsonGrammar.TokenNumber(2),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString("Three"),
					JsonGrammar.TokenPairDelim,
					JsonGrammar.TokenNumber(3),
					JsonGrammar.TokenObjectEnd
				};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Object Tests

		#region Boolean Tests

		[Fact]
		public void GetTokens_False_ReturnsFalseToken()
		{
			var input = false;

			var expected = new[]
				{
					JsonGrammar.TokenFalse
				};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_True_ReturnsTrueToken()
		{
			var input = true;

			var expected = new[]
				{
					JsonGrammar.TokenTrue
				};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Boolean Tests

		#region Number Case Tests

		[Fact]
		public void GetTokens_NaN_ReturnsNaNToken()
		{
			var input = Double.NaN;

			var expected = new[]
				{
					JsonGrammar.TokenNaN
				};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_PosInfinity_ReturnsPosInfinityToken()
		{
			var input = Double.PositiveInfinity;

			var expected = new[]
				{
					JsonGrammar.TokenPositiveInfinity
				};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_NegInfinity_ReturnsNegInfinityToken()
		{
			var input = Double.NegativeInfinity;

			var expected = new[]
				{
					JsonGrammar.TokenNegativeInfinity
				};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Number Tests

		#region Complex Graph Tests

		[Fact]
		public void GetTokens_GraphComplex_ReturnsObjectTokens()
		{
			var input = new object[] {
				"JSON Test Pattern pass1",
				new Dictionary<string, object>
				{
					{ "object with 1 member", new[] { "array with 1 element" } },
				},
				new Dictionary<string, object>(),
				new object[0],
				-42,
				true,
				false,
				null,
				new Dictionary<string, object> {
					{ "integer", 1234567890 },
					{ "real", -9876.543210 },
					{ "e", 0.123456789e-12 },
					{ "E", 1.234567890E+34 },
					{ "", 23456789012E66 },
					{ "zero", 0 },
					{ "one", 1 },
					{ "space", " " },
					{ "quote", "\"" },
					{ "backslash", "\\" },
					{ "controls", "\b\f\n\r\t" },
					{ "slash", "/ & /" },
					{ "alpha", "abcdefghijklmnopqrstuvwyz" },
					{ "ALPHA", "ABCDEFGHIJKLMNOPQRSTUVWYZ" },
					{ "digit", "0123456789" },
					{ "0123456789", "digit" },
					{ "special", "`1~!@#$%^&*()_+-={':[,]}|;.</>?" },
					{ "hex", "\u0123\u4567\u89AB\uCDEF\uabcd\uef4A" },
					{ "true", true },
					{ "false", false },
					{ "null", null },
					{ "array", new object[0] },
					{ "object", new Dictionary<string, object>() },
					{ "address", "50 St. James Street" },
					{ "url", "http://www.JSON.org/" },
					{ "comment", "// /* <!-- --" },
					{ "# -- --> */", " " },
					{ " s p a c e d ", new [] { 1,2,3,4,5,6,7 } },
					{ "compact", new [] { 1,2,3,4,5,6,7 } },
					{ "jsontext", "{\"object with 1 member\":[\"array with 1 element\"]}" },
					{ "quotes", "&#34; \u0022 %22 0x22 034 &#x22;" },
					{ "/\\\"\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?", "A key can be any string" }
				},
				0.5,
				98.6,
				99.44,
				1066,
				1e1,
				0.1e1,
				1e-1,
				1e00,
				2e+00,
				2e-00,
				"rosebud"
			};

			var expected = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("JSON Test Pattern pass1"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("object with 1 member"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("array with 1 element"),
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(-42),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenTrue,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenFalse,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("integer"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(1234567890),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("real"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(-9876.543210),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("e"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(0.123456789e-12),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("E"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(1.234567890E+34),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString(""),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(23456789012E66),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("zero"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("one"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(1),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("space"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString(" "),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("quote"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("\""),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("backslash"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("\\"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("controls"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("\b\f\n\r\t"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("slash"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("/ & /"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("alpha"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("abcdefghijklmnopqrstuvwyz"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("ALPHA"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("ABCDEFGHIJKLMNOPQRSTUVWYZ"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("digit"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("0123456789"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("0123456789"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("digit"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("special"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("`1~!@#$%^&*()_+-={':[,]}|;.</>?"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("hex"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("\u0123\u4567\u89AB\uCDEF\uabcd\uef4A"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("true"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenTrue,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("false"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenFalse,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("null"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("array"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("object"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("address"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("50 St. James Street"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("url"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("http://www.JSON.org/"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("comment"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("// /* <!-- --"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("# -- --> */"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString(" "),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString(" s p a c e d "),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenNumber(1),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(2),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(3),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(4),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(5),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(6),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(7),
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("compact"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenNumber(1),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(2),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(3),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(4),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(5),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(6),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(7),
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("jsontext"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("{\"object with 1 member\":[\"array with 1 element\"]}"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("quotes"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("&#34; \u0022 %22 0x22 034 &#x22;"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("/\\\"\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("A key can be any string"),
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(0.5),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(98.6),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(99.44),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(1066),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(10.0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(1.0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(0.1),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(1.0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(2.0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(2.0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("rosebud"),
				JsonGrammar.TokenArrayEnd
			};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Complex Graph Tests

		#region Graph Cycles Tests

		[Fact]
		public void GetTokens_GraphCycleTypeIgnore_ReplacesCycleStartWithNull()
		{
			var input = new Person
			{
				Name = "John, Jr.",
				Father = new Person
				{
					Name = "John, Sr."
				},
				Mother = new Person
				{
					Name = "Sally"
				}
			};

			// create multiple cycles
			input.Father.Children = input.Mother.Children = new Person[]
			{
				input
			};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings
			{
				GraphCycles = GraphCycleType.Ignore
			});

			var expected = new[]
				{
					JsonGrammar.TokenObjectBegin,
					JsonGrammar.TokenString("Name"),
					JsonGrammar.TokenPairDelim,
					JsonGrammar.TokenString("John, Jr."),
					JsonGrammar.TokenValueDelim,

					JsonGrammar.TokenString("Father"),
					JsonGrammar.TokenPairDelim,
					JsonGrammar.TokenObjectBegin,
					JsonGrammar.TokenString("Name"),
					JsonGrammar.TokenPairDelim,
					JsonGrammar.TokenString("John, Sr."),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString("Father"),
					JsonGrammar.TokenPairDelim,
					JsonGrammar.TokenNull,
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString("Mother"),
					JsonGrammar.TokenPairDelim,
					JsonGrammar.TokenNull,
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString("Children"),
					JsonGrammar.TokenPairDelim,
					JsonGrammar.TokenArrayBegin,
					JsonGrammar.TokenNull,
					JsonGrammar.TokenArrayEnd,
					JsonGrammar.TokenObjectEnd,
					JsonGrammar.TokenValueDelim,

					JsonGrammar.TokenString("Mother"),
					JsonGrammar.TokenPairDelim,
					JsonGrammar.TokenObjectBegin,
					JsonGrammar.TokenString("Name"),
					JsonGrammar.TokenPairDelim,
					JsonGrammar.TokenString("Sally"),
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString("Father"),
					JsonGrammar.TokenPairDelim,
					JsonGrammar.TokenNull,
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString("Mother"),
					JsonGrammar.TokenPairDelim,
					JsonGrammar.TokenNull,
					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString("Children"),
					JsonGrammar.TokenPairDelim,
					JsonGrammar.TokenArrayBegin,
					JsonGrammar.TokenNull,
					JsonGrammar.TokenArrayEnd,
					JsonGrammar.TokenObjectEnd,

					JsonGrammar.TokenValueDelim,
					JsonGrammar.TokenString("Children"),
					JsonGrammar.TokenPairDelim,
					JsonGrammar.TokenNull,

					JsonGrammar.TokenObjectEnd
				};

			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetTokens_GraphCycleTypeReferences_ThrowsGraphCycleException()
		{
			var input = new Person
			{
				Name = "John, Jr.",
				Father = new Person
				{
					Name = "John, Sr."
				},
				Mother = new Person
				{
					Name = "Sally"
				}
			};

			// create multiple cycles
			input.Father.Children = input.Mother.Children = new Person[]
			{
				input
			};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings
			{
				GraphCycles = GraphCycleType.Reference
			});

			GraphCycleException ex = Assert.Throws<GraphCycleException>(
				delegate
				{
					walker.GetTokens(input).ToArray();
				});

			Assert.Equal(GraphCycleType.Reference, ex.CycleType);
		}

		[Fact]
		public void GetTokens_GraphCycleTypeMaxDepth_ThrowsGraphCycleException()
		{
			var input = new Person
			{
				Name = "John, Jr.",
				Father = new Person
				{
					Name = "John, Sr."
				},
				Mother = new Person
				{
					Name = "Sally"
				}
			};

			// create multiple cycles
			input.Father.Children = input.Mother.Children = new Person[]
			{
				input
			};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings
			{
				GraphCycles = GraphCycleType.MaxDepth,
				MaxDepth = 25
			});

			GraphCycleException ex = Assert.Throws<GraphCycleException>(
				delegate
				{
					walker.GetTokens(input).ToArray();
				});

			Assert.Equal(GraphCycleType.MaxDepth, ex.CycleType);
		}

		[Fact]
		public void GetTokens_GraphCycleTypeMaxDepthNoMaxDepth_ThrowsArgumentException()
		{
			var input = new Person
			{
				Name = "John, Jr.",
				Father = new Person
				{
					Name = "John, Sr."
				},
				Mother = new Person
				{
					Name = "Sally"
				}
			};

			// create multiple cycles
			input.Father.Children = input.Mother.Children = new Person[]
			{
				input
			};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings
			{
				GraphCycles = GraphCycleType.MaxDepth,
				MaxDepth = 0
			});

			ArgumentException ex = Assert.Throws<ArgumentException>(
				delegate
				{
					walker.GetTokens(input).ToArray();
				});

			Assert.Equal("maxDepth", ex.ParamName);
		}

		[Fact]
		public void GetTokens_GraphCycleTypeMaxDepthFalsePositive_ThrowsGraphCycleException()
		{
			// input from fail18.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				new []
				{
					new []
					{
						new []
						{
							new []
							{
								new []
								{
									new []
									{
										new []
										{
											new []
											{
												new []
												{
													new []
													{
														new []
														{
															new []
															{
																new []
																{
																	new []
																	{
																		new []
																		{
																			new []
																			{
																				new []
																				{
																					new []
																					{
																						new []
																						{
																							"Too deep"
																						}
																					}
																				}
																			}
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings
			{
				GraphCycles = GraphCycleType.MaxDepth,
				MaxDepth = 19
			});

			GraphCycleException ex = Assert.Throws<GraphCycleException>(
				delegate
				{
					walker.GetTokens(input).ToArray();
				});

			Assert.Equal(GraphCycleType.MaxDepth, ex.CycleType);
		}

		#endregion Graph Cycles Tests

		#region Input Edge Case Tests

		[Fact]
		public void GetTokens_Null_ReturnsNullToken()
		{
			var input = (object)null;

			var expected = new[]
				{
					JsonGrammar.TokenNull
				};

			var walker = new JsonWriter.JsonWalker(new DataWriterSettings());
			var actual = walker.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Ctor_NullSettings_ThrowsArgumentNullException()
		{
			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate
				{
					var walker = new JsonWriter.JsonWalker(null);
				});

			// verify exception is coming from expected param
			Assert.Equal("settings", ex.ParamName);
		}

		#endregion Input Edge Case Tests
	}
}
