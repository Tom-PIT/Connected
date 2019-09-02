using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Services
{
	public interface IProcessHandler
	{
		void Initialize(IDataModelContext context);

		IDataModelContext Context { get; }
	}
}
