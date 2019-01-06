using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;

namespace TomPIT.ComponentModel
{
	internal class Component : IComponent
	{
		[InvalidateEnvironment(Ide.EnvironmentSection.Explorer | Ide.EnvironmentSection.Designer)]
		[Required]
		[MaxLength(128)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Name { get; set; }
		[Browsable(false)]
		public Guid MicroService { get; set; }
		[Browsable(false)]
		public Guid Feature { get; set; }
		[Browsable(false)]
		[KeyProperty]
		public Guid Token { get; set; }
		[Browsable(false)]
		public string Type { get; set; }
		[Browsable(false)]
		public string Category { get; set; }
		[Browsable(false)]
		public Guid RuntimeConfiguration { get; set; }
		[Browsable(false)]
		public DateTime Modified { get; set; }

		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return base.ToString();

			return Name;
		}
	}
}
