using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Elastic.Xunit.XunitPlumbing;
using Elasticsearch.Net;
using FluentAssertions;
using Nest6;
using Tests.Core.Extensions;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Framework;
using Tests.Framework.Integration;

namespace Tests.Document.Single.Index
{
	public class TestDocument
	{
		static TestDocument()
		{
			using (var stream = typeof(TestDocument).Assembly.GetManifestResourceStream("Tests.Document.Single.Index.Attachment_Test_Document.pdf"))
			{
				using (var memoryStream = new MemoryStream())
				{
					stream.CopyTo(memoryStream);
					TestPdfDocument = Convert.ToBase64String(memoryStream.ToArray());
				}
			}
		}

		// Base 64 encoded version of Attachment_Test_Document.pdf
		public static string TestPdfDocument { get; }
	}

	public class IngestedAttachment
	{
		public Attachment Attachment { get; set; }
		public string Content { get; set; }
		public int Id { get; set; }
	}

	public class IndexIngestAttachmentApiTests
		: ApiIntegrationTestBase<IntrusiveOperationCluster, IIndexResponse,
			IIndexRequest<IngestedAttachment>,
			IndexDescriptor<IngestedAttachment>,
			IndexRequest<IngestedAttachment>>
	{
		public IndexIngestAttachmentApiTests(IntrusiveOperationCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override object ExpectJson => new { id = 1, content = Content };
		protected override int ExpectStatusCode => 201;

		protected override Func<IndexDescriptor<IngestedAttachment>, IIndexRequest<IngestedAttachment>> Fluent => s => s
			.Index(CallIsolatedValue)
			.Refresh(Refresh.True)
			.Pipeline(PipelineId);

		protected override HttpMethod HttpMethod => HttpMethod.PUT;

		protected override IndexRequest<IngestedAttachment> Initializer =>
			new IndexRequest<IngestedAttachment>(Document, CallIsolatedValue)
			{
				Refresh = Refresh.True,
				Pipeline = PipelineId
			};

		protected override bool SupportsDeserialization => false;

		protected override string UrlPath
			=> $"/{CallIsolatedValue}/ingestedattachment/1?refresh=true&pipeline={PipelineId}";

		private static string Content => TestDocument.TestPdfDocument;

		private IngestedAttachment Document => new IngestedAttachment
		{
			Id = 1,
			Content = Content
		};

		private static string PipelineId { get; } = "pipeline-" + RandomString();

		protected override void IntegrationSetup(IElasticClient client, CallUniqueValues values)
		{
			foreach (var value in values)
			{
				var index = value.Value;

				client.CreateIndex(index, c => c
					.Mappings(m => m
						.Map<IngestedAttachment>(mm => mm
							.Properties(p => p
								.Text(s => s
									.Name(f => f.Content)
								)
								.Object<Attachment>(o => o
									.Name(f => f.Attachment)
								)
							)
						)
					)
				);
			}

			client.PutPipeline(new PutPipelineRequest(PipelineId)
			{
				Description = "Attachment pipeline test",
				Processors = new List<IProcessor>
				{
					new AttachmentProcessor
					{
						Field = "content",
						TargetField = "attachment"
					}
				}
			});
		}

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.Index<IngestedAttachment>(Document, f),
			(client, f) => client.IndexAsync<IngestedAttachment>(Document, f),
			(client, r) => client.Index(r),
			(client, r) => client.IndexAsync(r)
		);

		protected override IndexDescriptor<IngestedAttachment> NewDescriptor() => new IndexDescriptor<IngestedAttachment>(Document);

		protected override void ExpectResponse(IIndexResponse response)
		{
			response.ShouldBeValid();

			var getResponse = Client.Get<IngestedAttachment>(response.Id, g => g.Index(CallIsolatedValue));

			getResponse.ShouldBeValid();
			getResponse.Source.Should().NotBeNull();

			getResponse.Source.Attachment.Should().NotBeNull();

			var attachment = getResponse.Source.Attachment;

			attachment.Title.Should().Be("Attachment Test Document");
			attachment.Keywords.Should().Be("nest,test,document");
			attachment.Date.Should().Be(new DateTime(2016, 12, 08, 3, 5, 13, DateTimeKind.Utc));
			attachment.ContentType.Should().Be("application/pdf");
			attachment.Author.Should().Be("Russ Cam");
			attachment.Language.Should().Be("fr");
			attachment.ContentLength.Should().Be(96);
			attachment.Content.Should().Contain("mapper-attachment support");
		}
	}

	public class ReindexAttachmentApiTests : IClusterFixture<IntrusiveOperationCluster>
	{
		private readonly IElasticClient _client;

		private static string RandomString { get; } = Guid.NewGuid().ToString("N").Substring(0, 8);
		private static string Index { get; } = "project" + RandomString;
		private static string PipelineId { get; } = "pipeline-" + RandomString;
		private static string Content => TestDocument.TestPdfDocument;

		private IngestedAttachment Document => new IngestedAttachment
		{
			Id = 1,
			Content = Content
		};

		public ReindexAttachmentApiTests(IntrusiveOperationCluster cluster)
		{
			_client = cluster.Client;

			_client.PutPipeline(new PutPipelineRequest(PipelineId)
			{
				Description = "Attachment pipeline test",
				Processors = new List<IProcessor>
				{
					new AttachmentProcessor
					{
						Field = "content",
						TargetField = "attachment"
					}
				}
			});

			var createIndexResponse = _client.CreateIndex(Index, c => c
				.Mappings(m => m
					.Map<IngestedAttachment>(mm => mm
						.Properties(p => p
							.Text(s => s
								.Name(f => f.Content)
							)
							.Object<Attachment>(o => o
								.Name(f => f.Attachment)
							)
						)
					)
				)
			);

			createIndexResponse.ShouldBeValid();
			var indexResponse = _client.Index<IngestedAttachment>(Document, i => i
				.Index(Index)
				.Refresh(Refresh.True)
				.Pipeline(PipelineId)
			);

			indexResponse.ShouldBeValid();
		}

		[I]
		public void ReindexIntoAnotherIndexCorrectlySerializesAttachment()
		{
			var getResponse = _client.Get<IngestedAttachment>(1, g => g
				.Index(Index)
			);

			getResponse.ShouldBeValid();
			var ingestedAttachment = getResponse.Source;
			ingestedAttachment.Should().NotBeNull();

			var indexResponse = _client.Index<IngestedAttachment>(ingestedAttachment, i => i
				.Index(Index + "2")
				.Refresh(Refresh.True)
			);

			indexResponse.ShouldBeValid();

			getResponse = _client.Get<IngestedAttachment>(1, g => g
				.Index(Index + "2")
			);

			getResponse.ShouldBeValid();
			ingestedAttachment.Should().NotBeNull();
			ingestedAttachment.Attachment.Title.Should().Be("Attachment Test Document");
			ingestedAttachment.Attachment.Keywords.Should().Be("nest,test,document");
			ingestedAttachment.Attachment.Date.Should().Be(new DateTime(2016, 12, 08, 3, 5, 13, DateTimeKind.Utc));
			ingestedAttachment.Attachment.ContentType.Should().Be("application/pdf");
			ingestedAttachment.Attachment.Author.Should().Be("Russ Cam");
			ingestedAttachment.Attachment.Language.Should().Be("fr");
			ingestedAttachment.Attachment.ContentLength.Should().Be(96);
		}
	}
}
