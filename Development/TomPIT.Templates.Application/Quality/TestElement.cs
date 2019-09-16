using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Quality;

namespace TomPIT.MicroServices.Quality
{
	public abstract class TestElement : ConfigurationElement, ITestElement
	{
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[Required]
		public string Name { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		[DefaultValue(true)]
		public bool Enabled { get; set; } = true;
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		[DefaultValue(TestErrorBehavior.Stop)]
		public TestErrorBehavior ErrorBehavior { get; set; } = TestErrorBehavior.Stop;
		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name) ? base.ToString() : Name;
		}
	}
}
