using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Resources;

namespace TomPIT.MicroServices.Resources
{
	public class ScriptFileSystemSource : ScriptSource, IScriptFileSystemSource
	{
		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		public string VirtualPath { get; set; }
	}
}
