﻿using System;
using System.Linq;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.Extensions;
using Tests.Framework;
using Tests.Framework.Integration;
using Tests.Framework.ManagedElasticsearch.Clusters;

namespace Tests.XPack.MachineLearning.GetBuckets
{
	public class GetBucketsApiTests
		: MachineLearningIntegrationTestBase<IGetBucketsResponse, IGetBucketsRequest, GetBucketsDescriptor, GetBucketsRequest>
	{
		public GetBucketsApiTests(MachineLearningCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override object ExpectJson => null;
		protected override int ExpectStatusCode => 200;
		protected override Func<GetBucketsDescriptor, IGetBucketsRequest> Fluent => f => f;
		protected override HttpMethod HttpMethod => HttpMethod.POST;
		protected override GetBucketsRequest Initializer => new GetBucketsRequest(CallIsolatedValue);
		protected override string UrlPath => $"_xpack/ml/anomaly_detectors/{CallIsolatedValue}/results/buckets";

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values)
			{
				PutJob(client, callUniqueValue.Value);
				IndexBucket(client, callUniqueValue.Value, new DateTimeOffset(2016, 6, 2, 00, 00, 00, TimeSpan.Zero));
			}
		}

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.GetBuckets(CallIsolatedValue, f),
			(client, f) => client.GetBucketsAsync(CallIsolatedValue, f),
			(client, r) => client.GetBuckets(r),
			(client, r) => client.GetBucketsAsync(r)
		);

		protected override GetBucketsDescriptor NewDescriptor() => new GetBucketsDescriptor(CallIsolatedValue);

		protected override void ExpectResponse(IGetBucketsResponse response)
		{
			response.ShouldBeValid();
			response.Count.Should().Be(1);
			response.Buckets.Should().HaveCount(1);

			var bucket = response.Buckets.First();
			bucket.ResultType.Should().Be("bucket");
			bucket.AnomalyScore.Should().Be(90);
			bucket.BucketSpan.Should().Be(1);
			bucket.InitialAnomalyScore.Should().Be(0);
			bucket.EventCount.Should().Be(0);
			bucket.IsInterim.Should().Be(true);
			bucket.BucketInfluencers.Should().BeEmpty();
			bucket.ProcessingTimeMilliseconds.Should().Be(0);
			bucket.Timestamp.Should().BeBefore(DateTimeOffset.UtcNow);
		}
	}

	public class GetBucketsWithTimestampApiTests
		: MachineLearningIntegrationTestBase<IGetBucketsResponse, IGetBucketsRequest, GetBucketsDescriptor, GetBucketsRequest>
	{
		public GetBucketsWithTimestampApiTests(MachineLearningCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override object ExpectJson => null;
		protected override int ExpectStatusCode => 200;

		protected override Func<GetBucketsDescriptor, IGetBucketsRequest> Fluent =>
			f => f.Timestamp(new DateTimeOffset(2016, 6, 2, 00, 00, 00, TimeSpan.Zero));

		protected override HttpMethod HttpMethod => HttpMethod.POST;

		protected override GetBucketsRequest Initializer => new GetBucketsRequest(CallIsolatedValue)
		{
			Timestamp = new DateTimeOffset(2016, 6, 2, 00, 00, 00, TimeSpan.Zero)
		};

		protected override string UrlPath => $"_xpack/ml/anomaly_detectors/{CallIsolatedValue}/results/buckets";

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values)
			{
				PutJob(client, callUniqueValue.Value);
				IndexBucket(client, callUniqueValue.Value, new DateTimeOffset(2016, 6, 2, 00, 00, 00, TimeSpan.Zero));
			}
		}

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.GetBuckets(CallIsolatedValue, f),
			(client, f) => client.GetBucketsAsync(CallIsolatedValue, f),
			(client, r) => client.GetBuckets(r),
			(client, r) => client.GetBucketsAsync(r)
		);

		protected override GetBucketsDescriptor NewDescriptor() => new GetBucketsDescriptor(CallIsolatedValue);

		protected override void ExpectResponse(IGetBucketsResponse response)
		{
			response.ShouldBeValid();
			response.Count.Should().Be(1);
			response.Buckets.Should().HaveCount(1);

			var bucket = response.Buckets.First();
			bucket.ResultType.Should().Be("bucket");
			bucket.AnomalyScore.Should().Be(90);
			bucket.BucketSpan.Should().Be(1);
			bucket.InitialAnomalyScore.Should().Be(0);
			bucket.EventCount.Should().Be(0);
			bucket.IsInterim.Should().Be(true);
			bucket.BucketInfluencers.Should().BeEmpty();
			bucket.ProcessingTimeMilliseconds.Should().Be(0);
			bucket.Timestamp.Should().BeBefore(DateTimeOffset.UtcNow);
		}
	}
}
