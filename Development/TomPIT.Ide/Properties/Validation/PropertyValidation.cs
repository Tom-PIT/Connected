using TomPIT.Design.Ide.Validation;

namespace TomPIT.Ide.Properties.Validation
{
	public class PropertyValidation : IPropertyValidation
	{
		private MaxLengthValidation _maxLength = null;
		private RequiredValidation _requiredValidation = null;
		private MinValueValidation _minValue = null;
		private MaxValueValidation _maxValue = null;

		public IMaxValueValidation MaxValue
		{
			get
			{
				if (_maxValue == null)
					_maxValue = new MaxValueValidation();

				return _maxValue;
			}
		}

		public IMinValueValidation MinValue
		{
			get
			{
				if (_minValue == null)
					_minValue = new MinValueValidation();

				return _minValue;
			}
		}

		public IMaxLengthValidation MaxLength
		{
			get
			{
				if (_maxLength == null)
					_maxLength = new MaxLengthValidation();

				return _maxLength;
			}
		}

		public IRequiredValidation Required
		{
			get
			{
				if (_requiredValidation == null)
					_requiredValidation = new RequiredValidation();

				return _requiredValidation;
			}
		}
	}
}
