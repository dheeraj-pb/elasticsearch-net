﻿using System.Threading.Tasks;
using Elastic.Xunit.XunitPlumbing;
using Nest6;
using Tests.Framework;

namespace Tests.XPack.Sql.ClearSqlCursor
{
	public class ClearSqlCursorUrlTests : UrlTestsBase
	{
		[U] public override async Task Urls() => await UrlTester.POST("_xpack/sql/close")
			.Fluent(c => c.ClearSqlCursor(d => d))
			.Request(c => c.ClearSqlCursor(new ClearSqlCursorRequest()))
			.FluentAsync(c => c.ClearSqlCursorAsync(d => d))
			.RequestAsync(c => c.ClearSqlCursorAsync(new ClearSqlCursorRequest()));
	}
}
