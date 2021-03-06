using System;
using System.Threading.Tasks;
using Elastic.Managed.Ephemeral;
using Elastic.Xunit.XunitPlumbing;
using Nest6;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Framework.EndpointTests.TestState;
using Tests.Framework.Integration;
using Xunit;

namespace Tests.Framework
{
	public abstract class CoordinatedIntegrationTestBase<TCluster>
		: IClusterFixture<TCluster>, IClassFixture<EndpointUsage>
		where TCluster : IEphemeralCluster<EphemeralClusterConfiguration>, INestTestCluster, new()
	{
		private readonly CoordinatedUsage _coordinatedUsage;

		protected CoordinatedIntegrationTestBase(CoordinatedUsage coordinatedUsage) => _coordinatedUsage = coordinatedUsage;

		protected async Task Assert<TResponse>(string name, Action<TResponse> assert)
			where TResponse : class, IResponse
		{
			var lazyResponses = await ExecuteOnceInOrderUntil(name);
			if (lazyResponses == null) throw new Exception($"{name} is defined but it yields no LazyResponses object");

			await AssertOnAllResponses<TResponse>(name, lazyResponses, (v, r) => assert(r));
		}

		protected async Task Assert<TResponse>(string name, Action<string, TResponse> assert)
			where TResponse : class, IResponse
		{
			var lazyResponses = await ExecuteOnceInOrderUntil(name);
			if (lazyResponses == null) throw new Exception($"{name} is defined but it yields no LazyResponses object");

			await AssertOnAllResponses(name, lazyResponses, assert);
		}

		protected async Task AssertRunsToCompletion(string name)
		{
			var lazyResponses = await ExecuteOnceInOrderUntil(name);
			if (lazyResponses == null) throw new Exception($"{name} is defined but it yields no LazyResponses object");
		}

		private async Task AssertOnAllResponses<TResponse>(string name, LazyResponses responses, Action<string, TResponse> assert)
			where TResponse : class, IResponse
		{
			foreach (var (key, value) in await responses)
			{
				if (!(value is TResponse response))
					throw new Exception($"{value.GetType()} is not expected response type {typeof(TResponse)}");

				if (!_coordinatedUsage.MethodIsolatedValues.TryGetValue(key, out var isolatedValue))
					throw new Exception($"{name} is not a request observed and so no call isolated values could be located for it");

				assert(isolatedValue, response);
			}
		}

		private async Task<LazyResponses> ExecuteOnceInOrderUntil(string name)
		{
			if (!_coordinatedUsage.Contains(name)) throw new Exception($"{name} is not a keyed after create response");

			foreach (var lazyResponses in _coordinatedUsage)
			{
				await lazyResponses;
				if (lazyResponses.Name == name) return lazyResponses;
			}
			return null;
		}
	}
}
