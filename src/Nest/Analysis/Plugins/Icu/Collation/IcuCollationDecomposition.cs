﻿using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nest6
{
	/// <summary>
	/// Setting this decomposition property to canonical allows the Collator to handle unnormalized
	/// text properly, producing the same results as if the text were normalized. If no is set, it
	/// is the user’s responsibility to insure that all text is already in the appropriate form
	/// before a comparison or before getting a CollationKey. Adjusting decomposition mode
	/// allows the user to select between faster and more complete collation behavior. Since a
	/// great many of the world’s languages do not require text normalization, most locales
	/// set no as the default decomposition mode.
	/// </summary>
	/// <remarks>
	/// Requires analysis-icu plugin to be installed
	/// </remarks>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum IcuCollationDecomposition
	{
		[EnumMember(Value = "no")] No,
		[EnumMember(Value = "identical")] Canonical
	}
}
