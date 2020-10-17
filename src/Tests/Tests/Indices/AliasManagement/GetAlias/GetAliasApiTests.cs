﻿using System;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Configuration;
using Tests.Core.Extensions;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Core.ManagedElasticsearch.NodeSeeders;
using Tests.Domain;
using Tests.Framework;
using Tests.Framework.Integration;
using static Nest6.Infer;

namespace Tests.Indices.AliasManagement.GetAlias
{
	public class GetAliasApiTests : ApiIntegrationTestBase<ReadOnlyCluster, IGetAliasResponse, IGetAliasRequest, GetAliasDescriptor, GetAliasRequest>
	{
		private static readonly Names Names = Names(DefaultSeeder.ProjectsAliasName);

		public GetAliasApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => 200;

		protected override Func<GetAliasDescriptor, IGetAliasRequest> Fluent => d => d
			.AllIndices()
			.Name(Names);

		protected override HttpMethod HttpMethod => HttpMethod.GET;
		protected override GetAliasRequest Initializer => new GetAliasRequest(Nest.Indices.All, Names);
		protected override bool SupportsDeserialization => false;
		protected override string UrlPath => $"_all/_alias/{DefaultSeeder.ProjectsAliasName}";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.GetAlias(f),
			(client, f) => client.GetAliasAsync(f),
			(client, r) => client.GetAlias(r),
			(client, r) => client.GetAliasAsync(r)
		);

		protected override void ExpectResponse(IGetAliasResponse response)
		{
			response.Indices.Should().NotBeEmpty($"expect to find indices pointing to {DefaultSeeder.ProjectsAliasName}");
			var indexAliases = response.Indices[Index<Project>()];
			indexAliases.Should().NotBeNull("expect to find alias for project");
			indexAliases.Aliases.Should().NotBeEmpty("expect to find aliases dictionary definitions for project");
			var alias = indexAliases.Aliases[DefaultSeeder.ProjectsAliasName];
			alias.Should().NotBeNull();
		}
	}

	public class GetAliasPartialMatchApiTests
		: ApiIntegrationTestBase<ReadOnlyCluster, IGetAliasResponse, IGetAliasRequest, GetAliasDescriptor, GetAliasRequest>
	{
		private static readonly Names Names = Names(DefaultSeeder.ProjectsAliasName, "x", "y");

		public GetAliasPartialMatchApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => TestConfiguration.Instance.InRange("<5.5.0") ? 200 : 404;

		protected override Func<GetAliasDescriptor, IGetAliasRequest> Fluent => d => d
			.AllIndices()
			.Name(Names);

		protected override HttpMethod HttpMethod => HttpMethod.GET;
		protected override GetAliasRequest Initializer => new GetAliasRequest(Nest.Indices.All, Names);
		protected override bool SupportsDeserialization => false;
		protected override string UrlPath => $"_all/_alias/{DefaultSeeder.ProjectsAliasName}%2Cx%2Cy";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.GetAlias(f),
			(client, f) => client.GetAliasAsync(f),
			(client, r) => client.GetAlias(r),
			(client, r) => client.GetAliasAsync(r)
		);

		protected override void ExpectResponse(IGetAliasResponse response)
		{
			response.Indices.Should().NotBeNull();
			response.Indices.Count.Should().BeGreaterThan(0);
		}
	}

	public class GetAliasNotFoundApiTests
		: ApiIntegrationTestBase<ReadOnlyCluster, IGetAliasResponse, IGetAliasRequest, GetAliasDescriptor, GetAliasRequest>
	{
		private static readonly Names Names = Names("bad-alias");

		public GetAliasNotFoundApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => false;
		protected override int ExpectStatusCode => 404;

		protected override Func<GetAliasDescriptor, IGetAliasRequest> Fluent => d => d
			.Name(Names);

		protected override HttpMethod HttpMethod => HttpMethod.GET;
		protected override GetAliasRequest Initializer => new GetAliasRequest(Names);
		protected override bool SupportsDeserialization => false;
		protected override string UrlPath => $"/_alias/bad-alias";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.GetAlias(f),
			(client, f) => client.GetAliasAsync(f),
			(client, r) => client.GetAlias(r),
			(client, r) => client.GetAliasAsync(r)
		);

		protected override void ExpectResponse(IGetAliasResponse response)
		{
			response.ServerError.Should().NotBeNull();
			response.ServerError.Error.Reason.Should().Contain("missing");
			response.Indices.Should().NotBeNull();
		}
	}
}
