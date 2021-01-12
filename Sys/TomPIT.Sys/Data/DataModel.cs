using System;
using TomPIT.Caching;
using TomPIT.Sys.Data.Analytics;
using TomPIT.Sys.Data.BigData;
using TomPIT.Sys.Data.IoT;

namespace TomPIT.Sys.Data
{
	internal static class DataModel
	{
		private static readonly Lazy<MemoryCache> _cache = new Lazy<MemoryCache>();

		private static readonly Lazy<MicroServicesMeta> _meta = new Lazy<MicroServicesMeta>(() => { return new MicroServicesMeta(Container); });
		private static readonly Lazy<MicroServices> _microServices = new Lazy<MicroServices>(() => { return new MicroServices(Container); });
		private static readonly Lazy<MicroServiceStrings> _microServiceStrings = new Lazy<MicroServiceStrings>(() => { return new MicroServiceStrings(Container); });
		private static readonly Lazy<Settings> _settings = new Lazy<Settings>(() => { return new Settings(Container); });
		private static readonly Lazy<Users> _users = new Lazy<Users>(() => { return new Users(Container); });
		private static readonly Lazy<Roles> _roles = new Lazy<Roles>(() => { return new Roles(Container); });
		private static readonly Lazy<Blobs> _blobs = new Lazy<Blobs>(() => { return new Blobs(Container); });
		private static readonly Lazy<BlobsContents> _blobsContents = new Lazy<BlobsContents>(() => { return new BlobsContents(Container); });
		private static readonly Lazy<InstanceEndpoints> _instanceEndpoints = new Lazy<InstanceEndpoints>(() => { return new InstanceEndpoints(Container); });
		private static readonly Lazy<ResourceGroups> _resourceGroups = new Lazy<ResourceGroups>(() => { return new ResourceGroups(Container); });
		private static readonly Lazy<Languages> _languages = new Lazy<Languages>(() => { return new Languages(Container); });
		private static readonly Lazy<EnvironmentVariables> _environmentVars = new Lazy<EnvironmentVariables>(() => { return new EnvironmentVariables(Container); });
		private static readonly Lazy<Membership> _membership = new Lazy<Membership>(() => { return new Membership(Container); });
		private static readonly Lazy<Permissions> _permissions = new Lazy<Permissions>(() => { return new Permissions(Container); });
		private static readonly Lazy<Folders> _folders = new Lazy<Folders>(() => { return new Folders(Container); });
		private static readonly Lazy<Components> _components = new Lazy<Components>(() => { return new Components(Container); });
		private static readonly Lazy<MessageTopics> _messageTopics = new Lazy<MessageTopics>(() => { return new MessageTopics(Container); });
		private static readonly Lazy<MessageSubscribers> _messageSubscribers = new Lazy<MessageSubscribers>(() => { return new MessageSubscribers(Container); });
		private static readonly Lazy<MessageRecipients> _messageRecipients = new Lazy<MessageRecipients>(() => { return new MessageRecipients(Container); });
		private static readonly Lazy<Messages> _messages = new Lazy<Messages>(() => { return new Messages(Container); });
		private static readonly Lazy<Logging> _logging = new Lazy<Logging>(() => { return new Logging(); });
		private static readonly Lazy<Workers> _workers = new Lazy<Workers>(() => { return new Workers(Container); });
		private static readonly Lazy<Events> _events = new Lazy<Events>(() => { return new Events(); });
		private static readonly Lazy<Audit> _audit = new Lazy<Audit>(() => { return new Audit(); });
		private static readonly Lazy<AuthenticationTokens> _authTokens = new Lazy<AuthenticationTokens>(() => { return new AuthenticationTokens(Container); });
		private static readonly Lazy<Metrics> _metrics = new Lazy<Metrics>(() => { return new Metrics(); });
		private static readonly Lazy<IoTState> _iotState = new Lazy<IoTState>(() => { return new IoTState(Container); });
		private static readonly Lazy<Deployment> _deployment = new Lazy<Deployment>(() => { return new Deployment(); });
		private static readonly Lazy<VersionControl> _versionControl = new Lazy<VersionControl>(() => { return new VersionControl(); });
		private static readonly Lazy<UserData> _userData = new Lazy<UserData>(() => { return new UserData(Container); });
		private static readonly Lazy<TestSuite> _testSuite = new Lazy<TestSuite>(() => { return new TestSuite(); });
		private static readonly Lazy<Mail> _mail = new Lazy<Mail>(() => { return new Mail(); });
		private static readonly Lazy<Subscriptions> _subscribers = new Lazy<Subscriptions>(() => { return new Subscriptions(); });
		private static readonly Lazy<Aliens> _aliens = new Lazy<Aliens>(() => { return new Aliens(Container); });
		private static readonly Lazy<BigDataNodes> _bigDataNodes = new Lazy<BigDataNodes>(() => { return new BigDataNodes(Container); });
		private static readonly Lazy<BigDataPartitions> _bigDataPartitions = new Lazy<BigDataPartitions>(() => { return new BigDataPartitions(Container); });
		private static readonly Lazy<BigDataTransactions> _bigDataTransactions = new Lazy<BigDataTransactions>(() => { return new BigDataTransactions(Container); });
		private static readonly Lazy<BigDataTransactionBlocks> _bigDataTransactionBlocks = new Lazy<BigDataTransactionBlocks>(() => { return new BigDataTransactionBlocks(); });
		private static readonly Lazy<BigDataPartitionFiles> _bigDataPartitionFiles = new Lazy<BigDataPartitionFiles>(() => { return new BigDataPartitionFiles(Container); });
		private static readonly Lazy<BigDataPartitionFieldStatistics> _bigDataPartitionFieldStatistics = new Lazy<BigDataPartitionFieldStatistics>(() => { return new BigDataPartitionFieldStatistics(Container); });
		private static readonly Lazy<DevelopmentErrors> _devErrors = new Lazy<DevelopmentErrors>(() => { return new DevelopmentErrors(); });
		private static readonly Lazy<Queueing> _queue = new Lazy<Queueing>(() => { return new Queueing(Container); });
		private static readonly Lazy<Search> _search = new Lazy<Search>(() => { return new Search(); });
		private static readonly Lazy<SysSearch> _sysSearch = new Lazy<SysSearch>(() => { return new SysSearch(); });
		private static readonly Lazy<Tools> _tools = new Lazy<Tools>(() => { return new Tools(); });
		private static readonly Lazy<Printing> _printing = new Lazy<Printing>(() => { return new Printing(); });
		private static readonly Lazy<Mrus> _mrus = new Lazy<Mrus>(() => { return new Mrus(); });
		private static readonly Lazy<PartitionBuffering> _partitionBuffering = new Lazy<PartitionBuffering>(() => { return new PartitionBuffering(Container); });
		private static readonly Lazy<Locking> _locking = new Lazy<Locking>(() => { return new Locking(); });
		private static readonly Lazy<PrintingSpooler> _printingSpooler = new Lazy<PrintingSpooler>(() => { return new PrintingSpooler(); });
		private static readonly Lazy<Clients> _clients = new Lazy<Clients>(() => { return new Clients(Container); });

		public static MicroServicesMeta MicroServicesMeta { get { return _meta.Value; } }
		public static MicroServices MicroServices { get { return _microServices.Value; } }
		public static MicroServiceStrings MicroServiceStrings { get { return _microServiceStrings.Value; } }
		public static Settings Settings { get { return _settings.Value; } }
		public static Users Users { get { return _users.Value; } }
		public static Roles Roles { get { return _roles.Value; } }
		public static Blobs Blobs { get { return _blobs.Value; } }
		public static BlobsContents BlobsContents { get { return _blobsContents.Value; } }
		public static InstanceEndpoints InstanceEndpoints { get { return _instanceEndpoints.Value; } }
		public static ResourceGroups ResourceGroups { get { return _resourceGroups.Value; } }
		public static Languages Languages { get { return _languages.Value; } }
		public static EnvironmentVariables EnvironmentVariables { get { return _environmentVars.Value; } }
		public static Membership Membership { get { return _membership.Value; } }
		public static Permissions Permissions { get { return _permissions.Value; } }
		public static Folders Folders { get { return _folders.Value; } }
		public static Components Components { get { return _components.Value; } }
		public static MessageTopics MessageTopics { get { return _messageTopics.Value; } }
		public static MessageSubscribers MessageSubscribers { get { return _messageSubscribers.Value; } }
		public static Messages Messages { get { return _messages.Value; } }
		public static MessageRecipients MessageRecipients { get { return _messageRecipients.Value; } }
		public static Logging Logging { get { return _logging.Value; } }
		public static Workers Workers { get { return _workers.Value; } }
		public static Events Events { get { return _events.Value; } }
		public static Audit Audit { get { return _audit.Value; } }
		public static AuthenticationTokens AuthenticationTokens { get { return _authTokens.Value; } }
		public static Metrics Metrics { get { return _metrics.Value; } }
		public static IoTState IoTState { get { return _iotState.Value; } }
		public static Deployment Deployment { get { return _deployment.Value; } }
		public static VersionControl VersionControl { get { return _versionControl.Value; } }
		public static UserData UserData { get { return _userData.Value; } }
		public static TestSuite TestSuite { get { return _testSuite.Value; } }
		public static Mail Mail { get { return _mail.Value; } }
		public static Subscriptions Subscriptions { get { return _subscribers.Value; } }
		public static Aliens Aliens { get { return _aliens.Value; } }
		public static BigDataNodes BigDataNodes { get { return _bigDataNodes.Value; } }
		public static BigDataPartitions BigDataPartitions { get { return _bigDataPartitions.Value; } }
		public static BigDataTransactions BigDataTransactions { get { return _bigDataTransactions.Value; } }
		public static BigDataTransactionBlocks BigDataTransactionBlocks { get { return _bigDataTransactionBlocks.Value; } }
		public static BigDataPartitionFiles BigDataPartitionFiles { get { return _bigDataPartitionFiles.Value; } }
		public static BigDataPartitionFieldStatistics BigDataPartitionFieldStatistics { get { return _bigDataPartitionFieldStatistics.Value; } }
		public static DevelopmentErrors DevelopmentErrors { get { return _devErrors.Value; } }
		public static Queueing Queue { get { return _queue.Value; } }
		public static Search Search { get { return _search.Value; } }
		public static SysSearch SysSearch { get { return _sysSearch.Value; } }
		public static Tools Tools { get { return _tools.Value; } }
		public static Printing Printing { get { return _printing.Value; } }
		public static Mrus Mrus { get { return _mrus.Value; } }
		public static PartitionBuffering BigDataPartitionBuffering { get { return _partitionBuffering.Value; } }
		public static Locking Locking { get { return _locking.Value; } }
		public static PrintingSpooler PrintingSpooler { get { return _printingSpooler.Value; } }
		public static Clients Clients { get { return _clients.Value; } }
		internal static MemoryCache Container
		{
			get { return _cache.Value; }
		}

		public static bool Initialized { get; set; }
	}
}