using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.Events;
using TomPIT.Services;

namespace TomPIT.Application.Apis
{
	[DefaultEvent(nameof(Invoke))]
	[DomElement("TomPIT.Application.Design.Dom.ApiOperationElement, TomPIT.Application.Design")]
	[DomDesigner(DomDesignerAttribute.PermissionsDesigner, Mode = EnvironmentMode.Runtime)]
	public class Operation : Element, IApiOperation
	{
		private IServerEvent _invoke = null;
		private IServerEvent _prepare = null;
		private IServerEvent _commit = null;
		private IServerEvent _rollback = null;
		private OperationProtocolOptions _protocols = null;

		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Name { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name) ? GetType().ShortName() : Name;
		}

		[EventArguments(typeof(OperationPrepareArguments))]
		public IServerEvent Prepare
		{
			get
			{
				if (_prepare == null)
					_prepare = new ServerEvent { Parent = this };

				return _prepare;
			}
		}

		[EventArguments(typeof(OperationInvokeArguments))]
		public IServerEvent Invoke
		{
			get
			{
				if (_invoke == null)
					_invoke = new ServerEvent { Parent = this };

				return _invoke;
			}
		}

		[EventArguments(typeof(OperationTransactionArguments))]
		public IServerEvent Commit
		{
			get
			{
				if (_commit == null)
					_commit = new ServerEvent { Parent = this };

				return _commit;
			}
		}

		[EventArguments(typeof(OperationTransactionArguments))]
		public IServerEvent Rollback
		{
			get
			{
				if (_rollback == null)
					_rollback = new ServerEvent { Parent = this };

				return _rollback;
			}
		}

		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		public IOperationProtocolOptions Protocols
		{
			get
			{
				if (_protocols == null)
					_protocols = new OperationProtocolOptions { Parent = this };

				return _protocols;
			}
		}

		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[DefaultValue(ElementScope.Public)]
		public ElementScope Scope { get; set; } = ElementScope.Public;
	}
}
