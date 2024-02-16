using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Runtime;
internal class DebugService : IDebugService, IDisposable
{
	private static readonly ConfigurationBindings _binder = new();

	private enum DebugType
	{
		ConfigurationAdded = 1,
		ConfigurationChanged = 2,
		ConfigurationRemoved = 3,
		SourceTextChanged = 4
	}
	public DebugService()
	{
		Queue = new();
		Cancel = new();

		Initialize();

	}

	private void Initialize()
	{
		Shell.Configuration.Bind("debugTarget", _binder);

		if (!string.IsNullOrWhiteSpace(Url) && !string.IsNullOrWhiteSpace(AuthenticationToken))
		{
			new Task(OnFlush, Cancel.Token, TaskCreationOptions.LongRunning).Start();
		}
	}

	private ConcurrentQueue<DebugDescriptor> Queue { get; }
	private CancellationTokenSource Cancel { get; }
	private string Url => _binder.Url ?? string.Empty;
	private string AuthenticationToken => _binder.AuthenticationToken ?? string.Empty;
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
						switch (descriptor.MessageType)
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
							case DebugType.SourceTextChanged:
								MiddlewareDescriptor.Current.Tenant.Post(CreateUrl("SourceTextChanged"), new
								{
									descriptor.MicroService,
									descriptor.Component,
									descriptor.Token,
									descriptor.Type
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
			MessageType = DebugType.ConfigurationAdded,
			Component = component
		});
	}

	public void ConfigurationChanged(Guid component)
	{
		if (!Enabled)
			return;

		Queue.Enqueue(new DebugDescriptor
		{
			MessageType = DebugType.ConfigurationChanged,
			Component = component
		});
	}

	public void ConfigurationRemoved(Guid component)
	{
		if (!Enabled)
			return;

		Queue.Enqueue(new DebugDescriptor
		{
			MessageType = DebugType.ConfigurationRemoved,
			Component = component
		});
	}

	public void SourceTextChanged(Guid microService, Guid component, Guid token, int type)
	{
		if (!Enabled)
			return;

		Queue.Enqueue(new DebugDescriptor
		{
			Component = component,
			MicroService = microService,
			Token = token,
			Type = type,
			MessageType = DebugType.SourceTextChanged
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
		public DebugType MessageType { get; set; }
		public Guid MicroService { get; set; }
		public Guid Component { get; set; }
		public Guid Token { get; set; }
		public int Type { get; set; }
	}

	private class ConfigurationBindings
	{
		public string? Url { get; set; }

		public string? AuthenticationToken { get; set; }
	}

}
