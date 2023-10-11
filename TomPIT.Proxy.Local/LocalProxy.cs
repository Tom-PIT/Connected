namespace TomPIT.Proxy.Local
{
	public class LocalProxy : ISysProxy
	{
		public LocalProxy()
		{
			Management = new SysManagementProxy();
			Development = new SysDevelopmentProxy();

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
			MicroServices = new MicroServiceController();
			Printing = new PrintingController();
			Queue = new QueueController();
			ResourceGroups = new ResourceGroupController();
			Roles = new RoleController();
			Search = new SearchController();
			Security = new SecurityController();
			Settings = new SettingController();
			Storage = new StorageController();
			Subscriptions = new SubscriptionController();
			Users = new UserController();
			UserData = new UserDataController();
			XmlKeys = new XmlKeyController();
		}

		public ISysManagementProxy Management { get; }
		public ISysDevelopmentProxy Development { get; }

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
		public IMicroServiceController MicroServices { get; }
		public IPrintingController Printing { get; }
		public IQueueController Queue { get; }
		public IResourceGroupController ResourceGroups { get; }
		public IRoleController Roles { get; }
		public ISearchController Search { get; }
		public ISecurityController Security { get; }
		public ISettingController Settings { get; }
		public IStorageController Storage { get; }
		public ISubscriptionController Subscriptions { get; }
		public IUserController Users { get; }
		public IUserDataController UserData { get; }
		public IXmlKeyController XmlKeys { get; }
	}
}
