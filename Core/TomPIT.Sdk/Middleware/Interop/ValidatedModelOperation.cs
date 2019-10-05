using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TomPIT.Middleware.Interop
{
	public abstract class ValidatedModelOperation<TModel, TReturnValue> : ModelOperation<TModel, TReturnValue> where TModel : class
	{
		[JsonIgnore]
		public bool ValidateResult { get; set; }
		protected TReturnValue Validate(TReturnValue item, string errorMessage)
		{
			if (item == default && ValidateResult)
				throw new ValidationException(errorMessage);

			return item;
		}
	}
}
