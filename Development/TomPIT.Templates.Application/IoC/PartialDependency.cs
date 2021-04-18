using System.ComponentModel;
using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel.IoC;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.IoC
{
	[ClassRequired]
	public class PartialDependency : UIDependency, IPartialDependency
	{
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items(DesignUtils.PartialsItems)]
		public string Partial { get; set; }
		[Browsable(false)]
		public override string FileName => $"{ToString()}.csx";
	}
}
