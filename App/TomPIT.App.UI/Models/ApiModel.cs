using TomPIT.ComponentModel;
using TomPIT.Exceptions;
using TomPIT.Services;

namespace TomPIT.Models
{
	public class ApiModel : AjaxModel
	{
		protected override void OnDatabinding()
		{
			if (string.IsNullOrWhiteSpace(Body.Optional("__api", string.Empty)))
			{
				throw ExecutionException.ParameterExpected(this, new ExecutionContextState
				{
					Event = ExecutionEvents.DataRead,
				}, "__api");
			}

			QualifierName = Body.Optional("__api", string.Empty);

			var component = Body.Optional("__component", string.Empty);
			var tokens = component.Split('.');

			if (tokens.Length != 2)
			{
				throw ExecutionException.InvalidQualifier(this, new ExecutionContextState
				{
					Event = ExecutionEvents.DataRead,
				}, component, "microService.component");
			}

			var s = Instance.GetService<IMicroServiceService>().Select(tokens[0].AsGuid());

			if (s == null)
				throw new RuntimeException(SR.ErrMicroServiceNotFound);

			var c = Instance.GetService<IComponentService>().SelectComponent(tokens[1].AsGuid());

			if (c == null)
				throw new RuntimeException(SR.ErrComponentNotFound);

			Body.Remove("__api");
			Body.Remove("__component");

			SetIdentity(this.CreateIdentity(c.Category, c.Token.ToString(), s.Token.ToString()));
		}
	}
}
