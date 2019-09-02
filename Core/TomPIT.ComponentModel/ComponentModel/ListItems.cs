using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TomPIT.Annotations;

namespace TomPIT.ComponentModel
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
