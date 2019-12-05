using System.Collections.Generic;

namespace TomPIT.ComponentModel.Reports
{
	public interface IReportConfiguration : IConfiguration, IText
	{
		List<string> Apis { get; }
	}
}
