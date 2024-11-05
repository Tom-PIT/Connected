using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;

namespace TomPIT.ComponentModel
{
	public class Component : IComponent
	{
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[Required]
		[MaxLength(128)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Name { get; set; }
		[Browsable(false)]
		public Guid MicroService { get; set; }
		[Browsable(false)]
		public Guid Folder { get; set; }
		[Browsable(false)]
		[KeyProperty]
		public Guid Token { get; set; }
		[Browsable(false)]
		public string Type { get; set; }
		[Browsable(false)]
		public string Category { get; set; }
		[Browsable(false)]
		public DateTime Modified { get; set; }
		[Browsable(false)]
		public string NameSpace { get; set; }
		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return base.ToString();

			return Name;
		}
	}
}
