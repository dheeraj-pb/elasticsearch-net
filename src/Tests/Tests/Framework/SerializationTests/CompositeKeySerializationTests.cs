using System.Collections.Generic;
using System.IO;
using System.Text;
using Elastic.Xunit.XunitPlumbing;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.Client;
using Tests.Core.Extensions;

namespace Tests.Framework.SerializationTests
{
	public class CompositeKeySerializationTests
	{
		[U] public void NullValuesAreSerialized()
		{
			var compositeKey = new CompositeKey(new Dictionary<string, object>
			{
				{ "key_1", "value_1" },
				{ "key_2", null },
			});

			var serializer = TestClient.Default.RequestResponseSerializer;
			var json = serializer.SerializeToString(compositeKey, SerializationFormatting.None);
			json.Should().Be("{\"key_1\":\"value_1\",\"key_2\":null}");

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
			{
				stream.Position = 0;
				var dictionary = serializer.Deserialize<IReadOnlyDictionary<string, object>>(stream);
				var deserializedCompositeKey = new CompositeKey(dictionary);
				compositeKey.Should().Equal(deserializedCompositeKey);
			}
		}
	}
}
