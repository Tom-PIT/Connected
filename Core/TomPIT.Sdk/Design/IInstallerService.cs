using System;
using System.Collections.Generic;
using TomPIT.Development;

namespace TomPIT.Design
{
	public interface IInstallerService
	{
		void Pull(IRepositoryBinding binding);
		void Pull(IRepositoryBinding binding, List<Guid> components);
		void Push(IRepositoryBinding binding);
		void Push(IRepositoryBinding binding, List<Guid> components);
	}
}
