using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;

namespace TomPIT.MicroServices.Resources
{
	public abstract class ScriptSource : ConfigurationElement, IScriptSource
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[Required]
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		public string Name { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? base.ToString()
				: Name;
		}
	}
}
