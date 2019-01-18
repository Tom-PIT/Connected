using TomPIT.Data.Sql;
using TomPIT.SysDb.Data;
using TomPIT.SysDb.Development;
using TomPIT.SysDb.Diagnostics;
using TomPIT.SysDb.Environment;
using TomPIT.SysDb.Events;
using TomPIT.SysDb.Globalization;
using TomPIT.SysDb.Management;
using TomPIT.SysDb.Security;
using TomPIT.SysDb.Sql.Configuration;
using TomPIT.SysDb.Sql.Data;
using TomPIT.SysDb.Sql.Development;
using TomPIT.SysDb.Sql.Diagnostics;
using TomPIT.SysDb.Sql.Environment;
using TomPIT.SysDb.Sql.Events;
using TomPIT.SysDb.Sql.Globalization;
using TomPIT.SysDb.Sql.Security;
using TomPIT.SysDb.Sql.Storage;
using TomPIT.SysDb.Sql.Workers;
using TomPIT.SysDb.Storage;
using TomPIT.SysDb.Workers;

namespace TomPIT.SysDb.Sql
{
	public class SqlProxy : ISysDbProxy
	{
		private IDevelopmentHandler _development = null;
		private IManagementHandler _management = null;
		private ISecurityHandler _security = null;
		private IStorageHandler _storage = null;
		private IEnvironmentHandler _environment = null;
		private IGlobalizationHandler _globalization = null;
		private IDiagnosticHandler _diagnostics = null;
		private IWorkerHandler _workers = null;
		private IEventHandler _events = null;
		private IDataHandler _data = null;

		public IDataHandler Data
		{
			get
			{
				if (_data == null)
					_data = new DataHandler();

				return _data;
			}
		}

		public IEventHandler Events
		{
			get
			{
				if (_events == null)
					_events = new EventHandler();

				return _events;
			}
		}

		public IWorkerHandler Workers
		{
			get
			{
				if (_workers == null)
					_workers = new WorkerHandler();

				return _workers;
			}
		}

		public IGlobalizationHandler Globalization
		{
			get
			{
				if (_globalization == null)
					_globalization = new GlobalizationHandler();

				return _globalization;
			}
		}

		public IEnvironmentHandler Environment
		{
			get
			{
				if (_environment == null)
					_environment = new EnvironmentHandler();

				return _environment;
			}
		}

		public IStorageHandler Storage
		{
			get
			{
				if (_storage == null)
					_storage = new StorageHandler();

				return _storage;
			}
		}

		public IDevelopmentHandler Development
		{
			get
			{
				if (_development == null)
					_development = new DevelopmentHandler();

				return _development;
			}
		}

		public IManagementHandler Management
		{
			get
			{
				if (_management == null)
					_management = new ManagementHandler();

				return _management;
			}
		}

		public ISecurityHandler Security
		{
			get
			{
				if (_security == null)
					_security = new SecurityHandler();

				return _security;
			}
		}

		public IDiagnosticHandler Diagnostics
		{
			get
			{
				if (_diagnostics == null)
					_diagnostics = new DiagnosticHandler();

				return _diagnostics;
			}
		}

		public void Initialize(string connectionString)
		{
			ConnectionBase.DefaultConnectionString = connectionString;
		}
	}
}
