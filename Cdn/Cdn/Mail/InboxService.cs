using System;
using System.ComponentModel.DataAnnotations;
using TomPIT.Caching;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Cdn.Mail
{
	internal class InboxService : SynchronizedClientRepository<Type, Guid>, IInboxService
	{
		public InboxService(ITenant tenant) : base(tenant, "inboxmiddleware")
		{
			Tenant.GetService<IComponentService>().ConfigurationAdded += OnConfigurationAdded;
			Tenant.GetService<IComponentService>().ConfigurationChanged += OnConfigurationChanged;
			Tenant.GetService<IComponentService>().ConfigurationRemoved += OnConfigurationRemoved;
		}

		protected override void OnInitializing()
		{
			var rgs = Shell.GetConfiguration<IClientSys>().ResourceGroups;
			var configurations = Tenant.GetService<IComponentService>().QueryConfigurations(rgs, ComponentCategories.Inbox);

			foreach (var configuration in configurations)
			{
				if (configuration is IInboxConfiguration inbox)
					RegisterMiddleware(inbox);
			}

		}

		protected override void OnInvalidate(Guid id)
		{
			RegisterMiddleware(Tenant.GetService<IComponentService>().SelectConfiguration(id) as IInboxConfiguration);
		}

		private void RegisterMiddleware(IInboxConfiguration configuration)
		{
			if (configuration == null)
				return;

			var type = Tenant.GetService<ICompilerService>().ResolveType(configuration.MicroService(), configuration, configuration.ComponentName(), false);

			if (type == null)
				return;

			Set(configuration.Component, type);
		}

		private void OnConfigurationRemoved(ITenant sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.Inbox, true) != 0)
				return;

			Remove(e.Component);
		}

		private void OnConfigurationChanged(ITenant sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.Inbox, true) != 0)
				return;

			Refresh(e.Component);
		}

		private void OnConfigurationAdded(ITenant sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.Inbox, true) != 0)
				return;

			Refresh(e.Component);
		}

		public InboxMessageResult ProcessMail(string recipientAddress, IInboxMessage message)
		{
			Exception exception = null;

			var hit = false;
			var result = InboxMessageResult.OK;

			foreach (var type in All())
			{
				var ms = Tenant.GetService<ICompilerService>().ResolveMicroService(type);
				var inbox = Tenant.GetService<ICompilerService>().CreateInstance<IInboxMiddleware>(new MicroServiceContext(ms.Token, Tenant.Url), type);

				if (inbox.Addresses == null)
					continue;

				foreach (var address in inbox.Addresses)
				{
					if (string.Compare(address.Address, recipientAddress, true) == 0)
					{
						hit = true;

						try
						{
							result = inbox.Invoke(message);
						}
						catch (Exception ex)
						{
							exception = ex;
							break;
						}
						break;
					}
				}
			}

			if (!hit)
				return InboxMessageResult.MailboxNotFound;

			if (exception is ForbiddenException || exception is UnauthorizedException)
				return InboxMessageResult.AccessDenied;
			else if (exception is NotFoundException || exception is ValidationException)
				return InboxMessageResult.SyntaxErrorInParameters;
			else if (exception != null)
				return InboxMessageResult.Error;

			return result;
		}
	}
}