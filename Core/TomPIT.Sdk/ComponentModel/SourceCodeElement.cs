using System;
using System.ComponentModel;

namespace TomPIT.ComponentModel
{
	public abstract class SourceCodeElement : ConfigurationElement, IText
	{
		[Browsable(false)]
		public Guid TextBlob { get; set; }
	}
}
