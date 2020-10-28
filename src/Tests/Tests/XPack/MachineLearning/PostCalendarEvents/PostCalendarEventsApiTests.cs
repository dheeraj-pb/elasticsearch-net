﻿using System;
using System.Collections.Generic;
using System.Linq;
using Elastic.Xunit.XunitPlumbing;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.Extensions;
using Tests.Framework;
using Tests.Framework.Integration;
using Tests.Framework.ManagedElasticsearch.Clusters;

namespace Tests.XPack.MachineLearning.PostCalendarEvents
{
	[SkipVersion("<6.4.0", "Calendar functions for machine learning introduced in 6.4.0")]
	public class PostCalendarEventsApiTests
		: MachineLearningIntegrationTestBase<IPostCalendarEventsResponse, IPostCalendarEventsRequest, PostCalendarEventsDescriptor,
			PostCalendarEventsRequest>
	{
		public PostCalendarEventsApiTests(MachineLearningCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values)
			{
				PutCalendar(client, callUniqueValue.Value);
			}
		}

		protected override bool ExpectIsValid => true;

		protected override object ExpectJson => new
		{
			events = GetScheduledJsonEvents()
		};

		protected override int ExpectStatusCode => 200;

		private static readonly int StartDate = DateTime.Now.Year;

		private IEnumerable<object> GetScheduledJsonEvents()
		{
			for (var i = 0; i < 10; i++)
			{
				yield return new
				{
					start_time = new DateTimeOffset(StartDate + i, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds(),
					end_time = new DateTimeOffset(StartDate + 1 + i, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds(),
					description = $"Event {i}",
					calendar_id = CallIsolatedValue
				};
			}
		}

		private IEnumerable<ScheduledEvent> GetScheduledEvents()
		{
			for (var i = 0; i < 10; i++)
			{
				yield return new ScheduledEvent
				{
					StartTime = new DateTimeOffset(StartDate + i, 1, 1, 0, 0, 0, TimeSpan.Zero),
					EndTime = new DateTimeOffset(StartDate + 1 + i, 1, 1, 0, 0, 0, TimeSpan.Zero),
					Description = $"Event {i}",
					CalendarId = CallIsolatedValue
				};
			}
		}

		protected override Func<PostCalendarEventsDescriptor, IPostCalendarEventsRequest> Fluent => f => f.Events(GetScheduledEvents());

		protected override HttpMethod HttpMethod => HttpMethod.POST;

		protected override PostCalendarEventsRequest Initializer => new PostCalendarEventsRequest(CallIsolatedValue)
		{
			Events = GetScheduledEvents()
		};

		protected override bool SupportsDeserialization => false;
		protected override string UrlPath => $"_xpack/ml/calendars/{CallIsolatedValue}/events";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.PostCalendarEvents(CallIsolatedValue, f),
			(client, f) => client.PostCalendarEventsAsync(CallIsolatedValue, f),
			(client, r) => client.PostCalendarEvents(r),
			(client, r) => client.PostCalendarEventsAsync(r)
		);

		protected override PostCalendarEventsDescriptor NewDescriptor() =>
			new PostCalendarEventsDescriptor(CallIsolatedValue).Events(GetScheduledEvents());

		protected override void ExpectResponse(IPostCalendarEventsResponse response)
		{
			response.ShouldBeValid();

			response.Events.Should().NotBeNull();
			response.Events.Count().Should().Be(10);

			var @event = response.Events.First();
			@event.CalendarId.Should().Be(CallIsolatedValue);
			@event.Description.Should().Be($"Event 0");
		}
	}
}
