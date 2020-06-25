namespace TomPIT.Data
{
	public enum CommandTextType
	{
		Procedure = 1,
		Text = 2
	}

	public interface ICommandTextParser
	{
		ICommandTextDescriptor Parse(string text);
	}
}
