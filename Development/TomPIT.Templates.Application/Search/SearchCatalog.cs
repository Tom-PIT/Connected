using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Search;

namespace TomPIT.Application.Search
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class SearchCatalog : ComponentConfiguration, ISearchCatalog
	{
		[Browsable(false)]
		public Guid TextBlob { get; set; }
	}
}
