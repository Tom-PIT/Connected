using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;

namespace TomPIT.Collections
{
	[SuppressProperties("Capacity")]
	[JsonObject]
	public class ListItems<T> : ConnectedList<T, IElement>, IElement
	{
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[KeyProperty]
		[JsonProperty]
		public Guid Id { get; set; } = Guid.NewGuid();

		public void Reset()
		{
			Id = Guid.NewGuid();
		}

		public static implicit operator List<T>(ListItems<T> items)
		{
			if (items == null)
				return null;

			var r = new List<T>();

			if (items.Count > 0)
				r.AddRange(items);

			return r;
		}

		public static implicit operator ListItems<T>(List<T> items)
		{
			if (items == null)
				return null;

			var r = new ListItems<T>();

			if (items.Count > 0)
				r.AddRange(items);

			return r;
		}
	}
}
