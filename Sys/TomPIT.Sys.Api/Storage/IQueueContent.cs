namespace TomPIT.Api.Storage
{
	public interface IQueueContent
	{
		string Serialize();
		void Deserialize(string content);
	}
}
