﻿using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class HttpPatchAttribute : HttpMethodAttribute
	{
	}
}
