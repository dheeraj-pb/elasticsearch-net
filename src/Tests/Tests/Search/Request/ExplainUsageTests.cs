﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Xunit.XunitPlumbing;
using FluentAssertions;
using Nest6;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Domain;
using Tests.Framework.Integration;

namespace Tests.Search.Request
{
	public class ExplainUsageTests : SearchUsageTestBase
	{
		/**
		 * Enables explanation for each hit on how its score was computed.
		 *
		 * See the Elasticsearch documentation on {ref_current}/search-explain.html[Explain] for more detail.
		 */
		public ExplainUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

		protected override object ExpectJson =>
			new { explain = true };

		protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
			.Explain();

		protected override SearchRequest<Project> Initializer =>
			new SearchRequest<Project> { Explain = true };

		[I]
		protected async Task ExplanationIsSetOnHits() => await AssertOnAllResponses(r =>
		{
			r.Hits.Should().NotBeEmpty();
			r.Hits.Should().NotContain(hit => hit.Explanation == null);
			foreach (var explanation in r.Hits.Select(h => h.Explanation))
			{
				explanation.Description.Should().NotBeNullOrEmpty();
				explanation.Value.Should().BeGreaterThan(0);
			}
		});
	}
}
