namespace TomPIT.Deployment.Database
{
	public interface IReferentialConstraint
	{
		string Name { get; }
		string ReferenceSchema { get; }
		string ReferenceName { get; }
		string MatchOption { get; }
		string UpdateRule { get; }
		string DeleteRule { get; }
	}
}
