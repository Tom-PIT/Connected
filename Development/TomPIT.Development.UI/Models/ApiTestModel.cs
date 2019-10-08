using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Development.Quality;
using TomPIT.Exceptions;
using TomPIT.Ide.Analysis.Suggestions;
using TomPIT.Models;
using TomPIT.Serialization;

namespace TomPIT.Development.Models
{
	public class ApiTestModel : DevelopmentModel
	{
		private List<IApiTest> _tests = null;
		private List<string> _categories = null;
		private List<string> _operations = null;

		protected override void OnInitializing(ModelInitializeParams p)
		{
			Title = "API Testing console";

			if (Body == null)
				return;

			var api = (string)Body["api"];

			if (api == null)
				return;

			var url = string.Empty;
			var qs = string.Empty;

			if (api.Contains("?"))
			{
				var urlTokens = api.Split('?');

				url = urlTokens[0];
				qs = urlTokens[1];
			}
			else
				url = api;

			var tokens = url.Split('/');
			var ms = string.Empty;

			if (tokens.Length < 3)
				throw new RuntimeException(SR.ErrInvalidQualifier);

			ms = tokens[0];
			Api = tokens[1];
			Operation = tokens[2];

			var svc = Tenant.GetService<IMicroServiceService>().Select(ms);
			MicroService = svc ?? throw new RuntimeException("Api Test", SR.ErrMicroServiceNotFound);

			if (!string.IsNullOrWhiteSpace(qs))
			{
				var items = QueryHelpers.ParseQuery(qs);

				Body = new JObject();

				foreach (var i in items)
					Body.Add(i.Key, i.Value.ToString());
			}
			else
			{
				if (!(Body["body"] is JValue body))
					Body = new JObject();
				else
					Body = Serializer.Deserialize<JObject>(body.Value<string>());
			}
		}

		public JObject Body { get; set; }
		private string Api { get; set; }
		private string Operation { get; set; }

		public object Invoke()
		{
			return Interop.Invoke<object, object>(string.Format("{0}/{1}/{2}", MicroService.Name, Api, Operation), Body);
		}

		private List<IApiTest> Tests
		{
			get
			{
				if (_tests == null)
					_tests = Tenant.GetService<IQualityService>().Query();

				return _tests;
			}
		}

		public void Delete()
		{
			var identifier = Body.Required<Guid>("identifier");

			Tenant.GetService<IQualityService>().Delete(identifier);
		}

		public string SelectBody()
		{
			var identifier = Body.Required<Guid>("identifier");

			return Tenant.GetService<IQualityService>().SelectBody(identifier);
		}

		public List<IApiTest> QueryTests()
		{
			var body = Body.Optional("tags", string.Empty);

			if (body == null)
				return null;

			var r = new List<IApiTest>();
			var tokens = body.Split(',');

			foreach (var i in Tests)
			{
				var testTokens = i.Tags.Split(',');

				foreach (var j in testTokens)
				{
					if (string.IsNullOrWhiteSpace(j))
						continue;

					var testToken = j.Trim().ToLowerInvariant();
					var found = false;

					foreach (var k in tokens)
					{
						if (string.IsNullOrWhiteSpace(k))
							continue;

						var existingToken = k.Trim().ToLowerInvariant();

						if (string.Compare(testToken, existingToken, true) == 0)
						{
							found = true;
							r.Add(i);
							break;
						}
					}

					if (found)
						break;
				}
			}

			if (r.Count > 0)
				r = r.OrderBy(f => f.Title).ToList();

			return r;
		}

		public List<string> TestCategories
		{
			get
			{
				if (_categories == null)
				{
					_categories = new List<string>();

					foreach (var i in Tests)
					{
						var tags = i.Tags.Split(',');

						foreach (var j in tags)
						{
							if (!_categories.Contains(j))
								_categories.Add(j);
						}
					}

					_categories.Sort();
				}

				return _categories;
			}
		}

		public Guid Save()
		{
			var identifier = Body.Optional("identifier", Guid.Empty);
			var title = Body.Required<string>("title");
			var description = Body.Optional("description", string.Empty);
			var tags = Body.Required<string>("tags");
			var api = Body.Required<string>("api");
			var body = Body.Optional("body", string.Empty);

			if (identifier == Guid.Empty)
				return Tenant.GetService<IQualityService>().Insert(title, description, api, body, tags);
			else
			{
				Tenant.GetService<IQualityService>().Update(identifier, title, description, api, body, tags);

				return identifier;
			}
		}

		public static ApiTestModel Create(Controller controller, bool initializing)
		{
			var r = new ApiTestModel
			{
				Body = controller.RequestBody()
			};

			r.Initialize(controller, null, initializing);

			r.Databind();

			return r;
		}

		public List<string> Operations
		{
			get
			{
				if (_operations == null)
				{
					_operations = new List<string>();
					var ms = Tenant.GetService<IMicroServiceService>().Query();

					foreach (var microService in ms)
					{
						var apis = Tenant.GetService<IComponentService>().QueryComponents(microService.Token, ComponentCategories.Api);

						foreach (var api in apis)
						{
							var config = Tenant.GetService<IComponentService>().SelectConfiguration(api.Token) as IApiConfiguration;

							foreach (var operation in config.Operations)
							{
								if (string.IsNullOrWhiteSpace(operation.Name))
									continue;

								_operations.Add(string.Format("{0}/{1}/{2}", microService.Name, api.Name, operation.Name));
							}
						}
					}

					_operations.Sort();
				}

				return _operations;
			}
		}

		public List<ISuggestion> ProvideItems()
		{
			//-----------------------------------------------
			//TODO: implement parameters from manifest
			//-----------------------------------------------

			//var api = Body.Optional("api", string.Empty);

			//if (string.IsNullOrWhiteSpace(api))
			return null;

			//var descriptor = ComponentDescriptor.Api(this, api);

			//descriptor.Validate();

			//return descriptor.Configuration.Operations.FirstOrDefault(f => string.Compare(f.Name, descriptor.Element, true) == 0)?.DiscoverParameters(this);
		}
	}
}
