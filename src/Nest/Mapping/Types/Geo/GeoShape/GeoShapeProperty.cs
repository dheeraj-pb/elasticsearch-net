using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Nest6
{
	/// <summary>
	/// Maps a property as a geo_shape field
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public interface IGeoShapeProperty : IDocValuesProperty
	{
		/// <summary>
		/// Used as a hint to the Prefix<see cref="Tree" /> about how precise it should be.
		/// Defaults to 0.025 (2.5%) with 0.5 as the maximum supported value.
		/// </summary>
		/// <remarks>
		/// NOTE: This value will default to 0 if a <see cref="Precision" /> or <see cref="TreeLevels" /> definition
		/// is explicitly defined. This guarantees spatial precision at the level defined in the mapping.
		/// This can lead to significant memory usage for high resolution shapes with low error
		/// (e.g. large shapes at 1m with &lt; 0.001 error).
		/// To improve indexing performance (at the cost of query accuracy) explicitly define <see cref="TreeLevels" /> or
		/// <see cref="Precision" /> along with a reasonable <see cref="DistanceErrorPercentage" />,
		/// noting that large shapes will have greater false positives.
		/// </remarks>
		[JsonProperty("distance_error_pct")]
		[Obsolete("Removed in Elasticsearch 6.6")]
		double? DistanceErrorPercentage { get; set; }

		/// <summary>
		/// If <c>true</c>, malformed geojson shapes are ignored. If false (default),
		/// malformed geojson shapes throw an exception and reject the whole document.
		/// </summary>
		/// <remarks>
		/// Valid for Elasticsearch 6.1.0+
		/// </remarks>
		[JsonProperty("ignore_malformed")]
		bool? IgnoreMalformed { get; set; }

		/// <summary>
		/// If true (default) three dimension points will be accepted (stored in source) but
		/// only latitude and longitude values will be indexed; the third dimension is ignored. If false,
		/// geo-points containing any more than latitude and longitude (two dimensions) values throw
		/// an exception and reject the whole document.
		/// </summary>
		/// <remarks>
		/// Valid for Elasticsearch 6.3.0+
		/// </remarks>
		[JsonProperty("ignore_z_value")]
		bool? IgnoreZValue { get; set; }

		/// <summary>
		/// Defines how to interpret vertex order for polygons and multipolygons.
		/// Defaults to <see cref="GeoOrientation.CounterClockWise" />
		/// </summary>
		[JsonProperty("orientation")]
		GeoOrientation? Orientation { get; set; }

		/// <summary>
		/// Configures the geo_shape field type for point shapes only. Defaults to <c>false</c>.
		/// This optimizes index and search performance
		/// for the geohash and quadtree when it is known that only points will be indexed.
		/// At present geo_shape queries can not be executed on geo_point field types.
		/// This option bridges the gap by improving point performance on a geo_shape field
		/// so that geo_shape queries are optimal on a point only field.
		/// </summary>
		[JsonProperty("points_only")]
		[Obsolete("Removed in Elasticsearch 6.6")]
		bool? PointsOnly { get; set; }

		/// <summary>
		/// Used instead of <see cref="TreeLevels" /> to set an appropriate value for the <see cref="TreeLevels" />
		/// parameter. The value specifies the desired precision and Elasticsearch will calculate
		/// the best tree_levels value to honor this precision.
		/// </summary>
		[JsonProperty("precision")]
		[Obsolete("Removed in Elasticsearch 6.6")]
		Distance Precision { get; set; }

		/// <summary>
		/// Defines the approach for how to represent shapes at indexing and search time.
		/// It also influences the capabilities available so it is recommended to let
		/// Elasticsearch set this parameter automatically.
		/// </summary>
		[JsonProperty("strategy")]
		[Obsolete("Removed in Elasticsearch 6.6")]
		GeoStrategy? Strategy { get; set; }

		/// <summary>
		/// Name of the PrefixTree implementation to be used.
		/// Defaults to <see cref="GeoTree.Geohash" />
		/// </summary>
		[Obsolete("Removed in Elasticsearch 6.6")]
		[JsonProperty("tree")]
		GeoTree? Tree { get; set; }

		/// <summary>
		/// Maximum number of layers to be used by the Prefix<see cref="Tree" />. This can be used to control the
		/// precision of shape representations and therefore how many terms are indexed.
		/// Defaults to the default value of the chosen Prefix<see cref="Tree" /> implementation. Since this parameter requires a
		/// certain level of understanding of the underlying implementation, users may use the
		/// <see cref="Precision" /> parameter instead.
		/// </summary>
		[JsonProperty("tree_levels")]
		[Obsolete("Removed in Elasticsearch 6.6")]
		int? TreeLevels { get; set; }

		/// <summary>
		/// Should the data be coerced into becoming a valid geo shape (for instance closing a polygon)
		/// </summary>
		[JsonProperty("coerce")]
		bool? Coerce { get; set; }
	}

	/// <inheritdoc cref="IGeoShapeProperty" />
	[DebuggerDisplay("{DebugDisplay}")]
	public class GeoShapeProperty : DocValuesPropertyBase, IGeoShapeProperty
	{
		public GeoShapeProperty() : base(FieldType.GeoShape) { }

		/// <inheritdoc />
		[Obsolete("Removed in Elasticsearch 6.6")]
		public double? DistanceErrorPercentage { get; set; }

		/// <inheritdoc />
		public bool? IgnoreMalformed { get; set; }

		/// <inheritdoc />
		public bool? IgnoreZValue { get; set; }

		/// <inheritdoc />
		public GeoOrientation? Orientation { get; set; }

		/// <inheritdoc />
		[Obsolete("Removed in Elasticsearch 6.6")]
		public bool? PointsOnly { get; set; }

		/// <inheritdoc />
		[Obsolete("Removed in Elasticsearch 6.6")]
		public Distance Precision { get; set; }

		/// <inheritdoc />
		[Obsolete("Removed in Elasticsearch 6.6")]
		public GeoStrategy? Strategy { get; set; }

		/// <inheritdoc />
		[Obsolete("Removed in Elasticsearch 6.6")]
		public GeoTree? Tree { get; set; }

		/// <inheritdoc />
		[Obsolete("Removed in Elasticsearch 6.6")]
		public int? TreeLevels { get; set; }

		/// <inheritdoc />
		public bool? Coerce { get; set; }
	}

	/// <inheritdoc cref="IGeoShapeProperty" />
	[DebuggerDisplay("{DebugDisplay}")]
	public class GeoShapePropertyDescriptor<T>
		: DocValuesPropertyDescriptorBase<GeoShapePropertyDescriptor<T>, IGeoShapeProperty, T>, IGeoShapeProperty
		where T : class
	{
		public GeoShapePropertyDescriptor() : base(FieldType.GeoShape) { }

		bool? IGeoShapeProperty.IgnoreMalformed { get; set; }
		bool? IGeoShapeProperty.IgnoreZValue { get; set; }

		GeoOrientation? IGeoShapeProperty.Orientation { get; set; }

		bool? IGeoShapeProperty.Coerce { get; set; }

		[Obsolete("Removed in Elasticsearch 6.6")]
		double? IGeoShapeProperty.DistanceErrorPercentage { get; set; }

		[Obsolete("Removed in Elasticsearch 6.6")]
		bool? IGeoShapeProperty.PointsOnly { get; set; }

		[Obsolete("Removed in Elasticsearch 6.6")]
		Distance IGeoShapeProperty.Precision { get; set; }

		[Obsolete("Removed in Elasticsearch 6.6")]
		GeoStrategy? IGeoShapeProperty.Strategy { get; set; }

		[Obsolete("Removed in Elasticsearch 6.6")]
		GeoTree? IGeoShapeProperty.Tree { get; set; }

		[Obsolete("Removed in Elasticsearch 6.6")]
		int? IGeoShapeProperty.TreeLevels { get; set; }

		/// <inheritdoc cref="IGeoShapeProperty.Tree" />
		[Obsolete("Removed in Elasticsearch 6.6")]
		public GeoShapePropertyDescriptor<T> Tree(GeoTree? tree) => Assign(tree, (a, v) => a.Tree = v);

		/// <inheritdoc cref="IGeoShapeProperty.TreeLevels" />
		[Obsolete("Removed in Elasticsearch 6.6")]
		public GeoShapePropertyDescriptor<T> TreeLevels(int? treeLevels) => Assign(treeLevels, (a, v) => a.TreeLevels = v);

		/// <inheritdoc cref="IGeoShapeProperty.Strategy" />
		[Obsolete("Removed in Elasticsearch 6.6")]
		public GeoShapePropertyDescriptor<T> Strategy(GeoStrategy? strategy) => Assign(strategy, (a, v) => a.Strategy = v);

		/// <inheritdoc cref="IGeoShapeProperty.Precision" />
		[Obsolete("Removed in Elasticsearch 6.6")]
		public GeoShapePropertyDescriptor<T> Precision(double precision, DistanceUnit unit) =>
			Assign(new Distance(precision, unit), (a, v) => a.Precision = v);

		/// <inheritdoc cref="IGeoShapeProperty.Orientation" />
		public GeoShapePropertyDescriptor<T> Orientation(GeoOrientation? orientation) => Assign(orientation, (a, v) => a.Orientation = v);

		/// <inheritdoc cref="IGeoShapeProperty.DistanceErrorPercentage" />
		[Obsolete("Removed in Elasticsearch 6.6")]
		public GeoShapePropertyDescriptor<T> DistanceErrorPercentage(double? distanceErrorPercentage) =>
			Assign(distanceErrorPercentage, (a, v) => a.DistanceErrorPercentage = v);

		/// <inheritdoc cref="IGeoShapeProperty.PointsOnly" />
		[Obsolete("Removed in Elasticsearch 6.6")]
		public GeoShapePropertyDescriptor<T> PointsOnly(bool? pointsOnly = true) => Assign(pointsOnly, (a, v) => a.PointsOnly = v);

		/// <inheritdoc cref="IGeoShapeProperty.IgnoreMalformed" />
		public GeoShapePropertyDescriptor<T> IgnoreMalformed(bool? ignoreMalformed = true) =>
			Assign(ignoreMalformed, (a, v) => a.IgnoreMalformed = v);

		/// <inheritdoc cref="IGeoShapeProperty.IgnoreZValue" />
		public GeoShapePropertyDescriptor<T> IgnoreZValue(bool? ignoreZValue = true) =>
			Assign(ignoreZValue, (a, v) => a.IgnoreZValue = v);

		/// <inheritdoc cref="IGeoShapeProperty.Coerce" />
		public GeoShapePropertyDescriptor<T> Coerce(bool? coerce = true) =>
			Assign(coerce, (a, v) => a.Coerce = v);
	}
}
