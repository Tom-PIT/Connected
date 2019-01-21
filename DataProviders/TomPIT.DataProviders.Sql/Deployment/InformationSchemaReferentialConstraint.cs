namespace TomPIT.DataProviders.Sql.Deployment
{
	internal class InformationSchemaReferentialConstraint
	{
		public string ConstraintSchema { get; set; }
		public string ConstraintName { get; set; }
		public string UniqueConstraintSchema { get; set; }
		public string UniqueConstraintName { get; set; }
		public string MatchOption { get; set; }
		public string UpdateRule { get; set; }
		public string DeleteRule { get; set; }
	}
}
