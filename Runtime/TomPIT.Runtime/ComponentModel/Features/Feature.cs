using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;

namespace TomPIT.ComponentModel.Features
{
	internal class Feature : IFeature
	{
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[Required]
		[MaxLength(128)]
		public string Name { get; set; }

		[KeyProperty]
		[Browsable(false)]
		public Guid Token { get; set; }
		[Browsable(false)]
		public Guid MicroService { get; set; }

		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return GetType().ShortName();

			return Name;
		}
	}
}
