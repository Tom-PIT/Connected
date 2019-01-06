using System;
using TomPIT.Data;

namespace TomPIT.SysDb.Sql
{
	public static class SqlExtensions
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
