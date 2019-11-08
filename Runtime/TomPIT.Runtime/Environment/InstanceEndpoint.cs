using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Reflection;
using TomPIT.Runtime;

namespace TomPIT.Environment
{
	public class InstanceEndpoint : IInstanceEndpoint
	{
		[MaxLength(1024)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryRouting)]
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		public string Url { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		public InstanceStatus Status { get; set; }
		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[MaxLength(128)]
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		[InvalidateEnvironment(EnvironmentSection.Designer | EnvironmentSection.Explorer)]
		public string Name { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		public InstanceType Type { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		public InstanceVerbs Verbs { get; set; }
		[MaxLength(1024)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryRouting)]
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		public string ReverseProxyUrl { get; set; }
		[KeyProperty]
		[Browsable(false)]
		public Guid Token { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? GetType().ShortName()
				: Name;
		}
	}
}
