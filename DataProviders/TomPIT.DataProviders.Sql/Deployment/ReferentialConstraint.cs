using TomPIT.Deployment.Database;

namespace TomPIT.DataProviders.Sql.Deployment
{
	internal class ReferentialConstraint : IReferentialConstraint
	{
		public string Name { get; set; }
		public string ReferenceSchema { get; set; }
		public string ReferenceName { get; set; }
		public string MatchOption { get; set; }
		public string UpdateRule { get; set; }
		public string DeleteRule { get; set; }
	}
}
