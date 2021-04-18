using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;

namespace TomPIT.MicroServices.BigData
{
	[Syntax(SyntaxAttribute.Sql)]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	public class BigDataSqlQuery : Text, IBigDataQuery
	{
		[Required]
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Name { get; set; }

		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return base.ToString();

			return Name;
		}
		[Browsable(false)]
		public override string FileName => $"{ToString()}.sql";
	}
}
