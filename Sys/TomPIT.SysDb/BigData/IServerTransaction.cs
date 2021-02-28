using TomPIT.BigData;

namespace TomPIT.SysDb.BigData
{
	public interface IServerTransaction : ITransaction
	{
		void DecrementBlock();
	}
}
