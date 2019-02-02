using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Analysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Data.DataProviders.Deployment;
using TomPIT.Globalization;
using TomPIT.Storage;

namespace TomPIT.Deployment
{
	internal class Package : IPackage
	{
		private IPackageMetaData _metaData = null;
		private IPackageMicroService _microService = null;
		private List<IMicroServiceString> _strings = null;
		private List<IPackageFolder> _folders = null;
		private List<IPackageComponent> _components = null;
		private List<IPackageBlob> _blobs = null;
		private List<IPackageDependency> _dependencies = null;
		private List<IDatabase> _databases = null;
		private List<IConfiguration> _configurations = null;

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
		public List<IMicroServiceString> Strings
		{
			get
			{
				if (_strings == null)
					_strings = new List<IMicroServiceString>();

				return _strings;
			}
		}

		[JsonProperty(PropertyName = "features")]
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
		public List<IDatabase> Databases
		{
			get
			{
				if (_databases == null)
					_databases = new List<IDatabase>();

				return _databases;
			}
		}


		public static Package Create(PackageCreateArgs e)
		{
			var r = new Package();

			r.CreatePackage(e);

			return r;
		}

		[JsonIgnore]
		private PackageCreateArgs Args { get; set; }
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

		internal void CreatePackage(PackageCreateArgs e)
		{
			Args = e;

			_metaData = new PackageMetaData
			{
				Created = e.MetaData.Created,
				Description = e.MetaData.Description,
				Id = e.MetaData.Id,
				ImageUrl = e.MetaData.ImageUrl,
				LicenseUrl = e.MetaData.LicenseUrl,
				Name = e.MetaData.Name,
				Price = e.MetaData.Price,
				ProjectUrl = e.MetaData.ProjectUrl,
				Publisher = e.MetaData.Publisher,
				Scope = e.MetaData.Scope,
				ShellVersion = e.MetaData.ShellVersion,
				Tags = e.MetaData.Tags,
				Title = e.MetaData.Title,
				Version = e.MetaData.Version,
				Licenses = e.MetaData.Licenses,
				Trial = e.MetaData.Trial,
				TrialPeriod = e.MetaData.TrialPeriod
			};

			CreateMicroService();
			CreateFolders();
			CreateComponents();
			CreateStrings();
			CreateDependencies();
			CreateDatabases();
		}

		private void CreateDependencies()
		{
			var references = Args.Connection.GetService<IDiscoveryService>().References(Args.MicroService);

			if (references == null)
				return;

			foreach (var i in references.MicroServices)
			{
				Dependencies.Add(new PackageDependency
				{
					Name = i.MicroService
				});
			}
		}

		private void CreateComponents()
		{
			var components = Args.Connection.GetService<IComponentService>().QueryComponents(Args.MicroService);

			foreach (var i in components)
			{
				if (i.Folder != Guid.Empty && Folders.FirstOrDefault(f => f.Token == i.Folder) == null)
					continue;

				var e = new PackageProcessArgs(PackageEntity.Component, i.Token.ToString());

				Args.Callback?.Invoke(e);

				if (e.Cancel)
					continue;

				Components.Add(new PackageComponent
				{
					Category = i.Category,
					Folder = i.Folder,
					Name = i.Name,
					RuntimeConfiguration = i.RuntimeConfiguration,
					Token = i.Token,
					Type = i.Type,
				});

				var config = Args.Connection.GetService<IComponentService>().SelectConfiguration(i.Token);
				Configurations.Add(config);

				var texts = config.Children<IText>();

				foreach (var j in texts)
					CreateBlob(j.TextBlob);

				var er = config.Children<IExternalResourceElement>();

				foreach (var j in er)
				{
					var items = j.QueryResources();

					if (items == null || items.Count == 0)
						continue;

					foreach (var k in items)
						CreateBlob(k);
				}
			}
		}

		private void CreateBlob(Guid token)
		{
			var blob = Args.Connection.GetService<IStorageService>().Select(token);

			if (blob == null)
				return;

			var content = Args.Connection.GetService<IStorageService>().Download(token);

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

		private void CreateFolders()
		{
			var folders = Args.Connection.GetService<IComponentService>().QueryFolders(Args.MicroService);

			foreach (var i in folders)
			{
				var e = new PackageProcessArgs(PackageEntity.Folder, i.Token.ToString());

				Args.Callback?.Invoke(e);

				if (e.Cancel)
					continue;

				Folders.Add(new PackageFolder
				{
					Name = i.Name,
					Token = i.Token,
					Parent = i.Parent
				});
			}
		}

		private void CreateStrings()
		{
			var strings = Args.Connection.GetService<IMicroServiceManagementService>().QueryStrings(Args.MicroService);
			var languages = Args.Connection.GetService<ILanguageService>().Query();

			foreach (var i in strings)
			{
				if (!ElementIncluded(i.Element))
					continue;

				Strings.Add(new MicroServiceString
				{
					Element = i.Element,
					Lcid = languages.FirstOrDefault(f => f.Token == i.Language).Lcid,
					Property = i.Property,
					Value = i.Value
				});
			}
		}

		private bool ElementIncluded(Guid element)
		{
			var svc = Args.Connection.GetService<IDiscoveryService>();

			foreach (var i in Configurations)
			{
				if (svc.Find(i.Component, element) != null)
					return true;
			}

			return false;
		}

		private void CreateMicroService()
		{
			var ms = Args.Connection.GetService<IMicroServiceService>().Select(Args.MicroService);

			if (ms == null)
				throw new RuntimeException(SR.ErrMicroServiceNotFound);

			_microService = new PackageMicroService
			{
				MetaData = Args.Connection.GetService<IMicroServiceService>().SelectMeta(ms.Token),
				Name = ms.Name,
				Template = ms.Template,
				Token = ms.Token
			};
		}

		private void CreateDatabases()
		{
			var connections = Configurations.Where(f => f is IConnection);

			foreach (IConnection i in connections)
			{
				var dp = Args.Connection.GetService<IDataProviderService>().Select(i.DataProvider);

				if (dp == null || !dp.SupportsDeploy)
					continue;

				var database = new PackageDatabase();

				Databases.Add(database);
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
				var e = new PackageProcessArgs(PackageEntity.DatabaseRoutine, string.Format("{0}.{1}", i.Schema, i.Name));

				Args.Callback?.Invoke(e);

				if (e.Cancel)
					continue;

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
				var e = new PackageProcessArgs(PackageEntity.DatabaseView, string.Format("{0}.{1}", i.Schema, i.Name));

				Args.Callback?.Invoke(e);

				if (e.Cancel)
					continue;

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
				var e = new PackageProcessArgs(PackageEntity.DatabaseTable, string.Format("{0}.{1}", i.Schema, i.Name));

				Args.Callback?.Invoke(e);

				if (e.Cancel)
					continue;

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
							Name = j.Name,
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
