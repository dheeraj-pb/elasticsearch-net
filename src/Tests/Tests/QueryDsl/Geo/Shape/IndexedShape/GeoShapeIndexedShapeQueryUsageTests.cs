﻿using Elastic.Xunit.XunitPlumbing;
using Nest6;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Domain;
using Tests.Framework.Integration;
using static Nest6.Infer;

namespace Tests.QueryDsl.Geo.Shape.IndexedShape
{
	/**
	 * The GeoShape IndexedShape Query supports using a shape which has already been indexed in another index and/or index type within a geoshape query.
	 * This is particularly useful for when you have a pre-defined list of shapes which are useful to your application and you want to reference this
	 * using a logical name (for example __New Zealand__), rather than having to provide their coordinates within the request each time.
	 *
	 * See the Elasticsearch documentation on {ref_current}/query-dsl-geo-shape-query.html[geoshape queries] for more detail.
	 */
	[SkipVersion("<6.4.0", "Routing value parsed only in 6.4.0+. See https://github.com/elastic/elasticsearch/commit/89aa7bddfbab22ddd5478663717cf825991f84f9")]
	public class GeoShapeIndexedShapeQueryUsageTests : QueryDslUsageTestsBase
	{
		public GeoShapeIndexedShapeQueryUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

		protected override ConditionlessWhen ConditionlessWhen =>
			new ConditionlessWhen<IGeoIndexedShapeQuery>(a => a.GeoShape as IGeoIndexedShapeQuery)
			{
				q => q.Field = null,
				q => q.IndexedShape = null,
				q => q.IndexedShape.Id = null,
				q => q.IndexedShape.Index = null,
				q => q.IndexedShape.Type = null,
				q => q.IndexedShape.Path = null,
			};

		protected override QueryContainer QueryInitializer => new GeoIndexedShapeQuery
		{
			Name = "named_query",
			Boost = 1.1,
			Field = Field<Project>(p => p.LocationShape),
			IndexedShape = new FieldLookup
			{
				Id = Project.Instance.Name,
				Index = Index<Project>(),
				Type = Type<Project>(),
				Path = Field<Project>(p => p.LocationShape),
				Routing = Project.Instance.Name
			},
			Relation = GeoShapeRelation.Intersects
		};

		protected override object QueryJson => new
		{
			geo_shape = new
			{
				_name = "named_query",
				boost = 1.1,
				locationShape = new
				{
					indexed_shape = new
					{
						id = Project.Instance.Name,
						index = "project",
						type = "doc",
						path = "locationShape",
						routing = Project.Instance.Name
					},
					relation = "intersects"
				}
			}
		};

		protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
			.GeoIndexedShape(c => c
				.Name("named_query")
				.Boost(1.1)
				.Field(p => p.LocationShape)
				.IndexedShape(p => p
					.Id(Project.Instance.Name)
					.Path(pp => pp.LocationShape)
					.Routing(Project.Instance.Name)
				)
				.Relation(GeoShapeRelation.Intersects)
			);
	}
}
