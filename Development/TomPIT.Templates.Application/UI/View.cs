using System;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.Security;
using TomPIT.Services;

namespace TomPIT.Application.UI
{
	[Create("View")]
	[DomDesigner("TomPIT.Designers.TextDesigner, TomPIT.Ide")]
	[DomDesigner(DomDesignerAttribute.PermissionsDesigner, Mode = Services.EnvironmentMode.Runtime)]
	[DomElement("TomPIT.Application.Design.Dom.ViewElement, TomPIT.Application.Design")]
	[Syntax("razor")]
	public class View : ViewBase, IApplicationView, IAuthorizationChain
	{
		public const string ComponentCategory = "View";
		public const string ComponentAuthority = "View";

		[Browsable(false)]
		public Guid Area { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryRouting)]
		public string Url { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryAppearance)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items("TomPIT.Application.Design.Items.LayoutItems, TomPIT.Application.Design")]
		public string Layout { get; set; }

		[Browsable(false)]
		public Guid AuthorizationParent => Area;

		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDiagnostic)]
		public bool MetricEnabled { get; set; }
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDiagnostic)]
		public MetricLevel MetricLevel { get; set; } = MetricLevel.General;

		public override void ComponentCreated(ComponentModel.IComponent scope)
		{
			if (scope == null)
				throw new TomPITException(SR.ErrComponentScopeExpected);

			if (string.Compare(scope.Category, Application.Area.ComponentCategory, true) != 0)
				throw new TomPITException(string.Format(SR.ErrInvalidComponentScope, scope.Category, Application.Area.ComponentCategory));

			Area = scope.Token;
		}
	}
}
