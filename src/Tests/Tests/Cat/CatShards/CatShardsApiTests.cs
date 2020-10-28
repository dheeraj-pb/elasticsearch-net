﻿using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Framework;
using Tests.Framework.Integration;

namespace Tests.Cat.CatShards
{
	public class CatShardsApiTests
		: ApiIntegrationTestBase<ReadOnlyCluster, ICatResponse<CatShardsRecord>, ICatShardsRequest, CatShardsDescriptor, CatShardsRequest>
	{
		public CatShardsApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => 200;
		protected override HttpMethod HttpMethod => HttpMethod.GET;
		protected override string UrlPath => "/_cat/shards";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.CatShards(),
			(client, f) => client.CatShardsAsync(),
			(client, r) => client.CatShards(r),
			(client, r) => client.CatShardsAsync(r)
		);

		protected override void ExpectResponse(ICatResponse<CatShardsRecord> response) =>
			response.Records.Should().NotBeEmpty().And.Contain(a => !string.IsNullOrEmpty(a.PrimaryOrReplica));
	}
}
