﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.ComponentModel.Resources;
using TomPIT.Connectivity;
using TomPIT.Data.DataProviders;
using TomPIT.Deployment;
using TomPIT.Deployment.Database;
using TomPIT.Exceptions;
using TomPIT.Globalization;
using TomPIT.Management.ComponentModel;
using TomPIT.Management.Deployment.Packages.Database;
using TomPIT.Reflection;
using TomPIT.Storage;

namespace TomPIT.Management.Deployment.Packages
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

		public void Create(Guid microService, ITenant tenant)
		{
			CreateMicroService(microService, tenant);
			CreateFolders(tenant);
			CreateComponents(tenant);
			CreateStrings(tenant);
			CreateDependencies(tenant);
			CreateDatabases(tenant);
			CreateConfiguration(tenant);
		}

		private void CreateConfiguration(ITenant tenant)
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

		private void CreateDependencies(ITenant tenant)
		{
			var references = tenant.GetService<IDiscoveryService>().MicroServices.References.Select(MicroService.Token);

			if (references == null)
				return;

			foreach (var i in references.MicroServices)
			{
				var ms = tenant.GetService<IMicroServiceService>().Select(i.MicroService);

				Dependencies.Add(new PackageDependency
				{
					Title = ms.Name,
					MicroService = ms.Token,
					Plan = ms.Plan
				});
			}
		}

		private void CreateComponents(ITenant tenant)
		{
			var components = tenant.GetService<IComponentService>().QueryComponents(MicroService.Token);

			foreach (var i in components)
			{
				var config = tenant.GetService<IComponentService>().SelectConfiguration(i.Token);

				if (config == null)
					throw new RuntimeException($"{SR.ErrCannotFindConfiguration} ({i.Name}, {i.Category})");

				Components.Add(new PackageComponent
				{
					Category = i.Category,
					Folder = i.Folder,
					Name = i.Name,
					RuntimeConfiguration = i.RuntimeConfiguration,
					Token = i.Token,
					Type = i.Type,
				});

				Configurations.Add(config);

				if ((string.Compare(i.Category, ComponentCategories.StringTable, true) == 0 || Configuration.RuntimeConfigurationSupported) && i.RuntimeConfiguration != Guid.Empty)
					CreateBlob(tenant, i.RuntimeConfiguration);

				CreateBlob(tenant, i.Token);

				var texts = tenant.GetService<IDiscoveryService>().Configuration.Query<IText>(config);

				foreach (var j in texts)
					CreateBlob(tenant, j.TextBlob);

				var er = tenant.GetService<IDiscoveryService>().Configuration.Query<IExternalResourceElement>(config);

				foreach (var j in er)
				{
					var items = j.QueryResources();

					if (items == null || !items.Any())
						continue;

					foreach (var k in items)
						CreateBlob(tenant, k);
				}
			}
		}

		private void CreateBlob(ITenant tenant, Guid token)
		{
			if (token == Guid.Empty)
				return;

			var blob = tenant.GetService<IStorageService>().Select(token);

			if (blob == null)
				return;

			var content = tenant.GetService<IStorageService>().Download(token);

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

		private void CreateFolders(ITenant tenant)
		{
			var folders = tenant.GetService<IComponentService>().QueryFolders(MicroService.Token);

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

		private void CreateStrings(ITenant tenant)
		{
			var strings = tenant.GetService<IMicroServiceManagementService>().QueryStrings(MicroService.Token);
			var languages = tenant.GetService<ILanguageService>().Query();

			foreach (var i in strings)
			{
				if (!ElementIncluded(tenant, i.Element))
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

		private bool ElementIncluded(ITenant tenant, Guid element)
		{
			var svc = tenant.GetService<IDiscoveryService>();

			foreach (var i in Configurations)
			{
				if (svc.Configuration.Find(i.Component, element) != null)
					return true;
			}

			return false;
		}

		private void CreateMicroService(Guid microService, ITenant tenant)
		{
			var ms = tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				throw new RuntimeException(SR.ErrMicroServiceNotFound);

			_microService = new PackageMicroService
			{
				MetaData = tenant.GetService<IMicroServiceService>().SelectMeta(ms.Token),
				Name = ms.Name,
				Template = ms.Template,
				Token = ms.Token
			};
		}

		private void CreateDatabases(ITenant tenant)
		{
			var connections = Configurations.Where(f => f is IConnectionConfiguration);

			foreach (IConnectionConfiguration i in connections)
			{
				if (i.DataProvider == Guid.Empty)
					continue;

				var dp = tenant.GetService<IDataProviderService>().Select(i.DataProvider);

				if (dp == null)
					continue;

				var database = new PackageDatabase
				{
					Name = i.ComponentName(),
					Connection = i.Component,
					DataProvider = dp.Name,
					DataProviderId = dp.Id
				};

				Databases.Add(database);

				if (!(dp is IDeployDataProvider deploy))
					continue;

				var db = deploy.CreateSchema(i.Value);

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
					var idx = new Database.Index
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
