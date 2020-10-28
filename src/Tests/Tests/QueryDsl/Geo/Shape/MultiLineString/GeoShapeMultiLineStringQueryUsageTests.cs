﻿using System.Collections.Generic;
using Nest6;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Domain;
using Tests.Framework.Integration;
using static Nest6.Infer;

namespace Tests.QueryDsl.Geo.Shape.MultiLineString
{
	public class GeoShapeMultiLineStringQueryUsageTests : GeoShapeQueryUsageTestsBase
	{
		private readonly IEnumerable<IEnumerable<GeoCoordinate>> _coordinates = new[]
		{
			new GeoCoordinate[] { new[] { 12.0, 2.0 }, new[] { 13.0, 2.0 }, new[] { 13.0, 3.0 }, new[] { 12.0, 3.0 } },
			new GeoCoordinate[] { new[] { 10.0, 0.0 }, new[] { 11.0, 0.0 }, new[] { 11.0, 1.0 }, new[] { 10.0, 1.0 } },
			new GeoCoordinate[] { new[] { 10.2, 0.2 }, new[] { 10.8, 0.2 }, new[] { 10.8, 0.8 }, new[] { 12.0, 0.8 } },
		};

		public GeoShapeMultiLineStringQueryUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

		protected override ConditionlessWhen ConditionlessWhen =>
			new ConditionlessWhen<IGeoShapeMultiLineStringQuery>(a => a.GeoShape as IGeoShapeMultiLineStringQuery)
			{
				q => q.Field = null,
				q => q.Shape = null,
				q => q.Shape.Coordinates = null,
			};

		protected override QueryContainer QueryInitializer => new GeoShapeMultiLineStringQuery
		{
			Name = "named_query",
			Boost = 1.1,
			Field = Field<Project>(p => p.Location),
			Shape = new MultiLineStringGeoShape(_coordinates),
			Relation = GeoShapeRelation.Intersects,
		};

		protected override object ShapeJson => new
		{
			type = "multilinestring",
			coordinates = _coordinates
		};

		protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
			.GeoShapeMultiLineString(c => c
				.Name("named_query")
				.Boost(1.1)
				.Field(p => p.Location)
				.Coordinates(_coordinates)
				.Relation(GeoShapeRelation.Intersects)
			);
	}
}
