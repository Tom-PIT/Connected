using System;
using TomPIT.ComponentModel.Apis;

namespace TomPIT.Middleware.Interop
{
	public interface IApiExecutionScope
	{
		IApiConfiguration Api { get; }
	}
}
