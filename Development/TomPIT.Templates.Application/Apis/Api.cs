using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.MicroServices.Design;
using TomPIT.Runtime;

namespace TomPIT.MicroServices.Apis
{
	[Create(DesignUtils.ComponentApi)]
	[DomElement(DesignUtils.ApiElement)]
	[DomDesigner(DomDesignerAttribute.PermissionsDesigner, Mode = EnvironmentMode.Runtime)]
	[Manifest(DesignUtils.ApiManifest)]
	[DomDesigner(DomDesignerAttribute.TextDesigner, Mode = EnvironmentMode.Design)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ClassRequired]
	[ComponentCreatedHandler("TomPIT.MicroServices.Design.CreateHandlers.Api, TomPIT.MicroServices.Design")]
	public class Api : SourceCodeConfiguration, IApiConfiguration
	{
		private ListItems<IApiOperation> _ops = null;
		private ApiProtocolOptions _protocols = null;

		[Items(DesignUtils.ApiOperationItems)]
		[EnvironmentVisibility(EnvironmentMode.Any)]
		[DomDesigner(DomDesignerAttribute.EmptyDesigner, Mode = EnvironmentMode.Runtime)]
		public ListItems<IApiOperation> Operations
		{
			get
			{
				if (_ops == null)
					_ops = new ListItems<IApiOperation> { Parent = this };

				return _ops;
			}
		}

		[EnvironmentVisibility(EnvironmentMode.Any)]
		public IApiProtocolOptions Protocols
		{
			get
			{
				if (_protocols == null)
					_protocols = new ApiProtocolOptions
					{
						Parent = this
					};

				return _protocols;
			}
		}

		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[DefaultValue(ElementScope.Public)]
		public ElementScope Scope { get; set; } = ElementScope.Public;

	}
}
