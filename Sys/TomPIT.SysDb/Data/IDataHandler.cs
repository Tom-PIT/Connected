namespace TomPIT.SysDb.Data
{
	public interface IDataHandler
	{
		IAuditHandler Audit { get; }
		IUserDataHandler UserData { get; }
	}
}
