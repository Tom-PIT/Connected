using System;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Apis
{
	public interface IApiTransaction
	{
		Guid Id { get; }
		string Name { get; }

		void Commit();
		void Rollback();

		void Notify(IOperationBase operation);
	}
}
