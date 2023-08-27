using System;

namespace TomPIT.Annotations;

[AttributeUsage(AttributeTargets.Class)]
public sealed class MiddlewareTargetAttribute : Attribute
{
	public string? Component { get; set; }
	public string? Method { get; set; }
}
