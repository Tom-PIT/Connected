using TomPIT.BigData.Partitions;

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
