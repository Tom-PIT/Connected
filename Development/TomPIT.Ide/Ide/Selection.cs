using System;
using TomPIT.Annotations;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Ide
{
	internal class Selection : EnvironmentClient, ISelection
	{
		private PropertyProvider _properties = null;
		private EventProvider _events = null;
		private IDomDesigner _designer = null;
		private bool _designerResolved = false;
		private bool _transactionResolved = false;
		private ITransactionHandler _transaction = null;

		public Selection(IEnvironment environment) : base(environment)
		{
		}

		public IPropertyProvider Properties
		{
			get
			{
				if (_properties == null)
					_properties = new PropertyProvider(Environment);

				return _properties;
			}
		}

		public IEventProvider Events
		{
			get
			{
				if (_events == null)
					_events = new EventProvider(Environment);

				return _events;
			}
		}

		public IDomElement Element { get { return Environment.Selected(); } }

		public string Path { get { return Environment.SelectedPath(); } }
		public string Id { get; set; }
		public IDomDesigner Designer
		{
			get
			{
				if (_designer == null && Element != null)
				{
					if (_designerResolved)
						return _designer;

					_designerResolved = true;

					var current = Element;

					_designer = ResolveDesigner(current);

					while (_designer == null)
					{
						current = current.Parent;

						if (current == null)
							break;

						_designer = ResolveDesigner(current);
					}
				}

				return _designer;
			}
		}

		private IDomDesigner ResolveDesigner(IDomElement element)
		{
			try
			{
				return string.IsNullOrWhiteSpace(Property)
					? element.Designer
					: element.PropertyDesigner(Property);
			}
			catch (IdeException)
			{
				/*
				 * that's probably because property doesn't have designer
				 */
				return element.Designer;
			}
		}

		public ITransactionHandler Transaction
		{
			get
			{
				if (_transaction == null && Element != null)
				{
					if (_transactionResolved)
						return _transaction;

					_transactionResolved = true;

					var current = Element;

					_transaction = current.Transaction;

					while (_transaction == null)
					{
						current = current.Parent;

						if (current == null)
							break;

						_transaction = current.Transaction;
					}

					if (_transaction != null && Section == EnvironmentSection.Designer)
					{
						var cd = _transaction.Element;

						while (cd != null)
						{
							if (cd.Designer != null && cd.Transaction != null)
							{
								_transaction = cd.Transaction;
								break;
							}

							cd = cd.Parent;
						}
					}
				}

				return _transaction;
			}
		}
		private EnvironmentSection Section
		{
			get
			{
				var section = Environment.RequestBody.Optional("section", string.Empty);

				if (string.IsNullOrWhiteSpace(section))
					return EnvironmentSection.None;

				if (Enum.TryParse(section, true, out EnvironmentSection s))
					return s;

				return EnvironmentSection.None;
			}
		}

		public string Property { get { return Environment.RequestBody.Optional("property", string.Empty); } }
	}
}
