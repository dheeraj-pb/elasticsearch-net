﻿using System.Threading.Tasks;
using Elastic.Xunit.XunitPlumbing;
using Nest6;
using Tests.Framework;
using static Tests.Framework.UrlTester;

namespace Tests.Cat.CatThreadPool
{
	public class CatThreadPoolUrlTests : UrlTestsBase
	{
		[U] public override async Task Urls() => await GET("/_cat/thread_pool")
			.Fluent(c => c.CatThreadPool())
			.Request(c => c.CatThreadPool(new CatThreadPoolRequest()))
			.FluentAsync(c => c.CatThreadPoolAsync())
			.RequestAsync(c => c.CatThreadPoolAsync(new CatThreadPoolRequest()));
	}
}
