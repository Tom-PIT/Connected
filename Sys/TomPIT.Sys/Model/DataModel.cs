﻿using System;
using TomPIT.Caching;
using TomPIT.Sys.Model.Analytics;
using TomPIT.Sys.Model.BigData;
using TomPIT.Sys.Model.Blobs;
using TomPIT.Sys.Model.Cdn;
using TomPIT.Sys.Model.Components;
using TomPIT.Sys.Model.Configuration;
using TomPIT.Sys.Model.Data;
using TomPIT.Sys.Model.Diagnostics;
using TomPIT.Sys.Model.Environment;
using TomPIT.Sys.Model.Globalization;
using TomPIT.Sys.Model.IoT;
using TomPIT.Sys.Model.Printing;
using TomPIT.Sys.Model.Search;
using TomPIT.Sys.Model.Security;
using TomPIT.Sys.Model.Workers;

namespace TomPIT.Sys.Model
{
	public static class DataModel
	{
		private static readonly Lazy<MemoryCache> _cache = new Lazy<MemoryCache>();

		private static readonly Lazy<MicroServicesModel> _microServices = new Lazy<MicroServicesModel>(() => { return new MicroServicesModel(Container); });
		private static readonly Lazy<SettingsModel> _settings = new Lazy<SettingsModel>(() => { return new SettingsModel(Container); });
		private static readonly Lazy<UsersModel> _users = new Lazy<UsersModel>(() => { return new UsersModel(Container); });
		private static readonly Lazy<RolesModel> _roles = new Lazy<RolesModel>(() => { return new RolesModel(Container); });
		private static readonly Lazy<BlobsModel> _blobs = new Lazy<BlobsModel>(() => { return new BlobsModel(Container); });
		private static readonly Lazy<BlobsContentsModel> _blobsContents = new Lazy<BlobsContentsModel>(() => { return new BlobsContentsModel(Container); });
		private static readonly Lazy<InstanceEndpointsModel> _instanceEndpoints = new Lazy<InstanceEndpointsModel>(() => { return new InstanceEndpointsModel(Container); });
		private static readonly Lazy<ResourceGroupsModel> _resourceGroups = new Lazy<ResourceGroupsModel>(() => { return new ResourceGroupsModel(Container); });
		private static readonly Lazy<LanguagesModel> _languages = new Lazy<LanguagesModel>(() => { return new LanguagesModel(Container); });
		private static readonly Lazy<EnvironmentVariablesModel> _environmentVars = new Lazy<EnvironmentVariablesModel>(() => { return new EnvironmentVariablesModel(Container); });
		private static readonly Lazy<MembershipModel> _membership = new Lazy<MembershipModel>(() => { return new MembershipModel(Container); });
		private static readonly Lazy<PermissionsModel> _permissions = new Lazy<PermissionsModel>(() => { return new PermissionsModel(Container); });
		private static readonly Lazy<FoldersModel> _folders = new Lazy<FoldersModel>(() => { return new FoldersModel(Container); });
		private static readonly Lazy<ComponentsModel> _components = new Lazy<ComponentsModel>(() => { return new ComponentsModel(Container); });
		private static readonly Lazy<MessageTopicsModel> _messageTopics = new Lazy<MessageTopicsModel>(() => { return new MessageTopicsModel(Container); });
		private static readonly Lazy<MessageSubscribersModel> _messageSubscribers = new Lazy<MessageSubscribersModel>(() => { return new MessageSubscribersModel(Container); });
		private static readonly Lazy<MessageRecipientsModel> _messageRecipients = new Lazy<MessageRecipientsModel>(() => { return new MessageRecipientsModel(Container); });
		private static readonly Lazy<MessagesModel> _messages = new Lazy<MessagesModel>(() => { return new MessagesModel(Container); });
		private static readonly Lazy<LoggingModel> _logging = new Lazy<LoggingModel>(() => { return new LoggingModel(); });
		private static readonly Lazy<WorkersModel> _workers = new Lazy<WorkersModel>(() => { return new WorkersModel(Container); });
		private static readonly Lazy<EventsModel> _events = new Lazy<EventsModel>(() => { return new EventsModel(Container); });
		private static readonly Lazy<AuditModel> _audit = new Lazy<AuditModel>(() => { return new AuditModel(); });
		private static readonly Lazy<AuthenticationTokensModel> _authTokens = new Lazy<AuthenticationTokensModel>(() => { return new AuthenticationTokensModel(Container); });
		private static readonly Lazy<IoTStateModel> _iotState = new Lazy<IoTStateModel>(() => { return new IoTStateModel(Container); });
		private static readonly Lazy<UserDataModel> _userData = new Lazy<UserDataModel>(() => { return new UserDataModel(Container); });
		private static readonly Lazy<MailModel> _mail = new Lazy<MailModel>(() => { return new MailModel(); });
		private static readonly Lazy<SubscriptionsModel> _subscribers = new Lazy<SubscriptionsModel>(() => { return new SubscriptionsModel(); });
		private static readonly Lazy<AliensModel> _aliens = new Lazy<AliensModel>(() => { return new AliensModel(Container); });
		private static readonly Lazy<BigDataNodesModel> _bigDataNodes = new Lazy<BigDataNodesModel>(() => { return new BigDataNodesModel(Container); });
		private static readonly Lazy<BigDataPartitionsModel> _bigDataPartitions = new Lazy<BigDataPartitionsModel>(() => { return new BigDataPartitionsModel(Container); });
		private static readonly Lazy<BigDataTransactionsModel> _bigDataTransactions = new Lazy<BigDataTransactionsModel>(() => { return new BigDataTransactionsModel(Container); });
		private static readonly Lazy<BigDataTransactionBlocksModel> _bigDataTransactionBlocks = new Lazy<BigDataTransactionBlocksModel>(() => { return new BigDataTransactionBlocksModel(); });
		private static readonly Lazy<BigDataPartitionFilesModel> _bigDataPartitionFiles = new Lazy<BigDataPartitionFilesModel>(() => { return new BigDataPartitionFilesModel(Container); });
		private static readonly Lazy<BigDataPartitionFieldStatisticsModel> _bigDataPartitionFieldStatistics = new Lazy<BigDataPartitionFieldStatisticsModel>(() => { return new BigDataPartitionFieldStatisticsModel(Container); });
		private static readonly Lazy<BigDataTimezonesModel> _bigDataTimezones = new Lazy<BigDataTimezonesModel>(() => { return new BigDataTimezonesModel(Container); });
		private static readonly Lazy<QueueingModel> _queue = new Lazy<QueueingModel>(() => { return new QueueingModel(Container); });
		private static readonly Lazy<SearchModel> _search = new Lazy<SearchModel>(() => { return new SearchModel(); });
		private static readonly Lazy<SysSearchModel> _sysSearch = new Lazy<SysSearchModel>(() => { return new SysSearchModel(); });
		private static readonly Lazy<PrintingModel> _printing = new Lazy<PrintingModel>(() => { return new PrintingModel(Container); });
		private static readonly Lazy<MrusModel> _mrus = new Lazy<MrusModel>(() => { return new MrusModel(); });
		private static readonly Lazy<PartitionBufferingModel> _partitionBuffering = new Lazy<PartitionBufferingModel>(() => { return new PartitionBufferingModel(Container); });
		private static readonly Lazy<LockingModel> _locking = new Lazy<LockingModel>(() => { return new LockingModel(); });
		private static readonly Lazy<PrintingSpoolerModel> _printingSpooler = new Lazy<PrintingSpoolerModel>(() => { return new PrintingSpoolerModel(); });
		private static readonly Lazy<ClientsModel> _clients = new Lazy<ClientsModel>(() => { return new ClientsModel(Container); });
		private static readonly Lazy<PrintingSerialNumbersModel> _printingSerialNumbers = new Lazy<PrintingSerialNumbersModel>(() => { return new PrintingSerialNumbersModel(Container); });
		private static readonly Lazy<XmlKeysModel> _xmlKeys = new Lazy<XmlKeysModel>(() => { return new XmlKeysModel(Container); });
		private static readonly Lazy<SourceFilesModel> _sourceFiles = new Lazy<SourceFilesModel>(() => { return new SourceFilesModel(Container); });

		public static MicroServicesModel MicroServices => _microServices.Value;
		public static SettingsModel Settings => _settings.Value;
		public static UsersModel Users => _users.Value;
		public static RolesModel Roles => _roles.Value;
		public static BlobsModel Blobs => _blobs.Value;
		public static BlobsContentsModel BlobsContents => _blobsContents.Value;
		public static InstanceEndpointsModel InstanceEndpoints => _instanceEndpoints.Value;
		public static ResourceGroupsModel ResourceGroups => _resourceGroups.Value;
		public static LanguagesModel Languages => _languages.Value;
		public static EnvironmentVariablesModel EnvironmentVariables => _environmentVars.Value;
		public static MembershipModel Membership => _membership.Value;
		public static PermissionsModel Permissions => _permissions.Value;
		public static FoldersModel Folders => _folders.Value;
		public static ComponentsModel Components => _components.Value;
		public static MessageTopicsModel MessageTopics => _messageTopics.Value;
		public static MessageSubscribersModel MessageSubscribers => _messageSubscribers.Value;
		public static MessagesModel Messages => _messages.Value;
		public static MessageRecipientsModel MessageRecipients => _messageRecipients.Value;
		public static LoggingModel Logging => _logging.Value;
		public static WorkersModel Workers => _workers.Value;
		public static EventsModel Events => _events.Value;
		public static AuditModel Audit => _audit.Value;
		public static AuthenticationTokensModel AuthenticationTokens => _authTokens.Value;
		public static IoTStateModel IoTState => _iotState.Value;
		public static UserDataModel UserData => _userData.Value;
		public static MailModel Mail => _mail.Value;
		public static SubscriptionsModel Subscriptions => _subscribers.Value;
		public static AliensModel Aliens => _aliens.Value;
		public static BigDataNodesModel BigDataNodes => _bigDataNodes.Value;
		public static BigDataPartitionsModel BigDataPartitions => _bigDataPartitions.Value;
		public static BigDataTransactionsModel BigDataTransactions => _bigDataTransactions.Value;
		public static BigDataTransactionBlocksModel BigDataTransactionBlocks => _bigDataTransactionBlocks.Value;
		public static BigDataPartitionFilesModel BigDataPartitionFiles => _bigDataPartitionFiles.Value;
		public static BigDataPartitionFieldStatisticsModel BigDataPartitionFieldStatistics => _bigDataPartitionFieldStatistics.Value;
		public static BigDataTimezonesModel BigDataTimeZones => _bigDataTimezones.Value;
		public static QueueingModel Queue => _queue.Value;
		public static SearchModel Search => _search.Value;
		public static SysSearchModel SysSearch => _sysSearch.Value;
		public static PrintingModel Printing => _printing.Value;
		public static MrusModel Mrus => _mrus.Value;
		public static PartitionBufferingModel BigDataPartitionBuffering => _partitionBuffering.Value;
		public static LockingModel Locking => _locking.Value;
		public static PrintingSpoolerModel PrintingSpooler => _printingSpooler.Value;
		public static ClientsModel Clients => _clients.Value;
		public static PrintingSerialNumbersModel PrintingSerialNumbers => _printingSerialNumbers.Value;
		public static XmlKeysModel XmlKeys => _xmlKeys.Value;
		public static SourceFilesModel SourceFiles => _sourceFiles.Value;
		internal static MemoryCache Container => _cache.Value;

		public static bool Initialized { get; set; }
	}
}