using System;
using Nest6;

namespace Tests.Framework
{
	public interface ISniffRule : IRule
	{
		VirtualCluster NewClusterState { get; set; }
	}

	public class SniffRule : RuleBase<SniffRule>, ISniffRule
	{
		VirtualCluster ISniffRule.NewClusterState { get; set; }
		private ISniffRule Self => this;

		public SniffRule Fails(Union<TimesHelper.AllTimes, int> times, Union<Exception, int> errorState = null)
		{
			Self.Times = times;
			Self.Succeeds = false;
			Self.Return = errorState;
			return this;
		}

		public SniffRule Succeeds(Union<TimesHelper.AllTimes, int> times, VirtualCluster cluster = null)
		{
			Self.Times = times;
			Self.Succeeds = true;
			Self.NewClusterState = cluster;
			Self.Return = 200;
			return this;
		}

		public SniffRule SucceedAlways(VirtualCluster cluster = null) => Succeeds(TimesHelper.Always, cluster);

		public SniffRule FailAlways(Union<Exception, int> errorState = null) => Fails(TimesHelper.Always, errorState);
	}
}
