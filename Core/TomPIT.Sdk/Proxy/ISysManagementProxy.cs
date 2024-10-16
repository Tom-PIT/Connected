﻿using TomPIT.Proxy.Management;

namespace TomPIT.Proxy;

public interface ISysManagementProxy
{
    IRoleManagementController Roles { get; }
    ISettingManagementController Settings { get; }
    IBigDataManagementController BigData { get; }
    IUserManagementController Users { get; }
    IMicroServiceManagementController MicroServices { get; }
    IDeploymentManagementController Deployment { get; }
    IEnvironmentManagementController Environment { get; }
    IEventManagementController Events { get; }
    IGlobalizationManagementController Globalization { get; }
    IInstanceEndpointManagementController InstanceEndpoints { get; }
    ILoggingManagementController Logging { get; }
    IMailManagementController Mail { get; }
    IMetricManagementController Metrics { get; }
    IPrintManagementController Printing { get; }
    IQueueManagementController Queue { get; }
    IResourceGroupManagementController ResourceGroups { get; }
    ISearchManagementController Search { get; }
    ISecurityManagementController Security { get; }
    IStorageManagementController Storage { get; }
    ISubscriptionManagementController Subscriptions { get; }
    IWorkerManagementController Workers { get; }
}
