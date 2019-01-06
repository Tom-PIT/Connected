using System;
using System.Diagnostics;
using TomPIT.Runtime;

namespace TomPIT
{
	public static class Shell
	{
		private static ServiceContainer _sm = null;

		static Shell()
		{
			_sm = new ServiceContainer(null);
		}
		[DebuggerStepThrough]
		public static T GetService<T>()
		{
			return _sm.Get<T>();
		}

		public static void RegisterService(Type contract, object instance)
		{
			_sm.Register(contract, instance);
		}

		public static void RegisterService(Type contract, ServiceActivatorCallback callback)
		{
			_sm.Register(contract, callback);
		}
	}
}
