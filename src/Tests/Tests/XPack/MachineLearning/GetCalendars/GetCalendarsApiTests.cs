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

namespace Tests.XPack.MachineLearning.GetCalendars
{
	[SkipVersion("<6.4.0", "Calendar functions for machine learning introduced in 6.4.0")]
	public class GetCalendarsApiTests : MachineLearningIntegrationTestBase<IGetCalendarsResponse, IGetCalendarsRequest, GetCalendarsDescriptor, GetCalendarsRequest>
	{
		public GetCalendarsApiTests(MachineLearningCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values)
			{
				PutJob(client, callUniqueValue.Value + "_job");
				PutCalendar(client, callUniqueValue.Value);
				PutCalendarJob(client, callUniqueValue.Value, callUniqueValue.Value + "_job");
			}
		}

		protected override bool ExpectIsValid => true;

		protected override object ExpectJson => null;

		protected override int ExpectStatusCode => 200;

		protected override Func<GetCalendarsDescriptor, IGetCalendarsRequest> Fluent => f => f.CalendarId(CallIsolatedValue);

		protected override HttpMethod HttpMethod => HttpMethod.POST;

		protected override GetCalendarsRequest Initializer => new GetCalendarsRequest(CallIsolatedValue);

		protected override bool SupportsDeserialization => false;

		protected override string UrlPath => $"_xpack/ml/calendars/{CallIsolatedValue}";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.GetCalendars(f),
			(client, f) => client.GetCalendarsAsync(f),
			(client, r) => client.GetCalendars(r),
			(client, r) => client.GetCalendarsAsync(r)
		);

		protected override GetCalendarsDescriptor NewDescriptor() => new GetCalendarsDescriptor().CalendarId(CallIsolatedValue);


		protected override void ExpectResponse(IGetCalendarsResponse response)
		{
			response.ShouldBeValid();

			response.Calendars.Should().NotBeEmpty();

			var calendar = response.Calendars.First();

			calendar.CalendarId.Should().Be(CallIsolatedValue);
			calendar.JobIds.Should().NotBeNull();
			calendar.JobIds.First().Should().Be(CallIsolatedValue + "_job");
			calendar.Description.Should().Be("Planned outages");
		}
	}

	[SkipVersion("<6.4.0", "Calendar functions for machine learning introduced in 6.4.0")]
	public class GetCalendarsPagingApiTests : MachineLearningIntegrationTestBase<IGetCalendarsResponse, IGetCalendarsRequest, GetCalendarsDescriptor, GetCalendarsRequest>
	{
		public GetCalendarsPagingApiTests(MachineLearningCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values)
			{
				PutJob(client, callUniqueValue.Value + "_job");
				PutCalendar(client, callUniqueValue.Value);
				for (int i = 0; i < 20; i++)
				{
					PutCalendar(client, callUniqueValue.Value + "_" + i);
				}
			}
		}

		protected override bool ExpectIsValid => true;

		protected override object ExpectJson => null;

		protected override int ExpectStatusCode => 200;

		protected override Func<GetCalendarsDescriptor, IGetCalendarsRequest> Fluent => f => f.Page(p => p.Size(10).From(10));

		protected override HttpMethod HttpMethod => HttpMethod.POST;

		protected override GetCalendarsRequest Initializer => new GetCalendarsRequest
		{
			Page = new Page
			{
				Size = 10,
				From = 10
			}
		};

		protected override bool SupportsDeserialization => false;

		protected override string UrlPath => $"_xpack/ml/calendars";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.GetCalendars(f),
			(client, f) => client.GetCalendarsAsync(f),
			(client, r) => client.GetCalendars(r),
			(client, r) => client.GetCalendarsAsync(r)
		);

		protected override void ExpectResponse(IGetCalendarsResponse response)
		{
			response.ShouldBeValid();
			response.Count.Should().BeGreaterOrEqualTo(1);
			response.Calendars.Should().NotBeEmpty();
			response.Calendars.Count.Should().BeGreaterOrEqualTo(1);
		}
	}
}
