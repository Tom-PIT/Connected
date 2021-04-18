using System;

namespace TomPIT.Design
{
	public class DeployArgs : EventArgs
	{
		public bool ResetMicroService { get; set; }
	}
}
