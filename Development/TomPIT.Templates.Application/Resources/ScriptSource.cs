using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;

namespace TomPIT.Application.Resources
{
	public abstract class ScriptSource : Element, IScriptSource
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
