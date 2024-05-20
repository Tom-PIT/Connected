using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Http;

using Newtonsoft.Json.Linq;

using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Serialization;

namespace TomPIT.IoT.Hubs
{
	internal abstract class IoTProcessor : IDisposable
	{
		private ConfigurationDescriptor<IIoTHubConfiguration> _descriptor = null;
		private IMiddlewareComponent _middleware = null;
		private Type _schemaType = null;
		private object _schema = null;
		private IIoTDeviceMiddleware _device = null;
		public string DeviceName { get; protected set; }
		protected JObject Arguments { get; set; }
		private IMicroServiceContext _context;

		private IMicroServiceContext Context => _context ??= MicroServiceContext.FromIdentifier(DeviceName, MiddlewareDescriptor.Current.Tenant);
		public ConfigurationDescriptor<IIoTHubConfiguration> Descriptor
		{
			get
			{
				if (_descriptor == null)
					_descriptor = ComponentDescriptor.IoTHub(Context, DeviceName);

				return _descriptor;
			}
		}

		public IMiddlewareComponent Middleware
		{
			get
			{
				if (_middleware == null)
				{
					var type = Context.Tenant.GetService<ICompilerService>().ResolveType(Descriptor.MicroService.Token, Descriptor.Configuration, Descriptor.ComponentName);
					_middleware = Context.CreateMiddleware<IMiddlewareComponent>(type, Arguments);
				}

				return _middleware;
			}
		}

		public Type SchemaType
		{
			get
			{
				if (_schemaType == null)
				{
					_schemaType = Descriptor.Configuration.IoTHubSchemaType(Context);

					if (_schemaType == null)
						throw new RuntimeException(nameof(IoTServerHub), $"{SR.ErrTypeExpected} ({Device})", LogCategories.IoT);
				}

				return _schemaType;
			}
		}

		public object Schema
		{
			get
			{
				if (_schema == null)
					_schema = Middleware.GetType().GetProperty(nameof(IIoTHubMiddleware<object>.Schema)).GetValue(Middleware);

				return _schema;
			}
		}

		public IIoTDeviceMiddleware Device
		{
			get
			{
				if (_device == null)
					_device = FindDevice(DeviceName);

				return _device;
			}
		}

		protected List<IIoTDeviceMiddleware> Devices => Middleware.GetType().GetProperty(nameof(IIoTHubMiddleware<object>.Devices)).GetValue(Middleware) as List<IIoTDeviceMiddleware>;
		protected List<IIoTTransactionMiddleware> Transactions => Middleware.GetType().GetProperty(nameof(IIoTHubMiddleware<object>.Transactions)).GetValue(Middleware) as List<IIoTTransactionMiddleware>;

		public virtual void Commit()
		{
			foreach (var device in Devices)
				Context.Commit((IMiddlewareOperation)device);
		}

		protected IIoTDeviceMiddleware FindDevice(string name)
		{
			if (Devices == null)
				throw new NotFoundException($"{SR.ErrIoTHubDeviceNotFound} ({name})");

			var deviceName = name.Split('/')[^1];

			foreach (var device in Devices)
			{
				if (string.Compare(device.ToString(), deviceName, true) == 0)
				{
					Serializer.Populate(Schema, device);
					return device;
				}
			}

			throw new NotFoundException($"{SR.ErrIoTHubDeviceNotFound} ({name})");
		}

		protected IIoTTransactionMiddleware FindTransaction(string name)
		{
			if (Transactions == null)
				throw new NotFoundException($"{SR.ErrIoTTransactionNotAllowed} ({name})");

			if (Device.Transactions != null)
			{
				foreach (var deviceTransaction in Device.Transactions)
				{
					if (string.Compare(deviceTransaction.ToString(), name, true) == 0)
						return deviceTransaction;
				}
			}

			foreach (var transaction in Transactions)
			{
				if (string.Compare(transaction.ToString(), name, true) == 0)
					return transaction;
			}

			throw new NotFoundException($"{SR.ErrIoTTransactionNotAllowed} ({name})");
		}

		public void Dispose()
		{
			Middleware.Dispose();
			Context.Dispose();
		}
	}
}