using System;
using System.Diagnostics;
using TomPIT.Data;

namespace TomPIT.Diagnostics
{
	public interface ILogEntry : ILongPrimaryKeyRecord
	{
		string Category { get; }
		string Message { get; }
		TraceLevel Level { get; }
		string Source { get; }
		DateTime Created { get; }
		int EventId { get; }
		/// <summary>
		/// It's a microservice from which execution began
		/// </summary>
		Guid MicroService { get; }
		/// <summary>
		/// It's the id of the authority and sould uniquely identify the
		/// component in the component model
		/// </summary>
		string AuthorityId { get; }
		/// <summary>
		/// This is the root authority where execution began. Could be View, Worker
		/// or a similar component
		/// </summary>
		string Authority { get; }
		/// <summary>
		/// It's the currently executing component and part of the component
		/// </summary>
		string ContextAuthority { get; }
		/// <summary>
		/// It's the id of the currently executing authority. This value should uniquely 
		/// identify the component in the component model
		/// </summary>
		string ContextAuthorityId { get; }
		/// <summary>
		/// MicroService for which resource has been requested
		/// </summary>
		/// <remarks>
		/// It is not necessary that MicroService is the same as the context microservice
		/// because inter service calls are perfectly valid.
		/// </remarks>
		Guid ContextMicroService { get; }
		/// <summary>
		/// The context property inside the execution process. 
		/// </summary>
		string ContextProperty { get; }
	}
}
