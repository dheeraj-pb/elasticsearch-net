﻿using Elasticsearch.Net;
using Nest6;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Framework;
using Tests.Framework.Integration;

namespace Tests.Cat.CatTasks
{
	public class CatTasksApiTests
		: ApiIntegrationTestBase<ReadOnlyCluster, ICatResponse<CatTasksRecord>, ICatTasksRequest, CatTasksDescriptor, CatTasksRequest>
	{
		public CatTasksApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => 200;
		protected override HttpMethod HttpMethod => HttpMethod.GET;
		protected override string UrlPath => "/_cat/tasks";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.CatTasks(),
			(client, f) => client.CatTasksAsync(),
			(client, r) => client.CatTasks(r),
			(client, r) => client.CatTasksAsync(r)
		);
	}
}
