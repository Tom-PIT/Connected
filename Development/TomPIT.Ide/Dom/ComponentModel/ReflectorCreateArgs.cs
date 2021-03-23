using System.Reflection;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;

namespace TomPIT.Ide.Dom.ComponentModel
{
	public class ReflectorCreateArgs
	{
		public IEnvironment Environment { get; set; }
		public IDomElement Parent { get; set; }
		public object Instance { get; set; }
		public int Index { get; set; }
		public PropertyInfo Property { get; set; }
	}
}
