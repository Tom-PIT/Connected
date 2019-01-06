using System;
using System.Reflection;
using TomPIT.Annotations;

namespace TomPIT.Design
{
	public static class MethodSignature
	{
		public static ITextSignature CreateClick(string name)
		{
			var r = new Signature
			{
				Name = "function",
				ReturnValue = null
			};

			var e = new SignatureParameter
			{
				Name = "e",
				Type = null
			};

			r.Parameters.Add(e);

			return r;
		}

		public static ITextSignature CreateModel(Type modelType)
		{
			var r = new Signature
			{
				Name = "@model",
				ReturnValue = typeof(void),
				Language = "Razor"
			};

			var sender = new SignatureParameter
			{
				Name = "sender",
				Type = modelType
			};

			r.Parameters.Add(sender);

			return r;
		}

		public static ITextSignature CreateModel(string modelType)
		{
			return CreateModel(Types.GetType(modelType));
		}

		public static ITextSignature Create(object instance)
		{
			if (instance == null)
				return null;

			var att = instance.GetType().FindAttribute<EventArgumentsAttribute>();

			var type = att == null ? null : att.Type ?? Types.GetType(att.TypeName);

			return Create(instance.ToString(), type);
		}

		public static ITextSignature Create(PropertyInfo property)
		{
			var att = property.FindAttribute<EventArgumentsAttribute>();

			var type = att == null ? null : att.Type ?? Types.GetType(att.TypeName);

			return Create(property.Name, type);
		}

		private static ITextSignature Create(string scriptName, Type argumentsType)
		{
			var r = new Signature
			{
				Name = scriptName,
				ReturnValue = typeof(object),
				Language = "C#"
			};

			var sender = new SignatureParameter
			{
				Name = "sender",
				Type = typeof(object)
			};

			r.Parameters.Add(sender);

			if (argumentsType == null)
				return r;

			var e = new SignatureParameter
			{
				Name = "e",
				Type = argumentsType
			};

			r.Parameters.Add(e);

			return r;
		}
	}
}
