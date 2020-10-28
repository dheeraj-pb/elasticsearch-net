﻿using System;
using System.Threading.Tasks;
using Elastic.Xunit;
using Elastic.Xunit.XunitPlumbing;
using FluentAssertions;
using Nest6;
using Tests.Core.Client;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Core.Serialization;
using Tests.Framework.Integration;

namespace Tests.Analysis
{
	public interface IAnalysisAssertion
	{
		object Json { get; }
		string Name { get; }
	}

	public interface IAnalysisAssertion<out TComponent, out TContainer, in TDescriptor> : IAnalysisAssertion
		where TContainer : class
	{
		Func<string, TDescriptor, IPromise<TContainer>> Fluent { get; }
		TComponent Initializer { get; }
	}

	[IntegrationTestCluster(typeof(WritableCluster))]
	public abstract class AnalysisComponentTestBase<TAssertion, TComponent, TContainer, TDescriptor>
		: IAnalysisAssertion<TComponent, TContainer, TDescriptor>
		where TAssertion : AnalysisComponentTestBase<TAssertion, TComponent, TContainer, TDescriptor>, new()
		where TContainer : class
	{
		private static readonly SingleEndpointUsage<ICreateIndexResponse> Usage = new SingleEndpointUsage<ICreateIndexResponse>
		(
			(s, c) => c.CreateIndex(s, AssertionSetup.FluentCall),
			(s, c) => c.CreateIndexAsync(s, AssertionSetup.FluentCall),
			(s, c) => c.CreateIndex(AssertionSetup.InitializerCall(s)),
			(s, c) => c.CreateIndexAsync(AssertionSetup.InitializerCall(s)),
			$"test-{typeof(TAssertion).Name.ToLowerInvariant()}"
		)
		{
			OnAfterCall = c => c.DeleteIndex(Usage.CallUniqueValues.Value)
		};

		protected AnalysisComponentTestBase()
		{
			Client = (ElasticXunitRunner.CurrentCluster as INestTestCluster)?.Client ?? TestClient.DefaultInMemoryClient;
			Usage.KickOffOnce(Client, true);
		}

		public abstract Func<string, TDescriptor, IPromise<TContainer>> Fluent { get; }
		public abstract TComponent Initializer { get; }
		public abstract object Json { get; }

		public abstract string Name { get; }

		protected abstract object AnalysisJson { get; }
		protected static TAssertion AssertionSetup { get; } = new TAssertion();

		private IElasticClient Client { get; }

		private Func<CreateIndexDescriptor, ICreateIndexRequest> FluentCall => i => i.Settings(s => s.Analysis(FluentAnalysis));

		protected abstract IAnalysis FluentAnalysis(AnalysisDescriptor an);

		private CreateIndexRequest InitializerCall(string index) => new CreateIndexRequest(index)
		{
			Settings = new IndexSettings { Analysis = InitializerAnalysis() }
		};

		protected abstract Nest6.Analysis InitializerAnalysis();

		[U] public virtual async Task TestPutSettingsRequest() => await Usage.AssertOnAllResponses(r =>
		{
			var json = new { settings = new { analysis = AnalysisJson } };
			SerializationTestHelper.Expect(json).FromRequest(r);
		});

		[I] public virtual async Task TestPutSettingsResponse() => await Usage.AssertOnAllResponses(r =>
		{
			r.ApiCall.HttpStatusCode.Should().Be(200);
		});
	}
}
