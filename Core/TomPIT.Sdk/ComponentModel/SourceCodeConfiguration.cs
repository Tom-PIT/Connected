using System;
using System.ComponentModel;

namespace TomPIT.ComponentModel
{
	public class SourceCodeConfiguration : ComponentConfiguration, IText
	{
		[Browsable(false)]
		public Guid TextBlob { get; set; }
	}
}
