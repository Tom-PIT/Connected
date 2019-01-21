using System;
using System.Collections.Generic;

namespace TomPIT.ComponentModel
{
	public interface IExternalResourceElement : IElement
	{
		List<Guid> QueryResources();
	}
}
