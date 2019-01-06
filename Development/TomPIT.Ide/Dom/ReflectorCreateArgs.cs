using System.Reflection;
using TomPIT.Ide;

namespace TomPIT.Dom
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
