﻿using System;
using System.Collections.Generic;
using Elasticsearch.Net;
using Nest6;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Domain;
using Tests.Framework;
using Tests.Framework.Integration;

namespace Tests.Document.Single.Update
{
	public class UpdateWithScriptApiTests
		: ApiIntegrationTestBase<WritableCluster, IUpdateResponse<Project>, IUpdateRequest<Project, Project>, UpdateDescriptor<Project, Project>,
			UpdateRequest<Project, Project>>
	{
		public UpdateWithScriptApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;

		protected override object ExpectJson { get; } = new
		{
			scripted_upsert = true,
			script = new
			{
				source = "ctx._source.name = \"params.name\"",
				lang = "painless",
				@params = new
				{
					name = "foo",
					other = (object)null
				}
			}
		};

		protected override int ExpectStatusCode => 200;

		protected override Func<UpdateDescriptor<Project, Project>, IUpdateRequest<Project, Project>> Fluent => d => d
			.Routing(Project.Routing)
			.ScriptedUpsert()
			.Script(s => s
				.Source("ctx._source.name = \"params.name\"")
				.Lang("painless")
				.Params(p => p
					.Add("name", "foo")
					.Add("other", null)
				)
			);

		protected override HttpMethod HttpMethod => HttpMethod.POST;

		protected override UpdateRequest<Project, Project> Initializer => new UpdateRequest<Project, Project>(CallIsolatedValue)
		{
			Routing = Project.Routing,
			ScriptedUpsert = true,
			Script = new InlineScript("ctx._source.name = \"params.name\"")
			{
				Lang = "painless",
				Params = new Dictionary<string, object>
				{
					{ "name", "foo" },
					{ "other", null }
				}
			}
		};

		protected override bool SupportsDeserialization => false;
		protected override string UrlPath => $"/project/doc/{CallIsolatedValue}/_update?routing={U(Project.Routing)}";

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var id in values.Values)
				Client.Index(Project.Instance, i => i.Id(id));
		}

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.Update<Project>(CallIsolatedValue, f),
			(client, f) => client.UpdateAsync<Project>(CallIsolatedValue, f),
			(client, r) => client.Update<Project>(r),
			(client, r) => client.UpdateAsync<Project>(r)
		);

		protected override UpdateDescriptor<Project, Project> NewDescriptor() =>
			new UpdateDescriptor<Project, Project>(DocumentPath<Project>.Id(CallIsolatedValue));
	}
}
