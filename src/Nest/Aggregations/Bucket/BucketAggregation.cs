using System;
using System.Collections.Generic;

namespace Nest6
{
	public interface IBucketAggregation : IAggregation
	{
		AggregationDictionary Aggregations { get; set; }
	}

	public abstract class BucketAggregationBase : AggregationBase, IBucketAggregation
	{
		internal BucketAggregationBase() { }

		protected BucketAggregationBase(string name) : base(name) { }

		public AggregationDictionary Aggregations { get; set; }
	}

	public abstract class BucketAggregationDescriptorBase<TBucketAggregation, TBucketAggregationInterface, T>
		: IBucketAggregation, IDescriptor
		where TBucketAggregation : BucketAggregationDescriptorBase<TBucketAggregation, TBucketAggregationInterface, T>
		, TBucketAggregationInterface, IBucketAggregation
		where T : class
		where TBucketAggregationInterface : class, IBucketAggregation
	{
		protected TBucketAggregationInterface Self => (TBucketAggregation)this;
		AggregationDictionary IBucketAggregation.Aggregations { get; set; }

		IDictionary<string, object> IAggregation.Meta { get; set; }

		string IAggregation.Name { get; set; }

		[Obsolete("Use the overload that accepts TValue")]
		protected TBucketAggregation Assign(Action<TBucketAggregationInterface> assigner) =>
			Fluent.Assign((TBucketAggregation)this, assigner);

		protected TBucketAggregation Assign<TValue>(TValue value, Action<TBucketAggregationInterface, TValue> assigner) =>
			Fluent.Assign((TBucketAggregation)this, value, assigner);

		public TBucketAggregation Aggregations(Func<AggregationContainerDescriptor<T>, IAggregationContainer> selector) =>
			Assign(selector, (a, v) => a.Aggregations = v?.Invoke(new AggregationContainerDescriptor<T>())?.Aggregations);

		public TBucketAggregation Aggregations(AggregationDictionary aggregations) =>
			Assign(aggregations, (a, v) => a.Aggregations = v);

		public TBucketAggregation Meta(Func<FluentDictionary<string, object>, FluentDictionary<string, object>> selector) =>
			Assign(selector, (a, v) => a.Meta = v?.Invoke(new FluentDictionary<string, object>()));
	}
}
