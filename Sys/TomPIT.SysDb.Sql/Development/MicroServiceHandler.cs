﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Data.Sql;
using TomPIT.Environment;
using TomPIT.Globalization;
using TomPIT.SysDb.Development;

namespace TomPIT.SysDb.Sql.Development
{
	public class MicroServiceHandler : IMicroServiceHandler
	{
		private const string SelectProcedure = "tompit.service_sel";

		public void Delete(IMicroService service)
		{
			using var w = new Writer("tompit.service_del");

			w.CreateParameter("@id", service.GetId());

			w.Execute();
		}

		public void DeleteString(IMicroService microService, Guid element, string property)
		{
			using var p = new Writer("tomit.service_string_del");

			p.CreateParameter("@service", microService.GetId());
			p.CreateParameter("@element", element);
			p.CreateParameter("@property", property);

			p.Execute();
		}

		public List<IMicroServiceString> QueryStrings()
		{
			using var r = new Reader<MicroServiceString>("tompit.service_string_que");

			return r.Execute().ToList<IMicroServiceString>();
		}

		public IMicroServiceString SelectString(IMicroService microService, ILanguage language, Guid element, string property)
		{
			using var p = new Reader<MicroServiceString>("tompit.service_string_sel");

			p.CreateParameter("@service", microService.GetId());
			p.CreateParameter("@language", language.GetId());
			p.CreateParameter("@element", element);
			p.CreateParameter("@property", property);

			return p.ExecuteSingleRow();
		}

		public void Insert(Guid token, string name, string url, MicroServiceStatus status, IResourceGroup resourceGroup, Guid template, string meta, string version)
		{
			using var p = new Writer("tompit.service_ins");

			p.CreateParameter("@name", name);
			p.CreateParameter("@url", url);
			p.CreateParameter("@status", status);
			p.CreateParameter("@resource_group", resourceGroup.GetId());
			p.CreateParameter("@template", template);
			p.CreateParameter("@token", token);
			p.CreateParameter("@meta", meta);
			p.CreateParameter("@version", version, true);

			p.Execute();
		}

		public List<IMicroService> Query()
		{
			using var r = new Reader<MicroService>("tompit.service_que");

			return r.Execute().ToList<IMicroService>();
		}

		public void UpdateString(IMicroService microService, ILanguage language, Guid element, string property, string value)
		{
			using var p = new Writer("tomit.service_string_mdf");

			p.CreateParameter("@service", microService.GetId());
			p.CreateParameter("@language", language.GetId());
			p.CreateParameter("@element", element);
			p.CreateParameter("@property", property);
			p.CreateParameter("@value", value);

			p.Execute();
		}

		public IMicroService Select(int id)
		{
			using var p = new Reader<MicroService>(SelectProcedure);

			p.CreateParameter("@id", id);

			return p.ExecuteSingleRow();
		}

		public IMicroService SelectByUrl(string url)
		{
			using var p = new Reader<MicroService>(SelectProcedure);

			p.CreateParameter("@url", url);

			return p.ExecuteSingleRow();

		}

		public IMicroService Select(Guid token)
		{
			using var p = new Reader<MicroService>(SelectProcedure);

			p.CreateParameter("@token", token);

			return p.ExecuteSingleRow();
		}

		public IMicroService Select(string name)
		{
			using var p = new Reader<MicroService>(SelectProcedure);

			p.CreateParameter("@name", name);

			return p.ExecuteSingleRow();
		}

		public void Update(IMicroService microService, string name, string url, MicroServiceStatus status, Guid template,
			 IResourceGroup resourceGroup, Guid package, Guid plan, UpdateStatus updateStatus, CommitStatus commitStatus)
		{
			using var p = new Writer("tompit.service_upd");

			p.CreateParameter("@name", name);
			p.CreateParameter("@url", url);
			p.CreateParameter("@id", microService.GetId());
			p.CreateParameter("@status", status);
			p.CreateParameter("@template", template);
			p.CreateParameter("@resource_group", resourceGroup.GetId());
			p.CreateParameter("@package", package, true);
			p.CreateParameter("@plan", plan, true);
			p.CreateParameter("@update_status", updateStatus);
			p.CreateParameter("@commit_status", commitStatus);

			p.Execute();
		}

		public void UpdateMeta(IMicroService microService, byte[] meta)
		{
			using var w = new Writer("tompit.service_meta_upd");

			w.CreateParameter("@id", microService.GetId());
			w.CreateParameter("@meta", meta);

			w.Execute();
		}

		public string SelectMeta(IMicroService microService)
		{
			using var r = new Reader<MicroServiceMeta>("tompit.service_meta_sel");

			r.CreateParameter("@id", microService.GetId());

			return r.ExecuteSingleRow()?.Content;
		}

		public void RestoreStrings(List<IMicroServiceRestoreString> strings)
		{
			var e = new JArray();

			foreach (var i in strings)
			{
				var o = new JObject
					 {
						  {"language", i.Language.GetId().ToString() },
						  {"element", i.Element },
						  {"microService", i.MicroService.GetId().ToString() },
						  {"property", i.Property }
					 };

				if (!string.IsNullOrWhiteSpace(i.Value))
					o.Add("value", i.Value);

				e.Add(o);
			}

			using var w = new Writer("tompit.service_string_restore");

			w.CreateParameter("@items", JsonConvert.SerializeObject(e));

			w.Execute();
		}
	}
}