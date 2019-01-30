using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.Models
{
	public class PartialModel : AjaxModel
	{
		protected override void OnDatabinding()
		{
			if (string.IsNullOrWhiteSpace(Body.Optional("__name", string.Empty)))
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrDataParameterExpected, "__name"))
				{
					Event = ExecutionEvents.DataRead,
				}.WithMetrics(this);
			}

			QualifierName = Body.Optional("__name", string.Empty);

			if (string.IsNullOrWhiteSpace(Body.Optional("__component", string.Empty)))
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrDataParameterExpected, "__component"))
				{
					Event = ExecutionEvents.DataRead,
				}.WithMetrics(this);
			}

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

			Body.Remove("__name");
			Body.Remove("__component");

			SetIdentity(this.CreateIdentity(c.Category, c.Token.ToString(), s.Token.ToString()));
		}
	}
}
