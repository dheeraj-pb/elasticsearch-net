﻿using System;
using System.Collections.Generic;
using Elastic.Xunit.XunitPlumbing;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.Extensions;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Framework;
using Tests.Framework.Integration;

namespace Tests.Modules.Scripting.ExecutePainlessScript
{
	[SkipVersion("<6.3.0", "this API was introduced in 6.3.0")]
	public class ExecutePainlessScriptApiTests
		: ApiIntegrationTestBase<ReadOnlyCluster, IExecutePainlessScriptResponse<string>, IExecutePainlessScriptRequest,
			ExecutePainlessScriptDescriptor, ExecutePainlessScriptRequest>
	{
		private static readonly string _painlessScript = "params.count / params.total";

		public ExecutePainlessScriptApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;

		protected override object ExpectJson => new
		{
			script = new
			{
				source = _painlessScript,
				@params = new { count = 100.0, total = 1000.0 }
			},
		};

		protected override int ExpectStatusCode => 200;

		protected override Func<ExecutePainlessScriptDescriptor, IExecutePainlessScriptRequest> Fluent => d => d
			.Script(s => s
				.Source(_painlessScript)
				.Params(p => p.Add("count", 100.0).Add("total", 1000.0))
			);

		protected override HttpMethod HttpMethod => HttpMethod.POST;

		protected override ExecutePainlessScriptRequest Initializer => new ExecutePainlessScriptRequest
		{
			Script = new InlineScript(_painlessScript)
			{
				Params = new Dictionary<string, object>
				{
					{ "count", 100.0 },
					{ "total", 1000.0 },
				}
			}
		};

		protected override bool SupportsDeserialization => false;
		protected override string UrlPath => "/_scripts/painless/_execute";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.ExecutePainlessScript<string>(f),
			(client, f) => client.ExecutePainlessScriptAsync<string>(f),
			(client, r) => client.ExecutePainlessScript<string>(r),
			(client, r) => client.ExecutePainlessScriptAsync<string>(r)
		);

		protected override void ExpectResponse(IExecutePainlessScriptResponse<string> response)
		{
			response.ShouldBeValid();
			response.Result.Should().NotBeNullOrWhiteSpace();
		}
	}

	[SkipVersion("<6.4.0", "Context only tested on 6.4.0 when they were introduced")]
	public class ExecutePainlessScriptContextApiTests
		: ApiIntegrationTestBase<WritableCluster, IExecutePainlessScriptResponse<string>, IExecutePainlessScriptRequest,
			ExecutePainlessScriptDescriptor, ExecutePainlessScriptRequest>
	{
		private static readonly string _painlessScript = "doc['rank'].value / params.max_rank";

		public ExecutePainlessScriptContextApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;

		protected override object ExpectJson => new
		{
			context = "score",
			context_setup = new
			{
				document = new { rank = 4 },
				index = UniqueValues.FixedForAllCallsValue,
				query = new { match_all = new { } }
			},
			script = new
			{
				source = _painlessScript,
				@params = new { max_rank = 5.0 }
			},
		};

		protected override int ExpectStatusCode => 200;

		protected override Func<ExecutePainlessScriptDescriptor, IExecutePainlessScriptRequest> Fluent => d => d
			.ContextSetup(cs => cs
				.Index(UniqueValues.FixedForAllCallsValue)
				.Document(new ScriptDocument { Rank = 4 })
				.Query<ScriptDocument>(q => q.MatchAll())
			)
			.Context("score")
			.Script(s => s
				.Source(_painlessScript)
				.Params(p => p.Add("max_rank", 5.0))
			);

		protected override HttpMethod HttpMethod => HttpMethod.POST;

		protected override ExecutePainlessScriptRequest Initializer => new ExecutePainlessScriptRequest
		{
			ContextSetup = new PainlessContextSetup
			{
				Index = UniqueValues.FixedForAllCallsValue,
				Document = new ScriptDocument { Rank = 4 },
				Query = new MatchAllQuery()
			},
			Context = "score",
			Script = new InlineScript(_painlessScript)
			{
				Params = new Dictionary<string, object>
				{
					{ "max_rank", 5.0 },
				}
			}
		};

		protected override bool SupportsDeserialization => false;
		protected override string UrlPath => "/_scripts/painless/_execute";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.ExecutePainlessScript<string>(f),
			(client, f) => client.ExecutePainlessScriptAsync<string>(f),
			(client, r) => client.ExecutePainlessScript<string>(r),
			(client, r) => client.ExecutePainlessScriptAsync<string>(r)
		);

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			var create = client.CreateIndex(values.FixedForAllCallsValue, c => c.Mappings(map => map.Map<ScriptDocument>(m => m.AutoMap())));
			create.ShouldBeValid();
		}

		protected override void ExpectResponse(IExecutePainlessScriptResponse<string> response)
		{
			response.ShouldBeValid();
			response.Result.Should().NotBeNullOrWhiteSpace();
		}

		private class ScriptDocument
		{
			public string Field { get; set; }
			public long Rank { get; set; }
		}
	}
}
