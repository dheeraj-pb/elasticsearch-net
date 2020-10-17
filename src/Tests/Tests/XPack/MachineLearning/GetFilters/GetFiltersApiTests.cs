﻿using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Elastic.Xunit.XunitPlumbing;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.Extensions;
using Tests.Framework;
using Tests.Framework.Integration;
using Tests.Framework.ManagedElasticsearch.Clusters;

namespace Tests.XPack.MachineLearning.GetFilters
{
	[SkipVersion("<6.4.0", "Filter functions for machine learning stabilised in 6.4.0")]
	public class GetFiltersApiTests : MachineLearningIntegrationTestBase<IGetFiltersResponse, IGetFiltersRequest, GetFiltersDescriptor, GetFiltersRequest>
	{
		public GetFiltersApiTests(MachineLearningCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values)
				PutFilter(client, callUniqueValue.Value);
		}

		protected override void IntegrationTeardown(IElasticClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values)
				DeleteFilter(client, callUniqueValue.Value);
		}

		protected override bool ExpectIsValid => true;

		protected override object ExpectJson => null;

		protected override int ExpectStatusCode => 200;

		protected override Func<GetFiltersDescriptor, IGetFiltersRequest> Fluent => f => f.FilterId(CallIsolatedValue);

		protected override HttpMethod HttpMethod => HttpMethod.GET;

		protected override GetFiltersRequest Initializer => new GetFiltersRequest(CallIsolatedValue);

		protected override bool SupportsDeserialization => false;

		protected override string UrlPath => $"_xpack/ml/filters/{CallIsolatedValue}";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.GetFilters(f),
			(client, f) => client.GetFiltersAsync(f),
			(client, r) => client.GetFilters(r),
			(client, r) => client.GetFiltersAsync(r)
		);

		protected override GetFiltersDescriptor NewDescriptor() => new GetFiltersDescriptor().FilterId(CallIsolatedValue);

		protected override void ExpectResponse(IGetFiltersResponse response)
		{
			response.ShouldBeValid();
			response.Filters.Should().NotBeEmpty();
			var filter = response.Filters.First();
			filter.FilterId.Should().NotBeNullOrEmpty();
			filter.Items.Should().NotBeNull()
				.And.HaveCount(2)
				.And.Contain("*.google.com")
				.And.Contain("wikipedia.org");
			filter.Description.Should().Be("A list of safe domains");
		}
	}

	[SkipVersion("<6.4.0", "Filter functions for machine learning stabilised in 6.4.0")]
	public class GetFiltersPagingApiTests : MachineLearningIntegrationTestBase<IGetFiltersResponse, IGetFiltersRequest, GetFiltersDescriptor, GetFiltersRequest>
	{
		public GetFiltersPagingApiTests(MachineLearningCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values)
			{
				for (int i = 0; i < 3; i++)
					PutFilter(client, callUniqueValue.Value + "_" + (i + 1));
			}
		}

		protected override void IntegrationTeardown(IElasticClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values)
				for (var i = 0; i < 3; i++)
					DeleteFilter(client, callUniqueValue.Value + "_" + (i + 1));
		}

		protected override bool ExpectIsValid => true;

		protected override object ExpectJson => null;

		protected override int ExpectStatusCode => 200;

		protected override Func<GetFiltersDescriptor, IGetFiltersRequest> Fluent => f => f
			.Size(10)
			.From(10);

		protected override HttpMethod HttpMethod => HttpMethod.GET;

		protected override GetFiltersRequest Initializer => new GetFiltersRequest
		{
			Size = 10,
			From = 10
		};

		protected override bool SupportsDeserialization => false;

		protected override string UrlPath => $"_xpack/ml/filters?from=10&size=10";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.GetFilters(f),
			(client, f) => client.GetFiltersAsync(f),
			(client, r) => client.GetFilters(r),
			(client, r) => client.GetFiltersAsync(r)
		);

		protected override void ExpectResponse(IGetFiltersResponse response)
		{
			response.ShouldBeValid();
			response.Filters.Should().NotBeEmpty();
			var filter = response.Filters.First();
			filter.FilterId.Should().NotBeNullOrEmpty();
			filter.Items.Should().NotBeNull()
				.And.HaveCount(2)
				.And.Contain("*.google.com")
				.And.Contain("wikipedia.org");
			filter.Description.Should().Be("A list of safe domains");
		}
	}
}
