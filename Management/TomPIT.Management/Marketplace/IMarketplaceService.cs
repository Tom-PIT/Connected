using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TomPIT.Deployment;

namespace TomPIT.Marketplace
{
	public interface IMarketplaceService
	{
		bool IsLogged { get; }
		IPublisher Publisher { get; }
		void LogIn(string userName, string password, bool permanent);
		void LogOut();
		Guid SignUp(string company, string firstName, string lastName, string password, string email, int country, string phone, string website);

		bool IsConfirmed(Guid publisherKey);

		JObject QueryCountries();

		void CreatePackage(Guid microService, string name, string title, string version, PackageScope scope, bool trial, int trialPeriod,
			string description, double price, string tags, string projectUrl, string imageUrl, string licenseUrl, string licenses, PackageProcessHandler processCallback);

		IPackage SelectPackage(Guid microService);
		void PublishPackage(Guid microService);

		List<IPublishedPackage> QueryPublicPackages();
	}
}
