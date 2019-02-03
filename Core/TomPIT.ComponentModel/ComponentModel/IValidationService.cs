using System.Collections.Generic;

namespace TomPIT.ComponentModel
{
	public interface IValidationService
	{
		List<IValidationMessage> Validate(object component);
	}
}
