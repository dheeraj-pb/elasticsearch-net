﻿using FluentAssertions;
using Nest6;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Framework;
using Tests.Framework.Integration;

namespace Tests.Modules.Scripting
{
	public class ScriptingCrudTests
		: CrudTestBase<IntrusiveOperationCluster, IPutScriptResponse, IGetScriptResponse, IPutScriptResponse, IDeleteScriptResponse>
	{
		private readonly string _updatedScript = "2+2";

		public ScriptingCrudTests(IntrusiveOperationCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override LazyResponses Create() => Calls<PutScriptDescriptor, PutScriptRequest, IPutScriptRequest, IPutScriptResponse>(
			CreateInitializer,
			CreateFluent,
			(s, c, f) => c.PutScript(s, f),
			(s, c, f) => c.PutScriptAsync(s, f),
			(s, c, r) => c.PutScript(r),
			(s, c, r) => c.PutScriptAsync(r)
		);

		private PutScriptRequest CreateInitializer(string id) => new PutScriptRequest(id) { Script = new PainlessScript("1+1") };

		private IPutScriptRequest CreateFluent(string id, PutScriptDescriptor d) => d.Painless("1+1");

		protected override LazyResponses Read() => Calls<GetScriptDescriptor, GetScriptRequest, IGetScriptRequest, IGetScriptResponse>(
			id => new GetScriptRequest(id),
			(id, d) => d,
			(s, c, f) => c.GetScript(s, f),
			(s, c, f) => c.GetScriptAsync(s, f),
			(s, c, r) => c.GetScript(r),
			(s, c, r) => c.GetScriptAsync(r)
		);

		protected override LazyResponses Update() => Calls<PutScriptDescriptor, PutScriptRequest, IPutScriptRequest, IPutScriptResponse>(
			UpdateInitializer,
			UpdateFluent,
			(s, c, f) => c.PutScript(s, f),
			(s, c, f) => c.PutScriptAsync(s, f),
			(s, c, r) => c.PutScript(r),
			(s, c, r) => c.PutScriptAsync(r)
		);

		private PutScriptRequest UpdateInitializer(string id) => new PutScriptRequest(id) { Script = new PainlessScript(_updatedScript) };

		private IPutScriptRequest UpdateFluent(string id, PutScriptDescriptor d) => d.Painless(_updatedScript);

		protected override LazyResponses Delete() => Calls<DeleteScriptDescriptor, DeleteScriptRequest, IDeleteScriptRequest, IDeleteScriptResponse>(
			id => new DeleteScriptRequest(id),
			(id, d) => d,
			(s, c, f) => c.DeleteScript(s, f),
			(s, c, f) => c.DeleteScriptAsync(s, f),
			(s, c, r) => c.DeleteScript(r),
			(s, c, r) => c.DeleteScriptAsync(r)
		);

		protected override void ExpectAfterUpdate(IGetScriptResponse response) => response.Script.Source.Should().Be(_updatedScript);

		protected override void ExpectDeleteNotFoundResponse(IDeleteScriptResponse response)
		{
			response.ServerError.Should().NotBeNull();
			response.ServerError.Status.Should().Be(404);
			response.ServerError.Error.Reason.Should().Contain("not exist");
		}
	}
}
