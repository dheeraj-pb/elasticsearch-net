﻿using System.Threading.Tasks;
using Elastic.Xunit.XunitPlumbing;
using Nest6;
using Tests.Domain;
using Tests.Framework;

namespace Tests.Cluster.ClusterAllocationExplain
{
	public class ClusterAllocationExplainUrlTests : UrlTestsBase
	{
		[U] public override async Task Urls() => await UrlTester.POST("/_cluster/allocation/explain?include_yes_decisions=true")
			.Fluent(c => c.ClusterAllocationExplain(s => s.Index<Project>().Shard(0).Primary(true).IncludeYesDecisions()))
			.Request(c => c.ClusterAllocationExplain(new ClusterAllocationExplainRequest
				{ Index = typeof(Project), Shard = 0, Primary = true, IncludeYesDecisions = true }))
			.FluentAsync(c => c.ClusterAllocationExplainAsync(s => s.Index<Project>().Shard(0).Primary(true).IncludeYesDecisions()))
			.RequestAsync(c => c.ClusterAllocationExplainAsync(new ClusterAllocationExplainRequest
				{ Index = typeof(Project), Shard = 0, Primary = true, IncludeYesDecisions = true }));
	}
}
