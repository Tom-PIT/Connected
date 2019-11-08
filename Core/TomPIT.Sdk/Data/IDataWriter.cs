namespace TomPIT.Data
{
	public interface IDataWriter : IDataCommand
	{
		void Execute();
		T Execute<T>();

		IDataParameter SetReturnValueParameter(string name);
	}
}
