﻿using System.Threading.Tasks;
using Elastic.Xunit.XunitPlumbing;
using Nest6;
using Tests.Framework;
using static Tests.Framework.UrlTester;
using static Nest6.Indices;

namespace Tests.Indices.StatusManagement.ForceMerge
{
	public class ForceMergeUrlTests
	{
		[U] public async Task Urls()
		{
			await POST($"/_forcemerge")
					.Fluent(c => c.ForceMerge(All))
					.Request(c => c.ForceMerge(new ForceMergeRequest()))
					.FluentAsync(c => c.ForceMergeAsync(All))
					.RequestAsync(c => c.ForceMergeAsync(new ForceMergeRequest()))
				;

			var index = "index1,index2";
			await POST($"/index1%2Cindex2/_forcemerge")
					.Fluent(c => c.ForceMerge(index))
					.Request(c => c.ForceMerge(new ForceMergeRequest(index)))
					.FluentAsync(c => c.ForceMergeAsync(index))
					.RequestAsync(c => c.ForceMergeAsync(new ForceMergeRequest(index)))
				;
		}
	}
}
