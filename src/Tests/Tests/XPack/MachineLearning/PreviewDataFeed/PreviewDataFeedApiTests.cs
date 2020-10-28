﻿using System;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Domain;
using Tests.Framework;
using Tests.Framework.Integration;
using Tests.Framework.ManagedElasticsearch.Clusters;

namespace Tests.XPack.MachineLearning.PreviewDatafeed
{
	//TODO what does an invalid request return here? this API returns a json array for the happy path
	public class PreviewDatafeedApiTests
		: MachineLearningIntegrationTestBase<IPreviewDatafeedResponse<Metric>, IPreviewDatafeedRequest, PreviewDatafeedDescriptor,
			PreviewDatafeedRequest>
	{
		public PreviewDatafeedApiTests(MachineLearningCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override object ExpectJson => null;
		protected override int ExpectStatusCode => 200;
		protected override Func<PreviewDatafeedDescriptor, IPreviewDatafeedRequest> Fluent => f => f;
		protected override HttpMethod HttpMethod => HttpMethod.GET;
		protected override PreviewDatafeedRequest Initializer => new PreviewDatafeedRequest(CallIsolatedValue + "-datafeed");
		protected override bool SupportsDeserialization => false;
		protected override string UrlPath => $"/_xpack/ml/datafeeds/{CallIsolatedValue}-datafeed/_preview";

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values)
			{
				PutJob(client, callUniqueValue.Value);
				PutDatafeed(client, callUniqueValue.Value);
			}
		}

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.PreviewDatafeed<Metric>(CallIsolatedValue + "-datafeed", f),
			(client, f) => client.PreviewDatafeedAsync<Metric>(CallIsolatedValue + "-datafeed", f),
			(client, r) => client.PreviewDatafeed<Metric>(r),
			(client, r) => client.PreviewDatafeedAsync<Metric>(r)
		);

		protected override PreviewDatafeedDescriptor NewDescriptor() => new PreviewDatafeedDescriptor(CallIsolatedValue + "-datafeed");

		protected override void ExpectResponse(IPreviewDatafeedResponse<Metric> response)
		{
			response.IsValid.Should().BeTrue();
			response.Data.Count.Should().BeGreaterThan(0);
		}
	}
}
