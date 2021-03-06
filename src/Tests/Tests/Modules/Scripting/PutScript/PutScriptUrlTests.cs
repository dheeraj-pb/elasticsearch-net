﻿using System.Threading.Tasks;
using Elastic.Xunit.XunitPlumbing;
using Nest6;
using Tests.Framework;
using static Tests.Framework.UrlTester;

namespace Tests.Modules.Scripting.PutScript
{
	public class PutScriptUrlTests
	{
		[U] public async Task Urls()
		{
			var id = "id";

			await PUT($"/_scripts/{id}")
					.Fluent(c => c.PutScript(id, s => s.Painless("")))
					.Request(c => c.PutScript(new PutScriptRequest(id)))
					.FluentAsync(c => c.PutScriptAsync(id, s => s.Painless("")))
					.RequestAsync(c => c.PutScriptAsync(new PutScriptRequest(id)))
				;
		}
	}
}
