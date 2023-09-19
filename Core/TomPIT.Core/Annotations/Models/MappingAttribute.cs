using System;

namespace TomPIT.Annotations.Models;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
public abstract class MappingAttribute : Attribute
{
}
