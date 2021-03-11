using System;

namespace TomPIT.Annotations
{
	[Flags]
	public enum HttpVerbs
	{
		None = 0,
		Get = 1,
		Post = 2,
		Delete = 4,
		Head = 8,
		Options = 16,
		Patch = 32,
		Put = 64,
		Trace = 128
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple =true)]
	public abstract class HttpMethodAttribute : Attribute
	{
	}
}
