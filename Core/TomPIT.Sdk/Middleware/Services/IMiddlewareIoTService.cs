﻿using System.Collections.Generic;
using TomPIT.IoT;
using CAP = TomPIT.Annotations.Design.CodeAnalysisProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareIoTService
	{
		string Server { get; }
		List<IIoTFieldState> QueryState([CAP(CAP.IoTHubProvider)]string hub);
		IIoTFieldState SelectState([CAP(CAP.IoTHubProvider)]string hub, [CAP(CAP.IoTHubFieldProvider)]string field);
		T SelectValue<T>([CAP(CAP.IoTHubProvider)]string hub, [CAP(CAP.IoTHubFieldProvider)]string field);
	}
}