﻿using System;
using System.Linq;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.Extensions;
using Tests.Framework;
using Tests.Framework.Integration;
using Tests.Framework.ManagedElasticsearch.Clusters;

namespace Tests.XPack.MachineLearning.GetJobs
{
	public class GetJobsApiTests : MachineLearningIntegrationTestBase<IGetJobsResponse, IGetJobsRequest, GetJobsDescriptor, GetJobsRequest>
	{
		public GetJobsApiTests(MachineLearningCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override object ExpectJson => null;
		protected override int ExpectStatusCode => 200;
		protected override Func<GetJobsDescriptor, IGetJobsRequest> Fluent => f => f;
		protected override HttpMethod HttpMethod => HttpMethod.GET;
		protected override GetJobsRequest Initializer => new GetJobsRequest();
		protected override string UrlPath => $"/_xpack/ml/anomaly_detectors";

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values) PutJob(client, callUniqueValue.Value);
		}

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.GetJobs(f),
			(client, f) => client.GetJobsAsync(f),
			(client, r) => client.GetJobs(r),
			(client, r) => client.GetJobsAsync(r)
		);

		protected override void ExpectResponse(IGetJobsResponse response)
		{
			response.ShouldBeValid();
			response.Count.Should().BeGreaterOrEqualTo(1);
			response.Jobs.Count.Should().BeGreaterOrEqualTo(1);

			// "job_version" : "5.5.2"
			response.Jobs.First().JobType.Should().Be("anomaly_detector");
			response.Jobs.First().Description.Should().Be("Lab 1 - Simple example");
			response.Jobs.First().CreateTime.Should().BeBefore(DateTimeOffset.UtcNow);

			response.Jobs.First().AnalysisConfig.Should().NotBeNull();
			response.Jobs.First().AnalysisConfig.BucketSpan.Should().Be(new Time("30m"));
			response.Jobs.First().AnalysisConfig.Latency.Should().Be(new Time("0s"));

			response.Jobs.First().AnalysisConfig.Detectors.Should().NotBeNull();
			response.Jobs.First().AnalysisConfig.Detectors.OfType<SumDetector>().Should().NotBeNull();

			var sumDetector = response.Jobs.First().AnalysisConfig.Detectors.Cast<SumDetector>().First();
			sumDetector.DetectorDescription.Should().Be("sum(total)");
			sumDetector.Function.Should().Be("sum");
			sumDetector.FieldName.Name.Should().Be("total");
			sumDetector.DetectorIndex.Should().Be(0);

			response.Jobs.First().AnalysisConfig.Influencers.Should().BeEmpty();

			response.Jobs.First().DataDescription.TimeField.Name.Should().Be("@timestamp");
			response.Jobs.First().DataDescription.TimeFormat.Should().Be("epoch_ms");

			response.Jobs.First().ModelSnapshotRetentionDays.Should().Be(1);
			response.Jobs.First().ResultsIndexName.Should().Be("shared");
		}
	}

	public class GetJobsWithJobIdApiTests : MachineLearningIntegrationTestBase<IGetJobsResponse, IGetJobsRequest, GetJobsDescriptor, GetJobsRequest>
	{
		public GetJobsWithJobIdApiTests(MachineLearningCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override object ExpectJson => null;
		protected override int ExpectStatusCode => 200;
		protected override Func<GetJobsDescriptor, IGetJobsRequest> Fluent => f => f.JobId(CallIsolatedValue);
		protected override HttpMethod HttpMethod => HttpMethod.GET;
		protected override GetJobsRequest Initializer => new GetJobsRequest(CallIsolatedValue);
		protected override string UrlPath => $"/_xpack/ml/anomaly_detectors/{CallIsolatedValue}";

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values) PutJob(client, callUniqueValue.Value);
		}

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.GetJobs(f),
			(client, f) => client.GetJobsAsync(f),
			(client, r) => client.GetJobs(r),
			(client, r) => client.GetJobsAsync(r)
		);

		protected override void ExpectResponse(IGetJobsResponse response)
		{
			response.ShouldBeValid();
			response.Count.Should().BeGreaterOrEqualTo(1);
			response.Jobs.Count.Should().BeGreaterOrEqualTo(1);

			// "job_version" : "5.5.2"
			response.Jobs.First().JobType.Should().Be("anomaly_detector");
			response.Jobs.First().Description.Should().Be("Lab 1 - Simple example");
			response.Jobs.First().CreateTime.Should().BeBefore(DateTimeOffset.UtcNow);

			response.Jobs.First().AnalysisConfig.Should().NotBeNull();
			response.Jobs.First().AnalysisConfig.BucketSpan.Should().Be(new Time("30m"));
			response.Jobs.First().AnalysisConfig.Latency.Should().Be(new Time("0s"));

			response.Jobs.First().AnalysisConfig.Detectors.Should().NotBeNull();
			response.Jobs.First().AnalysisConfig.Detectors.OfType<SumDetector>().Should().NotBeNull();

			var sumDetector = response.Jobs.First().AnalysisConfig.Detectors.Cast<SumDetector>().First();
			sumDetector.DetectorDescription.Should().Be("sum(total)");
			sumDetector.Function.Should().Be("sum");
			sumDetector.FieldName.Name.Should().Be("total");
			sumDetector.DetectorIndex.Should().Be(0);

			response.Jobs.First().AnalysisConfig.Influencers.Should().BeEmpty();

			response.Jobs.First().DataDescription.TimeField.Name.Should().Be("@timestamp");
			response.Jobs.First().DataDescription.TimeFormat.Should().Be("epoch_ms");

			response.Jobs.First().ModelSnapshotRetentionDays.Should().Be(1);
			response.Jobs.First().ResultsIndexName.Should().Be("shared");
		}
	}
}
