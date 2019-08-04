using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Analysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Connectivity;
using TomPIT.Data.DataProviders;
using TomPIT.Deployment;
using TomPIT.Deployment.Database;
using TomPIT.Globalization;
using TomPIT.Storage;

namespace TomPIT.Management.Deployment
{
	internal class Package : IPackage
	{
		private IPackageMetaData _metaData = null;
		private IPackageMicroService _microService = null;
		private List<IPackageString> _strings = null;
		private List<IPackageFolder> _folders = null;
		private List<IPackageComponent> _components = null;
		private List<IPackageBlob> _blobs = null;
		private List<IPackageDependency> _dependencies = null;
		private List<IPackageDatabase> _databases = null;
		private List<IConfiguration> _configurations = null;
		private IPackageConfiguration _configuration = null;

		[JsonProperty(PropertyName = "configuration")]
		public IPackageConfiguration Configuration
		{
			get
			{
				if (_configuration == null)
					_configuration = new PackageConfiguration();

				return _configuration;
			}
		}

		[JsonProperty(PropertyName = "metaData")]
		public IPackageMetaData MetaData
		{
			get
			{
				if (_metaData == null)
					_metaData = new PackageMetaData();

				return _metaData;
			}
		}

		[JsonProperty(PropertyName = "microService")]
		public IPackageMicroService MicroService
		{
			get
			{
				if (_microService == null)
					_microService = new PackageMicroService();

				return _microService;
			}
		}

		[JsonProperty(PropertyName = "strings")]
		public List<IPackageString> Strings
		{
			get
			{
				if (_strings == null)
					_strings = new List<IPackageString>();

				return _strings;
			}
		}

		[JsonProperty(PropertyName = "folders")]
		public List<IPackageFolder> Folders
		{
			get
			{
				if (_folders == null)
					_folders = new List<IPackageFolder>();

				return _folders;
			}
		}

		[JsonProperty(PropertyName = "components")]
		public List<IPackageComponent> Components
		{
			get
			{
				if (_components == null)
					_components = new List<IPackageComponent>();

				return _components;
			}
		}
		[JsonProperty(PropertyName = "blobs")]
		public List<IPackageBlob> Blobs
		{
			get
			{
				if (_blobs == null)
					_blobs = new List<IPackageBlob>();

				return _blobs;
			}
		}

		[JsonProperty(PropertyName = "dependencies")]
		public List<IPackageDependency> Dependencies
		{
			get
			{
				if (_dependencies == null)
					_dependencies = new List<IPackageDependency>();

				return _dependencies;
			}
		}

		[JsonProperty(PropertyName = "databases")]
		public List<IPackageDatabase> Databases
		{
			get
			{
				if (_databases == null)
					_databases = new List<IPackageDatabase>();

				return _databases;
			}
		}

		[JsonIgnore]
		private List<IConfiguration> Configurations
		{
			get
			{
				if (_configurations == null)
					_configurations = new List<IConfiguration>();

				return _configurations;
			}
		}

		public void Create(Guid microService, ISysConnection connection)
		{
			CreateMicroService(microService, connection);
			CreateFolders(connection);
			CreateComponents(connection);
			CreateStrings(connection);
			CreateDependencies(connection);
			CreateDatabases(connection);
			CreateConfiguration(connection);
		}

		private void CreateConfiguration(ISysConnection connection)
		{
			foreach (var i in Databases)
			{
				Configuration.Databases.Add(new PackageConfigurationDatabase
				{
					Connection = i.Connection,
					DataProvider = i.DataProvider,
					DataProviderId = i.DataProviderId,
					Name = i.Name
				});
			}
		}

		private void CreateDependencies(ISysConnection connection)
		{
			var references = connection.GetService<IDiscoveryService>().References(MicroService.Token);

			if (references == null)
				return;

			foreach (var i in references.MicroServices)
			{
				var ms = connection.GetService<IMicroServiceService>().Select(i.MicroService);

				Dependencies.Add(new PackageDependency
				{
					Title = ms.Name,
					MicroService = ms.Token,
					Plan = ms.Plan
				});
			}
		}

		private void CreateComponents(ISysConnection connection)
		{
			var components = connection.GetService<IComponentService>().QueryComponents(MicroService.Token);

			foreach (var i in components)
			{
				Components.Add(new PackageComponent
				{
					Category = i.Category,
					Folder = i.Folder,
					Name = i.Name,
					RuntimeConfiguration = i.RuntimeConfiguration,
					Token = i.Token,
					Type = i.Type,
				});

				var config = connection.GetService<IComponentService>().SelectConfiguration(i.Token);
				Configurations.Add(config);

				if (Configuration.RuntimeConfigurationSupported && i.RuntimeConfiguration != Guid.Empty)
					CreateBlob(connection, i.RuntimeConfiguration);

				CreateBlob(connection, i.Token);

				var texts = config.Children<IText>();

				foreach (var j in texts)
					CreateBlob(connection, j.TextBlob);

				var er = config.Children<IExternalResourceElement>();

				foreach (var j in er)
				{
					var items = j.QueryResources();

					if (items == null || items.Count == 0)
						continue;

					foreach (var k in items)
						CreateBlob(connection, k);
				}
			}
		}

		private void CreateBlob(ISysConnection connection, Guid token)
		{
			if (token == Guid.Empty)
				return;

			var blob = connection.GetService<IStorageService>().Select(token);

			if (blob == null)
				return;

			var content = connection.GetService<IStorageService>().Download(token);

			Blobs.Add(new PackageBlob
			{
				Content = content != null ? Convert.ToBase64String(content.Content) : string.Empty,
				ContentType = blob.ContentType,
				FileName = blob.FileName,
				PrimaryKey = blob.PrimaryKey,
				MicroService = blob.MicroService,
				Token = blob.Token,
				Topic = blob.Topic,
				Type = blob.Type,
				Version = blob.Version
			});
		}

		private void CreateFolders(ISysConnection connection)
		{
			var folders = connection.GetService<IComponentService>().QueryFolders(MicroService.Token);

			foreach (var i in folders)
			{
				Folders.Add(new PackageFolder
				{
					Name = i.Name,
					Token = i.Token,
					Parent = i.Parent
				});
			}
		}

		private void CreateStrings(ISysConnection connection)
		{
			var strings = connection.GetService<IMicroServiceManagementService>().QueryStrings(MicroService.Token);
			var languages = connection.GetService<ILanguageService>().Query();

			foreach (var i in strings)
			{
				if (!ElementIncluded(connection, i.Element))
					continue;

				Strings.Add(new PackageString
				{
					Element = i.Element,
					Lcid = languages.FirstOrDefault(f => f.Token == i.Language).Lcid,
					Property = i.Property,
					Value = i.Value
				});
			}
		}

		private bool ElementIncluded(ISysConnection connection, Guid element)
		{
			var svc = connection.GetService<IDiscoveryService>();

			foreach (var i in Configurations)
			{
				if (svc.Find(i.Component, element) != null)
					return true;
			}

			return false;
		}

		private void CreateMicroService(Guid microService, ISysConnection connection)
		{
			var ms = connection.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				throw new RuntimeException(SR.ErrMicroServiceNotFound);

			_microService = new PackageMicroService
			{
				MetaData = connection.GetService<IMicroServiceService>().SelectMeta(ms.Token),
				Name = ms.Name,
				Template = ms.Template,
				Token = ms.Token
			};
		}

		private void CreateDatabases(ISysConnection connection)
		{
			var connections = Configurations.Where(f => f is IConnection);

			foreach (IConnection i in connections)
			{
				if (i.DataProvider == Guid.Empty)
					continue;

				var dp = connection.GetService<IDataProviderService>().Select(i.DataProvider);

				if (dp == null)
					continue;

				var database = new PackageDatabase
				{
					Name = i.ComponentName(connection),
					Connection = i.Component,
					DataProvider = dp.Name,
					DataProviderId = dp.Id
				};

				Databases.Add(database);

				if (!dp.SupportsDeploy)
					continue;

				var db = dp.CreateSchema(i.Value);

				CreateTables(database, db);
				CreateViews(database, db);
				CreateRoutines(database, db);
			}
		}

		private void CreateRoutines(PackageDatabase database, IDatabase provider)
		{
			database.Routines = new List<IRoutine>();

			foreach (var i in provider.Routines)
			{
				database.Routines.Add(new Routine
				{
					Definition = i.Definition,
					Name = i.Name,
					Schema = i.Schema,
					Type = i.Type
				});
			}
		}

		private void CreateViews(PackageDatabase database, IDatabase provider)
		{
			database.Views = new List<IView>();

			foreach (var i in provider.Views)
			{
				database.Views.Add(new View
				{
					Definition = i.Definition,
					Name = i.Name,
					Schema = i.Schema
				});
			}
		}

		private void CreateTables(PackageDatabase database, IDatabase provider)
		{
			database.Tables = new List<ITable>();

			foreach (var i in provider.Tables)
			{
				var t = new Table
				{
					Schema = i.Schema,
					Name = i.Name,
					Columns = new List<ITableColumn>()
				};

				database.Tables.Add(t);

				foreach (var j in i.Columns)
				{
					var c = new TableColumn
					{
						CharacterMaximumLength = j.CharacterMaximumLength,
						CharacterOctetLength = j.CharacterOctetLength,
						CharacterSetName = j.CharacterSetName,
						DataType = j.DataType,
						DateTimePrecision = j.DateTimePrecision,
						DefaultValue = j.DefaultValue,
						Identity = j.Identity,
						IsNullable = j.IsNullable,
						Name = j.Name,
						NumericPrecision = j.NumericPrecision,
						NumericPrecisionRadix = j.NumericPrecisionRadix,
						NumericScale = j.NumericScale,
						Ordinal = j.Ordinal
					};

					if (j.Reference != null)
					{
						var r = c.Reference as ReferentialConstraint;

						r.DeleteRule = j.Reference.DeleteRule;
						r.MatchOption = j.Reference.MatchOption;
						r.Name = j.Reference.Name;
						r.ReferenceName = j.Reference.ReferenceName;
						r.ReferenceSchema = j.Reference.ReferenceSchema;
						r.UpdateRule = j.Reference.UpdateRule;
					}

					foreach (var k in j.Constraints)
					{
						c.Constraints.Add(new TableConstraint
						{
							Name = k.Name,
							Schema = k.Schema,
							Type = k.Type
						});
					}

					t.Columns.Add(c);
				}

				foreach (var j in i.Indexes)
				{
					var idx = new Index
					{
						Name = j.Name
					};

					if (j.Columns.Count > 0)
						idx.Columns.AddRange(j.Columns);

					t.Indexes.Add(idx);
				}
			}
		}
	}
}
