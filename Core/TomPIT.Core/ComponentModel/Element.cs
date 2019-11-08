using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using TomPIT.Reflection;

namespace TomPIT.ComponentModel
{
	public class Element : IElement
	{
		public Element()
		{
			Id = Guid.NewGuid();
		}

		[ReadOnly(true)]
		[Editor("Label", "Label")]
		[Display(GroupName = "Design")]
		[Browsable(false)]
		public Guid Id { get; set; }

		[Browsable(false)]
		[JsonIgnore]
		public IElement Parent { get; set; }

		public void Reset()
		{
			Id = Guid.NewGuid();
		}

		public override string ToString()
		{
			return GetType().ShortName();
		}
	}
}
