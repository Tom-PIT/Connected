using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Reflection;

namespace TomPIT.Mcp.Controllers;

[Route("sys/ide")]
public class IdeApiController : Controller
{
	[HttpGet("microservices")]
	public IActionResult Microservices()
	{
		var services = Tenant.GetService<IMicroServiceService>().Query();

		var result = services
			.OrderBy(ms => ms.Name)
			.Select(ms => new
			{
				token = ms.Token,
				name = ms.Name,
				url = ms.Url
			});

		return JsonOk(result);
	}

	[HttpGet("components")]
	public IActionResult Components([FromQuery] string ms)
	{
		if (string.IsNullOrWhiteSpace(ms))
			return BadRequest("ms query parameter is required");

		var microService = Tenant.GetService<IMicroServiceService>().SelectByUrl(ms)
			?? Tenant.GetService<IMicroServiceService>().Select(ms);

		if (microService is null)
			return NotFound();

		var components = Tenant.GetService<IComponentService>().QueryComponents(microService.Token);
		var folders = Tenant.GetService<IComponentService>().QueryFolders(microService.Token);

		var result = new
		{
			components = components
				.OrderBy(c => c.Category)
				.ThenBy(c => c.Name)
				.Select(c => new
				{
					token = c.Token,
					name = c.Name,
					category = c.Category,
					nameSpace = c.NameSpace,
					folder = c.Folder == Guid.Empty ? (Guid?)null : c.Folder
				}),
			folders = folders
				.OrderBy(f => f.Name)
				.Select(f => new
				{
					token = f.Token,
					name = f.Name,
					parent = f.Parent == Guid.Empty ? (Guid?)null : f.Parent
				})
		};

		return JsonOk(result);
	}

	[HttpGet("source")]
	public IActionResult Source([FromQuery] string token, [FromQuery] string? element)
	{
		if (!Guid.TryParse(token, out var componentToken))
			return BadRequest("Invalid token");

		var component = Tenant.GetService<IComponentService>().SelectComponent(componentToken);
		if (component is null)
			return NotFound();

		var ms = Tenant.GetService<IMicroServiceService>().Select(component.MicroService);
		if (ms is null)
			return NotFound();

		var config = Tenant.GetService<IComponentService>().SelectConfiguration(componentToken);
		if (config is null)
			return NotFound();

		var texts = Tenant.GetService<IDiscoveryService>().Configuration.Query<IText>(config);

		if (!string.IsNullOrWhiteSpace(element))
		{
			var target = texts.FirstOrDefault(t =>
				string.Equals(Path.GetFileNameWithoutExtension(t.FileName), element, StringComparison.OrdinalIgnoreCase));

			if (target is null)
				return NotFound();

			return JsonOk(new[]
			{
				new
				{
					elementName = Path.GetFileNameWithoutExtension(target.FileName),
					fileName = target.FileName,
					source = Tenant.GetService<IComponentService>().SelectText(ms.Token, target) ?? string.Empty
				}
			});
		}

		var result = texts.Select(t => new
		{
			elementName = Path.GetFileNameWithoutExtension(t.FileName),
			fileName = t.FileName,
			source = Tenant.GetService<IComponentService>().SelectText(ms.Token, t) ?? string.Empty
		});

		return JsonOk(result);
	}

	private IActionResult JsonOk(object value)
	{
		var json = JsonConvert.SerializeObject(value, new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore,
			Formatting = Formatting.None
		});
		return Content(json, "application/json", Encoding.UTF8);
	}
}
