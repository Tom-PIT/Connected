namespace TomPIT.Configuration
{
	public interface ISetting
	{
		string Name { get; }
		string Value { get; }
		string Type { get; }
		string PrimaryKey { get; }
		string NameSpace { get; }
	}
}
