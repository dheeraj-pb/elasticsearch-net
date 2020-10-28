﻿using Elastic.Xunit.XunitPlumbing;
using FluentAssertions;
using Nest6;

namespace Tests.Reproduce
{
	public class GithubIssue2101
	{
		[U]
		public void BoolClausesShouldEvaluateOnlyOnce()
		{
			var must = 0;
			var mustNot = 0;
			var should = 0;
			var filter = 0;

			new BoolQueryDescriptor<object>()
				.Must(m =>
				{
					must++;
					return m;
				})
				.MustNot(mn =>
				{
					mustNot++;
					return mn;
				})
				.Should(sh =>
				{
					should++;
					return sh;
				})
				.Filter(f =>
				{
					filter++;
					return f;
				});

			filter.Should().Be(1);
			should.Should().Be(1);
			must.Should().Be(1);
			mustNot.Should().Be(1);
		}
	}
}
