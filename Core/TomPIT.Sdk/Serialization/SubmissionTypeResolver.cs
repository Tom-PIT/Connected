using System;
using System.Collections.Generic;
using System.Reflection;
using TomPIT.Reflection;

namespace TomPIT.Serialization
{
	public enum ResolverStrategy
	{
		Complete = 1,
		SkipRoot = 2
	}

	internal sealed class SubmissionTypeResolver
	{
		public List<Type> SubmissionTypes { get; private set; }
		private List<Type> References { get; set; }

		public SubmissionTypeResolver()
		{
			SubmissionTypes = new();
			References = new();
		}

		public bool IsCompatible(object instance)
		{
			if (instance is null)
				return true;

			if (!IsSubmissionType(instance.GetType()))
				return true;

			foreach (var type in SubmissionTypes)
			{
				if (!IsSameSubmission(instance, type))
					return false;
			}

			return true;
		}

		public void Resolve(Type type, ResolverStrategy strategy)
		{
			SubmissionTypes = new();
			References = new();

			switch (strategy)
			{
				case ResolverStrategy.Complete:
					ResolveType(type);
					break;
				case ResolverStrategy.SkipRoot:
					break;
				default:
					break;
			}
		}

		private void ResolveType(Type type)
		{
			if (References.Contains(type))
				return;

			References.Add(type);

			if (type.IsPrimitive || !string.IsNullOrWhiteSpace(type.Namespace))
				return;

			if (IsSubmissionType(type))
			{
				if (!SubmissionTypes.Contains(type))
					SubmissionTypes.Add(type);

				ResolveChildren(type);

				return;
			}
			else if (type.IsCollection())
			{
				var elementType = type.GetElementType();

				ResolveType(elementType);
			}
			else
			{
				if (type.IsGenericType)
				{
					var types = type.GetGenericArguments();

					for (var i = 0; i < types.Length; i++)
						ResolveType(types[i]);
				}
				else
					ResolveChildren(type);
			}
		}

		private void ResolveChildren(Type type)
		{
			var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (var member in members)
			{
				if (member is FieldInfo field)
					ResolveType(field.FieldType);
				else if (member is PropertyInfo property)
					ResolveType(property.PropertyType);
			}
		}

		private static bool IsSubmissionType(Type type)
		{
			return string.IsNullOrEmpty(type.Namespace) && string.IsNullOrEmpty(type.Assembly.Location);
		}

		private static bool IsSameSubmission(object instance, Type type)
		{
			return IsSameSubmission(instance.GetType(), type);
		}

		private static bool IsSameSubmission(Type instanceType, Type type)
		{
			return string.IsNullOrWhiteSpace(type.Namespace) && string.IsNullOrWhiteSpace(instanceType.Namespace)
				&& string.Equals(type.Assembly.FullName, instanceType.Assembly.FullName, StringComparison.Ordinal);
		}
	}
}
