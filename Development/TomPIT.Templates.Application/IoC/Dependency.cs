using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoC;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.IoC
{
	public abstract class Dependency : SourceCodeElement, IDependency
	{
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Name { get; set; }
		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name) ? GetType().ShortName() : Name;
		}
	}
}
