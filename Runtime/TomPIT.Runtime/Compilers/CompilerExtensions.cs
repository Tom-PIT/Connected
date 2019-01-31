using System;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;
using TomPIT.Connectivity;

namespace TomPIT.Compilers
{
	public static class CompilerExtensions
	{
		public static string ScriptName(this IText sourceCode, ISysConnection connection)
		{
			if (sourceCode is IServerEvent)
			{
				var parent = sourceCode.Parent;
				var props = parent.GetType().GetProperties();

				foreach (var i in props)
				{
					var value = i.GetValue(parent);

					if (value == sourceCode)
						return string.Format("{0}.{1}.csx", parent.ToString(), i.Name);
				}
			}

			return sourceCode.ToString();
		}

		public static Guid ScriptId(this IText sourceCode)
		{
			return sourceCode.Id;
		}
	}
}
