﻿using System;
using System.Linq;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.Extensions;
using Tests.Framework;
using Tests.Framework.Integration;
using Tests.Framework.ManagedElasticsearch.Clusters;

namespace Tests.XPack.MachineLearning.GetAnomalyRecords
{
	public class GetAnomalyRecordsApiTests
		: MachineLearningIntegrationTestBase<IGetAnomalyRecordsResponse, IGetAnomalyRecordsRequest, GetAnomalyRecordsDescriptor,
			GetAnomalyRecordsRequest>
	{
		public GetAnomalyRecordsApiTests(MachineLearningCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override object ExpectJson => null;
		protected override int ExpectStatusCode => 200;
		protected override Func<GetAnomalyRecordsDescriptor, IGetAnomalyRecordsRequest> Fluent => f => f;
		protected override HttpMethod HttpMethod => HttpMethod.POST;
		protected override GetAnomalyRecordsRequest Initializer => new GetAnomalyRecordsRequest(CallIsolatedValue);
		protected override string UrlPath => $"/_xpack/ml/anomaly_detectors/{CallIsolatedValue}/results/records";

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values)
			{
				PutJob(client, callUniqueValue.Value);
				IndexAnomalyRecord(client, callUniqueValue.Value, new DateTimeOffset(2016, 6, 2, 00, 00, 00, TimeSpan.Zero));
			}
		}

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.GetAnomalyRecords(CallIsolatedValue, f),
			(client, f) => client.GetAnomalyRecordsAsync(CallIsolatedValue, f),
			(client, r) => client.GetAnomalyRecords(r),
			(client, r) => client.GetAnomalyRecordsAsync(r)
		);

		protected override void ExpectResponse(IGetAnomalyRecordsResponse response)
		{
			response.ShouldBeValid();
			response.Count.Should().Be(1);
			response.Records.Should().HaveCount(1);
			response.Records.First().ResultType.Should().Be("record");
			response.Records.First().Probability.Should().Be(0);
			response.Records.First().RecordScore.Should().Be(80);
			response.Records.First().InitialRecordScore.Should().Be(0);
			response.Records.First().BucketSpan.Should().Be(1);
			response.Records.First().DetectorIndex.Should().Be(0);
			response.Records.First().IsInterim.Should().Be(true);
			response.Records.First().Timestamp.Should().BeBefore(DateTimeOffset.UtcNow);
		}
	}
}
