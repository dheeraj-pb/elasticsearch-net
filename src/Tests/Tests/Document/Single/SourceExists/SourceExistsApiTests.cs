﻿using System;
using Elastic.Xunit.XunitPlumbing;
using Elasticsearch.Net;
using Nest6;
using Tests.Core.Extensions;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Domain;
using Tests.Framework;
using Tests.Framework.Integration;

namespace Tests.Document.Single.SourceExists
{
	[SkipVersion("<5.4.0", "API was documented from 5.4.0 and over")]
	public class SourceExistsApiTests
		: ApiIntegrationTestBase<WritableCluster, IExistsResponse, ISourceExistsRequest, SourceExistsDescriptor<Project>, SourceExistsRequest<Project>
		>
	{
		public SourceExistsApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => 200;
		protected override Func<SourceExistsDescriptor<Project>, ISourceExistsRequest> Fluent => d => d.Routing(Project.Routing);
		protected override HttpMethod HttpMethod => HttpMethod.HEAD;

		protected override SourceExistsRequest<Project> Initializer => new SourceExistsRequest<Project>(CallIsolatedValue)
		{
			Routing = Project.Routing
		};

		protected override bool SupportsDeserialization => false;
		protected override string UrlPath => $"/project/doc/{CallIsolatedValue}/_source?routing={U(Project.Routing)}";

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var id in values.Values)
				Client.Index(Project.Instance, i => i.Id(id));
		}

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.SourceExists<Project>(CallIsolatedValue, f),
			(client, f) => client.SourceExistsAsync<Project>(CallIsolatedValue, f),
			(client, r) => client.SourceExists(r),
			(client, r) => client.SourceExistsAsync(r)
		);

		protected override SourceExistsDescriptor<Project> NewDescriptor() => new SourceExistsDescriptor<Project>(CallIsolatedValue);
	}

	public class SourceExistsNotFoundApiTests
		: ApiIntegrationTestBase<WritableCluster, IExistsResponse, ISourceExistsRequest, SourceExistsDescriptor<Project>, SourceExistsRequest<Project>
		>
	{
		public SourceExistsNotFoundApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => 404;
		protected override Func<SourceExistsDescriptor<Project>, ISourceExistsRequest> Fluent => d => d.Routing(Project.Routing);
		protected override HttpMethod HttpMethod => HttpMethod.HEAD;

		protected override SourceExistsRequest<Project> Initializer => new SourceExistsRequest<Project>(Doc(CallIsolatedValue))
		{
			Routing = Project.Routing
		};

		protected override bool SupportsDeserialization => false;
		protected override string UrlPath => $"/{IndexWithNoSource.Name}/doc/{CallIsolatedValue}/_source?routing={U(Project.Routing)}";

		private static IndexName IndexWithNoSource { get; } = "project-with-no-source";

		private static DocumentPath<Project> Doc(string id) => new DocumentPath<Project>(id).Index(IndexWithNoSource);

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			var index = client.CreateIndex(IndexWithNoSource, i => i
				.Mappings(m => m
					.Map<Project>(mm => mm
						.SourceField(sf => sf.Enabled(false))
					)
				)
			);
			index.ShouldBeValid();

			foreach (var id in values.Values)
				Client.Index(Project.Instance, i => i.Id(id).Index(IndexWithNoSource));
		}

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.SourceExists<Project>(Doc(CallIsolatedValue), f),
			(client, f) => client.SourceExistsAsync<Project>(Doc(CallIsolatedValue), f),
			(client, r) => client.SourceExists(r),
			(client, r) => client.SourceExistsAsync(r)
		);

		protected override SourceExistsDescriptor<Project> NewDescriptor() => new SourceExistsDescriptor<Project>(Doc(CallIsolatedValue));
	}


	public class SourceExistsIndexNotFoundApiTests
		: ApiIntegrationTestBase<WritableCluster, IExistsResponse, ISourceExistsRequest, SourceExistsDescriptor<Project>, SourceExistsRequest<Project>
		>
	{
		public SourceExistsIndexNotFoundApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => 404;
		protected override Func<SourceExistsDescriptor<Project>, ISourceExistsRequest> Fluent => f => null;
		protected override HttpMethod HttpMethod => HttpMethod.HEAD;
		protected override SourceExistsRequest<Project> Initializer => new SourceExistsRequest<Project>(Doc(CallIsolatedValue));
		protected override bool SupportsDeserialization => false;
		protected override string UrlPath => $"/{IndexWithNoSource.Name}/doc/{CallIsolatedValue}/_source";

		private static IndexName IndexWithNoSource { get; } = "source-no-index";

		protected override SourceExistsDescriptor<Project> NewDescriptor() => new SourceExistsDescriptor<Project>(Doc(CallIsolatedValue));

		private static DocumentPath<Project> Doc(string id) => new DocumentPath<Project>(id).Index(IndexWithNoSource);

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.SourceExists<Project>(Doc(CallIsolatedValue), f),
			(client, f) => client.SourceExistsAsync<Project>(Doc(CallIsolatedValue), f),
			(client, r) => client.SourceExists(r),
			(client, r) => client.SourceExistsAsync(r)
		);
	}
}
