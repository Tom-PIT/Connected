using Microsoft.AspNetCore.Mvc;
using System;
using TomPIT.Sys.Model;
using TomPIT.Sys.Model.Components;

namespace TomPIT.Sys.Controllers;

public class SourceFileController : SysController
{
	[HttpPost]
	public Guid Upload()
	{
		var body = FromBody();

		var type = body.Required<int>("type");
		var primaryKey = body.Optional("primaryKey", string.Empty);
		var microService = body.Optional("microService", Guid.Empty);
		var configuration = body.Optional("configuration", Guid.Empty);
		var fileName = body.Required<string>("fileName");
		var contentType = body.Required<string>("contentType");
		var content = body.Optional("content", string.Empty);
		var token = body.Optional("token", Guid.Empty);
		var version = body.Optional("version", 0);

		if (token == Guid.Empty)
			token = Guid.NewGuid();

		DataModel.SourceFiles.Update(token, type, primaryKey, microService, configuration, fileName, contentType, version, Convert.FromBase64String(content));

		return token;
	}

	[HttpPost]
	public byte[] Download()
	{
		var body = FromBody();

		var microService = body.Required<Guid>("microService");
		var token = body.Required<Guid>("token");
		var type = body.Required<int>("type");

		return DataModel.SourceFiles.Select(microService, token, type);
	}

	[HttpPost]
	public void Delete()
	{
		var body = FromBody();

		var microService = body.Required<Guid>("microService");
		var token = body.Required<Guid>("token");
		var type = body.Required<int>("type");

		DataModel.SourceFiles.Delete(microService, token, type);
	}

	[HttpPost]
	public SourceFile Select()
	{
		var body = FromBody();

		var token = body.Required<Guid>("token");
		var type = body.Required<int>("type");

		return DataModel.SourceFiles.SelectFileInfo(token, type);
	}
}
