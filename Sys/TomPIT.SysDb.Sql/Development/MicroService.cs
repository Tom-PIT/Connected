using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Development
{
	internal class MicroService : PrimaryKeyRecord, IMicroService
	{
		[MaxLength(128)]
		[Required]
		public string Name { get; set; }
		[Browsable(false)]
		public string Url { get; set; }
		[Browsable(false)]
		[KeyProperty]
		public Guid Token { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		public MicroServiceStages SupportedStages { get; set; }
		[Browsable(false)]
		public Guid ResourceGroup { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items("TomPIT.Management.Items.MicroServiceTemplatesItems, TomPIT.Management")]
		public Guid Template { get; set; }
		[Editable(false)]
		public string Version { get; set; }

		[Editable(false)]
		public string Commit { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Name = GetString("name");
			Url = GetString("url");
			Token = GetGuid("token");
			SupportedStages = GetValue("supported_stages", MicroServiceStages.Any);
			ResourceGroup = GetGuid("resource_token");
			Template = GetGuid("template");
			Version = GetString("version");
			Commit = GetString("commit");
		}

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				 ? base.ToString()
				 : Name;
		}
	}
}