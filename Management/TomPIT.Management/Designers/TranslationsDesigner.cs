using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.ActionResults;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Design;
using TomPIT.Designers;
using TomPIT.Dom;
using TomPIT.Environment;
using TomPIT.Globalization;

namespace TomPIT.Management.Designers
{
	public class TranslationsDesigner : DomDesigner<TomPIT.Dom.Element>
	{
		private List<ILanguage> _languages = null;
		public TranslationsDesigner(TomPIT.Dom.Element element) : base(element)
		{
		}

		public override object ViewModel => this;
		public override string View => "~/Views/Ide/Designers/Translations.cshtml";

		private Guid Leading { get; set; } = Guid.Empty;
		private Guid Translation { get; set; } = Guid.Empty;
		private bool NonLocalized = false;

		public JArray Items
		{
			get
			{
				var r = new JArray();
				var resourceGroups = GetService<IResourceGroupService>().Query().Select(f=>f.Name);
				var translationlanguage = Translation == Guid.Empty ? null : GetService<ILanguageService>().Select(Translation);
				var lcid = translationlanguage == null ? 0 : translationlanguage.Lcid;
				var leadingLcid = 0;

				if(Leading != Guid.Empty)
				{
					var leadinglanguage = GetService<ILanguageService>().Select(Leading);

					if (leadinglanguage != null)
						leadingLcid = leadinglanguage.Lcid;
				}

				foreach(var resourceGroup in resourceGroups)
				{
					var stringTables = GetService<IComponentService>().QueryConfigurations(resourceGroups.ToList(), "StringTable");

					foreach (var stringTable in stringTables)
					{
						var microService = stringTable.MicroService(Connection);
						var ms = GetService<IMicroServiceService>().Select(microService);
						var componentName = stringTable.ComponentName(Connection);

						if (!(stringTable is IStringTable table))
							continue;

						foreach (var str in table.Strings)
						{
							if (!str.IsLocalizable)
								continue;

							var defaultValue = str.DefaultValue;
							var translatedValue = string.Empty;

							if (leadingLcid > 0)
							{
								var leadingTranslation = str.Translations.FirstOrDefault(f => f.Lcid == leadingLcid);

								if (leadingTranslation != null)
									defaultValue = leadingTranslation.Value;
							}

							if(lcid > 0)
							{
								var translation = str.Translations.FirstOrDefault(f => f.Lcid == lcid);

								if (NonLocalized && translation != null && !string.IsNullOrEmpty(translation.Value))
									continue;

								if (translation != null)
									translatedValue = translation.Value;
							}

							r.Add(new JObject
							{
								{"microService", ms.Name },
								{"component", componentName },
								{ "id", str.Id.ToString() },
								{ "key", str.Key },
								{"defaultValue", defaultValue },
								{"translatedValue", translatedValue }
							});
						}
					}
				}

				return r;
			}
		}

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "update", true) == 0)
				return Update(data);
			else if (string.Compare(action, "data", true) == 0)
				return Data(data);

			return base.OnAction(data, action);
		}

		private IDesignerActionResult Update(JObject data)
		{
			var ms = data.Required<string>("microService");
			var key = data.Required<string>("key");
			var component = data.Required<string>("component");
			var language = data.Required<Guid>("translation");
			var value = data.Optional("translatedValue", string.Empty);

			var microService = GetService<IMicroServiceService>().Select(ms);

			if (microService == null)
				throw new RuntimeException(SR.ErrMicroServiceNotFound);

			var config = GetService<IComponentService>().SelectConfiguration(microService.Token, "StringTable", component) as IStringTable;
			var lang = GetService<ILanguageService>().Select(language);

			if (lang == null)
				throw new RuntimeException(SR.ErrLanguageNotFound);

			var str = config.Strings.FirstOrDefault(f => string.Compare(f.Key, key, true) == 0);

			if (str == null)
				throw new RuntimeException(SR.ErrStringResourceNotFound);

			str.UpdateTranslation(lang.Lcid, value);

			GetService<IComponentDevelopmentService>().Update(config);

			return Result.EmptyResult(ViewModel);
		}

		private IDesignerActionResult Data(JObject data)
		{
			Leading = data.Optional("leading", Guid.Empty);
			Translation = data.Optional("translation", Guid.Empty);
			NonLocalized = data.Required<bool>("nonLocalized");

			return Result.JsonResult(ViewModel, Items);
		}

		public List<ILanguage> Languages
		{
			get
			{
				if (_languages == null)
					_languages = GetService<ILanguageService>().Query();

				return _languages;
			}
		}
	}
}
