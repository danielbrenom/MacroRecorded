﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MacroRecorded.Services;

public class PluginServiceFactory
{
    private readonly Dictionary<Type, object> _services;

    public PluginServiceFactory()
    {
        _services = new Dictionary<Type, object>();
    }

    public PluginServiceFactory RegisterService<T>() where T : class
    {
        _services.Add(typeof(T), Create<T>());
        return this;
    }

    public PluginServiceFactory RegisterService<T>(T instance) where T : class
    {
        _services.Add(typeof(T), instance);
        return this;
    }

    public T Create<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var service))
            return service as T;
        return Create(typeof(T)) as T;
    }

    private object Create(Type type)
    {
        if (_services.TryGetValue(type, out var service))
            return service;
        var defaultConstructor = type.GetConstructors()[0];
        var defaultParams = defaultConstructor.GetParameters();
        var parameters = defaultParams.Select(param => Create(param.ParameterType)).ToArray();
        return defaultConstructor.Invoke(parameters);
    }

    public void Dispose()
    {
        foreach (var (type, service) in _services)
        {
            try
            {
                var disposeMethod = type.GetMethod("Dispose");
                if (disposeMethod != null && type.GetInterfaceMap(typeof(IDisposable)).TargetMethods.Contains(disposeMethod))
                    ((IDisposable)service).Dispose();
            }
            catch (ArgumentException){}
            catch (AmbiguousMatchException)
            {
                //Non disposable services should not interfere
            }
        }
    }
}