using System;
using Nest6;
#if DOTNETCORE
using System.Net.Http;
using TheException = System.Net.Http.HttpRequestException;
#else
using TheException = System.Net.WebException;
#endif

namespace Tests.Framework
{
	public interface IClientCallRule : IRule { }

	public class ClientCallRule : RuleBase<ClientCallRule>, IClientCallRule
	{
		private IClientCallRule Self => this;

		public ClientCallRule Fails(Union<TimesHelper.AllTimes, int> times, Union<Exception, int> errorState = null)
		{
			Self.Times = times;
			Self.Succeeds = false;
			Self.Return = errorState ?? new TheException();
			return this;
		}

		public ClientCallRule Succeeds(Union<TimesHelper.AllTimes, int> times, int? validResponseCode = 200)
		{
			Self.Times = times;
			Self.Succeeds = true;
			Self.Return = validResponseCode;
			return this;
		}

		public ClientCallRule AfterSucceeds(Union<Exception, int> errorState = null)
		{
			Self.AfterSucceeds = errorState;
			return this;
		}

		public ClientCallRule ThrowsAfterSucceeds()
		{
			Self.AfterSucceeds = new TheException();
			return this;
		}

		public ClientCallRule SucceedAlways(int? validResponseCode = 200) => Succeeds(TimesHelper.Always, validResponseCode);

		public ClientCallRule FailAlways(Union<Exception, int> errorState = null) => Fails(TimesHelper.Always, errorState);
	}
}
