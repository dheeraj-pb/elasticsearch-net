﻿using Nest6;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Domain;
using Tests.Framework.Integration;

namespace Tests.QueryDsl
{
	public class MatchNoneQueryUsageTests : QueryDslUsageTestsBase
	{
		public MatchNoneQueryUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override QueryContainer QueryInitializer => new MatchNoneQuery
		{
			Name = "named_query",
			Boost = 1.1
		};

		protected override object QueryJson => new
		{
			match_none = new
			{
				_name = "named_query",
				boost = 1.1
			}
		};

		protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
			.MatchNone(c => c
				.Name("named_query")
				.Boost(1.1)
			);
	}
}
