using System;

namespace TomPIT.ComponentModel
{
	public interface IConfiguration
	{
		Guid Component { get; set; }

		void ComponentCreated(IComponent scope);
	}
}
