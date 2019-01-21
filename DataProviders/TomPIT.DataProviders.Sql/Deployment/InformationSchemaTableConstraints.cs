namespace TomPIT.DataProviders.Sql.Deployment
{
	internal class InformationSchemaTableConstraints
	{
		public string ConstraintSchema { get; set; }
		public string TableSchema { get; set; }
		public string TableName { get; set; }
		public string ConstraintType { get; set; }
		public string ConstraintName { get; set; }

	}
}
