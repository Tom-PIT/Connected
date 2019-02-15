using System;
using System.Collections.Generic;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment
{
	public interface ISignupDesigner
	{
		Guid PublisherKey { get; }
		List<ICountry> Countries { get; }
	}
}
