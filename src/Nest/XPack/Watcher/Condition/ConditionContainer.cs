﻿using System;
using Newtonsoft.Json;

namespace Nest6
{
	/// <summary>
	/// Conditions for a watch
	/// </summary>
	[JsonObject]
	[ContractJsonConverter(typeof(ReserializeJsonConverter<ConditionContainer, IConditionContainer>))]
	public interface IConditionContainer
	{
		/// <summary>
		/// Forces the watch actions to be executed unless they are throttled.
		/// </summary>
		[JsonProperty("always")]
		IAlwaysCondition Always { get; set; }

		/// <summary>
		/// Compares an array of values in the execution context to a given value.
		/// </summary>
		[JsonProperty("array_compare")]
		IArrayCompareCondition ArrayCompare { get; set; }

		/// <summary>
		/// Performs a simple comparison against a value in the watch payload.
		/// </summary>
		[JsonProperty("compare")]
		ICompareCondition Compare { get; set; }

		/// <summary>
		/// Watch actions are never executed when the watch is triggered.
		/// The watch input is executed, a record is added to the watch history,
		/// and the watch execution ends.
		/// </summary>
		/// <remarks>
		/// This condition is generally used for testing.
		/// </remarks>
		[JsonProperty("never")]
		INeverCondition Never { get; set; }

		/// <summary>
		/// A watch condition that evaluates a script.
		/// The default scripting language is groovy.
		/// You can use any of the scripting languages supported by Elasticsearch as long as the
		/// language supports evaluating expressions to Boolean values.
		/// Note that the mustache and expression languages are too limited to be used by this condition.
		/// </summary>
		/// <remarks>
		/// For Groovy, you must explicitly enable dynamic scripts in elasticsearch.yml
		/// to use inline or stored scripts.
		/// To enable groovy scripting for watches only,
		/// you can set script.engine.groovy.inline.xpack_watch: true.
		/// </remarks>
		[JsonProperty("script")]
		IScriptCondition Script { get; set; }
	}

	/// <inheritdoc />
	public class ConditionContainer : IConditionContainer, IDescriptor
	{
		internal ConditionContainer() { }

		public ConditionContainer(ConditionBase condition)
		{
			condition.ThrowIfNull(nameof(condition));
			condition.WrapInContainer(this);
		}

		/// <inheritdoc />
		IAlwaysCondition IConditionContainer.Always { get; set; }

		/// <inheritdoc />
		IArrayCompareCondition IConditionContainer.ArrayCompare { get; set; }

		/// <inheritdoc />
		ICompareCondition IConditionContainer.Compare { get; set; }

		/// <inheritdoc />
		INeverCondition IConditionContainer.Never { get; set; }

		/// <inheritdoc />
		IScriptCondition IConditionContainer.Script { get; set; }

		public static implicit operator ConditionContainer(ConditionBase condition) => condition == null
			? null
			: new ConditionContainer(condition);
	}

	/// <inheritdoc />
	public class ConditionDescriptor : ConditionContainer
	{
		private ConditionDescriptor Assign<TValue>(TValue value, Action<IConditionContainer, TValue> assigner) => Fluent.Assign(this, value, assigner);

		/// <inheritdoc />
		public ConditionDescriptor Always() => Assign(new AlwaysCondition(), (a,v) => a.Always = v);

		/// <inheritdoc />
		public ConditionDescriptor Never() => Assign(new NeverCondition(), (a, v) => a.Never = v);

		/// <inheritdoc />
		public ConditionDescriptor Compare(Func<CompareConditionDescriptor, ICompareCondition> selector) =>
			Assign(selector, (a, v) => a.Compare = v?.Invoke(new CompareConditionDescriptor()));

		/// <inheritdoc />
		public ConditionDescriptor ArrayCompare(Func<ArrayCompareConditionDescriptor, IArrayCompareCondition> selector) =>
			Assign(selector,(a, v) => a.ArrayCompare = v?.Invoke(new ArrayCompareConditionDescriptor()));

		/// <inheritdoc />
		public ConditionDescriptor Script(Func<ScriptConditionDescriptor, IScriptCondition> selector) =>
			Assign(selector, (a, v) => a.Script = v?.Invoke(new ScriptConditionDescriptor()));
	}
}
