﻿using System.Collections.Generic;
using Elastic.Xunit.XunitPlumbing;
using FluentAssertions;
using Nest6;
using Tests.Core.Client;
using Tests.Core.Extensions;

namespace Tests.XPack.Migration.MigrationAssistance
{
	public class MigrationAssistanceApiTests
	{
		[U] public void ShouldDeserialize()
		{
			var fixedResponse = new
			{
				indices = new Dictionary<string, object>()
				{
					{ ".watches", new { action_required = "upgrade" } },
					{ ".security", new { action_required = "upgrade" } },
					{ "my_old_index", new { action_required = "reindex" } },
					{ "my_other_old_index", new { action_required = "reindex" } },
				}
			};

			var client = FixedResponseClient.Create(fixedResponse);

			var response = client.MigrationAssistance();
			response.ShouldBeValid();
			response.Indices.Should().NotBeNull().And.HaveCount(4);

			foreach (var index in response.Indices)
			{
				index.Value.Should().NotBeNull();
				if (index.Key.Name == ".watches" || index.Key.Name == ".security")
					index.Value.ActionRequired.Should().Be(UpgradeActionRequired.Upgrade);
				else
					index.Value.ActionRequired.Should().Be(UpgradeActionRequired.Reindex);
			}
		}
	}
}
