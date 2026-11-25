using System;
using System.Collections.Generic;
using System.Reflection;
using TomPIT.ComponentModel.Search;
using TomPIT.Connectivity;
using Xunit;

namespace TomPIT.Search
{
	/// <summary>
	/// Regression tests for <see cref="SearchService"/>: the results of the search node proxy must be consumed
	/// through <see cref="ISearchResults"/>, whichever implementation the proxy returns.
	/// </summary>
	public class SearchServiceTests
	{
		/// <summary>
		/// A local Search node returns its own <see cref="ISearchResults"/> implementation, not the runtime's
		/// <c>SearchResults</c> class. The service used to cast to the runtime class and failed with an
		/// <see cref="InvalidCastException"/> ("Unable to cast ... Catalogs.SearchResults to ... SearchResults").
		/// </summary>
		[Fact]
		public void Search_ResultsOfAnotherImplementation_AreMapped()
		{
			var proxy = new FakeProxy(new ForeignResults { Total = 3, SearchTime = 7 });
			var service = new SearchService(TenantStub.Create(proxy));

			var results = service.Search(null);

			Assert.Equal(3, results.Total);
			Assert.Equal(7, results.SearchTime);
			Assert.Empty(results.Items);
		}

		/// <summary>
		/// The runtime's own <c>SearchResults</c> (what the remote proxy returns) keeps working.
		/// </summary>
		[Fact]
		public void Search_RuntimeResults_AreMapped()
		{
			var proxy = new FakeProxy(new SearchResults());
			var service = new SearchService(TenantStub.Create(proxy));

			var results = service.Search(null);

			Assert.Equal(0, results.Total);
			Assert.Empty(results.Items);
		}

		private sealed class FakeProxy : ISearchNodeProxy
		{
			private readonly ISearchResults _results;

			public FakeProxy(ISearchResults results)
			{
				_results = results;
			}

			public ISearchResults Search(ISearchOptions options) => _results;
		}

		/// <summary>
		/// Stands in for the Search node's results class, which is a different type than the runtime's.
		/// </summary>
		private sealed class ForeignResults : ISearchResults
		{
			public List<ISearchResult> Items { get; } = new();
			public List<ISearchResultMessage> Messages { get; } = new();
			public int Total { get; set; }
			public int SearchTime { get; set; }
		}

		/// <summary>
		/// Tenant that only resolves <see cref="ISearchNodeProxy"/>; every other member is not needed by the test.
		/// </summary>
		public class TenantStub : DispatchProxy
		{
			private ISearchNodeProxy _proxy;

			public static ITenant Create(ISearchNodeProxy proxy)
			{
				var tenant = Create<ITenant, TenantStub>();

				((TenantStub)(object)tenant)._proxy = proxy;

				return tenant;
			}

			protected override object Invoke(MethodInfo targetMethod, object[] args)
			{
				if (targetMethod.Name == nameof(ITenant.GetService) && targetMethod.GetGenericArguments()[0] == typeof(ISearchNodeProxy))
					return _proxy;

				throw new NotSupportedException(targetMethod.Name);
			}
		}
	}
}
