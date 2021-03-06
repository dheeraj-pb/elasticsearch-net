﻿using System.Threading.Tasks;
using Elastic.Xunit.XunitPlumbing;
using Nest6;
using Tests.Framework;
using static Tests.Framework.UrlTester;

namespace Tests.XPack.MachineLearning.DeleteDatafeed
{
	public class DeleteDatafeedUrlTests : UrlTestsBase
	{
		[U] public override async Task Urls() => await DELETE("/_xpack/ml/datafeeds/datafeed_id")
			.Fluent(c => c.DeleteDatafeed("datafeed_id"))
			.Request(c => c.DeleteDatafeed(new DeleteDatafeedRequest("datafeed_id")))
			.FluentAsync(c => c.DeleteDatafeedAsync("datafeed_id"))
			.RequestAsync(c => c.DeleteDatafeedAsync(new DeleteDatafeedRequest("datafeed_id")));
	}
}
