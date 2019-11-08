using System.ComponentModel;
using Newtonsoft.Json;
using TomPIT.Design.Validation;

namespace TomPIT.ComponentModel
{
	public abstract class ConfigurationElement : Element, IConfigurationElement
	{
		private ElementValidation _validation = null;

		[Browsable(false)]
		[JsonIgnore]
		public IElementValidation Validation
		{
			get
			{
				if (_validation == null)
				{
					_validation = new ElementValidation();
					_validation.Validating += OnValidate;
				}

				return _validation;
			}
		}

		private void OnValidate(object sender, ElementValidationArgs e)
		{
			Validate(e);
			OnValidating(sender, e);
		}

		protected virtual void OnValidating(object sender, ElementValidationArgs e)
		{
		}

		private void Validate(ElementValidationArgs e)
		{
			var messages = e.Context.Tenant.GetService<IValidationService>().Validate(this);

			if (messages != null && messages.Count > 0)
				e.Messages.AddRange(messages);
		}
	}
}
