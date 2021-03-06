﻿using Newtonsoft.Json;

namespace Nest6
{
	public interface IPutRoleResponse : IResponse
	{
		[JsonProperty("role")]
		PutRoleStatus Role { get; }
	}

	public class PutRoleResponse : ResponseBase, IPutRoleResponse
	{
		public PutRoleStatus Role { get; internal set; }
	}

	public class PutRoleStatus
	{
		[JsonProperty("created")]
		public bool Created { get; internal set; }
	}
}
