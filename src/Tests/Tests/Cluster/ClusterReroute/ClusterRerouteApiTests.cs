﻿using System;
using System.Collections.Generic;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.Extensions;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Domain;
using Tests.Framework;
using Tests.Framework.Integration;

namespace Tests.Cluster.ClusterReroute
{
	public class ClusterRerouteApiTests
		: ApiIntegrationTestBase<IntrusiveOperationSeededCluster, IClusterRerouteResponse, IClusterRerouteRequest, ClusterRerouteDescriptor,
			ClusterRerouteRequest>
	{
		public ClusterRerouteApiTests(IntrusiveOperationSeededCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => false;

		protected override object ExpectJson => new
		{
			commands = new[]
			{
				new Dictionary<string, object>
				{
					{
						"allocate_empty_primary", new
						{
							index = "project",
							node = "x",
							shard = 0,
							accept_data_loss = true
						}
					}
				},
				new Dictionary<string, object>
				{
					{
						"allocate_stale_primary", new
						{
							index = "project",
							node = "x",
							shard = 0,
							accept_data_loss = true
						}
					}
				},
				new Dictionary<string, object>
				{
					{
						"allocate_replica", new
						{
							index = "project",
							node = "x",
							shard = 0
						}
					}
				},
				new Dictionary<string, object>
				{
					{
						"move", new
						{
							to_node = "y",
							from_node = "x",
							index = "project",
							shard = 0
						}
					}
				},
				new Dictionary<string, object>
				{
					{
						"cancel", new
						{
							index = "project",
							node = "x",
							shard = 1
						}
					}
				},
			}
		};

		protected override int ExpectStatusCode => 400;

		protected override Func<ClusterRerouteDescriptor, IClusterRerouteRequest> Fluent => c => c
			.AllocateEmptyPrimary(a => a
				.Index<Project>()
				.Node("x")
				.Shard(0)
				.AcceptDataLoss(true)
			)
			.AllocateStalePrimary(a => a
				.Index<Project>()
				.Node("x")
				.Shard(0)
				.AcceptDataLoss(true)
			)
			.AllocateReplica(a => a
				.Index<Project>()
				.Node("x")
				.Shard(0)
			)
			.Move(a => a
				.ToNode("y")
				.FromNode("x")
				.Index("project")
				.Shard(0)
			)
			.Cancel(a => a
				.Index("project")
				.Node("x")
				.Shard(1)
			);

		protected override HttpMethod HttpMethod => HttpMethod.POST;

		protected override ClusterRerouteRequest Initializer => new ClusterRerouteRequest
		{
			Commands = new List<IClusterRerouteCommand>
			{
				new AllocateEmptyPrimaryRerouteCommand { Index = IndexName.From<Project>(), Node = "x", Shard = 0, AcceptDataLoss = true },
				new AllocateStalePrimaryRerouteCommand { Index = IndexName.From<Project>(), Node = "x", Shard = 0, AcceptDataLoss = true },
				new AllocateReplicaClusterRerouteCommand { Index = IndexName.From<Project>(), Node = "x", Shard = 0 },
				new MoveClusterRerouteCommand { Index = IndexName.From<Project>(), FromNode = "x", ToNode = "y", Shard = 0 },
				new CancelClusterRerouteCommand() { Index = "project", Node = "x", Shard = 1 }
			}
		};

		protected override string UrlPath => "/_cluster/reroute";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.ClusterReroute(f),
			(client, f) => client.ClusterRerouteAsync(f),
			(client, r) => client.ClusterReroute(r),
			(client, r) => client.ClusterRerouteAsync(r)
		);

		protected override void ExpectResponse(IClusterRerouteResponse response)
		{
			response.ShouldNotBeValid();
			response.ServerError.Should().NotBeNull();
			response.ServerError.Status.Should().Be(400);
			response.ServerError.Error.Should().NotBeNull();
			response.ServerError.Error.Reason.Should().Contain("failed to resolve");
			response.ServerError.Error.Type.Should().Contain("illegal_argument_exception");
		}
	}


	//TODO simple integration test against isolated index to test happy flow
}
