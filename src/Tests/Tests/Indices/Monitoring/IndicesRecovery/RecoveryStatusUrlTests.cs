﻿using System.Threading.Tasks;
using Elastic.Xunit.XunitPlumbing;
using Nest6;
using Tests.Framework;

namespace Tests.Indices.Monitoring.IndicesRecovery
{
	public class RecoveryStatusUrlTests
	{
		[U] public async Task Urls()
		{
			await UrlTester.GET($"/_recovery")
					.Fluent(c => c.RecoveryStatus(Nest6.Indices.All))
					.Request(c => c.RecoveryStatus(new RecoveryStatusRequest()))
					.FluentAsync(c => c.RecoveryStatusAsync(Nest6.Indices.All))
					.RequestAsync(c => c.RecoveryStatusAsync(new RecoveryStatusRequest()))
				;

			var index = "index1,index2";
			await UrlTester.GET($"/index1%2Cindex2/_recovery")
					.Fluent(c => c.RecoveryStatus(index))
					.Request(c => c.RecoveryStatus(new RecoveryStatusRequest(index)))
					.FluentAsync(c => c.RecoveryStatusAsync(index))
					.RequestAsync(c => c.RecoveryStatusAsync(new RecoveryStatusRequest(index)))
				;
		}
	}
}
