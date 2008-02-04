#region BuildTools License
/*---------------------------------------------------------------------------------*\

	BuildTools distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2007 Stephen M. McKamey

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
#endregion BuildTools License

using System;

namespace JsonFx.Json
{
	/// <summary>
	/// Specifies the naming to use when serializing to JSON.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple=false)]
	public sealed class JsonIgnoreAttribute : Attribute
	{
		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public JsonIgnoreAttribute()
		{
		}

		#endregion Init

		#region Methods

		/// <summary>
		/// Gets a value which indicates if should be ignored in Json serialization.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsJsonIgnore(object value)
		{
			if (value == null)
				return false;

			Type type = value.GetType();
			System.Reflection.MemberInfo memberInfo = null;

			if (type.IsEnum)
			{
				memberInfo = type.GetField(Enum.GetName(type, value));
			}
			else
			{
				memberInfo = value as System.Reflection.MemberInfo;
			}

			if (memberInfo == null)
			{
				throw new NotImplementedException();
			}

			return JsonIgnoreAttribute.IsDefined(memberInfo, typeof(JsonIgnoreAttribute));
		}

		#endregion Methods
	}
}