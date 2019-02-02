using Newtonsoft.Json;
using System.ComponentModel;

namespace TomPIT.ComponentModel
{
	public abstract class ContextElement : Element, IContextElement
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
					_validation.Validate += OnValidate;
				}

				return _validation;
			}
		}

		protected virtual void OnValidate(object sender, ElementValidationArgs e)
		{
		}
	}
}
