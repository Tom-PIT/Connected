using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TomPIT.Middleware;

namespace TomPIT.Data.DataProviders
{
    /// <summary>
    /// Defines contract for implementing DataProvider which interacts with physical
    /// data storages.
    /// </summary>
    public interface IDataProvider : IDisposable
    {
        /// <summary>
        /// Returns records based on specified criteria.
        /// </summary>
        /// <param name="command">This parameter contains data needed for connecting
        /// and querying physical data source.</param>
        /// <returns></returns>
        Task<List<R>> Query<R>(IMiddlewareContext context, IDataCommandDescriptor command);
        Task<List<R>> Query<R>(IMiddlewareContext context, IDataCommandDescriptor command, IDataConnection connection);
        Task<R> Select<R>(IMiddlewareContext context, IDataCommandDescriptor command);
        Task<R> Select<R>(IMiddlewareContext context, IDataCommandDescriptor command, IDataConnection connection);
        /// <summary>
        /// Executes transaction command on the physical data storage.
        /// </summary>
        /// <param name="command">This parameter contains data needed for connecting
        /// and executing command on the physical data source.</param>
        /// <param name="connection">This parameter can be null. If passed non null
        /// reference implementators should use this connection instead of opening 
        /// a new one.</param>
        Task<int> Execute(IMiddlewareContext context, IDataCommandDescriptor command, IDataConnection connection);
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
        Task<IDataConnection> OpenConnection(IMiddlewareContext context, string connectionString, ConnectionBehavior behavior);

        [Obsolete]
        void TestConnection(IMiddlewareContext context, string connectionString);
    }
}