﻿using System;
using System.Linq;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.Extensions;
using Tests.Framework;
using Tests.Framework.Integration;
using Tests.Framework.ManagedElasticsearch.Clusters;

namespace Tests.XPack.MachineLearning.GetDatafeeds
{
	public class GetDatafeedsApiTests
		: MachineLearningIntegrationTestBase<IGetDatafeedsResponse, IGetDatafeedsRequest, GetDatafeedsDescriptor, GetDatafeedsRequest>
	{
		public GetDatafeedsApiTests(MachineLearningCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override object ExpectJson => null;
		protected override int ExpectStatusCode => 200;
		protected override Func<GetDatafeedsDescriptor, IGetDatafeedsRequest> Fluent => f => f;
		protected override HttpMethod HttpMethod => HttpMethod.GET;
		protected override string UrlPath => $"_xpack/ml/datafeeds";

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values)
			{
				PutJob(client, callUniqueValue.Value);
				PutDatafeed(client, callUniqueValue.Value);
			}
		}

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.GetDatafeeds(f),
			(client, f) => client.GetDatafeedsAsync(f),
			(client, r) => client.GetDatafeeds(r),
			(client, r) => client.GetDatafeedsAsync(r)
		);

		protected override void ExpectResponse(IGetDatafeedsResponse response)
		{
			response.ShouldBeValid();
			response.Count.Should().BeGreaterOrEqualTo(1);

			var firstDatafeed = response.Datafeeds.FirstOrDefault();

			firstDatafeed.Should().NotBeNull();
			firstDatafeed.DatafeedId.Should().NotBeNullOrWhiteSpace();
			firstDatafeed.JobId.Should().NotBeNullOrWhiteSpace();

			firstDatafeed.QueryDelay.Should().NotBeNull("QueryDelay");
			firstDatafeed.QueryDelay.Should().BeGreaterThan(new Time("1nanos"));

			firstDatafeed.Indices.Should().NotBeNull("Indices");
			firstDatafeed.Indices.Should().Be(Nest6.Indices.Parse("server-metrics"));

			firstDatafeed.Types.Should().NotBeNull("Types");
			firstDatafeed.Types.Should().Be(Types.Parse("metric"));

			firstDatafeed.ScrollSize.Should().Be(1000);

			firstDatafeed.ChunkingConfig.Should().NotBeNull();
			firstDatafeed.ChunkingConfig.Mode.Should().Be(ChunkingMode.Auto);

			firstDatafeed.Query.Should().NotBeNull();

			response.ShouldBeValid();
		}
	}

	public class GetDatafeedsWithDatafeedIdApiTests
		: MachineLearningIntegrationTestBase<IGetDatafeedsResponse, IGetDatafeedsRequest, GetDatafeedsDescriptor, GetDatafeedsRequest>
	{
		public GetDatafeedsWithDatafeedIdApiTests(MachineLearningCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override object ExpectJson => null;
		protected override int ExpectStatusCode => 200;
		protected override Func<GetDatafeedsDescriptor, IGetDatafeedsRequest> Fluent => f => f.DatafeedId(CallIsolatedValue + "-datafeed");
		protected override HttpMethod HttpMethod => HttpMethod.GET;
		protected override GetDatafeedsRequest Initializer => new GetDatafeedsRequest(CallIsolatedValue + "-datafeed");
		protected override string UrlPath => $"_xpack/ml/datafeeds/{CallIsolatedValue}-datafeed";

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values)
			{
				PutJob(client, callUniqueValue.Value);
				PutDatafeed(client, callUniqueValue.Value);
			}
		}

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.GetDatafeeds(f),
			(client, f) => client.GetDatafeedsAsync(f),
			(client, r) => client.GetDatafeeds(r),
			(client, r) => client.GetDatafeedsAsync(r)
		);

		protected override void ExpectResponse(IGetDatafeedsResponse response)
		{
			response.ShouldBeValid();
			response.Count.Should().BeGreaterOrEqualTo(1);

			var firstDatafeed = response.Datafeeds.FirstOrDefault();

			firstDatafeed.Should().NotBeNull();
			firstDatafeed.DatafeedId.Should().NotBeNullOrWhiteSpace();
			firstDatafeed.JobId.Should().NotBeNullOrWhiteSpace();

			firstDatafeed.QueryDelay.Should().NotBeNull("QueryDelay");
			firstDatafeed.QueryDelay.Should().BeGreaterThan(new Time("1nanos"));

			firstDatafeed.Indices.Should().NotBeNull("Indices");
			firstDatafeed.Indices.Should().Be(Nest6.Indices.Parse("server-metrics"));

			firstDatafeed.Types.Should().NotBeNull("Types");
			firstDatafeed.Types.Should().Be(Types.Parse("metric"));

			firstDatafeed.ScrollSize.Should().Be(1000);

			firstDatafeed.ChunkingConfig.Should().NotBeNull();
			firstDatafeed.ChunkingConfig.Mode.Should().Be(ChunkingMode.Auto);

			firstDatafeed.Query.Should().NotBeNull();

			response.ShouldBeValid();
		}
	}
}
