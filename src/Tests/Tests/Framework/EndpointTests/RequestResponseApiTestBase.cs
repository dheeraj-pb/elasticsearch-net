﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elastic.Managed.Ephemeral;
using Elastic.Xunit.XunitPlumbing;
using Nest6;
using Tests.Configuration;
using Tests.Core.Client;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Core.Serialization;
using Tests.Framework.Integration;
using Xunit;

namespace Tests.Framework
{
	public abstract class RequestResponseApiTestBase<TCluster, TResponse, TInterface, TDescriptor, TInitializer>
		: ExpectJsonTestBase, IClusterFixture<TCluster>, IClassFixture<EndpointUsage>
		where TCluster : IEphemeralCluster<EphemeralClusterConfiguration>, INestTestCluster, new()
		where TResponse : class, IResponse
		where TInterface : class
		where TDescriptor : class, TInterface
		where TInitializer : class, TInterface
	{
		private readonly EndpointUsage _usage;

		protected RequestResponseApiTestBase(TCluster cluster, EndpointUsage usage) : base(cluster.Client)
		{
			_usage = usage ?? throw new ArgumentNullException(nameof(usage));

			if (cluster == null) throw new ArgumentNullException(nameof(cluster));

			Cluster = cluster;
			Responses = usage.CallOnce(ClientUsage, 0);
			UniqueValues = usage.CallUniqueValues;
		}

		public virtual IElasticClient Client => TestConfiguration.Instance.RunIntegrationTests ? Cluster.Client : TestClient.DefaultInMemoryClient;

		public TCluster Cluster { get; }

		protected string CallIsolatedValue => UniqueValues.Value;
		protected virtual Func<TDescriptor, TInterface> Fluent { get; } = null;
		protected virtual TInitializer Initializer { get; } = null;
		protected bool RanIntegrationSetup => _usage?.CalledSetup ?? false;
		protected LazyResponses Responses { get; }

		protected CallUniqueValues UniqueValues { get; }

		protected static string RandomString() => Guid.NewGuid().ToString("N").Substring(0, 8);

		protected string U(string s) => Uri.EscapeDataString(s);

		protected T ExtendedValue<T>(string key) where T : class => UniqueValues.ExtendedValue<T>(key);

		protected bool TryGetExtendedValue<T>(string key, out T t) where T : class => UniqueValues.TryGetExtendedValue(key, out t);

		protected void ExtendedValue<T>(string key, T value) where T : class => UniqueValues.ExtendedValue(key, value);

		protected virtual TDescriptor NewDescriptor() => Activator.CreateInstance<TDescriptor>();

		protected virtual void IntegrationSetup(IElasticClient client, CallUniqueValues values) { }

		protected virtual void IntegrationTeardown(IElasticClient client, CallUniqueValues values) { }

		protected virtual void OnBeforeCall(IElasticClient client) { }

		protected virtual void OnAfterCall(IElasticClient client) { }

		protected abstract LazyResponses ClientUsage();

		protected LazyResponses Calls(
			Func<IElasticClient, Func<TDescriptor, TInterface>, TResponse> fluent,
			Func<IElasticClient, Func<TDescriptor, TInterface>, Task<TResponse>> fluentAsync,
			Func<IElasticClient, TInitializer, TResponse> request,
			Func<IElasticClient, TInitializer, Task<TResponse>> requestAsync
		) => new LazyResponses(async () =>
		{
			var client = Client;

			void IntegrateOnly(Action<IElasticClient> act)
			{
				if (!TestClient.Configuration.RunIntegrationTests) return;

				act(client);
			}

			if (TestClient.Configuration.RunIntegrationTests)
			{
				IntegrationSetup(client, UniqueValues);
				_usage.CalledSetup = true;
			}

			var dict = new Dictionary<ClientMethod, IResponse>();
			UniqueValues.CurrentView = ClientMethod.Fluent;

			IntegrateOnly(OnBeforeCall);
			dict.Add(ClientMethod.Fluent, fluent(client, Fluent));
			IntegrateOnly(OnAfterCall);

			UniqueValues.CurrentView = ClientMethod.FluentAsync;
			IntegrateOnly(OnBeforeCall);
			dict.Add(ClientMethod.FluentAsync, await fluentAsync(client, Fluent));
			IntegrateOnly(OnAfterCall);

			UniqueValues.CurrentView = ClientMethod.Initializer;
			IntegrateOnly(OnBeforeCall);
			dict.Add(ClientMethod.Initializer, request(client, Initializer));
			IntegrateOnly(OnAfterCall);

			UniqueValues.CurrentView = ClientMethod.InitializerAsync;
			IntegrateOnly(OnBeforeCall);
			dict.Add(ClientMethod.InitializerAsync, await requestAsync(client, Initializer));
			IntegrateOnly(OnAfterCall);

			if (TestClient.Configuration.RunIntegrationTests)
			{
				IntegrationTeardown(client, UniqueValues);
				_usage.CalledTeardown = true;
			}

			return dict;
		});

		protected virtual async Task AssertOnAllResponses(Action<TResponse> assert)
		{
			var responses = await Responses;
			foreach (var kv in responses)
			{
				var response = kv.Value as TResponse;
				try
				{
					UniqueValues.CurrentView = kv.Key;
					assert(response);
				}
#pragma warning disable 7095 //enable this if you expect a single overload to act up
#pragma warning disable 8360
				catch (Exception ex) when (false)
#pragma warning restore 7095
#pragma warning restore 8360
#pragma warning disable 0162 //dead code while the previous exception filter is false
				{
					throw new Exception($"asserting over the response from: {kv.Key} failed: {ex.Message}", ex);
				}
#pragma warning restore 0162
			}
		}
	}
}
