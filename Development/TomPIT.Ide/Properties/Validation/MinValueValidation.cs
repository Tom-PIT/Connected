﻿using TomPIT.Design.Ide.Validation;

namespace TomPIT.Ide.Properties.Validation
{
	public class MinValueValidation : ValidationSettings, IMinValueValidation
	{
		public double Value
		{
			get; set;
		}
	}
}
