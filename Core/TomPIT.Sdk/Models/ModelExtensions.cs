using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace TomPIT.Models
{
	public static class ModelExtensions
	{
		public static void Validate(this IModel model, Controller controller, JObject requestBody)
		{
			var ctx = new ValidationContext(model);

			ValidateProperties(model, ctx, requestBody);

			var results = model.Validate();

			if (results != null)
			{
				foreach (ValidationResult i in results)
				{
					if (i != null)
						controller.ModelState.AddModelError(string.Empty, i.ErrorMessage);
				}
			}
		}

		private static void ValidateProperties(this IModel model, ValidationContext context, JObject requestBody)
		{
			var props = model.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty);

			foreach (var i in props)
			{
				var attributes = i.GetCustomAttributes(false);

				foreach (var j in attributes)
				{
					var value = Types.Convert(requestBody.Optional<object>(i.Name, false), i.PropertyType);

					if (j is ValidationAttribute)
					{
						var va = j as ValidationAttribute;

						va.Validate(value, i.Name);
					}
				}
			}
		}
	}
}
