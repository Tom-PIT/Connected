namespace TomPIT.Proxy
{
	public interface ISysProxy
	{
		ISysManagementProxy Management { get; }
		ISysDevelopmentProxy Development { get; }
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
		IIoTController IoT { get; }
		ILanguageController Languages { get; }
		ILockingController Locking { get; }
		ILoggingController Logging { get; }
		IMailController Mail { get; }
		IMicroServiceController MicroServices { get; }
		IPrintingController Printing { get; }
		IQueueController Queue { get; }
		IResourceGroupController ResourceGroups { get; }
		IRoleController Roles { get; }
		ISearchController Search { get; }
		ISecurityController Security { get; }
		ISettingController Settings { get; }
		IStorageController Storage { get; }
		ISubscriptionController Subscriptions { get; }
		IUserController Users { get; }
		IUserDataController UserData { get; }
		IXmlKeyController XmlKeys { get; }
	}
}
