using System;
using TomPIT.Caching;

namespace TomPIT.Sys.Data
{
	internal static class DataModel
	{
		private static Lazy<MemoryCache> _cache = new Lazy<MemoryCache>();

		private static Lazy<MicroServicesMeta> _meta = new Lazy<MicroServicesMeta>(() => { return new MicroServicesMeta(Container); });
		private static Lazy<MicroServices> _microServices = new Lazy<MicroServices>(() => { return new MicroServices(Container); });
		private static Lazy<MicroServiceStrings> _microServiceStrings = new Lazy<MicroServiceStrings>(() => { return new MicroServiceStrings(Container); });
		private static Lazy<Settings> _settings = new Lazy<Settings>(() => { return new Settings(Container); });
		private static Lazy<Users> _users = new Lazy<Users>(() => { return new Users(Container); });
		private static Lazy<Roles> _roles = new Lazy<Roles>(() => { return new Roles(Container); });
		private static Lazy<Blobs> _blobs = new Lazy<Blobs>(() => { return new Blobs(Container); });
		private static Lazy<BlobsContents> _blobsContents = new Lazy<BlobsContents>(() => { return new BlobsContents(Container); });
		private static Lazy<EnvironmentUnits> _environmentUnits = new Lazy<EnvironmentUnits>(() => { return new EnvironmentUnits(Container); });
		private static Lazy<InstanceEndpoints> _instanceEndpoints = new Lazy<InstanceEndpoints>(() => { return new InstanceEndpoints(Container); });
		private static Lazy<ResourceGroups> _resourceGroups = new Lazy<ResourceGroups>(() => { return new ResourceGroups(Container); });
		private static Lazy<Languages> _languages = new Lazy<Languages>(() => { return new Languages(Container); });
		private static Lazy<EnvironmentVariables> _environmentVars = new Lazy<EnvironmentVariables>(() => { return new EnvironmentVariables(Container); });
		private static Lazy<Membership> _membership = new Lazy<Membership>(() => { return new Membership(Container); });
		private static Lazy<Permissions> _permissions = new Lazy<Permissions>(() => { return new Permissions(Container); });
		private static Lazy<Features> _features = new Lazy<Features>(() => { return new Features(Container); });
		private static Lazy<Components> _components = new Lazy<Components>(() => { return new Components(Container); });
		private static Lazy<MessageTopics> _messageTopics = new Lazy<MessageTopics>(() => { return new MessageTopics(Container); });
		private static Lazy<MessageSubscribers> _messageSubscribers = new Lazy<MessageSubscribers>(() => { return new MessageSubscribers(Container); });
		private static Lazy<MessageRecipients> _messageRecipients = new Lazy<MessageRecipients>(() => { return new MessageRecipients(Container); });
		private static Lazy<Messages> _messages = new Lazy<Messages>(() => { return new Messages(Container); });
		private static Lazy<Logging> _logging = new Lazy<Logging>(() => { return new Logging(Container); });
		private static Lazy<Workers> _workers = new Lazy<Workers>(() => { return new Workers(Container); });
		private static Lazy<Events> _events = new Lazy<Events>(() => { return new Events(); });
		private static Lazy<Audit> _audit = new Lazy<Audit>(() => { return new Audit(); });

		public static MicroServicesMeta MicroServicesMeta { get { return _meta.Value; } }
		public static MicroServices MicroServices { get { return _microServices.Value; } }
		public static MicroServiceStrings MicroServiceStrings { get { return _microServiceStrings.Value; } }
		public static Settings Settings { get { return _settings.Value; } }
		public static Users Users { get { return _users.Value; } }
		public static Roles Roles { get { return _roles.Value; } }
		public static Blobs Blobs { get { return _blobs.Value; } }
		public static BlobsContents BlobsContents { get { return _blobsContents.Value; } }
		public static EnvironmentUnits EnvironmentUnits { get { return _environmentUnits.Value; } }
		public static InstanceEndpoints InstanceEndpoints { get { return _instanceEndpoints.Value; } }
		public static ResourceGroups ResourceGroups { get { return _resourceGroups.Value; } }
		public static Languages Languages { get { return _languages.Value; } }
		public static EnvironmentVariables EnvironmentVariables { get { return _environmentVars.Value; } }
		public static Membership Membership { get { return _membership.Value; } }
		public static Permissions Permissions { get { return _permissions.Value; } }
		public static Features Features { get { return _features.Value; } }
		public static Components Components { get { return _components.Value; } }
		public static MessageTopics MessageTopics { get { return _messageTopics.Value; } }
		public static MessageSubscribers MessageSubscribers { get { return _messageSubscribers.Value; } }
		public static Messages Messages { get { return _messages.Value; } }
		public static MessageRecipients MessageRecipients { get { return _messageRecipients.Value; } }
		public static Logging Logging { get { return _logging.Value; } }
		public static Workers Workers { get { return _workers.Value; } }
		public static Events Events { get { return _events.Value; } }
		public static Audit Audit { get { return _audit.Value; } }

		internal static MemoryCache Container
		{
			get { return _cache.Value; }
		}
	}
}