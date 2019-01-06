using System;

namespace TomPIT.ComponentModel
{
	public interface IApiTransaction
	{
		Guid Id { get; }
		string Name { get; }

		void Commit();
		void Rollback();

		void Notify(IApiOperation operation);
	}
}
