using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;

namespace TomPIT.Models
{
	public class ApiTestModel : DevelopmentModel
	{
		private List<IApiTest> _tests = null;
		private List<string> _categories = null;

		protected override void OnInitializing(ModelInitializeParams p)
		{
			p.Authority = "ApiTest";

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

			if (tokens.Length == 1)
				return;

			if (tokens.Length == 3)
			{
				ms = tokens[0];
				Api = tokens[1];
				Operation = tokens[2];
			}
			else
			{
				Api = tokens[0];
				Operation = tokens[1];
			}

			if (!string.IsNullOrWhiteSpace(ms))
			{
				var svc = Connection.GetService<IMicroServiceService>().Select(ms);

				if (svc == null)
					throw new RuntimeException("Api Test", SR.ErrMicroServiceNotFound);

				p.ContextId = svc.Token.ToString();
			}
			else
			{
				var component = Connection.GetService<IComponentService>().SelectComponent("Api", tokens[0]);

				if (component == null)
					throw new RuntimeException("Api Test", string.Format("{0} ({1})", SR.ErrComponentNotFound, tokens[0]));

				p.ContextId = component.MicroService.ToString();
			}

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
					Body = JsonConvert.DeserializeObject<JObject>(body.Value<string>());
			}
		}

		public JObject Body { get; set; }
		private string Api { get; set; }
		private string Operation { get; set; }

		public object Invoke()
		{
			var ms = Connection.GetService<IMicroServiceService>().Select(Identity.ContextId.AsGuid());

			var r = Invoke<object>(string.Format("{0}/{1}/{2}", ms.Name, Api, Operation), Body);

			if (r == null)
				return null;

			if (r is JObject)
				return r as JObject;

			var s = JsonConvert.SerializeObject(r);

			return JsonConvert.DeserializeObject(s);

		}

		private List<IApiTest> Tests
		{
			get
			{
				if (_tests == null)
					_tests = Connection.GetService<IQaService>().Query();

				return _tests;
			}
		}

		public void Delete()
		{
			var identifier = Body.Required<Guid>("identifier");

			Connection.GetService<IQaService>().Delete(identifier);
		}

		public string SelectBody()
		{
			var identifier = Body.Required<Guid>("identifier");

			return Connection.GetService<IQaService>().SelectBody(identifier);
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
				return Connection.GetService<IQaService>().Insert(title, description, api, body, tags);
			else
			{
				Connection.GetService<IQaService>().Update(identifier, title, description, api, body, tags);

				return identifier;
			}
		}

		public static ApiTestModel Create(Controller controller, bool initialize)
		{
			var r = new ApiTestModel
			{
				Body = controller.RequestBody()
			};

			if (initialize)
				r.Initialize(controller);

			r.Databind();

			return r;
		}
	}
}
