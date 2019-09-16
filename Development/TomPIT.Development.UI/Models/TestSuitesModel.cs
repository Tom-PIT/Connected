using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Quality;

namespace TomPIT.Development.Models
{
	public class TestSuitesModel : DevelopmentModel
	{
		private JArray _suites = null;
		private ITestSuiteConfiguration _selection = null;
		private IComponent _component = null;

		public static TestSuitesModel Create(Controller controller, bool initializing)
		{
			var r = new TestSuitesModel
			{
				Body = controller.RequestBody()
			};

			r.Initialize(controller, null, initializing);

			r.Databind();

			return r;
		}

		public JObject Body { get; set; }

		public JArray Suites
		{
			get
			{
				if (_suites == null)
				{
					_suites = new JArray();

					var microServices = Tenant.GetService<IMicroServiceService>().Query();

					foreach (var microService in microServices)
					{
						var msElement = new JObject
						{
							{"key", microService.Name },
							{"token", microService.Token }
						};

						var items = new JArray();

						msElement.Add("items", items);

						var suites = Tenant.GetService<IComponentService>().QueryComponents(microService.Token, "TestSuite");

						if (suites.Count == 0)
							continue;

						foreach (var suite in suites)
							items.Add(new JObject {
								{"text", suite.Name },
								{"value", suite.Token }
							});

						_suites.Add(msElement);
					}
				}

				return _suites;
			}
		}

		public IComponent SelectionComponent
		{
			get
			{
				if (_component == null)
				{
					if (Selection == null)
						return null;

					_component = Tenant.GetService<IComponentService>().SelectComponent(Selection.Component);
				}

				return _component;
			}
		}

		public ITestSuiteConfiguration Selection
		{
			get
			{
				if (_selection == null)
				{
					var suite = Body.Optional("suite", Guid.Empty);

					if (suite == Guid.Empty)
						return _selection;

					_selection = Tenant.GetService<IComponentService>().SelectConfiguration(suite) as ITestSuiteConfiguration;
				}

				return _selection;
			}
		}
	}
}
