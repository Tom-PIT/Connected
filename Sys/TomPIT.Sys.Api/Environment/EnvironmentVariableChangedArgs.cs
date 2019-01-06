using System;

namespace TomPIT.Sys.Api.Environment
{
	public delegate void EnvironmentVariableChangedHandler(object sender, EnvironmentVariableChangedArgs e);

	public class EnvironmentVariableChangedArgs : EventArgs
	{
		public EnvironmentVariableChangedArgs(string variable, string newValue)
		{
			Variable = variable;
			NewValue = newValue;
		}

		public string Variable { get; }
		public string NewValue { get; }
	}
}
