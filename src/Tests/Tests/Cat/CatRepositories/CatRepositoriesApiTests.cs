﻿using System;
using System.IO;
using Elastic.Xunit.XunitPlumbing;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.Client;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Framework;
using Tests.Framework.Integration;

namespace Tests.Cat.CatRepositories
{
	[SkipVersion("<2.1.0", "")]
	public class CatRepositoriesApiTests
		: ApiIntegrationTestBase<IntrusiveOperationCluster, ICatResponse<CatRepositoriesRecord>, ICatRepositoriesRequest, CatRepositoriesDescriptor,
			CatRepositoriesRequest>
	{
		private static readonly string RepositoryName = RandomString();

		public CatRepositoriesApiTests(IntrusiveOperationCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => 200;
		protected override HttpMethod HttpMethod => HttpMethod.GET;
		protected override string UrlPath => $"/_cat/repositories";

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			if (!TestClient.Configuration.RunIntegrationTests) return;

			var repositoryLocation = Path.Combine(Cluster.FileSystem.RepositoryPath, RandomString());

			var create = Client.CreateRepository(RepositoryName, cr => cr
				.FileSystem(fs => fs
					.Settings(repositoryLocation)
				)
			);

			if (!create.IsValid || !create.Acknowledged)
				throw new Exception("Setup: failed to create snapshot repository");
		}

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.CatRepositories(f),
			(client, f) => client.CatRepositoriesAsync(f),
			(client, r) => client.CatRepositories(r),
			(client, r) => client.CatRepositoriesAsync(r)
		);

		protected override void ExpectResponse(ICatResponse<CatRepositoriesRecord> response) => response.Records.Should()
			.NotBeEmpty()
			.And.OnlyContain(r =>
				!string.IsNullOrEmpty(r.Id)
				&& !string.IsNullOrEmpty(r.Type)
			);
	}
}
