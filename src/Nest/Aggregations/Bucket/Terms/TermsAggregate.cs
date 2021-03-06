﻿namespace Nest6
{
	public class TermsAggregate<TKey> : MultiBucketAggregate<KeyedBucket<TKey>>
	{
		public long? DocCountErrorUpperBound { get; set; }
		public long? SumOtherDocCount { get; set; }
	}
}
