﻿using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Nest6
{
	public class IdResolver
	{
		private static readonly ConcurrentDictionary<Type, Func<object, string>> IdDelegates = new ConcurrentDictionary<Type, Func<object, string>>();

		private static readonly MethodInfo MakeDelegateMethodInfo =
			typeof(IdResolver).GetMethod(nameof(MakeDelegate), BindingFlags.Static | BindingFlags.NonPublic);

		private readonly IConnectionSettingsValues _connectionSettings;
		private readonly ConcurrentDictionary<Type, Func<object, string>> LocalIdDelegates = new ConcurrentDictionary<Type, Func<object, string>>();

		public IdResolver(IConnectionSettingsValues connectionSettings) => _connectionSettings = connectionSettings;

		private PropertyInfo GetPropertyCaseInsensitive(Type type, string fieldName)
			=> type.GetProperty(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

		internal Func<T, string> CreateIdSelector<T>() where T : class
		{
			Func<T, string> idSelector = Resolve;
			return idSelector;
		}

		internal static Func<object, object> MakeDelegate<T, TReturn>(MethodInfo @get)
		{
			var f = (Func<T, TReturn>)@get.CreateDelegate(typeof(Func<T, TReturn>));
			return t => f((T)t);
		}

		//TODO where T : class prevents boxing on null check.
		public string Resolve<T>(T @object) =>
			_connectionSettings.DefaultDisableIdInference || @object == null ? null : Resolve(@object.GetType(), @object);

		public string Resolve(Type type, object @object)
		{
			if (type == null || @object == null) return null;
			if (_connectionSettings.DefaultDisableIdInference || _connectionSettings.DisableIdInference.Contains(type))
				return null;

			var preferLocal = _connectionSettings.IdProperties.TryGetValue(type, out _);

			if (LocalIdDelegates.TryGetValue(type, out var cachedLookup))
				return cachedLookup(@object);

			if (!preferLocal && IdDelegates.TryGetValue(type, out cachedLookup))
				return cachedLookup(@object);

			var idProperty = GetInferredId(type);
			if (idProperty == null) return null;

			var getMethod = idProperty.GetGetMethod();
			var generic = MakeDelegateMethodInfo.MakeGenericMethod(type, getMethod.ReturnType);
			var func = (Func<object, object>)generic.Invoke(null, new object[] { getMethod });
			cachedLookup = o =>
			{
				var v = func(o);
				return v?.ToString();
			};
			if (preferLocal)
				LocalIdDelegates.TryAdd(type, cachedLookup);
			else
				IdDelegates.TryAdd(type, cachedLookup);
			return cachedLookup(@object);
		}


		private PropertyInfo GetInferredId(Type type)
		{
			// if the type specifies through ElasticAttribute what the id prop is
			// use that no matter what

			_connectionSettings.IdProperties.TryGetValue(type, out var propertyName);
			if (!propertyName.IsNullOrEmpty())
				return GetPropertyCaseInsensitive(type, propertyName);

			var esTypeAtt = ElasticsearchTypeAttribute.From(type);
			propertyName = esTypeAtt?.IdProperty.IsNullOrEmpty() ?? true ? "Id" : esTypeAtt?.IdProperty;

			return GetPropertyCaseInsensitive(type, propertyName);
		}
	}
}
