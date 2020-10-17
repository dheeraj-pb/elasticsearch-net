﻿using System.Threading.Tasks;
using Elastic.Xunit.XunitPlumbing;
using Nest6;
using Tests.Framework;
using static Tests.Framework.UrlTester;

namespace Tests.Document.Multiple.DeleteByQueryRethrottle
{
	public class DeleteByQueryRethrottleUrlTests : UrlTestsBase
	{
		private readonly TaskId _taskId = "rhtoNesNR4aXVIY2bRR4GQ:13056";

		[U] public override async Task Urls() =>
			await POST($"/_delete_by_query/{EscapeUriString(_taskId.ToString())}/_rethrottle")
				.Fluent(c => c.DeleteByQueryRethrottle(_taskId))
				.Request(c => c.DeleteByQueryRethrottle(new DeleteByQueryRethrottleRequest(_taskId)))
				.FluentAsync(c => c.DeleteByQueryRethrottleAsync(_taskId))
				.RequestAsync(c => c.DeleteByQueryRethrottleAsync(new DeleteByQueryRethrottleRequest(_taskId)));
	}
}
