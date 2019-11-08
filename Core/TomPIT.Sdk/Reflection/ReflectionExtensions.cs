namespace TomPIT.Reflection
{
	public static class ReflectionExtensions
	{
		public static void SetPropertyValue(object instance, string propertyName, object value)
		{
			var property = instance.GetType().GetProperty(propertyName);

			if (property == null)
				return;

			if (!property.CanWrite)
			{
				if (property.DeclaringType == null)
					return;

				property = property.DeclaringType.GetProperty(propertyName);
			}

			if (property == null || property.SetMethod == null)
				return;

			property.SetMethod.Invoke(instance, new object[] { value });
		}
	}
}
