using System;
using System.ComponentModel;

namespace TomPIT.ComponentModel
{
	public abstract class TextConfiguration : ComponentConfiguration, IText
	{
		[Browsable(false)]
		public Guid TextBlob { get; set; }

		[Browsable(false)]
		public abstract string FileName { get; }
	}
}
