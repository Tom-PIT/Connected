using System.Collections.Generic;
using TomPIT.Annotations;
using TomPIT.IoT;

namespace TomPIT.Services.Context
{
	public interface IContextIoTService
	{
		string Server { get; }
		List<IIoTFieldState> QueryState([CodeAnalysisProvider(ExecutionContext.IoTHubProvider)]string hub);
		IIoTFieldState SelectState([CodeAnalysisProvider(ExecutionContext.IoTHubProvider)]string hub, [CodeAnalysisProvider(ExecutionContext.IoTHubFieldProvider)]string field);
		T SelectValue<T>([CodeAnalysisProvider(ExecutionContext.IoTHubProvider)]string hub, [CodeAnalysisProvider(ExecutionContext.IoTHubFieldProvider)]string field);
	}
}
