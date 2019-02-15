using System;

namespace TomPIT.Deployment
{
	public interface IPackageString
	{
		Guid Element { get; }
		string Value { get; }
		int Lcid { get; }
		string Property { get; }
	}
}
