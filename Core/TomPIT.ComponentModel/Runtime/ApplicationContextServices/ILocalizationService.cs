using System;

namespace TomPIT.Runtime.ApplicationContextServices
{
	public interface ILocalizationService
	{
		Guid Language { get; }
	}
}
