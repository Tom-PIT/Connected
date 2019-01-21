using TomPIT.Designers;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	public class PackageElement : Element
	{
		private IDomDesigner _designer = null;
		public const string DomId = "{1C2FADC9-8E2F-4277-8551-4BDEE39871D7}";

		public PackageElement(IEnvironment environment, IDomElement parent) : base(environment, parent)
		{
			Title = "Package";
			Id = DomId;
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new PackageDesigner(Environment, this);

				return _designer;
			}
		}
	}
}
