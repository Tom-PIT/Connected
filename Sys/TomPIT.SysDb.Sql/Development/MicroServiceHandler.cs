using System;
using System.Collections.Generic;
using System.Linq;
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
			var w = new Writer("tompit.service_del");

			w.CreateParameter("@id", service.GetId());

			w.Execute();
		}

		public void DeleteString(IMicroService microService, Guid element, string property)
		{
			var p = new Writer("tomit.service_string_del");

			p.CreateParameter("@service", microService.GetId());
			p.CreateParameter("@element", element);
			p.CreateParameter("@property", property);

			p.Execute();
		}

		public List<IMicroServiceString> QueryStrings()
		{
			return new Reader<MicroServiceString>("tompit.service_string_que").Execute().ToList<IMicroServiceString>();
		}

		public IMicroServiceString SelectString(IMicroService microService, ILanguage language, Guid element, string property)
		{
			var p = new Reader<MicroServiceString>("tompit.service_string_sel");

			p.CreateParameter("@service", microService.GetId());
			p.CreateParameter("@language", language.GetId());
			p.CreateParameter("@element", element);
			p.CreateParameter("@property", property);

			return p.ExecuteSingleRow();
		}

		public void Insert(Guid token, string name, string url, MicroServiceStatus status, IResourceGroup resourceGroup, Guid template, string meta)
		{
			var p = new Writer("tompit.service_ins");

			p.CreateParameter("@name", name);
			p.CreateParameter("@url", url);
			p.CreateParameter("@status", status);
			p.CreateParameter("@resource_group", resourceGroup.GetId());
			p.CreateParameter("@template", template);
			p.CreateParameter("@token", token);
			p.CreateParameter("@meta", meta);

			p.Execute();
		}

		public List<IMicroService> Query()
		{
			return new Reader<MicroService>("tompit.service_que").Execute().ToList<IMicroService>();
		}

		public void UpdateString(IMicroService microService, ILanguage language, Guid element, string property, string value)
		{
			var p = new Writer("tomit.service_string_mdf");

			p.CreateParameter("@service", microService.GetId());
			p.CreateParameter("@language", language.GetId());
			p.CreateParameter("@element", element);
			p.CreateParameter("@property", property);
			p.CreateParameter("@value", value);

			p.Execute();
		}

		public IMicroService Select(int id)
		{
			var p = new Reader<MicroService>(SelectProcedure);

			p.CreateParameter("@id", id);

			return p.ExecuteSingleRow();
		}

		public IMicroService SelectByUrl(string url)
		{
			var p = new Reader<MicroService>(SelectProcedure);

			p.CreateParameter("@url", url);

			return p.ExecuteSingleRow();

		}

		public IMicroService Select(Guid token)
		{
			var p = new Reader<MicroService>(SelectProcedure);

			p.CreateParameter("@token", token);

			return p.ExecuteSingleRow();
		}

		public IMicroService Select(string name)
		{
			var p = new Reader<MicroService>(SelectProcedure);

			p.CreateParameter("@name", name);

			return p.ExecuteSingleRow();
		}

		public void Update(IMicroService microService, string name, string url, MicroServiceStatus status, Guid template, IResourceGroup resourceGroup)
		{
			var p = new Writer("tompit.service_upd");

			p.CreateParameter("@name", name);
			p.CreateParameter("@url", url);
			p.CreateParameter("@id", microService.GetId());
			p.CreateParameter("@status", status);
			p.CreateParameter("@template", template);
			p.CreateParameter("@resource_group", resourceGroup.GetId());

			p.Execute();
		}

		public void UpdateMeta(IMicroService microService, byte[] meta)
		{
			var w = new Writer("tompit.service_meta_upd");

			w.CreateParameter("@id", microService.GetId());
			w.CreateParameter("@meta", meta);

			w.Execute();
		}

		public byte[] SelectMeta(IMicroService microService)
		{
			var r = new Reader<MicroServiceMeta>("tompit.service_meta_sel");

			r.CreateParameter("@id", microService.GetId());

			return r.ExecuteSingleRow()?.Content;
		}
	}
}
