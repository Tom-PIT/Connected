using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TomPIT.Middleware.Interop
{
	public abstract class ValidatedOperation<TReturnValue> : Operation<TReturnValue>
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
