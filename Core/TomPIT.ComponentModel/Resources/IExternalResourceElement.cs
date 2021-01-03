using System;
using System.Collections.Generic;

namespace TomPIT.ComponentModel.Resources
{
	public interface IExternalResourceElement : IElement
	{
		List<Guid> QueryResources();
		void Clean(Guid resource);
		void Reset(Guid existingValue, Guid newValue);
	}
}
