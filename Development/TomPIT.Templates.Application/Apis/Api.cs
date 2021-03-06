﻿using System.ComponentModel;
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
	public class Api : TextConfiguration, IApiConfiguration
	{
		private ListItems<IApiOperation> _ops = null;

		[Items(DesignUtils.ApiOperationItems)]
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

		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[DefaultValue(ElementScope.Public)]
		public ElementScope Scope { get; set; } = ElementScope.Public;
		[Browsable(false)]
		public override string FileName => $"{ToString()}.csx";

		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Namespace { get; set; }
	}
}
