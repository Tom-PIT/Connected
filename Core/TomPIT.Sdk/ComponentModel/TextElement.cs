using System;
using System.ComponentModel;

namespace TomPIT.ComponentModel
{
	public abstract class TextElement : ConfigurationElement, IText
	{
		[Browsable(false)]
		public Guid TextBlob { get; set; }
		[Browsable(false)]
		public abstract string FileName { get; }

	}
}
