﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;

namespace Nest6
{
	public partial interface IElasticClient
	{
		/// <inheritdoc />
		ICatResponse<CatHealthRecord> CatHealth(Func<CatHealthDescriptor, ICatHealthRequest> selector = null);

		/// <inheritdoc />
		ICatResponse<CatHealthRecord> CatHealth(ICatHealthRequest request);

		/// <inheritdoc />
		Task<ICatResponse<CatHealthRecord>> CatHealthAsync(
			Func<CatHealthDescriptor, ICatHealthRequest> selector = null,
			CancellationToken cancellationToken = default(CancellationToken)
		);

		/// <inheritdoc />
		Task<ICatResponse<CatHealthRecord>> CatHealthAsync(ICatHealthRequest request, CancellationToken cancellationToken = default(CancellationToken)
		);
	}

	public partial class ElasticClient
	{
		/// <inheritdoc />
		public ICatResponse<CatHealthRecord> CatHealth(Func<CatHealthDescriptor, ICatHealthRequest> selector = null) =>
			CatHealth(selector.InvokeOrDefault(new CatHealthDescriptor()));

		/// <inheritdoc />
		public ICatResponse<CatHealthRecord> CatHealth(ICatHealthRequest request) =>
			DoCat<ICatHealthRequest, CatHealthRequestParameters, CatHealthRecord>(request,
				LowLevelDispatch.CatHealthDispatch<CatResponse<CatHealthRecord>>);

		/// <inheritdoc />
		public Task<ICatResponse<CatHealthRecord>> CatHealthAsync(
			Func<CatHealthDescriptor, ICatHealthRequest> selector = null,
			CancellationToken cancellationToken = default(CancellationToken)
		) => CatHealthAsync(selector.InvokeOrDefault(new CatHealthDescriptor()), cancellationToken);

		/// <inheritdoc />
		public Task<ICatResponse<CatHealthRecord>> CatHealthAsync(ICatHealthRequest request,
			CancellationToken cancellationToken = default(CancellationToken)
		) =>
			DoCatAsync<ICatHealthRequest, CatHealthRequestParameters, CatHealthRecord>(request, cancellationToken,
				LowLevelDispatch.CatHealthDispatchAsync<CatResponse<CatHealthRecord>>);
	}
}
