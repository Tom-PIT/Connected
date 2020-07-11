using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.StaticFiles;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Configuration;
using TomPIT.Ide.Dom.ComponentModel;

namespace TomPIT.Ide.Designers
{
	public abstract class FileManagerDesigner : DomDesigner<ReflectionElement>
	{
		private List<string> _extensions = null;
		protected FileManagerDesigner(ReflectionElement element) : base(element)
		{

		}

		public override string View => "~/Views/Ide/Designers/FileManager.cshtml";
		public override object ViewModel => this;

		public IMediaResourcesConfiguration Media => Element.Component as IMediaResourcesConfiguration;

		public string MediaName => Media.ComponentName();

		public List<string> AllowedFileExtensions
		{
			get
			{
				if (_extensions == null)
				{
					var setting = Environment.Context.Tenant.GetService<ISettingService>().GetValue<string>(Guid.Empty, "Allowed file extensions");

					if (setting != null)
					{
						_extensions = new List<string>();
						var tokens = setting.Split(',');

						foreach (var token in tokens)
						{
							var value = token.Trim();

							if (!value.StartsWith("."))
								value = $".{value}";

							_extensions.Add(value);
						}
					}
					else
						_extensions = new FileExtensionContentTypeProvider().Mappings.Keys.ToList();
				}

				return _extensions;
			}
		}
	}
}
