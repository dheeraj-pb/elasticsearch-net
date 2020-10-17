﻿using System;
using System.Linq;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.Extensions;
using Tests.Framework;
using Tests.Framework.Integration;
using Tests.Framework.ManagedElasticsearch.Clusters;

namespace Tests.XPack.MachineLearning.GetInfluencers
{
	public class GetInfluencersApiTests
		: MachineLearningIntegrationTestBase<IGetInfluencersResponse, IGetInfluencersRequest, GetInfluencersDescriptor, GetInfluencersRequest>
	{
		public GetInfluencersApiTests(MachineLearningCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override object ExpectJson => null;
		protected override int ExpectStatusCode => 200;
		protected override Func<GetInfluencersDescriptor, IGetInfluencersRequest> Fluent => f => f;
		protected override HttpMethod HttpMethod => HttpMethod.POST;
		protected override GetInfluencersRequest Initializer => new GetInfluencersRequest(CallIsolatedValue);
		protected override string UrlPath => $"/_xpack/ml/anomaly_detectors/{CallIsolatedValue}/results/influencers";

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values)
			{
				PutJob(client, callUniqueValue.Value);
				IndexInfluencer(client, callUniqueValue.Value, new DateTimeOffset(2016, 6, 2, 00, 00, 00, TimeSpan.Zero));
			}
		}

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.GetInfluencers(CallIsolatedValue, f),
			(client, f) => client.GetInfluencersAsync(CallIsolatedValue, f),
			(client, r) => client.GetInfluencers(r),
			(client, r) => client.GetInfluencersAsync(r)
		);

		protected override void ExpectResponse(IGetInfluencersResponse response)
		{
			response.ShouldBeValid();
			response.Influencers.Should().HaveCount(1);
			response.Count.Should().Be(1);

			response.Influencers.First().JobId.Should().Be(CallIsolatedValue);
			response.Influencers.First().ResultType.Should().Be("influencer");
			response.Influencers.First().InfluencerFieldName.Should().Be("foo");
			response.Influencers.First().InfluencerFieldValue.Should().Be("bar");
			response.Influencers.First().InfluencerScore.Should().Be(50);
			response.Influencers.First().InitialInfluencerScore.Should().Be(0);
			response.Influencers.First().Probability.Should().Be(0);
			response.Influencers.First().BucketSpan.Should().Be(1);
			response.Influencers.First().IsInterim.Should().Be(false);
			response.Influencers.First().Timestamp.Should().Be(new DateTimeOffset(2016, 6, 2, 00, 00, 00, TimeSpan.Zero));
		}
	}
}
