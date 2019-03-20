namespace TomPIT.SysDb.Messaging
{
	public interface IQueueContent
	{
		string Serialize();
		void Deserialize(string content);
	}
}
