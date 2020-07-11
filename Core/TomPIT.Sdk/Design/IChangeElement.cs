using System;
using System.Collections.Generic;

namespace TomPIT.Design
{
	public interface IChangeElement
	{
		Guid Id { get; }
		string Name { get; }
		List<IChangeElement> Elements { get; }
		IChangeBlob Blob { get; }
	}
}
