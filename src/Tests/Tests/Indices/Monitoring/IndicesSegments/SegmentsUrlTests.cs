﻿using System.Threading.Tasks;
using Elastic.Xunit.XunitPlumbing;
using Nest6;
using Tests.Framework;

namespace Tests.Indices.Monitoring.IndicesSegments
{
	public class SegmentsUrlTests
	{
		[U] public async Task Urls()
		{
			await UrlTester.GET($"/_segments")
					.Fluent(c => c.Segments(Nest6.Indices.All))
					.Request(c => c.Segments(new SegmentsRequest()))
					.FluentAsync(c => c.SegmentsAsync(Nest6.Indices.All))
					.RequestAsync(c => c.SegmentsAsync(new SegmentsRequest()))
				;

			var index = "index1,index2";
			await UrlTester.GET($"/index1%2Cindex2/_segments")
					.Fluent(c => c.Segments(index))
					.Request(c => c.Segments(new SegmentsRequest(index)))
					.FluentAsync(c => c.SegmentsAsync(index))
					.RequestAsync(c => c.SegmentsAsync(new SegmentsRequest(index)))
				;
		}
	}
}
