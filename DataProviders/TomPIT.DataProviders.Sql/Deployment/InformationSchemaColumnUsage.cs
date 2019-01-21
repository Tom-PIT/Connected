namespace TomPIT.DataProviders.Sql.Deployment
{
	internal class InformationSchemaColumnUsage
	{
		public string TableSchema { get; set; }
		public string TableName { get; set; }
		public string ColumnName { get; set; }
		public string ConstraintSchema { get; set; }
		public string ConstraintName { get; set; }
	}
}
