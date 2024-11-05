using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;

namespace TomPIT.ComponentModel;
public class PackageReference : Element, IPackageReference
{
	[Required]
	[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
	public string PackageName { get; set; }

	[Required]
	[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
	public string Version { get; set; }

	public override string ToString()
	{
		return string.IsNullOrWhiteSpace(PackageName)
			? base.ToString()
			: PackageName;
	}
}
