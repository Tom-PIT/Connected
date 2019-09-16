using System;
using System.Collections.Generic;
using TomPIT.Deployment;

namespace TomPIT.Management.Designers
{
	public interface ISignupDesigner
	{
		Guid PublisherKey { get; }
		List<ICountry> Countries { get; }
	}
}
