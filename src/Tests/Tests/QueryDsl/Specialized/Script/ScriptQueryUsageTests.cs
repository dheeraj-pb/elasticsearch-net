﻿using System.Collections.Generic;
using Nest6;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Domain;
using Tests.Framework.Integration;

namespace Tests.QueryDsl.Specialized.Script
{
	/**
	* A query allowing to define {ref_current}/modules-scripting.html[scripts] as queries.
	*
	* See the Elasticsearch documentation on {ref_current}/query-dsl-script-query.html[script query] for more details.
	*/
	public class ScriptQueryUsageTests : QueryDslUsageTestsBase
	{
		private static readonly string _templateString = "doc['numberOfCommits'].value > params.param1";

		public ScriptQueryUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

		protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<IScriptQuery>(a => a.Script)
		{
			q =>
			{
				q.Source = "";
				q.Id = null;
			},
			q =>
			{
				q.Source = null;
				q.Id = null;
			}
		};

		protected override QueryContainer QueryInitializer => new ScriptQuery
		{
			Name = "named_query",
			Boost = 1.1,
			Source = _templateString,
			Params = new Dictionary<string, object>
			{
				{ "param1", 50 }
			}
		};

		protected override object QueryJson => new
		{
			script = new
			{
				_name = "named_query",
				boost = 1.1,
				script = new
				{
					source = "doc['numberOfCommits'].value > params.param1",
					@params = new { param1 = 50 }
				}
			}
		};

		protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
			.Script(sn => sn
				.Name("named_query")
				.Boost(1.1)
				.Source(_templateString)
				.Params(p => p.Add("param1", 50))
			);
	}
}
