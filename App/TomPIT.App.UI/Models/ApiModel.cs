using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.Models
{
	public class ApiModel : AjaxModel
	{
		protected override void OnDatabinding()
		{
			if (string.IsNullOrWhiteSpace(Body.Optional("__api", string.Empty)))
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrDataParameterExpected, "__api"))
				{
					Event = ExecutionEvents.DataRead,
				}.WithMetrics(this);
			}

			QualifierName = Body.Optional("__api", string.Empty);

			var component = Body.Optional("__component", string.Empty);
			var tokens = component.Split('.');

			if (tokens.Length != 2)
			{
				throw new RuntimeException(string.Format("{0} ({1}). {2}: {3}.", SR.ErrInvalidQualifier, component, SR.ErrInvalidQualifierExpected, "microService.component"))
				{
					Event = ExecutionEvents.DataRead
				}.WithMetrics(this);
			}

			var s = Instance.GetService<IMicroServiceService>().Select(tokens[0].AsGuid());

			if (s == null)
				throw new RuntimeException(SR.ErrMicroServiceNotFound);

			var c = Instance.GetService<IComponentService>().SelectComponent(tokens[1].AsGuid());

			if (c == null)
				throw new RuntimeException(SR.ErrComponentNotFound);

			Body.Remove("__api");
			Body.Remove("__component");

			MicroService = s;
		}
	}
}
