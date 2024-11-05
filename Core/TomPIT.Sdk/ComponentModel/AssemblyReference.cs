using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;

namespace TomPIT.ComponentModel;
public class AssemblyReference : Element, IAssemblyReference
{
	[Required]
	[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
	public string AssemblyName { get; set; }

	public override string ToString()
	{
		return string.IsNullOrWhiteSpace(AssemblyName)
			? base.ToString()
			: AssemblyName;
	}
}
