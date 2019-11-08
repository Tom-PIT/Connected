using System;

namespace TomPIT.Compilation
{
	internal static class ScriptTypeComparer
	{
		public static bool Compare(Type left, Type right)
		{
			if (string.Compare(left.Name, right.Name, false) != 0)
				return false;

			if (left.GetMembers().Length != right.GetMembers().Length)
				return false;

			return true;
			//TODO: implemet additional checking
		}
	}
}
