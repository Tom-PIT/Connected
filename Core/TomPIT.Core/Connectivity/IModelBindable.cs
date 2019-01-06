namespace TomPIT.Connectivity
{
	public interface IModelBindable
	{
		string Serialize();
		void Deserialize(string value);
	}
}
