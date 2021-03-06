using Newtonsoft.Json.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Newtonsoft.Json.Linq
{
	public struct JEnumerable<T> : IEnumerable, IEnumerable<T>, IJEnumerable<T> where T : JToken
	{
		public static readonly JEnumerable<T> Empty = new JEnumerable<T>(Enumerable.Empty<T>());

		private IEnumerable<T> _enumerable;

		public IJEnumerable<JToken> this[object key]
		{
			get
			{
				return new JEnumerable<JToken>(this._enumerable.Values(key));
			}
		}

		public JEnumerable(IEnumerable<T> enumerable)
		{
			ValidationUtils.ArgumentNotNull(enumerable, "enumerable");
			this._enumerable = enumerable;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this._enumerable.GetEnumerator();
		}

		public override bool Equals(object obj)
		{
			return obj is JEnumerable<T> && this._enumerable.Equals(((JEnumerable<T>)obj)._enumerable);
		}

		public override int GetHashCode()
		{
			return this._enumerable.GetHashCode();
		}
	}
}
