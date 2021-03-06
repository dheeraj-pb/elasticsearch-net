﻿using Newtonsoft.Json;

namespace Nest6
{
	[JsonObject]
	public class RequestCacheStats
	{
		[JsonProperty("evictions")]
		public long Evictions { get; set; }

		[JsonProperty("hit_count")]
		public long HitCount { get; set; }

		[JsonProperty("memory_size")]
		public string MemorySize { get; set; }

		[JsonProperty("memory_size_in_bytes")]
		public long MemorySizeInBytes { get; set; }

		[JsonProperty("miss_count")]
		public long MissCount { get; set; }
	}
}
