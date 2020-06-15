namespace TomPIT.Data
{
	public enum CommandTextType
	{
		Procedure = 1,
		Text = 2
	}

	public enum CommandStatementType
	{
		NotSet = 0,
		Select = 1,
		Insert = 2,
		Update = 3,
		Delete = 4
	}

	public interface ICommandTextParser
	{
		ICommandTextDescriptor Parse(string text);
	}
}
