using System.Collections.Generic;

namespace TomPIT.Design.Validation
{
	public interface IValidationService
	{
		List<IValidationMessage> Validate(object component);
	}
}
