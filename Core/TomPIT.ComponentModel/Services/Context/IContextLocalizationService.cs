using System;

namespace TomPIT.Services.Context
{
	public interface IContextLocalizationService
	{
		Guid Language { get; }
	}
}
