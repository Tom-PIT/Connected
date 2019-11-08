using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Scripting;
using TomPIT.MicroServices.Design;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.Scripting
{
	[Create(DesignUtils.Class, nameof(Name))]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[EventArguments(typeof(IMiddlewareContext))]
	public class CSharpScript : Text, IPartialSourceCode
	{
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		public string Name { get; set; }

		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return string.Format("{0}.csx", GetType().ShortName());

			return string.Format("{0}.csx", Name);
		}
	}
}
