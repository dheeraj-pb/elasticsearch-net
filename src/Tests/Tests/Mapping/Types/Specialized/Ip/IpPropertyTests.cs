﻿using System;
using Nest6;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Domain;
using Tests.Framework.Integration;

namespace Tests.Mapping.Types.Core.Ip
{
	public class IpPropertyTests : PropertyTestsBase
	{
		public IpPropertyTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override object ExpectJson => new
		{
			properties = new
			{
				name = new
				{
					type = "ip",
					index = false,
					boost = 1.3,
					null_value = "127.0.0.1",
					doc_values = true,
					store = true,
				}
			}
		};

		protected override Func<PropertiesDescriptor<Project>, IPromise<IProperties>> FluentProperties => f => f
			.Ip(s => s
				.Name(p => p.Name)
				.Index(false)
				.Boost(1.3)
				.NullValue("127.0.0.1")
				.DocValues()
				.Store()
			);


		protected override IProperties InitializerProperties => new Properties
		{
			{
				"name", new IpProperty
				{
					Index = false,
					Boost = 1.3,
					NullValue = "127.0.0.1",
					DocValues = true,
					Store = true,
				}
			}
		};
	}
}
