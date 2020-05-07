namespace TomPIT.VersionControl
{
	public interface IVersionControlFormatter
	{
		string SerializeHead();
		void DeserializeHead(string content);

		string SerializeBody();
		void DeserializeBody(string content);
	}
}
