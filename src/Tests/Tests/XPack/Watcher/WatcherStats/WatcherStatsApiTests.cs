﻿using System;
using System.Linq;
using Elastic.Xunit.XunitPlumbing;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Framework;
using Tests.Framework.Integration;

namespace Tests.XPack.Watcher.WatcherStats
{
	// can sometimes return an invalid cast exception
	[SkipVersion("<6.7.0", "https://github.com/elastic/elasticsearch/pull/39821")]
	public class WatcherStatsApiTests
		: ApiIntegrationTestBase<XPackCluster, IWatcherStatsResponse, IWatcherStatsRequest, WatcherStatsDescriptor, WatcherStatsRequest>
	{
		public WatcherStatsApiTests(XPackCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;

		protected override object ExpectJson => null;
		protected override int ExpectStatusCode => 200;

		protected override Func<WatcherStatsDescriptor, IWatcherStatsRequest> Fluent => f => f
			.WatcherStatsMetric(WatcherStatsMetric.All);

		protected override HttpMethod HttpMethod => HttpMethod.GET;

		protected override WatcherStatsRequest Initializer => new WatcherStatsRequest(WatcherStatsMetric.All);

		protected override string UrlPath => "/_xpack/watcher/stats/_all";

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values)
			{
				var putWatchResponse = client.PutWatch(callUniqueValue.Value, p => p
					.Active()
					.Input(i => i
						.Simple(s => s
							.Add("payload", new { send = "yes" })
						)
					)
					.Trigger(t => t
						.Schedule(s => s
							.Interval("1s")
						)
					)
					.Actions(a => a
						.Index("test_index", i => i
							.ThrottlePeriod("1s")
							.Index("test-" + CallIsolatedValue)
							.DocType("acknowledgement")
						)
					)
				);

				if (!putWatchResponse.IsValid)
					throw new Exception("Problem setting up PutWatch for integration test");
			}
		}

		protected override void OnBeforeCall(IElasticClient client)
		{
			var executeWatchResponse = client.ExecuteWatch(e => e
				.Id(CallIsolatedValue)
				.TriggerData(tr => tr
					.TriggeredTime("now")
					.ScheduledTime("now")
				)
				.ActionModes(f => f
					.Add("_all", ActionExecutionMode.Execute)
				)
				.RecordExecution()
			);

			if (!executeWatchResponse.IsValid)
				throw new Exception($"Problem with ExecuteWatch for integration test: {executeWatchResponse.DebugInformation}");
		}

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.WatcherStats(f),
			(client, f) => client.WatcherStatsAsync(f),
			(client, r) => client.WatcherStats(r),
			(client, r) => client.WatcherStatsAsync(r)
		);

		protected override void ExpectResponse(IWatcherStatsResponse response)
		{
			response.ClusterName.Should().NotBeNullOrWhiteSpace();
			response.Stats.Should().NotBeEmpty();
			var nodeStats = response.Stats.First();

			nodeStats.WatchCount.Should().BeGreaterThan(0);
			nodeStats.WatcherState.Should().Be(WatcherState.Started);

			nodeStats.ExecutionThreadPool.Should().NotBeNull();

			// TODO: Would be good if we can test these too
			nodeStats.CurrentWatches.Should().NotBeNull();
			nodeStats.QueuedWatches.Should().NotBeNull();
		}
	}
}
