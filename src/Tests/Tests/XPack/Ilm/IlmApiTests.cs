using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Xunit.XunitPlumbing;
using FluentAssertions;
using Nest6;
using Tests.Core.Client;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Domain;
using Tests.Framework;
using Tests.Framework.EndpointTests.TestState;
using Tests.Framework.Integration;

namespace Tests.XPack.Ilm
{
	[SkipVersion("<6.7.0", "All APIs exist in Elasticsearch 6.7.0")]
	public class IlmApiTests : CoordinatedIntegrationTestBase<XPackCluster>
	{
		private const string IlmGetStatusStep = nameof(IlmGetStatusStep);
		private const string IlmPutLifecycleStep = nameof(IlmPutLifecycleStep);
		private const string IlmGetLifecycleStep = nameof(IlmGetLifecycleStep);
		private const string IlmGeAllLifecycleStep = nameof(IlmGeAllLifecycleStep);
		private const string IlmDeleteLifecycleStep = nameof(IlmDeleteLifecycleStep);
		private const string PutDocumentStep = nameof(PutDocumentStep);
		private const string IlmExplainLifecycleStep = nameof(IlmExplainLifecycleStep);
		private const string IlmRemovePolicyStep = nameof(IlmRemovePolicyStep);
		private const string IlmStopStep = nameof(IlmStopStep);

		public IlmApiTests(XPackCluster cluster, EndpointUsage usage) : base(new CoordinatedUsage(cluster, usage)
		{
			{
				PutDocumentStep, u => u.Calls<IndexDescriptor<Project>, IndexRequest<Project>, IIndexRequest<Project>, IIndexResponse>(
					v => new IndexRequest<Project>(Document),
					(v, d) => d,
					(v, c, f) => c.Index(Document, f),
					(v, c, f) => c.IndexAsync(Document, f),
					(v, c, r) => c.Index(r),
					(v, c, r) => c.IndexAsync(r)
				)
			},
			{
				IlmExplainLifecycleStep, u => u.Calls<ExplainLifecycleDescriptor, ExplainLifecycleRequest, IExplainLifecycleRequest, IExplainLifecycleResponse>(
					v => new ExplainLifecycleRequest("project"),
					(v, d) => d,
					(v, c, f) => c.ExplainLifecycle("project", f),
					(v, c, f) => c.ExplainLifecycleAsync("project", f),
					(v, c, r) => c.ExplainLifecycle(r),
					(v, c, r) => c.ExplainLifecycleAsync(r)
				)
			},
			{
				IlmGetStatusStep, u => u.Calls<GetIlmStatusDescriptor, GetIlmStatusRequest, IGetIlmStatusRequest, IGetIlmStatusResponse>(
					v => new GetIlmStatusRequest(),
					(v, d) => d,
					(v, c, f) => c.GetIlmStatus(f),
					(v, c, f) => c.GetIlmStatusAsync(f),
					(v, c, r) => c.GetIlmStatus(r),
					(v, c, r) => c.GetIlmStatusAsync(r)
				)
			},
			{
				IlmPutLifecycleStep, u => u.Calls<PutLifecycleDescriptor, PutLifecycleRequest, IPutLifecycleRequest, IPutLifecycleResponse>(
					v => new PutLifecycleRequest("policy" + v)
					{
						Policy = new Policy
						{
							Phases = new Phases
							{
								Warm = new Phase
								{
									MinimumAge = "10d",
									Actions = new LifecycleActions
									{
										new ForceMergeLifecycleAction
										{
											MaximumNumberOfSegments = 1
										}
									}
								},
								Delete = new Phase
								{
									MinimumAge = "30d",
									Actions = new LifecycleActions
									{
										new DeleteLifecycleAction()
									}
								}
							}
						}
					},
					(v, d) => d
						.Policy(p => p.Phases(a => a.Warm(w => w.MinimumAge("10d")
																.Actions(ac => ac.ForceMerge(f => f.MaximumNumberOfSegments(1))))
													.Delete(w => w.MinimumAge("30d")
		       													  .Actions(ac => ac.Delete(f => f)))))
					,
					(v, c, f) => c.PutLifecycle("policy" + v, f),
					(v, c, f) => c.PutLifecycleAsync("policy" + v, f),
					(v, c, r) => c.PutLifecycle(r),
					(v, c, r) => c.PutLifecycleAsync(r)
				)
			},
			{
				IlmRemovePolicyStep, u => u.Calls<RemovePolicyDescriptor, RemovePolicyRequest, IRemovePolicyRequest, IRemovePolicyResponse>(
					v => new RemovePolicyRequest("project"),
					(v, d) => d,
					(v, c, f) => c.RemovePolicy("project", f),
					(v, c, f) => c.RemovePolicyAsync("project", f),
					(v, c, r) => c.RemovePolicy(r),
					(v, c, r) => c.RemovePolicyAsync(r)
				)
			},
			{
				IlmGetLifecycleStep, u => u.Calls<GetLifecycleDescriptor, GetLifecycleRequest, IGetLifecycleRequest, IGetLifecycleResponse>(
					v => new GetLifecycleRequest("policy" + v),
					(v, d) => d.PolicyId("policy" + v),
					(v, c, f) => c.GetLifecycle(f),
					(v, c, f) => c.GetLifecycleAsync(f),
					(v, c, r) => c.GetLifecycle(r),
					(v, c, r) => c.GetLifecycleAsync(r)
				)
			},
			{
				IlmGeAllLifecycleStep, u => u.Calls<GetLifecycleDescriptor, GetLifecycleRequest, IGetLifecycleRequest, IGetLifecycleResponse>(
					v => new GetLifecycleRequest(),
					(v, d) => d,
					(v, c, f) => c.GetLifecycle(f),
					(v, c, f) => c.GetLifecycleAsync(f),
					(v, c, r) => c.GetLifecycle(r),
					(v, c, r) => c.GetLifecycleAsync(r)
				)
			},
			{
				IlmDeleteLifecycleStep, u => u.Calls<DeleteLifecycleDescriptor, DeleteLifecycleRequest, IDeleteLifecycleRequest, IDeleteLifecycleResponse>(
					v => new DeleteLifecycleRequest("policy" + v),
					(v, d) => d,
					(v, c, f) => c.DeleteLifecycle("policy" + v, f),
					(v, c, f) => c.DeleteLifecycleAsync("policy" + v, f),
					(v, c, r) => c.DeleteLifecycle(r),
					(v, c, r) => c.DeleteLifecycleAsync(r)
				)
			},
			{
				IlmStopStep, u => u.Calls<StopIlmDescriptor, StopIlmRequest, IStopIlmRequest, IStopIlmResponse>(
					v => new StopIlmRequest(),
					(v, d) => d,
					(v, c, f) => c.StopIlm(f),
					(v, c, f) => c.StopIlmAsync(f),
					(v, c, r) => c.StopIlm(r),
					(v, c, r) => c.StopIlmAsync(r)
				)
			},
		}) { }

		private static Project Document => new Project
		{
			State = StateOfBeing.Stable,
			Name = "Name",
			StartedOn = FixedDate,
			LastActivity = FixedDate,
			CuratedTags = new List<Tag> { new Tag { Name = "x", Added = FixedDate } },
			SourceOnly = null
		};

		private static DateTime FixedDate { get; } = new DateTime(2015, 06, 06, 12, 01, 02, 123);

		[I] public async Task IlmExplainLifecycleResponse() => await Assert<ExplainLifecycleResponse>(IlmExplainLifecycleStep, (v, r) =>
		{
			r.IsValid.Should().BeTrue();
			r.ApiCall.HttpStatusCode.Should().Be(200);

			var index = $"project";
			var hasIndex = r.Indices.TryGetValue(index, out var indexDict);

			hasIndex.Should().BeTrue($"expect `{index}` to be returned");
			indexDict.Should().NotBeNull($"expect `{index}`'s value not to be null");

			indexDict.Index.Should().Be("project");
			indexDict.Managed.Should().Be(false);

		});

		[I] public async Task IlmStopResponse() => await Assert<StopIlmResponse>(IlmStopStep, (v, r) =>
		{
			r.IsValid.Should().BeTrue();
			r.ApiCall.HttpStatusCode.Should().Be(200);
			r.Acknowledged.Should().BeTrue();
		});

		[I] public async Task IlmRemovePolicyResponse() => await Assert<RemovePolicyResponse>(IlmRemovePolicyStep, (v, r) =>
		{
			r.IsValid.Should().BeTrue();
			r.ApiCall.HttpStatusCode.Should().Be(200);
			r.HasFailures.Should().Be(false);
			r.FailedIndexes.Should().BeEmpty();
		});

		[I] public async Task IlmGetStatusResponse() => await Assert<GetIlmStatusResponse>(IlmGetStatusStep, (v, r) =>
		{
			r.IsValid.Should().BeTrue();
			r.ApiCall.HttpStatusCode.Should().Be(200);
			r.OperationMode.Should().Be(LifecycleOperationMode.Running);
		});

		[I] public async Task IlmPutLifecycleResponse() => await Assert<PutLifecycleResponse>(IlmPutLifecycleStep, (v, r) =>
		{
			r.IsValid.Should().BeTrue();
			r.ApiCall.HttpStatusCode.Should().Be(200);
			r.Acknowledged.Should().BeTrue();
		});

		[I] public async Task IlmGetLifecycleResponse() => await Assert<GetLifecycleResponse>(IlmGetLifecycleStep, (v, r) =>
		{
			r.IsValid.Should().BeTrue();
			r.ApiCall.HttpStatusCode.Should().Be(200);
			r.Policies.Should().NotBeEmpty();

			var policy = $"policy{v}";
			var hasPolicy = r.Policies.TryGetValue(policy, out var policyDict);

			hasPolicy.Should().BeTrue($"expect `{policy}` to be returned");
			policyDict.Should().NotBeNull($"expect `{policy}`'s value not to be null");

			policyDict.Version.Should().Be(1);
			policyDict.ModifiedDate.Should().BeBefore(DateTimeOffset.UtcNow);
			policyDict.Policy.Phases.Should().NotBe(null);

			policyDict.Policy.Phases.Warm.Should().NotBe(null);
			policyDict.Policy.Phases.Warm.MinimumAge.Should().Be(new Time("10d"));
			policyDict.Policy.Phases.Warm.Actions.Should().NotBeEmpty();
			policyDict.Policy.Phases.Warm.Actions.Should().HaveCount(1);

			var warmAction = policyDict.Policy.Phases.Warm.Actions.First();
			warmAction.Key.Should().Be("forcemerge");
			warmAction.Value.Should().BeOfType<ForceMergeLifecycleAction>();
			var forceMerge = warmAction.Value.As<ForceMergeLifecycleAction>();
			forceMerge.MaximumNumberOfSegments.Should().Be(1);

			policyDict.Policy.Phases.Delete.Should().NotBe(null);
			policyDict.Policy.Phases.Delete.MinimumAge.Should().Be(new Time("30d"));
			policyDict.Policy.Phases.Delete.Actions.Should().NotBeEmpty();
			policyDict.Policy.Phases.Delete.Actions.Should().HaveCount(1);

			var deleteAction = policyDict.Policy.Phases.Delete.Actions.First();
			deleteAction.Key.Should().Be("delete");
			deleteAction.Value.Should().BeOfType<DeleteLifecycleAction>();
		});

		[I] public async Task IlmDeleteLifecycleResponse() => await Assert<DeleteLifecycleResponse>(IlmDeleteLifecycleStep, (v, r) =>
		{
			r.IsValid.Should().BeTrue();
			r.ApiCall.HttpStatusCode.Should().Be(200);
			r.Acknowledged.Should().BeTrue();
		});
	}
}
