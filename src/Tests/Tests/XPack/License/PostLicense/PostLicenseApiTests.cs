﻿using System;
using Elastic.Xunit.XunitPlumbing;
using Elasticsearch.Net;
using Nest6;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Framework;
using Tests.Framework.Integration;

namespace Tests.XPack.License.PostLicense
{
	[SkipVersion("<2.3.0", "")]
	public class PostLicenseApiTests : ApiTestBase<XPackCluster, IPostLicenseResponse, IPostLicenseRequest, PostLicenseDescriptor, PostLicenseRequest>
	{
		public PostLicenseApiTests(XPackCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override object ExpectJson { get; } = new
		{
			license = new
			{
				expiry_date_in_millis = 1,
				issue_date_in_millis = 2,
				issued_to = "nest test framework",
				issuer = "martijn",
				max_nodes = 20,
				signature = "<redacted>",
				type = "gold",
				uid = "uuid"
			}
		};

		protected override Func<PostLicenseDescriptor, IPostLicenseRequest> Fluent => d => d
			.Acknowledge()
			.License(FakeLicense);

		protected override HttpMethod HttpMethod => HttpMethod.PUT;

		protected override PostLicenseRequest Initializer => new PostLicenseRequest
		{
			Acknowledge = true,
			License = FakeLicense
		};

		protected override bool SupportsDeserialization => false;
		protected override string UrlPath => $"/_xpack/license?acknowledge=true";

		private Nest6.License FakeLicense { get; } = new Nest6.License
		{
			UID = "uuid",
			ExpiryDateInMilliseconds = 1,
			IssueDateInMilliseconds = 2,
			IssuedTo = "nest test framework",
			Issuer = "martijn",
			Type = LicenseType.Gold,
			MaxNodes = 20,
			Signature = "<redacted>"
		};

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.PostLicense(f),
			(client, f) => client.PostLicenseAsync(f),
			(client, r) => client.PostLicense(r),
			(client, r) => client.PostLicenseAsync(r)
		);
	}
}
