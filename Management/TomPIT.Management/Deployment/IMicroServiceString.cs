using System;

namespace TomPIT.Deployment
{
	public interface IMicroServiceString
	{
		Guid Element { get; }
		string Value { get; }
		int Lcid { get; }
		string Property { get; }
	}
}
