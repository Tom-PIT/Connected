using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace TomPIT.Design.Validation
{
	internal class ValidationService : IValidationService
	{
		public List<IValidationMessage> Validate(object component)
		{
			var r = new List<IValidationMessage>();

			if (component == null)
				return r;

			var props = component.GetType().GetProperties();

			foreach (var i in props)
				ValidateProperty(component, i, r);

			return r;
		}

		private void ValidateProperty(object component, PropertyInfo property, List<IValidationMessage> messages)
		{
			var attributes = property.GetCustomAttributes(true);

			if (attributes == null || attributes.Length == 0)
				return;

			var value = property.GetValue(component);

			foreach (var i in attributes)
			{
				if (i is ValidationAttribute val)
				{
					if (!val.IsValid(value))
					{
						messages.Add(new ValidationMessage
						{
							Message = val.FormatErrorMessage(property.Name),
							Type = ValidationMessageType.Error
						});
					}
				}
			}
		}
	}
}
