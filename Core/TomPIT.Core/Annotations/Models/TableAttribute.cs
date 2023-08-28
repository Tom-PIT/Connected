using System;

namespace TomPIT.Annotations.Models;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = false)]
public class TableAttribute : SchemaAttribute
{
}
