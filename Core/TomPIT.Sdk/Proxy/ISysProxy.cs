namespace TomPIT.Proxy
{
	public interface ISysProxy
	{
		IAlienController Alien { get; }
		IAnalyticsController Analytics { get; }
		IAuditController Audit { get; }
		IAuthenticationController Authentication { get; }
		IClientController Clients { get; }
		IComponentController Components { get; }
		ICryptographyController Cryptography { get; }
		IDataCacheController DataCache { get; }
		IEventController Events { get; }
		IFolderController Folders { get; }
		IInstanceEndpointController InstanceEndpoints { get; }
		IIoTController IoT { get; }
		ILanguageController Languages { get; }
		ILockingController Locking { get; }
		ILoggingController Logging { get; }
		IMailController Mail { get; }
		IMetricController Metrics { get; }
		IMicroServiceController MicroServices { get; }
		IPrintingController Printing { get; }
		IQueueController Queue { get; }
	}
}
