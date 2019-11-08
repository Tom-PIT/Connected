using System.Collections.Generic;
using TomPIT.Connectivity;

namespace TomPIT.Rest.ComponentModel
{
	internal static class Connection
	{
		private static List<string> _resourceGroups = null;

		public static T GetService<T>()
		{
			return Shell.GetService<IConnectivityService>().SelectDefaultTenant().GetService<T>();
		}

		public static List<string> ResourceGroups
		{
			get
			{
				if (_resourceGroups == null)
					_resourceGroups = new List<string>();

				return _resourceGroups;
			}
		}

		public static string Endpoint { get; set; }
	}
}
