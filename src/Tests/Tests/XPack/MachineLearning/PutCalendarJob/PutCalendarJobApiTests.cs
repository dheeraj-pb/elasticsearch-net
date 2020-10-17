﻿using System;
using System.Linq;
using Elastic.Xunit.XunitPlumbing;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.Extensions;
using Tests.Framework;
using Tests.Framework.Integration;
using Tests.Framework.ManagedElasticsearch.Clusters;

namespace Tests.XPack.MachineLearning.PutCalendarJob
{
	[SkipVersion("<6.4.0", "Calendar functions for machine learning introduced in 6.4.0")]
	public class PutCalendarJobApiTests : MachineLearningIntegrationTestBase<IPutCalendarJobResponse, IPutCalendarJobRequest, PutCalendarJobDescriptor, PutCalendarJobRequest>
	{
		public PutCalendarJobApiTests(MachineLearningCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values)
			{
				PutJob(client, callUniqueValue.Value + "_job");
				PutCalendar(client, callUniqueValue.Value + "_calendar");
			}
		}


		protected override bool ExpectIsValid => true;

		protected override object ExpectJson => null;

		protected override int ExpectStatusCode => 200;

		protected override Func<PutCalendarJobDescriptor, IPutCalendarJobRequest> Fluent => f => f;

		protected override HttpMethod HttpMethod => HttpMethod.PUT;

		protected override PutCalendarJobRequest Initializer => new PutCalendarJobRequest(CallIsolatedValue + "_calendar",CallIsolatedValue + "_job");

		protected override bool SupportsDeserialization => false;
		protected override string UrlPath => $"_xpack/ml/calendars/{CallIsolatedValue}_calendar/jobs/{CallIsolatedValue}_job";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.PutCalendarJob(CallIsolatedValue + "_calendar",CallIsolatedValue + "_job", f),
			(client, f) => client.PutCalendarJobAsync(CallIsolatedValue + "_calendar",CallIsolatedValue + "_job", f),
			(client, r) => client.PutCalendarJob(r),
			(client, r) => client.PutCalendarJobAsync(r)
		);

		protected override PutCalendarJobDescriptor NewDescriptor() => new PutCalendarJobDescriptor(CallIsolatedValue + "_calendar",CallIsolatedValue + "_job");

		protected override void ExpectResponse(IPutCalendarJobResponse response)
		{
			response.ShouldBeValid();

			response.CalendarId.Should().Be(CallIsolatedValue + "_calendar");

			response.JobIds.Should().NotBeNull();

			response.JobIds.First().Should().Be(CallIsolatedValue + "_job");

			response.Description.Should().Be("Planned outages");
		}
	}
}
