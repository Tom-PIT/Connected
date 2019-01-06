using System;
using System.ComponentModel;

namespace TomPIT.ComponentModel
{
	public abstract class Text : Element, IText
	{
		[Browsable(false)]
		public Guid TextBlob { get; set; }
	}
}
