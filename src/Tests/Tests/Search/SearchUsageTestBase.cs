﻿using System.Threading.Tasks;
using Elastic.Xunit.XunitPlumbing;
using Elasticsearch.Net;
using Nest6;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Domain;
using Tests.Framework;
using Tests.Framework.Integration;

namespace Tests.Search
{
	public abstract class SearchUsageTestBase
		: ApiIntegrationTestBase<ReadOnlyCluster, ISearchResponse<Project>, ISearchRequest, SearchDescriptor<Project>, SearchRequest<Project>>
	{
		protected TermQuery ProjectFilter = new TermQuery
		{
			Field = Infer.Field<Project>(p => p.Type),
			Value = Project.TypeName
		};

		protected object ProjectFilterExpectedJson = new { term = new { type = new { value = Project.TypeName } } };

		protected SearchUsageTestBase(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => 200;
		protected override HttpMethod HttpMethod => HttpMethod.POST;
		protected override string UrlPath => "/project/doc/_search";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.Search<Project>(f),
			(client, f) => client.SearchAsync<Project>(f),
			(client, r) => client.Search<Project>(r),
			(client, r) => client.SearchAsync<Project>(r)
		);

		// https://youtrack.jetbrains.com/issue/RIDER-19912
		[U] protected override Task HitsTheCorrectUrl() => base.HitsTheCorrectUrl();

		[U] protected override Task UsesCorrectHttpMethod() => base.UsesCorrectHttpMethod();

		[U] protected override void SerializesInitializer() => base.SerializesInitializer();

		[U] protected override void SerializesFluent() => base.SerializesFluent();

		[I] public override Task ReturnsExpectedStatusCode() => base.ReturnsExpectedResponse();

		[I] public override Task ReturnsExpectedIsValid() => base.ReturnsExpectedIsValid();

		[I] public override Task ReturnsExpectedResponse() => base.ReturnsExpectedResponse();
	}
}
