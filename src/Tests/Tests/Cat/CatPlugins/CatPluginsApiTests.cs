﻿using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Framework;
using Tests.Framework.Integration;

namespace Tests.Cat.CatPlugins
{
	public class CatPluginsApiTests
		: ApiIntegrationTestBase<ReadOnlyCluster, ICatResponse<CatPluginsRecord>, ICatPluginsRequest, CatPluginsDescriptor, CatPluginsRequest>
	{
		public CatPluginsApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => 200;
		protected override HttpMethod HttpMethod => HttpMethod.GET;
		protected override string UrlPath => "/_cat/plugins";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.CatPlugins(),
			(client, f) => client.CatPluginsAsync(),
			(client, r) => client.CatPlugins(r),
			(client, r) => client.CatPluginsAsync(r)
		);

		protected override void ExpectResponse(ICatResponse<CatPluginsRecord> response) => response.Records.Should()
			.NotBeEmpty()
			.And.Contain(a => !string.IsNullOrEmpty(a.Name) && a.Component == "mapper-murmur3");
	}
}
