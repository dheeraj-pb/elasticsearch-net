﻿using System.Threading.Tasks;
using Elastic.Xunit.XunitPlumbing;
using FluentAssertions;
using Nest6;
using Tests.Core.Extensions;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Framework;

namespace Tests.Indices.AliasManagement.GetIndicesPointingToAlias
{
	public class GetIndicesPointingToAliasTests : IntegrationDocumentationTestBase, IClusterFixture<WritableCluster>
	{
		private static readonly string Unique = RandomString();
		private static readonly string Alias = "alias-" + Unique;

		private static readonly string[] Indices =
		{
			$"alias-index-{Unique}-1",
			$"alias-index-{Unique}-2",
			$"alias-index-{Unique}-3"
		};

		private readonly IElasticClient _client;
		private readonly WritableCluster _cluster;

		public GetIndicesPointingToAliasTests(WritableCluster cluster) : base(cluster)
		{
			_cluster = cluster;
			_client = _cluster.Client;

			foreach (var index in Indices)
			{
				if (_client.IndexExists(index).Exists) continue;

				lock (Unique)
				{
					if (_client.IndexExists(index).Exists) continue;

					var createResponse = _client.CreateIndex(index, c => c
						.Settings(s => s
							.NumberOfShards(1)
							.NumberOfReplicas(0)
						)
						.Aliases(a => a
							.Alias(Alias)
						)
					);
					createResponse.ShouldBeValid();
				}
			}
		}

		[I]
		public void ShouldGetAliasesPointingToIndex()
		{
			var indicesPointingToAlias = _client.GetIndicesPointingToAlias(Alias);

			indicesPointingToAlias.Should().NotBeEmpty().And.HaveCount(3);
			indicesPointingToAlias.Should().Contain(Indices);
		}

		[I]
		public async Task ShouldGetAliasesPointingToIndexAsync()
		{
			var indicesPointingToAlias = await _client.GetIndicesPointingToAliasAsync(Alias);

			indicesPointingToAlias.Should().NotBeEmpty().And.HaveCount(3);
			indicesPointingToAlias.Should().Contain(Indices);
		}
	}
}
