﻿using System;
using System.Linq;
using FluentAssertions;
using Nest6;
using Tests.Core.Extensions;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Domain;
using Tests.Framework.Integration;

namespace Tests.Aggregations.Pipeline.Derivative
{
	public class DerivativeAggregationUsageTests : AggregationUsageTestBase
	{
		public DerivativeAggregationUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override object AggregationJson => new
		{
			projects_started_per_month = new
			{
				date_histogram = new
				{
					field = "startedOn",
					interval = "month",
				},
				aggs = new
				{
					commits = new
					{
						sum = new
						{
							field = "numberOfCommits"
						}
					},
					commits_derivative = new
					{
						derivative = new
						{
							buckets_path = "commits"
						}
					}
				}
			}
		};

		protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
			.DateHistogram("projects_started_per_month", dh => dh
				.Field(p => p.StartedOn)
				.Interval(DateInterval.Month)
				.Aggregations(aa => aa
					.Sum("commits", sm => sm
						.Field(p => p.NumberOfCommits)
					)
					.Derivative("commits_derivative", d => d
						.BucketsPath("commits")
					)
				)
			);

		protected override AggregationDictionary InitializerAggs =>
			new DateHistogramAggregation("projects_started_per_month")
			{
				Field = "startedOn",
				Interval = DateInterval.Month,
				Aggregations =
					new SumAggregation("commits", "numberOfCommits") &&
					new DerivativeAggregation("commits_derivative", "commits")
			};

		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.ShouldBeValid();

			var projectsPerMonth = response.Aggregations.DateHistogram("projects_started_per_month");
			projectsPerMonth.Should().NotBeNull();
			projectsPerMonth.Buckets.Should().NotBeNull();
			projectsPerMonth.Buckets.Count.Should().BeGreaterThan(0);

			var notNullDerivativeSeen = 0;
			// derivative not calculated for the first bucket
			foreach (var item in projectsPerMonth.Buckets.Skip(1))
			{
				if (item.DocCount == 0) continue;
				var commitsDerivative = item.Derivative("commits_derivative");
				commitsDerivative.Should().NotBeNull();
				if (commitsDerivative.Value != null) notNullDerivativeSeen++;
			}
			notNullDerivativeSeen.Should().BeGreaterThan(0, "atleast one bucket should yield a derivative value surely!");


		}
	}
}
