﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using Nest6;

namespace Tests.QueryDsl
{
	public abstract class NotConditionlessWhen : List<Action<QueryContainer>> { }

	public class NotConditionlessWhen<TQuery> : NotConditionlessWhen where TQuery : IQuery
	{
		private readonly Func<IQueryContainer, TQuery> _dispatch;

		public NotConditionlessWhen(Func<IQueryContainer, TQuery> dispatch) => _dispatch = dispatch;

		public void Add(Action<TQuery> when) => Add(q => Assert(q, when));

		private void Assert(IQueryContainer c, Action<TQuery> when)
		{
			var q = _dispatch(c);
			when(q);
			q.Conditionless.Should().BeFalse();
			c.IsConditionless.Should().BeFalse();
		}
	}
}
