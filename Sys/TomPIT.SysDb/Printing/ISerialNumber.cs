namespace TomPIT.SysDb.Printing
{
	public interface ISerialNumber
	{
		string Category { get; }
		long SerialNumber { get; }

		void Increment();
	}
}
