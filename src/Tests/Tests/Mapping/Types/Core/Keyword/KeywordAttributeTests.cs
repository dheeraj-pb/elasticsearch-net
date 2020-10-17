﻿using System;
using Nest6;

namespace Tests.Mapping.Types.Core.Keyword
{
	public class KeywordTest
	{
		public char Char { get; set; }

		[Keyword(
			Boost = 1.2,
			EagerGlobalOrdinals = true,
			IgnoreAbove = 50,
			Index = false,
			IndexOptions = IndexOptions.Offsets,
			NullValue = "null",
			Norms = false
		)]
		public string Full { get; set; }

		public Guid Guid { get; set; }

		[Keyword]
		public string Minimal { get; set; }
	}

	public class KeywordAttributeTests : AttributeTestsBase<KeywordTest>
	{
		protected override object ExpectJson => new
		{
			properties = new
			{
				full = new
				{
					type = "keyword",
					boost = 1.2,
					eager_global_ordinals = true,
					ignore_above = 50,
					index = false,
					index_options = "offsets",
					null_value = "null",
					norms = false
				},
				minimal = new
				{
					type = "keyword"
				},
				@char = new
				{
					type = "keyword"
				},
				@guid = new
				{
					type = "keyword"
				}
			}
		};
	}
}
