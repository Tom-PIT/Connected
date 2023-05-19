using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace TomPIT.Runtime
{
    public delegate object ServiceActivatorCallback(Type serviceType);
    public class ServiceContainer
    {
        private ConcurrentDictionary<Type, ServiceInstance> _services = null;

        public ServiceContainer(IDependencyInjector di)
        {
            DI = di;
            _services = new ConcurrentDictionary<Type, ServiceInstance>();
        }

        private IDependencyInjector DI { get; }

        public bool Exists(Type contract)
        {
            return _services.TryGetValue(contract, out _);
        }

        public void Register(Type contract, object value)
        {
            if (value == null || contract == null)
                return;

            var sc = new ServiceInstance
            {
                Value = value
            };

            if (!_services.TryAdd(contract, sc))
                throw new Exception(string.Format("{0} ({1})", SR.ServiceRegistered, contract.FullName));
        }

        [DebuggerStepThrough]
        public T Get<T>()
        {
            return Get<T>(true);
        }

        [DebuggerStepThrough]
        public T Get<T>(bool throwException)
        {
            if (!_services.ContainsKey(typeof(T)))
                return default(T);

            var value = _services[typeof(T)];

            if (value.Value is ServiceActivatorCallback)
            {
                lock (value.SyncRoot)
                {
                    if (value.Value is ServiceActivatorCallback)
                    {
                        var cb = value.Value as ServiceActivatorCallback;

                        T instance = (T)cb.Invoke(typeof(T));

                        value.Value = instance;
                    }
                }
            }
            else if (value.Value is string)
            {
                lock (value.SyncRoot)
                {
                    if (value.Value is string)
                    {
                        var t = Type.GetType(value.Value.ToString());

                        if (t == null)
                        {
                            if (throwException)
                                throw new Exception(string.Format("{0} ({1})", SR.ServiceTypeNull, value.Value.ToString()));
                            else
                                return default(T);
                        }

                        object instance = CreateInstance(t);

                        if (instance == null)
                        {
                            if (throwException)
                                throw new Exception(string.Format("{0} ({1})", SR.ServiceTypeNull, value.Value.ToString()));
                            else
                                return default(T);
                        }

                        value.Value = instance;
                    }
                }
            }
            else if (value.Value is Type)
            {
                lock (value.SyncRoot)
                {
                    if (value.Value is Type)
                    {
                        var t = value.Value as Type;
                        object instance = CreateInstance(t);

                        if (instance == null)
                        {
                            if (throwException)
                                throw new Exception(string.Format("{0} ({1})", SR.ServiceTypeNull, value.Value.ToString()));
                            else
                                return default(T);
                        }

                        value.Value = instance;
                    }
                }
            }
            else if (value == null)
                return default(T);

            return (T)value.Value;
        }

        private object CreateInstance(Type type)
        {
            if (DI == null)
                return type.Assembly.CreateInstance(type.FullName);

            var constructors = type.GetConstructors();

            foreach (var i in constructors)
            {
                var pars = i.GetParameters();

                if (pars.Length == 0)
                    continue;

                var arguments = new List<object>();
                bool success = true;

                foreach (var j in pars)
                {
                    if (DI.ResolveParameter(j.ParameterType, out object r))
                        arguments.Add(r);
                    else
                    {
                        success = false;
                        break;
                    }
                }

                if (success)
                    return type.Assembly.CreateInstance(type.FullName, false, System.Reflection.BindingFlags.Default, null, arguments.ToArray(), null, null);
            }

            return type.Assembly.CreateInstance(type.FullName);
        }
    }
}