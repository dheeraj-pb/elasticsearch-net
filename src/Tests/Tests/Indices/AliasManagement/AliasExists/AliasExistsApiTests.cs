﻿using System;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Framework;
using Tests.Framework.Integration;
using static Nest6.Infer;

namespace Tests.Indices.AliasManagement.AliasExists
{
	public class AliasExistsApiTests
		: ApiIntegrationTestBase<WritableCluster, IExistsResponse, IAliasExistsRequest, AliasExistsDescriptor, AliasExistsRequest>
	{
		public AliasExistsApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => 200;

		protected override Func<AliasExistsDescriptor, IAliasExistsRequest> Fluent => d => d;
		protected override HttpMethod HttpMethod => HttpMethod.HEAD;

		protected override AliasExistsRequest Initializer => new AliasExistsRequest(Names(CallIsolatedValue + "-alias"));

		protected override bool SupportsDeserialization => false;
		protected override string UrlPath => $"/_alias/{CallIsolatedValue}-alias";

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var index in values.Values)
				client.CreateIndex(index, c => c
					.Aliases(aa => aa.Alias(index + "-alias"))
				);
		}

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.AliasExists(CallIsolatedValue + "-alias", f),
			(client, f) => client.AliasExistsAsync(CallIsolatedValue + "-alias", f),
			(client, r) => client.AliasExists(r),
			(client, r) => client.AliasExistsAsync(r)
		);

		protected override AliasExistsDescriptor NewDescriptor() => new AliasExistsDescriptor(Names(CallIsolatedValue + "-alias"));

		protected override void ExpectResponse(IExistsResponse response) => response.Exists.Should().BeTrue();
	}

	public class AliasExistsNotFoundApiTests
		: ApiIntegrationTestBase<ReadOnlyCluster, IExistsResponse, IAliasExistsRequest, AliasExistsDescriptor, AliasExistsRequest>

	{
		public AliasExistsNotFoundApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => 404;

		protected override Func<AliasExistsDescriptor, IAliasExistsRequest> Fluent => d => d;
		protected override HttpMethod HttpMethod => HttpMethod.HEAD;

		protected override AliasExistsRequest Initializer => new AliasExistsRequest(Names("unknown-alias"));

		protected override bool SupportsDeserialization => false;
		protected override string UrlPath => $"/_alias/unknown-alias";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.AliasExists("unknown-alias", f),
			(client, f) => client.AliasExistsAsync("unknown-alias", f),
			(client, r) => client.AliasExists(r),
			(client, r) => client.AliasExistsAsync(r)
		);

		protected override AliasExistsDescriptor NewDescriptor() => new AliasExistsDescriptor(Names("unknown-alias"));

		protected override void ExpectResponse(IExistsResponse response)
		{
			response.ServerError.Should().BeNull();
			response.Exists.Should().BeFalse();
		}
	}
}
