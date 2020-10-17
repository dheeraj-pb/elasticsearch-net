﻿using System;
using Nest6;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Domain;
using Tests.Framework.Integration;
using static Nest6.Infer;

namespace Tests.Mapping.Types.Core.Join
{
	public class JoinPropertyTests : PropertyTestsBase
	{
		public JoinPropertyTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override object ExpectJson => new
		{
			properties = new
			{
				name = new
				{
					type = "join",
					relations = new
					{
						project = "commits"
					}
				}
			}
		};

		protected override Func<PropertiesDescriptor<Project>, IPromise<IProperties>> FluentProperties => f => f
			.Join(pr => pr
				.Name(p => p.Name)
				.Relations(r => r.Join<Project, CommitActivity>())
			);


		protected override IProperties InitializerProperties => new Properties
		{
			{
				"name", new JoinProperty
				{
					Relations = new Relations
					{
						{ Relation<Project>(), Relation<CommitActivity>() }
					}
				}
			}
		};
	}

	public class JoinPropertyComplexTests : PropertyTestsBase
	{
		public JoinPropertyComplexTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override object ExpectJson => new
		{
			properties = new
			{
				name = new
				{
					type = "join",
					relations = new
					{
						project = "commits",
						parent2 = new[] { "child2", "child3" }
					}
				}
			}
		};

		protected override Func<PropertiesDescriptor<Project>, IPromise<IProperties>> FluentProperties => f => f
			.Join(pr => pr
				.Name(p => p.Name)
				.Relations(r => r
					.Join<Project, CommitActivity>()
					.Join("parent2", "child2", "child3")
				)
			);


		protected override IProperties InitializerProperties => new Properties
		{
			{
				"name", new JoinProperty
				{
					Relations = new Relations
					{
						{ Relation<Project>(), typeof(CommitActivity) },
						{ "parent2", "child2", "child3" }
					}
				}
			}
		};
	}
}
