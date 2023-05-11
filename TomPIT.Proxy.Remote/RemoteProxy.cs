namespace TomPIT.Proxy.Remote
{
	public class RemoteProxy : ISysProxy
	{
		public RemoteProxy()
		{
			Alien = new AlienController();
			Analytics = new AnalyticsController();
			Audit = new AuditController();
			Authentication = new AuthenticationController();
			Clients = new ClientController();
			Components = new ComponentController();
			Cryptography = new CryptographyController();
			DataCache = new DataCacheController();
			Events = new EventController();
			Folders = new FolderController();
			InstanceEndpoints = new InstanceEndpointController();
			IoT = new IoTController();
			Languages = new LanguageController();
			Locking = new LockingController();
			Logging = new LoggingController();
			Mail = new MailController();
			Metrics = new MetricController();
			MicroServices = new MicroServiceController();
			Printing = new PrintingController();
			Queue = new QueueController();
		}

		public IAlienController Alien { get; }
		public IAnalyticsController Analytics { get; }
		public IAuditController Audit { get; }
		public IAuthenticationController Authentication { get; }
		public IClientController Clients { get; }
		public IComponentController Components { get; }
		public ICryptographyController Cryptography { get; }
		public IDataCacheController DataCache { get; }
		public IEventController Events { get; }
		public IFolderController Folders { get; }
		public IInstanceEndpointController InstanceEndpoints { get; }
		public IIoTController IoT { get; }
		public ILanguageController Languages { get; }
		public ILockingController Locking { get; }
		public ILoggingController Logging { get; }
		public IMailController Mail { get; }
		public IMetricController Metrics { get; }
		public IMicroServiceController MicroServices { get; }
		public IPrintingController Printing { get; }
		public IQueueController Queue { get; }
	}
}
