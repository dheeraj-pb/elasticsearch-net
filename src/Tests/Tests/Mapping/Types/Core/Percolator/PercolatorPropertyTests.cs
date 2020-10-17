﻿using System;
using Nest6;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Domain;
using Tests.Framework.Integration;

namespace Tests.Mapping.Types.Core.Percolator
{
	public class PercolatorPropertyTests : PropertyTestsBase
	{
		public PercolatorPropertyTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override object ExpectJson => new
		{
			properties = new
			{
				name = new
				{
					type = "percolator"
				}
			}
		};

		protected override Func<PropertiesDescriptor<Project>, IPromise<IProperties>> FluentProperties => f => f
			.Percolator(pr => pr
				.Name(p => p.Name)
			);


		protected override IProperties InitializerProperties => new Properties
		{
			{ "name", new PercolatorProperty() }
		};
	}
}
