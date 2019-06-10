using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Features;

namespace TomPIT.Application.Features
{
	[Create("ViewFeature", nameof(Name))]
	public class ViewFeature : Feature, IViewFeature
	{
		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryRouting)]
		public string Url { get; set; }
	}
}
