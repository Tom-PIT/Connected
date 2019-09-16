using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Features;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Features
{
	[Create(DesignUtils.ViewFeature, nameof(Name))]
	public class ViewFeature : Feature, IViewFeature
	{
		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryRouting)]
		public string Url { get; set; }
	}
}
