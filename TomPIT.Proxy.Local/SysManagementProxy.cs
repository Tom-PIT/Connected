﻿using TomPIT.Proxy.Local.Management;
using TomPIT.Proxy.Management;

namespace TomPIT.Proxy.Local
{
    internal class SysManagementProxy : ISysManagementProxy
    {
        public SysManagementProxy()
        {
            Roles = new RoleManagementController();
            Settings = new SettingManagementController();
            BigData = new BigDataManagementController();
            Users = new UserManagementController();
            MicroServices = new MicroServiceManagementController();
            Deployment = new DeploymentManagementController();
            Environment = new EnvironmentManagementController();
            Events = new EventManagementController();
            Globalization = new GlobalizationManagementController();
            InstanceEndpoints = new InstanceEndpointManagementController();
            Logging = new LoggingManagementController();
            Mail = new MailManagementController();
            Metrics = new MetricManagementController();
            Printing = new PrintManagementController();
            Queue = new QueueManagementController();
            ResourceGroups = new ResourceGroupManagementController();
            Search = new SearchManagementController();
            Security = new SecurityManagementController();
            Storage = new StorageManagementController();
            Subscriptions = new SubscriptionManagementController();
            Workers = new WorkerManagementController();
        }

        public IRoleManagementController Roles { get; }
        public ISettingManagementController Settings { get; }
        public IBigDataManagementController BigData { get; }
        public IUserManagementController Users { get; }
        public IMicroServiceManagementController MicroServices { get; }
        public IDeploymentManagementController Deployment { get; }
        public IEnvironmentManagementController Environment { get; }
        public IEventManagementController Events { get; }
        public IGlobalizationManagementController Globalization { get; }
        public IInstanceEndpointManagementController InstanceEndpoints { get; }
        public ILoggingManagementController Logging { get; }
        public IMailManagementController Mail { get; }
        public IMetricManagementController Metrics { get; }
        public IPrintManagementController Printing { get; }
        public IQueueManagementController Queue { get; }
        public IResourceGroupManagementController ResourceGroups { get; }
        public ISearchManagementController Search { get; }
        public ISecurityManagementController Security { get; }
        public IStorageManagementController Storage { get; }
        public ISubscriptionManagementController Subscriptions { get; }
        public IWorkerManagementController Workers { get; }
    }
}