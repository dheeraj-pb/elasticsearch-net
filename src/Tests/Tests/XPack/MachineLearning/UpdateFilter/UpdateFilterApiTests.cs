﻿using System;
using Elastic.Xunit.XunitPlumbing;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.Extensions;
using Tests.Framework;
using Tests.Framework.Integration;
using Tests.Framework.ManagedElasticsearch.Clusters;
using static Elasticsearch.Net.HttpMethod;

namespace Tests.XPack.MachineLearning.UpdateFilter
{
	[SkipVersion("<6.4.0", "Filter functions for machine learning stabilised in 6.4.0")]
	public class UpdateFilterApiTests : MachineLearningIntegrationTestBase<IUpdateFilterResponse, IUpdateFilterRequest, UpdateFilterDescriptor, UpdateFilterRequest>
	{
		public UpdateFilterApiTests(MachineLearningCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values)
				PutFilter(client, callUniqueValue.Value);
		}

		protected override bool ExpectIsValid => true;

		protected override object ExpectJson => new
		{
			description = "A list of safe domains",
			add_items = new[] { "*.microsoft.com" },
			remove_items = new[] { "*.google.com" }
		};

		protected override int ExpectStatusCode => 200;

		protected override Func<UpdateFilterDescriptor, IUpdateFilterRequest> Fluent => f => f
			.Description("A list of safe domains")
			.AddItems("*.microsoft.com")
			.RemoveItems("*.google.com");

		protected override HttpMethod HttpMethod => POST;

		protected override UpdateFilterRequest Initializer =>
			new UpdateFilterRequest(CallIsolatedValue)
			{
				Description = "A list of safe domains",
				AddItems = new [] { "*.microsoft.com" },
				RemoveItems = new [] { "*.google.com" }
			};

		protected override bool SupportsDeserialization => false;
		protected override string UrlPath => $"_xpack/ml/filters/{CallIsolatedValue}/_update";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.UpdateFilter(CallIsolatedValue, f),
			(client, f) => client.UpdateFilterAsync(CallIsolatedValue, f),
			(client, r) => client.UpdateFilter(r),
			(client, r) => client.UpdateFilterAsync(r)
		);

		protected override UpdateFilterDescriptor NewDescriptor() => new UpdateFilterDescriptor(CallIsolatedValue);

		protected override void ExpectResponse(IUpdateFilterResponse response)
		{
			response.ShouldBeValid();
			response.FilterId.Should().Be(CallIsolatedValue);
			response.Items.Should().NotBeNull()
				.And.HaveCount(2)
				.And.Contain("*.microsoft.com")
				.And.Contain("wikipedia.org");

			response.Description.Should().Be("A list of safe domains");
		}
	}
}
