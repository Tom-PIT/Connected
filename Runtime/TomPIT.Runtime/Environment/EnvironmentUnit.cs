using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;

namespace TomPIT.Environment
{
	internal class EnvironmentUnit : IEnvironmentUnit
	{
		[EnvironmentVisibility(true)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[Required]
		[MaxLength(128)]
		public string Name { get; set; }
		[Browsable(false)]
		public Guid Token { get; set; }
		[Browsable(false)]
		public Guid Parent { get; set; }
		[Browsable(false)]
		public int Ordinal { get; set; }
	}
}
