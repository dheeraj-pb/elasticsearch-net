﻿using System;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.Extensions;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Domain;
using Tests.Framework;
using Tests.Framework.Integration;

namespace Tests.Cluster.TaskManagement.GetTask
{
	public class GetTaskApiTests : ApiIntegrationTestBase<WritableCluster, IGetTaskResponse, IGetTaskRequest, GetTaskDescriptor, GetTaskRequest>
	{
		private static TaskId _taskId = new TaskId("fakeid:1");

		public GetTaskApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => 200;

		protected override Func<GetTaskDescriptor, IGetTaskRequest> Fluent => s => s;
		protected override HttpMethod HttpMethod => HttpMethod.GET;

		protected override GetTaskRequest Initializer => new GetTaskRequest(_taskId);
		protected override string UrlPath => $"/_tasks/fakeid%3A1";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.GetTask(_taskId, f),
			(client, f) => client.GetTaskAsync(_taskId, f),
			(client, r) => client.GetTask(r),
			(client, r) => client.GetTaskAsync(r)
		);

		protected override void ExpectResponse(IGetTaskResponse response)
		{
			response.ShouldBeValid();
			response.Task.Should().NotBeNull();
			var task = response.Task;
			task.Node.Should().NotBeNullOrEmpty();
			task.Id.Should().BeGreaterThan(0);
			task.Type.Should().Be("transport");
			task.Action.Should().Be("indices:data/write/reindex");
			task.Status.Should().NotBeNull();
			task.StartTimeInMilliseconds.Should().BeGreaterThan(0);
			task.RunningTimeInNanoseconds.Should().BeGreaterThan(0);
			task.Cancellable.Should().BeTrue();
		}

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			// get a suitable load of projects in order to get a decent task status out
			var bulkResponse = client.IndexMany(Project.Generator.Generate(10000), "project-origin");
			if (!bulkResponse.IsValid)
				throw new Exception("failure in setting up integration");

			var response = client.ReindexOnServer(r => r
				.Source(s => s
					.Index("project-origin")
					.Type(typeof(Project))
				)
				.Destination(d => d
					.Index("tasks-list-projects")
					.OpType(OpType.Create)
				)
				.Conflicts(Conflicts.Proceed)
				.WaitForCompletion(false)
				.Refresh()
			);

			_taskId = response.Task;
		}
	}
}
