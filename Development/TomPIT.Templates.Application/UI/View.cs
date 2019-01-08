using System;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Security;

namespace TomPIT.Application.UI
{
	[Create("View")]
	[DomDesigner("TomPIT.Designers.TextDesigner, TomPIT.Ide")]
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
