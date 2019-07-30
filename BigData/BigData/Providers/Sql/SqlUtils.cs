using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.BigData.Services;

namespace TomPIT.BigData.Providers.Sql
{
	internal static class SqlUtils
	{
		public static string TableName(this DataFileContext context)
		{
			return TableName(context.File);
		}

		public static string TableName(this IPartitionFile file)
		{
			return file.FileName.ToString("N");
		}
	}
}
