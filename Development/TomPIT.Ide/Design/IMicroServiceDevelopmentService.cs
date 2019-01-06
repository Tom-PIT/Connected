using System;

namespace TomPIT.Design
{
	public interface IMicroServiceDevelopmentService
	{
		void UpdateString(Guid microService, Guid language, Guid element, string property, string value);
		void DeleteString(Guid microService, Guid element, string property);
	}
}
