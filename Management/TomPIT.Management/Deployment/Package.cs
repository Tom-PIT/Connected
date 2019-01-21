using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Analysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.ComponentModel.Features;
using TomPIT.Data.DataProviders;
using TomPIT.Data.DataProviders.Deployment;
using TomPIT.Globalization;
using TomPIT.Storage;

namespace TomPIT.Deployment
{
	public class Package
	{
		[JsonProperty(PropertyName = "metaData")]
		public PackageMetaData MetaData { get; set; }
		[JsonProperty(PropertyName = "microService")]
		public MicroService MicroService { get; set; }
		[JsonProperty(PropertyName = "strings")]
		public List<MicroServiceString> Strings { get; set; }
		[JsonProperty(PropertyName = "features")]
		public List<Feature> Features { get; set; }
		[JsonProperty(PropertyName = "components")]
		public List<Component> Components { get; set; }
		[JsonProperty(PropertyName = "blobs")]
		public List<Blob> Blobs { get; set; }
		[JsonProperty(PropertyName = "dependencies")]
		public List<Dependency> Dependencies { get; set; }
		[JsonProperty(PropertyName = "databases")]
		public List<Database> Databases { get; set; }

		private List<IConfiguration> _configurations = null;

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

			MetaData = e.MetaData;
			Blobs = new List<Blob>();

			CreateMicroService();
			CreateFeatures();
			CreateComponents();
			CreateStrings();
			CreateDependencies();
			CreateDatabases();
		}

		private void CreateDependencies()
		{
			var references = Args.Connection.GetService<IDiscoveryService>().References(Args.MicroService);
			Dependencies = new List<Dependency>();

			if (references == null)
				return;

			foreach (var i in references.MicroServices)
			{
				Dependencies.Add(new Dependency
				{
					Name = i.MicroService
				});
			}
		}

		private void CreateComponents()
		{
			var components = Args.Connection.GetService<IComponentService>().QueryComponents(Args.MicroService);
			Components = new List<Component>();

			foreach (var i in components)
			{
				if (i.Feature != Guid.Empty && Features.FirstOrDefault(f => f.Token == i.Feature) == null)
					continue;

				var e = new PackageProcessArgs(PackageEntity.Component, i.Token.ToString());

				Args.Callback(e);

				if (e.Cancel)
					continue;

				Components.Add(new Component
				{
					Category = i.Category,
					Feature = i.Feature,
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

			Blobs.Add(new Blob
			{
				Content = content != null ? Convert.ToBase64String(content.Content) : string.Empty,
				ContentType = blob.ContentType,
				FileName = blob.FileName,
				PrimaryKey = blob.PrimaryKey,
				Service = blob.MicroService,
				Token = blob.Token,
				Topic = blob.Topic,
				Type = blob.Type,
				Version = blob.Version
			});
		}

		private void CreateFeatures()
		{
			var features = Args.Connection.GetService<IFeatureService>().Query(Args.MicroService);
			Features = new List<Feature>();

			foreach (var i in features)
			{
				var e = new PackageProcessArgs(PackageEntity.Feature, i.Token.ToString());

				Args.Callback(e);

				if (e.Cancel)
					continue;

				Features.Add(new Feature
				{
					Name = i.Name,
					Token = i.Token
				});
			}
		}

		private void CreateStrings()
		{
			var strings = Args.Connection.GetService<IMicroServiceManagementService>().QueryStrings(Args.MicroService);
			var languages = Args.Connection.GetService<ILanguageService>().Query();

			Strings = new List<MicroServiceString>();

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

			MicroService = new MicroService
			{
				Meta = Args.Connection.GetService<IMicroServiceService>().SelectMeta(ms.Token),
				Name = ms.Name,
				Status = (int)ms.Status,
				Template = ms.Template,
				Token = ms.Token
			};
		}

		private void CreateDatabases()
		{
			var connections = Configurations.Where(f => f is IConnection);
			Databases = new List<Database>();

			foreach (IConnection i in connections)
			{
				var dp = Args.Connection.GetService<IDataProviderService>().Select(i.DataProvider);

				if (dp == null || !dp.SupportsDeploy)
					continue;

				var database = new Database();

				Databases.Add(database);
				var db = dp.CreateSchema(i.Value);

				CreateTables(database, db);
				CreateViews(database, db);
				CreateRoutines(database, db);
			}
		}

		private void CreateRoutines(Database database, IDatabase provider)
		{
			database.Routines = new List<IRoutine>();

			foreach (var i in provider.Routines)
			{
				var e = new PackageProcessArgs(PackageEntity.DatabaseRoutine, string.Format("{0}.{1}", i.Schema, i.Name));

				Args.Callback(e);

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

		private void CreateViews(Database database, IDatabase provider)
		{
			database.Views = new List<IView>();

			foreach (var i in provider.Views)
			{
				var e = new PackageProcessArgs(PackageEntity.DatabaseView, string.Format("{0}.{1}", i.Schema, i.Name));

				Args.Callback(e);

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

		private void CreateTables(Database database, IDatabase provider)
		{
			database.Tables = new List<ITable>();

			foreach (var i in provider.Tables)
			{
				var e = new PackageProcessArgs(PackageEntity.DatabaseTable, string.Format("{0}.{1}", i.Schema, i.Name));

				Args.Callback(e);

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
