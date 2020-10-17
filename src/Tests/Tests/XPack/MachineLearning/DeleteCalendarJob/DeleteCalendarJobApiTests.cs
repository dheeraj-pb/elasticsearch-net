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

namespace Tests.XPack.MachineLearning.DeleteCalendarJob
{
	[SkipVersion("<6.4.0", "Calendar functions for machine learning introduced in 6.4.0")]
	public class DeleteCalendarJobApiTests : MachineLearningIntegrationTestBase<IDeleteCalendarJobResponse, IDeleteCalendarJobRequest, DeleteCalendarJobDescriptor, DeleteCalendarJobRequest>
	{
		public DeleteCalendarJobApiTests(MachineLearningCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values)
			{
				PutJob(client, callUniqueValue.Value + "_job");
				PutCalendar(client, callUniqueValue.Value + "_calendar");
				PutCalendarJob(client, callUniqueValue.Value + "_calendar", callUniqueValue.Value + "_job");
			}
		}


		protected override bool ExpectIsValid => true;

		protected override object ExpectJson => null;

		protected override int ExpectStatusCode => 200;

		protected override Func<DeleteCalendarJobDescriptor, IDeleteCalendarJobRequest> Fluent => f => f;

		protected override HttpMethod HttpMethod => HttpMethod.DELETE;

		protected override DeleteCalendarJobRequest Initializer => new DeleteCalendarJobRequest(CallIsolatedValue + "_calendar",CallIsolatedValue + "_job");

		protected override bool SupportsDeserialization => false;
		protected override string UrlPath => $"_xpack/ml/calendars/{CallIsolatedValue}_calendar/jobs/{CallIsolatedValue}_job";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.DeleteCalendarJob(CallIsolatedValue + "_calendar",CallIsolatedValue + "_job", f),
			(client, f) => client.DeleteCalendarJobAsync(CallIsolatedValue + "_calendar",CallIsolatedValue + "_job", f),
			(client, r) => client.DeleteCalendarJob(r),
			(client, r) => client.DeleteCalendarJobAsync(r)
		);

		protected override DeleteCalendarJobDescriptor NewDescriptor() => new DeleteCalendarJobDescriptor(CallIsolatedValue + "_calendar",CallIsolatedValue + "_job");

		protected override void ExpectResponse(IDeleteCalendarJobResponse response)
		{
			response.ShouldBeValid();

			response.CalendarId.Should().Be(CallIsolatedValue + "_calendar");

			response.JobIds.Should().NotBeNull();

			response.JobIds.Should().BeEmpty();

			response.Description.Should().Be("Planned outages");
		}
	}
}
