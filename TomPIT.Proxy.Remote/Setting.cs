﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Configuration;

namespace TomPIT.Proxy.Remote
{
	internal class Setting : ISetting
	{
		[KeyProperty]
		[Browsable(false)]
		public string Name { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[MaxLength(1024)]
		[InvalidateEnvironment(EnvironmentSection.Designer)]
		public string Value { get; set; }

		public string Type { get; set; }

		public string PrimaryKey { get; set; }

		public string NameSpace { get; set; }
		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? base.ToString()
				: Name;
		}
	}
}