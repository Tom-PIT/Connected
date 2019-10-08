using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Development;
using TomPIT.Exceptions;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Data
{
	internal class VersionControl
	{
		public Guid InsertCommit(Guid microService, Guid user, string comment, List<IComponent> components)
		{
			var ms = DataModel.MicroServices.Select(microService);

			if (ms == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			ms.DemandDevelopmentStage();

			var u = DataModel.Users.Select(user);

			if (u == null)
				throw new SysException(SR.ErrUserNotFound);

			var id = Guid.NewGuid();

			Shell.GetService<IDatabaseService>().Proxy.Development.VersionControl.InsertCommit(id, ms, u, DateTime.UtcNow, comment, components);

			return id;
		}

		public ILockInfo SelectLockInfo(Guid component, Guid user)
		{
			var c = DataModel.Components.Select(component);

			if (c == null)
				throw new SysException(SR.ErrComponentNotFound);

			var r = new LockInfo();

			if (c.LockStatus == LockStatus.Commit)
				r.Result = LockInfoResult.ShouldLock;
			else
			{
				if (user == c.LockUser)
					r.Result = LockInfoResult.NoAction;
				else
				{
					r.Result = LockInfoResult.Locked;
					r.Owner = c.LockUser;
				}
			}

			return r;
		}

		public List<ICommit> QueryCommits(Guid microService)
		{
			var ms = DataModel.MicroServices.Select(microService);

			if (ms == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			return Shell.GetService<IDatabaseService>().Proxy.Development.VersionControl.QueryCommits(ms);
		}

		public void DeleteHistory(Guid component)
		{
			var c = DataModel.Components.Select(component);

			if (c == null)
				throw new RuntimeException(SR.ErrComponentNotFound);

			c.DemandDevelopmentStage();

			var history = QueryHistory(component);

			foreach (var i in history)
			{
				try
				{
					Shell.GetService<IDatabaseService>().Proxy.Development.VersionControl.DeleteHistory(i);
					DataModel.Blobs.Delete(i.Blob);
				}
				catch { }
			}
		}

		public List<IComponentHistory> QueryHistory(Guid component)
		{
			var c = DataModel.Components.Select(component);

			if (c == null)
				throw new RuntimeException(SR.ErrComponentNotFound);

			return Shell.GetService<IDatabaseService>().Proxy.Development.VersionControl.QueryHistory(c);
		}

		public List<IComponentHistory> QueryCommitDetails(Guid commit)
		{
			var c = SelectCommit(commit);

			if (c == null)
				throw new SysException(SR.ErrCommitNotFound);

			return Shell.GetService<IDatabaseService>().Proxy.Development.VersionControl.QueryCommitDetails(c);
		}

		public ICommit SelectCommit(Guid token)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Development.VersionControl.SelectCommit(token);
		}

		public List<IComponent> QueryCommitComponents(Guid commit)
		{
			var c = SelectCommit(commit);

			if (c == null)
				throw new SysException(SR.ErrCommitNotFound);

			return Shell.GetService<IDatabaseService>().Proxy.Development.VersionControl.QueryCommitComponents(c);
		}

		public List<ICommit> QueryCommitsForComponent(Guid microService, Guid component)
		{
			var ms = DataModel.MicroServices.Select(microService);

			if (ms == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			var c = DataModel.Components.Select(component);

			if (c == null)
				throw new SysException(SR.ErrComponentNotFound);

			return Shell.GetService<IDatabaseService>().Proxy.Development.VersionControl.QueryCommits(ms, c);
		}

		public List<ICommit> QueryCommits(Guid microService, Guid user)
		{
			var ms = DataModel.MicroServices.Select(microService);

			if (ms == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			var u = DataModel.Users.Select(user);

			if (u == null)
				throw new SysException(SR.ErrUserNotFound);

			return Shell.GetService<IDatabaseService>().Proxy.Development.VersionControl.QueryCommits(ms, u);
		}

		public void Lock(Guid component, Guid user, LockVerb verb, Guid blob)
		{
			var c = DataModel.Components.Select(component);

			if (c == null)
				throw new SysException(SR.ErrComponentNotFound);

			c.DemandDevelopmentStage();

			var u = DataModel.Users.Select(user);

			if (u == null)
				throw new SysException(SR.ErrUserNotFound);

			if (c.LockStatus != LockStatus.Commit)
				throw new SysException(SR.ErrComponentLocked);

			Shell.GetService<IDatabaseService>().Proxy.Development.Components.Update(c, u, LockStatus.Lock, verb, DateTime.UtcNow);
			Shell.GetService<IDatabaseService>().Proxy.Development.VersionControl.InsertComponentHistory(u, c, c.Name, verb, DateTime.UtcNow, blob);

			DataModel.Components.NotifyChanged(c);
		}

		public void Undo(Guid component)
		{
			var c = DataModel.Components.Select(component);

			if (c == null)
				throw new SysException(SR.ErrComponentNotFound);

			c.DemandDevelopmentStage();

			Shell.GetService<IDatabaseService>().Proxy.Development.Components.Update(c, null, LockStatus.Commit, LockVerb.None, DateTime.MinValue);
			Shell.GetService<IDatabaseService>().Proxy.Development.VersionControl.UndoComponentHistory(c);

			DataModel.Components.NotifyChanged(c);
		}
	}
}
