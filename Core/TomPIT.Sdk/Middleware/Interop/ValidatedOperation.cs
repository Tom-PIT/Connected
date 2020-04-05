using System.ComponentModel.DataAnnotations;

namespace TomPIT.Middleware.Interop
{
	public abstract class ValidatedOperation<TReturnValue> : Operation<TReturnValue>
	{
		public bool ValidateResult { get; set; } = true;
		protected TReturnValue Validate(TReturnValue item, string errorMessage)
		{
			if (item == null && ValidateResult)
				throw new ValidationException(errorMessage, new Exceptions.NotFoundException(errorMessage));

			return item;
		}
	}
}
