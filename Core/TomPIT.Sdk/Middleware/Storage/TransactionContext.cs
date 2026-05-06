using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TomPIT.Data.Storage;

namespace TomPIT.Middleware.Storage;

internal class TransactionContext : ITransactionContext
{
	private MiddlewareTransactionState _state = MiddlewareTransactionState.Active;
	private readonly object _lock = new();

	public event EventHandler? StateChanged;

	public TransactionContext(MiddlewareContext owner)
	{
		Owner = owner;
		Operations = new();
		OperationSet = new();
	}

	private MiddlewareContext Owner { get; }

	public MiddlewareTransactionState State
	{
		get { lock (_lock) return _state; }
		private set
		{
			bool changed;
			lock (_lock)
			{
				changed = _state != value;
				if (changed) _state = value;
			}
			if (changed)
				TriggerStateChanged();
		}
	}

	private ConcurrentStack<IMiddlewareOperation> Operations { get; }
	private HashSet<IMiddlewareOperation> OperationSet { get; }
	private IMiddlewareOperation? _rootOperation;

	private volatile bool _isDirty;
	public bool IsDirty { get => _isDirty; set => _isDirty = value; }

	public void Register(IMiddlewareOperation operation)
	{
		if (operation is null)
			return;

		lock (_lock)
		{
			if (!OperationSet.Add(operation))
				return;

			if (Operations.IsEmpty)
				_rootOperation = operation;

			Operations.Push(operation);
		}
	}

	public void Commit(IMiddlewareOperation operation)
	{
		List<MiddlewareOperation>? toCommit;

		lock (_lock)
		{
			if (Operations.IsEmpty || _rootOperation != operation)
				return;

			_state = MiddlewareTransactionState.Committing;

			toCommit = new List<MiddlewareOperation>(Operations.Count);
			while (Operations.TryPop(out var op))
			{
				if (op is MiddlewareOperation middleware)
					toCommit.Add(middleware);
			}

			OperationSet.Clear();
			_rootOperation = null;
		}

		TriggerStateChanged();

		foreach (var middleware in toCommit)
			middleware.CommitOperation();

		var commitTask = Owner.GetService<IMultiContextOrchestrator>()?.Commit();
		if (commitTask is not null)
			AsyncUtils.RunSync(() => commitTask);

		State = MiddlewareTransactionState.Completed;
	}

	public void Rollback()
	{
		List<MiddlewareOperation>? toRollback;

		lock (_lock)
		{
			_state = MiddlewareTransactionState.Reverting;

			toRollback = new List<MiddlewareOperation>(Operations.Count);
			while (Operations.TryPop(out var op))
			{
				if (op is MiddlewareOperation middleware)
					toRollback.Add(middleware);
			}

			OperationSet.Clear();
			_rootOperation = null;
		}

		TriggerStateChanged();

		foreach (var middleware in toRollback)
			middleware.RollbackOperation();

		var rollbackTask = Owner.GetService<IMultiContextOrchestrator>()?.Rollback();
		if (rollbackTask is not null)
			AsyncUtils.RunSync(() => rollbackTask);

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
