﻿#pragma warning disable 612, 618
using Nest6;

namespace Tests.Mapping.Types.Geo.GeoShape
{
	public class GeoShapeTest
	{
		[GeoShape(
			Tree = GeoTree.Quadtree,
			Orientation = GeoOrientation.ClockWise,
			Strategy = GeoStrategy.Recursive,
			TreeLevels = 3,
			PointsOnly = true,
			DistanceErrorPercentage = 1.0,
			Coerce = true)]
		public object Full { get; set; }

		[GeoShape]
		public object Minimal { get; set; }
	}

	public class GeoShapeAttributeTests : AttributeTestsBase<GeoShapeTest>
	{
		protected override object ExpectJson => new
		{
			properties = new
			{
				full = new
				{
					type = "geo_shape",
					tree = "quadtree",
					orientation = "cw",
					strategy = "recursive",
					tree_levels = 3,
					points_only = true,
					distance_error_pct = 1.0,
					coerce = true
				},
				minimal = new
				{
					type = "geo_shape"
				}
			}
		};
	}
}
#pragma warning restore 612, 618
