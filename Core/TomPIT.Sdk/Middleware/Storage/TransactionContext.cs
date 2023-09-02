using System;
using System.Collections.Concurrent;
using System.Linq;
using TomPIT.Data.Storage;

namespace TomPIT.Middleware.Storage;
internal class TransactionContext : ITransactionContext
{
	private MiddlewareTransactionState _state = MiddlewareTransactionState.Active;

	public event EventHandler? StateChanged;

	public TransactionContext()
	{
		Operations = new();
	}

	public MiddlewareTransactionState State
	{
		get => _state; private set
		{
			if (_state != value)
			{
				_state = value;
				TriggerStateChanged();
			}
		}
	}

	private ConcurrentStack<IMiddlewareOperation> Operations { get; }

	public bool IsDirty { get; set; }

	public void Register(IMiddlewareOperation operation)
	{
		if (operation is null || Operations.Contains(operation))
			return;

		Operations.Push(operation);
	}

	public void Commit(IMiddlewareOperation operation)
	{
		if (!Operations.Any() || Operations.Last() != operation)
			return;

		State = MiddlewareTransactionState.Committing;

		while (!Operations.IsEmpty)
		{
			if (Operations.TryPop(out IMiddlewareOperation? op))
			{
				if (op is not null && op is MiddlewareOperation middleware)
					middleware.CommitOperation();
			}
		}

		State = MiddlewareTransactionState.Completed;
	}

	public void Rollback()
	{
		State = MiddlewareTransactionState.Reverting;

		while (!Operations.IsEmpty)
		{
			if (Operations.TryPop(out IMiddlewareOperation? op))
			{
				if (op is not null && op is MiddlewareOperation middleware)
					middleware.RollbackOperation();
			}
		}

		State = MiddlewareTransactionState.Completed;
	}

	private void TriggerStateChanged()
	{
		try
		{
			StateChanged?.Invoke(this, EventArgs.Empty);
		}
		catch { }
	}
}
