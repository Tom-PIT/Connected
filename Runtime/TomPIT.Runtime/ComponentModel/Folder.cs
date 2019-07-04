using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Deployment;

namespace TomPIT.ComponentModel
{
	//[DomDesigner(DomDesignerAttribute.PermissionsDesigner, Mode = Services.EnvironmentMode.Runtime)]
	internal class Folder : IFolder
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
		[Browsable(false)]
		public Guid Parent { get; set; }

		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return GetType().ShortName();

			return Name;
		}

		public static IFolder FromPackageFolder(Guid microservice, IPackageFolder folder)
		{
			return new Folder
			{
				Name = folder.Name,
				Parent = folder.Parent,
				Token = folder.Token,
				MicroService = microservice
			};
		}
	}
}
