﻿using System;
using System.Collections.Generic;
using System.Linq;
using Elastic.Xunit.XunitPlumbing;
using FluentAssertions;
using Nest6;
using Tests.Configuration;
using Tests.Core.Extensions;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Domain;
using Tests.Framework.Integration;
using static Nest6.Infer;

namespace Tests.Aggregations.Bucket.Composite
{
	/**
	 * A multi-bucket aggregation that creates composite buckets from different sources.
     *
     * Unlike the other multi-bucket aggregation the composite aggregation can be
	 * used to paginate all buckets from a multi-level aggregation efficiently.
	 * This aggregation provides a way to stream all buckets of a specific aggregation
	 * similarly to what scroll does for documents.
     *
     * The composite buckets are built from the combinations of the values extracted/created
	 * for each document and each combination is considered as a composite bucket.
	 *
	 * NOTE: Only available in Elasticsearch 6.1.0+
	 *
	 * Be sure to read the Elasticsearch documentation on {ref_current}/search-aggregations-bucket-composite-aggregation.html[Composite Aggregation].
	*/
	[SkipVersion("<6.1.0", "Composite Aggregation is only available in Elasticsearch 6.1.0+")]
	public class CompositeAggregationUsageTests : ProjectsOnlyAggregationUsageTestBase
	{
		public CompositeAggregationUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

		protected override object AggregationJson => new
		{
			my_buckets = new
			{
				composite = new
				{
					sources = new object[]
					{
						new
						{
							branches = new
							{
								terms = new
								{
									field = "branches.keyword"
								}
							}
						},
						new
						{
							started = new
							{
								date_histogram = new
								{
									field = "startedOn",
									interval = "month"
								}
							}
						},
						new
						{
							branch_count = new
							{
								histogram = new
								{
									field = "requiredBranches",
									interval = 1d
								}
							}
						},
					}
				},
				aggs = new
				{
					project_tags = new
					{
						nested = new
						{
							path = "tags"
						},
						aggs = new
						{
							tags = new
							{
								terms = new { field = "tags.name" }
							}
						}
					}
				}
			}
		};

		protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
			.Composite("my_buckets", date => date
				.Sources(s => s
					.Terms("branches", t => t
						.Field(f => f.Branches.Suffix("keyword"))
					)
					.DateHistogram("started", d => d
						.Field(f => f.StartedOn)
						.Interval(DateInterval.Month)
					)
					.Histogram("branch_count", h => h
						.Field(f => f.RequiredBranches)
						.Interval(1)
					)
				)
				.Aggregations(childAggs => childAggs
					.Nested("project_tags", n => n
						.Path(p => p.Tags)
						.Aggregations(nestedAggs => nestedAggs
							.Terms("tags", avg => avg.Field(p => p.Tags.First().Name))
						)
					)
				)
			);

		protected override AggregationDictionary InitializerAggs =>
			new CompositeAggregation("my_buckets")
			{
				Sources = new List<ICompositeAggregationSource>
				{
					new TermsCompositeAggregationSource("branches")
					{
						Field = Field<Project>(f => f.Branches.Suffix("keyword"))
					},
					new DateHistogramCompositeAggregationSource("started")
					{
						Field = Field<Project>(f => f.StartedOn),
						Interval = DateInterval.Month
					},
					new HistogramCompositeAggregationSource("branch_count")
					{
						Field = Field<Project>(f => f.RequiredBranches),
						Interval = 1
					}
				},
				Aggregations = new NestedAggregation("project_tags")
				{
					Path = Field<Project>(p => p.Tags),
					Aggregations = new TermsAggregation("tags")
					{
						Field = Field<Project>(p => p.Tags.First().Name)
					}
				}
			};

		/**==== Handling Responses
		 * Each Composite aggregation bucket key is a `CompositeKey` type, a specialized
		 * `IReadOnlyDictionary<string, object>` type with methods to convert values to supported types
		 */
		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.ShouldBeValid();

			var composite = response.Aggregations.Composite("my_buckets");
			composite.Should().NotBeNull();
			composite.Buckets.Should().NotBeNullOrEmpty();
			composite.AfterKey.Should().NotBeNull();
			if (TestConfiguration.Instance.InRange(">=6.3.0"))
				composite.AfterKey.Should()
					.HaveCount(3)
					.And.ContainKeys("branches", "started", "branch_count");
			foreach (var item in composite.Buckets)
			{
				var key = item.Key;
				key.Should().NotBeNull();

				key.TryGetValue("branches", out string branches).Should().BeTrue();
				branches.Should().NotBeNullOrEmpty();

				key.TryGetValue("started", out DateTime started).Should().BeTrue();
				started.Should().BeAfter(default(DateTime));

				key.TryGetValue("branch_count", out int branchCount).Should().BeTrue();
				branchCount.Should().BeGreaterThan(0);

				item.DocCount.Should().BeGreaterThan(0);

				var nested = item.Nested("project_tags");
				nested.Should().NotBeNull();

				if (nested.DocCount > 0)
				{
					var nestedTerms = nested.Terms("tags");
					nestedTerms.Buckets.Count.Should().BeGreaterThan(0);
				}
			}
		}
	}

	/**[float]
	* === Missing buckets
	* By default documents without a value for a given source are ignored.
	* It is possible to include them in the response by setting missing_bucket to `true` (defaults to `false`):
	*
	* NOTE: Only available in Elasticsearch 6.4.0+
	*/
	[SkipVersion("<6.4.0", "Missing buckets added to Composite Aggregation Elasticsearch 6.4.0+")]
	public class CompositeAggregationMissingBucketUsageTests : ProjectsOnlyAggregationUsageTestBase
	{
		public CompositeAggregationMissingBucketUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

		protected override object AggregationJson => new
		{
			my_buckets = new
			{
				composite = new
				{
					sources = new object[]
					{
						new
						{
							branches = new
							{
								terms = new
								{
									field = "branches.keyword",
									order = "asc",
									missing_bucket = true
								}
							}
						},
					}
				},
				aggs = new
				{
					project_tags = new
					{
						nested = new { path = "tags" },
						aggs = new
						{
							tags = new { terms = new { field = "tags.name" } }
						}
					}
				}
			}
		};

		protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
			.Composite("my_buckets", date => date
				.Sources(s => s
					.Terms("branches", t => t
						.Field(f => f.Branches.Suffix("keyword"))
						.MissingBucket()
						.Order(SortOrder.Ascending)
					)
				)
				.Aggregations(childAggs => childAggs
					.Nested("project_tags", n => n
						.Path(p => p.Tags)
						.Aggregations(nestedAggs => nestedAggs
							.Terms("tags", avg => avg.Field(p => p.Tags.First().Name))
						)
					)
				)
			);

		protected override AggregationDictionary InitializerAggs =>
			new CompositeAggregation("my_buckets")
			{
				Sources = new List<ICompositeAggregationSource>
				{
					new TermsCompositeAggregationSource("branches")
					{
						Field = Field<Project>(f => f.Branches.Suffix("keyword")),
						MissingBucket = true,
						Order = SortOrder.Ascending
					}
				},
				Aggregations = new NestedAggregation("project_tags")
				{
					Path = Field<Project>(p => p.Tags),
					Aggregations = new TermsAggregation("tags")
					{
						Field = Field<Project>(p => p.Tags.First().Name)
					}
				}
			};

		/**==== Handling Responses
		 * Each Composite aggregation bucket key is an `CompositeKey`, a specialized
		 * `IReadOnlyDictionary<string, object>` type with methods to convert values to supported types
		 */
		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.ShouldBeValid();

			var composite = response.Aggregations.Composite("my_buckets");
			composite.Should().NotBeNull();
			composite.Buckets.Should().NotBeNullOrEmpty();
			composite.AfterKey.Should().NotBeNull();

			if (TestConfiguration.Instance.InRange(">=6.3.0"))
				composite.AfterKey.Should().HaveCount(1).And.ContainKeys("branches");

			var i = 0;
			foreach (var item in composite.Buckets)
			{
				var key = item.Key;
				key.Should().NotBeNull();

				key.TryGetValue("branches", out string branches).Should().BeTrue("expected to find 'branches' in composite bucket");
				if (i == 0) branches.Should().BeNull("First key should be null as we expect to have some projects with no branches");
				else branches.Should().NotBeNullOrEmpty();

				var nested = item.Nested("project_tags");
				nested.Should().NotBeNull();

				var nestedTerms = nested.Terms("tags");
				nestedTerms.Buckets.Count.Should().BeGreaterThan(0);
				i++;
			}
		}
	}

	// hide
#pragma warning disable 618
	[SkipVersion("<6.4.0", "Missing added to Composite Aggregation Elasticsearch 6.4.0+")]
	public class CompositeAggregationMissingUsageTests : ProjectsOnlyAggregationUsageTestBase
	{
		public CompositeAggregationMissingUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

		protected override object AggregationJson => new
		{
			my_buckets = new
			{
				composite = new
				{
					sources = new object[]
					{
						new
						{
							branches = new
							{
								terms = new
								{
									field = "branches.keyword",
									order = "asc",
									missing = "missing_branch"
								}
							}
						},
					}
				},
				aggs = new
				{
					project_tags = new
					{
						nested = new { path = "tags" },
						aggs = new
						{
							tags = new { terms = new { field = "tags.name" } }
						}
					}
				}
			}
		};

		protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
			.Composite("my_buckets", date => date
				.Sources(s => s
					.Terms("branches", t => t
						.Field(f => f.Branches.Suffix("keyword"))

						.Missing("missing_branch")
						.Order(SortOrder.Ascending)
					)
				)
				.Aggregations(childAggs => childAggs
					.Nested("project_tags", n => n
						.Path(p => p.Tags)
						.Aggregations(nestedAggs => nestedAggs
							.Terms("tags", avg => avg.Field(p => p.Tags.First().Name))
						)
					)
				)
			);

		protected override AggregationDictionary InitializerAggs =>
			new CompositeAggregation("my_buckets")
			{
				Sources = new List<ICompositeAggregationSource>
				{
					new TermsCompositeAggregationSource("branches")
					{
						Field = Field<Project>(f => f.Branches.Suffix("keyword")),
						Missing = "missing_branch",
						Order = SortOrder.Ascending
					}
				},
				Aggregations = new NestedAggregation("project_tags")
				{
					Path = Field<Project>(p => p.Tags),
					Aggregations = new TermsAggregation("tags")
					{
						Field = Field<Project>(p => p.Tags.First().Name)
					}
				}
			};

		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.ShouldBeValid();

			var composite = response.Aggregations.Composite("my_buckets");
			composite.Should().NotBeNull();
			composite.Buckets.Should().NotBeNullOrEmpty();
			composite.AfterKey.Should().NotBeNull();

			if (TestConfiguration.Instance.InRange(">=6.3.0"))
				composite.AfterKey.Should().HaveCount(1).And.ContainKeys("branches");

			var i = 0;
			var seenMissingBranches = false;
			foreach (var item in composite.Buckets)
			{
				var key = item.Key;
				key.Should().NotBeNull();

				key.TryGetValue("branches", out string branches).Should().BeTrue("expected to find 'branches' in composite bucket");
				branches.Should().NotBeNullOrEmpty();
				if (branches == "missing_branch")
				{
					seenMissingBranches = true;
				}

				var nested = item.Nested("project_tags");
				nested.Should().NotBeNull();

				var nestedTerms = nested.Terms("tags");
				nestedTerms.Buckets.Count.Should().BeGreaterThan(0);
				i++;
			}

			seenMissingBranches.Should().BeTrue();
		}
	}
#pragma warning restore 618

	//hide
	[SkipVersion("<6.3.0", "Date histogram source only supports format starting from Elasticsearch 6.3.0+")]
	public class DateFormatCompositeAggregationUsageTests : ProjectsOnlyAggregationUsageTestBase
	{
		public DateFormatCompositeAggregationUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

		protected override object AggregationJson => new
		{
			my_buckets = new
			{
				composite = new
				{
					sources = new object[]
					{
						new
						{
							started = new
							{
								date_histogram = new
								{
									field = "startedOn",
									interval = "month",
									format = "yyyy-MM-dd"
								}
							}
						},
					}
				},
				aggs = new
				{
					project_tags = new
					{
						nested = new
						{
							path = "tags"
						},
						aggs = new
						{
							tags = new
							{
								terms = new { field = "tags.name" }
							}
						}
					}
				}
			}
		};

		protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
			.Composite("my_buckets", date => date
				.Sources(s => s
					.DateHistogram("started", d => d
						.Field(f => f.StartedOn)
						.Interval(DateInterval.Month)
						.Format("yyyy-MM-dd")
					)
				)
				.Aggregations(childAggs => childAggs
					.Nested("project_tags", n => n
						.Path(p => p.Tags)
						.Aggregations(nestedAggs => nestedAggs
							.Terms("tags", avg => avg.Field(p => p.Tags.First().Name))
						)
					)
				)
			);

		protected override AggregationDictionary InitializerAggs =>
			new CompositeAggregation("my_buckets")
			{
				Sources = new List<ICompositeAggregationSource>
				{
					new DateHistogramCompositeAggregationSource("started")
					{
						Field = Field<Project>(f => f.StartedOn),
						Interval = DateInterval.Month,
						Format = "yyyy-MM-dd"
					},
				},
				Aggregations = new NestedAggregation("project_tags")
				{
					Path = Field<Project>(p => p.Tags),
					Aggregations = new TermsAggregation("tags")
					{
						Field = Field<Project>(p => p.Tags.First().Name)
					}
				}
			};

		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.ShouldBeValid();

			var composite = response.Aggregations.Composite("my_buckets");
			composite.Should().NotBeNull();
			composite.Buckets.Should().NotBeNullOrEmpty();
			composite.AfterKey.Should().NotBeNull();
			composite.AfterKey.Should().HaveCount(1).And.ContainKeys("started");
			foreach (var item in composite.Buckets)
			{
				var key = item.Key;
				key.Should().NotBeNull();

				key.TryGetValue("started", out string startedString).Should().BeTrue();
				startedString.Should().NotBeNullOrWhiteSpace();
			}
		}
	}
}
