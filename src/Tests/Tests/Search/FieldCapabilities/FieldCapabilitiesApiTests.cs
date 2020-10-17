﻿using System;
using System.Linq;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.Extensions;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Domain;
using Tests.Framework;
using Tests.Framework.Integration;
using static Nest6.Infer;

namespace Tests.Search.FieldCapabilities
{
	public class FieldCapabilitiesApiTests
		: ApiIntegrationTestBase<ReadOnlyCluster, IFieldCapabilitiesResponse, IFieldCapabilitiesRequest, FieldCapabilitiesDescriptor,
			FieldCapabilitiesRequest>
	{
		public FieldCapabilitiesApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;

		protected override int ExpectStatusCode => 200;

		protected override Func<FieldCapabilitiesDescriptor, IFieldCapabilitiesRequest> Fluent => d => d
			.Fields(Fields<Project>(p => p.Name).And("*"));

		protected override HttpMethod HttpMethod => HttpMethod.GET;

		protected override FieldCapabilitiesRequest Initializer => new FieldCapabilitiesRequest(Index<Project>().And<Developer>())
		{
			Fields = Fields<Project>(p => p.Name).And("*"),
		};

		protected override string UrlPath => "/project%2Cdevs/_field_caps?fields=name%2C%2A";

		protected override LazyResponses ClientUsage() => Calls(
			(c, f) => c.FieldCapabilities(Index<Project>().And<Developer>(), f),
			(c, f) => c.FieldCapabilitiesAsync(Index<Project>().And<Developer>(), f),
			(c, r) => c.FieldCapabilities(r),
			(c, r) => c.FieldCapabilitiesAsync(r)
		);

		protected override void ExpectResponse(IFieldCapabilitiesResponse response)
		{
			response.Fields.Should().ContainKey("_uid");

			var uidFieldCaps = response.Fields.First(kv => kv.Value.Uid != null).Value.Uid;
			uidFieldCaps.Aggregatable.Should().BeTrue();
			uidFieldCaps.Searchable.Should().BeFalse();

			uidFieldCaps = response.Fields["_uid"].Uid;
			uidFieldCaps.Should().NotBeNull();

			uidFieldCaps.Aggregatable.Should().BeTrue();
			uidFieldCaps.Searchable.Should().BeFalse();

			response.Fields.Should().ContainKey("state");
			var stateCapabilities = response.Fields["state"].Keyword;
			stateCapabilities.Aggregatable.Should().BeTrue();
			stateCapabilities.Searchable.Should().BeTrue();

			stateCapabilities = response.Fields[Field<Project>(p => p.State)].Keyword;
			stateCapabilities.Aggregatable.Should().BeTrue();
			stateCapabilities.Searchable.Should().BeTrue();
		}
	}
}
