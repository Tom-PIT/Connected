namespace TomPIT.Net
{
	public interface IModelBindable
	{
		string Serialize();
		void Deserialize(string value);
	}
}
