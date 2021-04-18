using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Cdn;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers.Management
{
	public class MailManagementController : SysController
	{
		[HttpPost]
		public void Clear()
		{
			DataModel.Mail.Clear();
		}

		[HttpPost]
		public void Update()
		{
			var body = FromBody();
			var popReceipt = body.Required<Guid>("popReceipt");
			var error = body.Optional("error", string.Empty);
			var delay = body.Optional("delay", 0);

			DataModel.Mail.Response(popReceipt, error, delay);
		}

		[HttpPost]
		public void Delete()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");

			DataModel.Mail.Delete(token);
		}

		[HttpPost]
		public void DeleteByPopReceipt()
		{
			var body = FromBody();
			var popReceipt = body.Required<Guid>("popReceipt");

			DataModel.Mail.DeleteByPopReceipt(popReceipt);
		}

		[HttpPost]
		public List<IMailMessage> Dequeue()
		{
			var body = FromBody();
			var count = body.Required<int>("count");

			return DataModel.Mail.Dequeue(count);
		}

		[HttpGet]
		public List<IMailMessage> Query()
		{
			return DataModel.Mail.Query();
		}

		[HttpPost]
		public void Reset()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");

			DataModel.Mail.Reset(token);
		}

		[HttpPost]
		public IMailMessage Select()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");

			return DataModel.Mail.Select(token);
		}

		[HttpPost]
		public IMailMessage SelectByPopReceipt()
		{
			var body = FromBody();
			var popReceipt = body.Required<Guid>("popReceipt");

			return DataModel.Mail.SelectByPopReceipt(popReceipt);
		}
	}
}
