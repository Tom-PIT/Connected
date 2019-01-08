using TomPIT.ComponentModel;
using TomPIT.Exceptions;
using TomPIT.Services;

namespace TomPIT.Models
{
	public class PartialModel : AjaxModel
	{
		protected override void OnDatabinding()
		{
			if (string.IsNullOrWhiteSpace(Body.Optional("__name", string.Empty)))
			{
				throw ExecutionException.ParameterExpected(this, new ExecutionContextState
				{
					Event = ExecutionEvents.DataRead,
				}, "__name");
			}

			QualifierName = Body.Optional("__name", string.Empty);

			if (string.IsNullOrWhiteSpace(Body.Optional("__component", string.Empty)))
			{
				throw ExecutionException.ParameterExpected(this, new ExecutionContextState
				{
					Event = ExecutionEvents.DataRead,
				}, "__component");
			}

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

			Body.Remove("__name");
			Body.Remove("__component");

			SetIdentity(this.CreateIdentity(c.Category, c.Token.ToString(), s.Token.ToString()));
		}
	}
}
