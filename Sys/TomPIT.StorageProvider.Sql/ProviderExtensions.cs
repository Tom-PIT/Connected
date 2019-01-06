using System;
using TomPIT.Data;

namespace TomPIT.StorageProvider.Sql
{
	public static class ProviderExtensions
	{
		public static object GetId(this object descriptor)
		{
			if (descriptor is IPrimaryKeyRecord)
				return ((IPrimaryKeyRecord)descriptor).Id;
			else if (descriptor is ILongPrimaryKeyRecord)
				return ((ILongPrimaryKeyRecord)descriptor).Id;

			throw new NullReferenceException();
		}
	}
}
