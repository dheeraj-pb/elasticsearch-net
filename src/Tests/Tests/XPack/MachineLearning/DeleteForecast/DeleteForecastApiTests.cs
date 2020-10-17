﻿using System;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Framework;
using Tests.Framework.Integration;
using Tests.Framework.ManagedElasticsearch.Clusters;

namespace Tests.XPack.MachineLearning.DeleteForecast
{
	public class DeleteForecastApiTests
		: MachineLearningIntegrationTestBase<IDeleteForecastResponse, IDeleteForecastRequest, DeleteForecastDescriptor, DeleteForecastRequest>
	{
		public DeleteForecastApiTests(MachineLearningCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		private const int BucketSpanSeconds = 3600;

		protected override bool ExpectIsValid => true;
		protected override object ExpectJson => null;
		protected override int ExpectStatusCode => 200;
		protected override Func<DeleteForecastDescriptor, IDeleteForecastRequest> Fluent => f => f;
		protected override HttpMethod HttpMethod => HttpMethod.DELETE;
		protected override DeleteForecastRequest Initializer => new DeleteForecastRequest(CallIsolatedValue + "-job", CallIsolatedValue);
		protected override string UrlPath => $"_xpack/ml/anomaly_detectors/{CallIsolatedValue + "-job"}/_forecast/{CallIsolatedValue}";

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values)
			{
				var putJobResponse = client.PutJob<object>(callUniqueValue.Value + "-job", f => f
					.Description("DeleteForecastApiTests")
					.AnalysisConfig(a => a
						.BucketSpan($"{BucketSpanSeconds}s")
						.Detectors(d => d
							.Mean(m => m
								.FieldName("value")
							)
						)
					)
					.DataDescription(d => d
						.TimeFormat("epoch")
					)
				);

				if (!putJobResponse.IsValid)
					throw new Exception($"Problem putting job {callUniqueValue.Value} for integration test: {putJobResponse.DebugInformation}");

				OpenJob(client, callUniqueValue.Value + "-job");
				IndexForecast(client, callUniqueValue.Value + "-job", callUniqueValue.Value);
			}
		}

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.DeleteForecast(CallIsolatedValue + "-job", CallIsolatedValue),
			(client, f) => client.DeleteForecastAsync(CallIsolatedValue + "-job", CallIsolatedValue),
			(client, r) => client.DeleteForecast(r),
			(client, r) => client.DeleteForecastAsync(r)
		);

		protected override DeleteForecastDescriptor NewDescriptor() => new DeleteForecastDescriptor(CallIsolatedValue + "-job", CallIsolatedValue);

		protected override void ExpectResponse(IDeleteForecastResponse response) => response.Acknowledged.Should().BeTrue();
	}
}
