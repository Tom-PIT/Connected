using System;
using System.Collections.Immutable;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Globalization;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers
{
	public class XmlKeyController : SysController
	{
		[HttpGet]
		public ImmutableList<XElement> Query()
		{
			return DataModel.XmlKeys.Query();
		}

		[HttpGet]
		public XElement Select(string id)
		{
			return DataModel.XmlKeys.Select(id);
		}

		[HttpPost]
		public void Upsert()
		{
			var body = FromBody();

			var id = body.Required<string>("id");
			var element = body.Required<XElement>("element");
			var friendlyName = body.Required<string>("friendlyName");

			DataModel.XmlKeys.Upsert(id, friendlyName, element);
		}
	}
}
