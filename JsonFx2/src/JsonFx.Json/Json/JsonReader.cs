#region License
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

using JsonFx.Json.Filters;
using JsonFx.Serialization;

namespace JsonFx.Json
{
	/// <summary>
	/// JSON deserializer
	/// </summary>
	public partial class JsonReader : DataReader<DataTokenType>
	{
		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public JsonReader()
			: this(new DataReaderSettings())
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		public JsonReader(DataReaderSettings settings)
			: base(settings, new IDataFilter<DataTokenType>[] { new Iso8601DateFilter() })
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="filters"></param>
		public JsonReader(DataReaderSettings settings, params IDataFilter<DataTokenType>[] filters)
			: base(settings, (IEnumerable<IDataFilter<DataTokenType>>)filters)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="filters"></param>
		public JsonReader(DataReaderSettings settings, IEnumerable<IDataFilter<DataTokenType>> filters)
			: base(settings, filters)
		{
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the supported content type of the serialized data
		/// </summary>
		public override IEnumerable<string> ContentType
		{
			get { yield return "application/json"; }
		}

		#endregion Properties

		#region IDataReader Methods

		protected override ITextTokenizer<DataTokenType> GetTokenizer()
		{
			return new JsonReader.JsonTokenizer();
		}

		protected override IDataAnalyzer<DataTokenType> GetAnalyzer()
		{
			return new DataReader.DataAnalyzer(this.Settings, this.Filters);
		}

		#endregion IDataReader Methods
	}
}
