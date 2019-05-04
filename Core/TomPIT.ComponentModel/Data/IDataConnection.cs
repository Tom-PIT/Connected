using System;
using System.Data;

namespace TomPIT.Data
{
	public interface IDataConnection : IDisposable
	{
		void Begin();
		void Begin(IsolationLevel isolationLevel);
		void Commit();
		void Rollback();

		void Open();
		void Close();
	}
}
