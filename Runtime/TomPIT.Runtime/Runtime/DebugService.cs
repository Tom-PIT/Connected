using System;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Runtime;
internal class DebugService : IDebugService, IDisposable
{
	private enum DebugType
	{
		ConfigurationAdded = 1,
		ConfigurationChanged = 2,
		ConfigurationRemoved = 3,
		ScriptChanged = 4
	}
	public DebugService()
	{
		Queue = new();
		Cancel = new();

		if (Shell.Configuration.RootElement.TryGetProperty("debugTarget", out JsonElement debugTarget))
		{
			if (debugTarget.TryGetProperty("url", out JsonElement urlNode))
				Url = urlNode.GetString();

			if (debugTarget.TryGetProperty("token", out JsonElement tokenNode))
				AuthenticationToken = Encoding.UTF8.GetString(Convert.FromBase64String(tokenNode.GetString()));

			new Task(OnFlush, Cancel.Token, TaskCreationOptions.LongRunning).Start();
		}
	}

	private ConcurrentQueue<DebugDescriptor> Queue { get; }
	private CancellationTokenSource Cancel { get; }
	private string Url { get; }
	private string AuthenticationToken { get; }
	public bool Enabled => !string.IsNullOrEmpty(Url);

	private void OnFlush()
	{
		var token = Cancel.Token;

		while (!token.IsCancellationRequested)
		{
			try
			{
				while (Queue.TryDequeue(out DebugDescriptor descriptor))
				{
					try
					{
						switch (descriptor.Type)
						{
							case DebugType.ConfigurationAdded:
								MiddlewareDescriptor.Current.Tenant.Post(CreateUrl("ConfigurationAdded"), new
								{
									descriptor.Component
								}, new HttpRequestArgs().WithBearerCredentials(AuthenticationToken));

								break;
							case DebugType.ConfigurationChanged:
								MiddlewareDescriptor.Current.Tenant.Post(CreateUrl("ConfigurationChanged"), new
								{
									descriptor.Component
								}, new HttpRequestArgs().WithBearerCredentials(AuthenticationToken));

								break;
							case DebugType.ConfigurationRemoved:
								MiddlewareDescriptor.Current.Tenant.Post(CreateUrl("ConfigurationRemoved"), new
								{
									descriptor.Component
								}, new HttpRequestArgs().WithBearerCredentials(AuthenticationToken));
								break;
							case DebugType.ScriptChanged:
								MiddlewareDescriptor.Current.Tenant.Post(CreateUrl("ScriptChanged"), new
								{
									descriptor.MicroService,
									descriptor.Component,
									descriptor.Element,
									descriptor.Token
								}, new HttpRequestArgs().WithBearerCredentials(AuthenticationToken));

								break;
						}
					}
					catch { }
				}

				token.WaitHandle.WaitOne(TimeSpan.FromSeconds(1));
			}
			catch { }
		}
	}

	public void ConfigurationAdded(Guid component)
	{
		if (!Enabled)
			return;

		Queue.Enqueue(new DebugDescriptor
		{
			Type = DebugType.ConfigurationAdded,
			Component = component
		});
	}

	public void ConfigurationChanged(Guid component)
	{
		if (!Enabled)
			return;

		Queue.Enqueue(new DebugDescriptor
		{
			Type = DebugType.ConfigurationChanged,
			Component = component
		});
	}

	public void ConfigurationRemoved(Guid component)
	{
		if (!Enabled)
			return;

		Queue.Enqueue(new DebugDescriptor
		{
			Type = DebugType.ConfigurationRemoved,
			Component = component
		});
	}

	public void ScriptChanged(Guid microService, Guid component, Guid element, Guid token)
	{
		if (!Enabled)
			return;

		Queue.Enqueue(new DebugDescriptor
		{
			Component = component,
			MicroService = microService,
			Element = element,
			Token = token,
			Type = DebugType.ScriptChanged
		});
	}

	private string CreateUrl(string action)
	{
		return $"{Url}/sys/debug/{action}";
	}

	public void Dispose()
	{
		Cancel.Cancel();
	}

	private class DebugDescriptor
	{
		public DebugType Type { get; set; }
		public Guid MicroService { get; set; }
		public Guid Component { get; set; }
		public Guid Element { get; set; }
		public Guid Token { get; set; }
	}
}
