#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2009 Stephen M. McKamey

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
using System.Net;
using System.Web;
using System.Web.Mvc;

using JsonFx.Json;

namespace JsonFx.Mvc
{
	/// <summary>
	/// Serializes the data according to the specified format
	/// </summary>
	public class DataResult : ActionResult
	{
		#region Constants

		private const string DefaultContentType = "text/plain";

		#endregion Constants

		#region Fields

		private readonly IDataWriter Writer;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="serializer">the serialization implementation</param>
		public DataResult(IDataWriter writer)
		{
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}

			this.Writer = writer;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets the data to be serialized
		/// </summary>
		public object Data
		{
			get;
			set;
		}

		/// <summary>
		/// Gets and sets the HTTP status code of the response
		/// </summary>
		public HttpStatusCode HttpStatusCode
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the underlying IDataWriter
		/// </summary>
		public IDataWriter DataWriter
		{
			get { return this.Writer; }
		}

		#endregion Properties

		#region ActionResult Members

		/// <summary>
		/// Executes the result
		/// </summary>
		/// <param name="context">ControllerContext</param>
		public override void ExecuteResult(ControllerContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			HttpResponseBase response = context.HttpContext.Response;

			if (this.HttpStatusCode != default(HttpStatusCode))
			{
				response.StatusCode = (int)this.HttpStatusCode;
			}

			if (String.IsNullOrEmpty(this.Writer.ContentType))
			{
				// use the default content type
				response.ContentType = DataResult.DefaultContentType;
			}
			else
			{
				// set the response content type
				response.ContentType = this.Writer.ContentType;
			}

			if (this.Writer.ContentEncoding != null)
			{
				// set the response content encoding
				response.ContentEncoding = this.Writer.ContentEncoding;
			}

			string ext = this.Writer.FileExtension;
			if (!String.IsNullOrEmpty(ext))
			{
				if (!ext.StartsWith("."))
				{
					ext = '.'+ext;
				}

				// this helps IE determine the Content-Type
				response.Headers["Content-Disposition"] = "inline;filename=data"+ext;
			}

			if (this.Data != null)
			{
				this.Writer.Serialize(response.Output, this.Data);
			}
		}

		#endregion ActionResult Members

		#region Header Methods

		/// <summary>
		/// Parses HTTP headers for Media-Types
		/// </summary>
		/// <param name="accept">HTTP Accept header</param>
		/// <param name="contentType">HTTP Content-Type header</param>
		/// <returns>sequence of Media-Types</returns>
		/// <remarks>
		/// http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html
		/// </remarks>
		public static IEnumerable<string> ParseHeaders(string accept, string contentType)
		{
			string mime;

			// check for a matching accept type
			foreach (string type in DataResult.SplitTrim(accept, ','))
			{
				mime = DataResult.ParseMediaType(type);
				if (!String.IsNullOrEmpty(mime))
				{
					yield return mime;
				}
			}

			// fallback on content-type
			mime = DataResult.ParseMediaType(contentType);
			if (!String.IsNullOrEmpty(mime))
			{
				yield return mime;
			}
		}

		private static string ParseMediaType(string type)
		{
			foreach (string mime in DataResult.SplitTrim(type, ';'))
			{
				// only return first part
				return mime;
			}

			// if no parts was empty
			return String.Empty;
		}

		private static IEnumerable<string> SplitTrim(string source, char ch)
		{
			if (String.IsNullOrEmpty(source))
			{
				yield break;
			}

			int length = source.Length;
			for (int prev=0, next=0; prev<length && next>=0; prev=next+1)
			{
				next = source.IndexOf(ch, prev);
				if (next < 0)
				{
					next = length;
				}

				string part = source.Substring(prev, next-prev).Trim();
				if (part.Length > 0)
				{
					yield return part;
				}
			}
		}

		#endregion Methods
	}
}
