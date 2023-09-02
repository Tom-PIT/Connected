namespace TomPIT.Data
{
	public interface IDataWriter : IDataCommand
	{
		int Execute();
		T Execute<T>();

		IDataParameter SetReturnValueParameter(string name);
	}
}
