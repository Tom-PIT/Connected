using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace TomPIT.ComponentModel
{
	public class SourceCodeConfiguration : ComponentConfiguration, ISourceCode
	{
		[Browsable(false)]
		public Guid TextBlob { get; set; }
	}
}
