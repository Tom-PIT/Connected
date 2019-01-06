using TomPIT.ComponentModel.Events;

namespace TomPIT.ComponentModel.Apis
{
	/// <summary>
	/// Each <code>IApiOperation</code> implements business logic which is based on the
	/// input parameters and can return data in any data structure.
	/// </summary>
	public interface IApiOperation : IElement
	{
		/// <summary>
		/// This event serves as a pre invoke stage. If the <code>IApiOperation</code>
		/// implementation triggers <code>IEvent</code> inside this call with a 
		/// <code>ICallback</code> defined, this operation will be called asynchronously
		/// on the <code>Worker</code> server. Its primary purpose is for dependency
		/// <code>IMicroService</code>s to have a chance to cancel the operation, for example
		/// when deleting data. If <code>ICallback</code> is not activated inside this
		/// <code>IEvent</code>, <code>IApiOperation</code> is called synchronously.
		/// </summary>
		IServerEvent Prepare { get; }
		/// <summary>
		/// This is the actual implementation of the <code>IApiOperation</code>.
		/// </summary>
		IServerEvent Invoke { get; }
		/// <summary>
		/// If <code>Saga</code> transaction is activated inside <code>Invoke</code> operation,
		/// this <code>IEvent</code> is triggered in case of successfull <code>Invoke</code> call
		/// and any dependency <code>IApi</code>s calls. The <code>IEvent</code> is called in the
		/// reversed order of the execution path.
		/// </summary>
		IServerEvent Commit { get; }
		/// <summary>
		/// If <code>Saga</code> transaction is activated inside <code>Invoke</code> operation,
		/// this <code>IEvent</code> is triggered is case of failed <code>Invoke</code> call
		/// on primary <code>IApoOperation</code> or any dependent <code>IApi</code>.The <code>IEvent</code> 
		/// is called in the reversed order of the execution path.
		/// </summary>
		IServerEvent Rollback { get; }
		/// <summary>
		/// The name of the operation. This is used as part of the identifier
		/// in the form of <code>MicroService/Api/Operation</code>.
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Access modifier for this operation. This is a second level modifier and stands
		/// behind <code>IApi.Scope</code> property.
		/// </summary>
		ElementScope Scope { get; }
		/// <summary>
		/// Protocol settings used by the <code>IApiOperation</code>.
		/// </summary>
		IOperationProtocolOptions Protocols { get; }
	}
}
