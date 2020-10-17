﻿using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Framework;
using Tests.Framework.Integration;

namespace Tests.Cat.CatAllocation
{
	public class CatAllocationApiTests
		: ApiIntegrationTestBase<ReadOnlyCluster, ICatResponse<CatAllocationRecord>, ICatAllocationRequest, CatAllocationDescriptor,
			CatAllocationRequest>
	{
		public CatAllocationApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => 200;
		protected override HttpMethod HttpMethod => HttpMethod.GET;
		protected override string UrlPath => "/_cat/allocation";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.CatAllocation(),
			(client, f) => client.CatAllocationAsync(),
			(client, r) => client.CatAllocation(r),
			(client, r) => client.CatAllocationAsync(r)
		);

		protected override void ExpectResponse(ICatResponse<CatAllocationRecord> response) =>
			response.Records.Should().NotBeEmpty().And.Contain(a => !string.IsNullOrEmpty(a.Node));
	}
}
