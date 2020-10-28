﻿using System;
using Elasticsearch.Net;
using Nest6;
using Tests.Core.Extensions;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Framework;
using Tests.Framework.Integration;
using static Nest6.Infer;

namespace Tests.Document.Multiple.ReindexOnServer
{
	public class ReindexOnServerSourceApiTests
		: ApiIntegrationTestBase<IntrusiveOperationCluster, IReindexOnServerResponse, IReindexOnServerRequest, ReindexOnServerDescriptor,
			ReindexOnServerRequest>
	{
		public ReindexOnServerSourceApiTests(IntrusiveOperationCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;

		protected override object ExpectJson =>
			new
			{
				dest = new
				{
					index = $"{CallIsolatedValue}-clone",
					type = "test",
				},
				source = new
				{
					index = CallIsolatedValue,
					_source = new[] { "id", "flag" },
					type = new[] { "test" },
				},
				conflicts = "proceed"
			};

		protected override int ExpectStatusCode => 200;

		protected override Func<ReindexOnServerDescriptor, IReindexOnServerRequest> Fluent => d => d
			.Source(s => s
				.Index(CallIsolatedValue)
				.Type("test")
				.Source<Test>(f => f
					.Field(ff => ff.Id)
					.Field(ff => ff.Flag)
				)
			)
			.Destination(s => s
				.Index(CallIsolatedValue + "-clone")
				.Type("test")
			)
			.Conflicts(Conflicts.Proceed)
			.Refresh();

		protected override HttpMethod HttpMethod => HttpMethod.POST;

		protected override ReindexOnServerRequest Initializer => new ReindexOnServerRequest
		{
			Source = new ReindexSource
			{
				Index = CallIsolatedValue,
				Type = "test",
				Source = Fields<Test>(
					ff => ff.Id,
					ff => ff.Flag
				)
			},
			Destination = new ReindexDestination
			{
				Index = CallIsolatedValue + "-clone",
				Type = Type<Test>(),
			},
			Conflicts = Conflicts.Proceed,
			Refresh = true,
		};

		protected override bool SupportsDeserialization => false;

		protected override string UrlPath => $"/_reindex?refresh=true";

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var index in values.Values)
				Client.Bulk(b => b
					.Index(index)
					.IndexMany(new[]
					{
						new Test { Id = 1, Flag = "bar" },
						new Test { Id = 2, Flag = "bar" }
					})
					.Refresh(Refresh.WaitFor)
				);
		}

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.ReindexOnServer(f),
			(client, f) => client.ReindexOnServerAsync(f),
			(client, r) => client.ReindexOnServer(r),
			(client, r) => client.ReindexOnServerAsync(r)
		);

		protected override void ExpectResponse(IReindexOnServerResponse response) => response.ShouldBeValid();

		public class Test
		{
			public string Flag { get; set; }
			public long Id { get; set; }
		}
	}
}
