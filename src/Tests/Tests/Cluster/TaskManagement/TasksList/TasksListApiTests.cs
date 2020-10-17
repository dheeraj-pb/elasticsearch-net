﻿using System;
using System.Linq;
using Elastic.Xunit.XunitPlumbing;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.Extensions;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Core.ManagedElasticsearch.NodeSeeders;
using Tests.Domain;
using Tests.Framework;
using Tests.Framework.Integration;

namespace Tests.Cluster.TaskManagement.TasksList
{
	public class TasksListApiTests
		: ApiIntegrationTestBase<ReadOnlyCluster, IListTasksResponse, IListTasksRequest, ListTasksDescriptor, ListTasksRequest>
	{
		public TasksListApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => 200;

		protected override Func<ListTasksDescriptor, IListTasksRequest> Fluent => s => s
			.Actions("*lists*");

		protected override HttpMethod HttpMethod => HttpMethod.GET;

		protected override ListTasksRequest Initializer => new ListTasksRequest
		{
			Actions = new[] { "*lists*" }
		};

		protected override string UrlPath => "/_tasks?actions=%2Alists%2A";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.ListTasks(f),
			(client, f) => client.ListTasksAsync(f),
			(client, r) => client.ListTasks(r),
			(client, r) => client.ListTasksAsync(r)
		);

		protected override void ExpectResponse(IListTasksResponse response)
		{
			response.Nodes.Should().NotBeEmpty();
			var taskExecutingNode = response.Nodes.First().Value;
			taskExecutingNode.Host.Should().NotBeNullOrWhiteSpace();
			taskExecutingNode.Ip.Should().NotBeNullOrWhiteSpace();
			taskExecutingNode.Name.Should().NotBeNullOrWhiteSpace();
			taskExecutingNode.TransportAddress.Should().NotBeNullOrWhiteSpace();
			taskExecutingNode.Tasks.Should().NotBeEmpty();
			taskExecutingNode.Tasks.Count().Should().BeGreaterOrEqualTo(2);

			var task = taskExecutingNode.Tasks.Values.First(p => p.ParentTaskId != null);
			task.Action.Should().NotBeNullOrWhiteSpace();
			task.Type.Should().NotBeNullOrWhiteSpace();
			task.Id.Should().BePositive();
			task.Node.Should().NotBeNullOrWhiteSpace();
			task.RunningTimeInNanoSeconds.Should().BeGreaterThan(0);
			task.StartTimeInMilliseconds.Should().BeGreaterThan(0);
			task.ParentTaskId.Should().NotBeNull();

			var parentTask = taskExecutingNode.Tasks[task.ParentTaskId];
			parentTask.Should().NotBeNull();
			parentTask.ParentTaskId.Should().BeNull();
		}
	}

	[SkipVersion("<2.3.0", "")]
	public class TasksListDetailedApiTests
		: ApiIntegrationTestBase<IntrusiveOperationCluster, IListTasksResponse, IListTasksRequest, ListTasksDescriptor, ListTasksRequest>
	{
		private static TaskId _taskId = new TaskId("fakeid:1");

		public TasksListDetailedApiTests(IntrusiveOperationCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => 200;

		protected override Func<ListTasksDescriptor, IListTasksRequest> Fluent => s => s
			.Detailed();

		protected override HttpMethod HttpMethod => HttpMethod.GET;

		protected override ListTasksRequest Initializer => new ListTasksRequest()
		{
			Detailed = true
		};

		protected override string UrlPath => $"/_tasks?detailed=true";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.ListTasks(f),
			(client, f) => client.ListTasksAsync(f),
			(client, r) => client.ListTasks(r),
			(client, r) => client.ListTasksAsync(r)
		);

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			var seeder = new DefaultSeeder(Cluster.Client);
			seeder.SeedNode();

			// get a suitable load of projects in order to get a decent task status out
			var bulkResponse = client.IndexMany(Project.Generator.Generate(10000));
			if (!bulkResponse.IsValid)
				throw new Exception("failure in setting up integration");

			client.Refresh(typeof(Project));

			var targetIndex = "tasks-list-projects";

			var createIndex = client.CreateIndex(targetIndex, i => i
				.Settings(settings => settings.Analysis(DefaultSeeder.ProjectAnalysisSettings))
				.Mappings(m => m
					.Map<Project>(mm =>mm
						.RoutingField(r => r.Required())
						.AutoMap()
						.Properties(DefaultSeeder.ProjectProperties)
						.Properties<CommitActivity>(props => props
							.Object<Developer>(o => o
								.AutoMap()
								.Name(p => p.Committer)
								.Properties(DefaultSeeder.DeveloperProperties)
							)
							.Text(t => t
								.Name(p => p.ProjectName)
								.Index(false)
							)
						)
					)
				)
			);

			createIndex.ShouldBeValid();
			var response = client.ReindexOnServer(r => r
				.Source(s => s
					.Index(typeof(Project))
					.Type(typeof(Project))
				)
				.Destination(d => d
					.Index(targetIndex)
					.OpType(OpType.Create)
				)
				.Conflicts(Conflicts.Proceed)
				.WaitForCompletion(false)
				.Refresh()
			);

			_taskId = response.Task;
		}

		protected override void ExpectResponse(IListTasksResponse response)
		{
			response.Nodes.Should().NotBeEmpty();
			var taskExecutingNode = response.Nodes.First().Value;
			taskExecutingNode.Host.Should().NotBeNullOrWhiteSpace();
			taskExecutingNode.Ip.Should().NotBeNullOrWhiteSpace();
			taskExecutingNode.Name.Should().NotBeNullOrWhiteSpace();
			taskExecutingNode.TransportAddress.Should().NotBeNullOrWhiteSpace();
			taskExecutingNode.Tasks.Should().NotBeEmpty();
			taskExecutingNode.Tasks.Count().Should().BeGreaterOrEqualTo(1);

			var task = taskExecutingNode.Tasks[_taskId];
			task.Action.Should().NotBeNullOrWhiteSpace();
			task.Type.Should().NotBeNullOrWhiteSpace();
			task.Id.Should().BePositive();
			task.Node.Should().NotBeNullOrWhiteSpace();
			task.RunningTimeInNanoSeconds.Should().BeGreaterThan(0);
			task.StartTimeInMilliseconds.Should().BeGreaterThan(0);

			var status = task.Status;
			status.Should().NotBeNull();
			status.Total.Should().BeGreaterOrEqualTo(0);
			status.Batches.Should().BeGreaterOrEqualTo(0);
		}
	}
}
