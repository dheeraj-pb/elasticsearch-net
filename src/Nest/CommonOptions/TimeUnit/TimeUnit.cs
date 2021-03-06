using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nest6
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum TimeUnit
	{
		[EnumMember(Value = "nanos")]
		Nanoseconds,

		[EnumMember(Value = "micros")]
		Microseconds,

		[EnumMember(Value = "ms")]
		Millisecond,

		[EnumMember(Value = "s")]
		Second,

		[EnumMember(Value = "m")]
		Minute,

		[EnumMember(Value = "h")]
		Hour,

		[EnumMember(Value = "d")]
		Day
	}

	public static class TimeUnitExtensions
	{
		public static string GetStringValue(this TimeUnit value)
		{
			switch (value)
			{
				case TimeUnit.Nanoseconds:
					return "nanos";
				case TimeUnit.Microseconds:
					return "micros";
				case TimeUnit.Millisecond:
					return "ms";
				case TimeUnit.Second:
					return "s";
				case TimeUnit.Minute:
					return "m";
				case TimeUnit.Hour:
					return "h";
				case TimeUnit.Day:
					return "d";
				default:
					throw new ArgumentOutOfRangeException(nameof(value), value, null);
			}
		}
	}
}
