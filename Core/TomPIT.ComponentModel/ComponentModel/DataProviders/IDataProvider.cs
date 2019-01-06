using System;
using System.Data;
using Newtonsoft.Json.Linq;
using TomPIT.Deployment;

namespace TomPIT.ComponentModel.DataProviders
{
	/// <summary>
	/// Defines contract for implementing DataProvider which interacts with physical
	/// data storages.
	/// </summary>
	public interface IDataProvider
	{
		/// <summary>
		/// Returns records based on specified criteria.
		/// </summary>
		/// <param name="command">This parameter contains data needed for connecting
		/// and querying physical data source.</param>
		/// <param name="schema">Preferred schema that should be used when returning
		/// results. Implementators should use this parameter for resolving
		/// which data fields they should return as part of the result set.</param>
		/// <returns></returns>
		JObject Query(IDataCommandDescriptor command, DataTable schema);
		/// <summary>
		/// Executes transaction command on the physical data storage.
		/// </summary>
		/// <param name="command">This parameter contains data needed for connecting
		/// and executing command on the physical data source.</param>
		/// <param name="connection">This parameter can be null. If passed non null
		/// reference implementators should use this connection instead of opening 
		/// a new one.</param>
		void Execute(IDataCommandDescriptor command, IDataConnection connection);
		/// <summary>
		/// Id of the DataProvider which is used by data source and transaction
		/// components to referring to the specific provider.
		/// </summary>
		Guid Id { get; }
		/// <summary>
		/// A name of the DataProvider. Serves only for display purposes when selecting
		/// DataProvider on the DataSource and Transaction components.
		/// </summary>
		string Name { get; }
		/// <summary>
		/// This method should create a new physical connection to the data source and can
		/// be reused multiple times inside Api calls.
		/// </summary>
		/// <param name="connectionString">The actual connection string of the data source 
		/// to connect to.</param>
		/// <returns></returns>
		IDataConnection OpenConnection(string connectionString);

		IDeploymentDatabase CreateDatabasePackage(string connectionString);
		void DeployPackage(string connectionString, IDeploymentDatabase package);
	}
}