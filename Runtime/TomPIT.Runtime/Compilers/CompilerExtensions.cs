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
			else if (sourceCode is IPartialSourceCode ps)
				return string.Format("{0}.csx", ps.Configuration().ComponentName(connection));

			return sourceCode.ToString();
		}

		public static Guid ScriptId(this IText sourceCode)
		{
			var id = sourceCode.Id;

			if (sourceCode is IPartialSourceCode)
			{
				var container = sourceCode.Closest<ISourceCodeContainer>();

				if (container == null)
					return id;

				id = container.Configuration().Component;
			}

			return id;
		}
	}
}
