using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchDemoProject.Suggest {

	public class CategoryObject {
		public string Language { get; set; }
		public CompletionField Suggest { get; set; }
	}

	public class CategorySuggest {
		private const string INDEX_NAME = "categorysuggest";
		private readonly ElasticClient client;

		public CategorySuggest() {
			// Connection
			var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
			var connectionSettings = new ConnectionSettings(pool).DefaultIndex(INDEX_NAME);
			client = new ElasticClient(connectionSettings);

			// Index configuration
			if (client.IndexExists(INDEX_NAME).Exists) client.DeleteIndex(INDEX_NAME);

			client.CreateIndex(INDEX_NAME, ci => ci
				.Mappings(m => m
					.Map<CategoryObject>(mm => mm
						.AutoMap()
						.Properties(p => p
							.Completion(c => c
								.Contexts(ctx => ctx
									.Category(csug => csug
										.Name("material")
										.Path("m")
									)
									.Category(csug => csug
										.Name("size")
										.Path("s")
									)
								)
								.Name(n => n.Suggest)
							)
						)
					)
				)
			);

			client.IndexMany(new[] {
				new CategoryObject() {
					Language = "Table",
					Suggest = new CompletionField()
					{
						Input = new[] { "Table" },
						Contexts = new Dictionary<string, IEnumerable<string>>
						{
							{ "material", new [] { "wood", "steel", "plastic" } },
							{ "size", new [] { "big", "medium" } }
						}
					}
				},
				new CategoryObject() {
					Language = "Taxi",
					Suggest = new CompletionField()
					{
						Input = new[] { "Taxi" },
						Contexts = new Dictionary<string, IEnumerable<string>>
						{
							{ "material", new [] { "steel", "plastic" } },
							{ "size", new [] { "big" } }
						}
					}
				}
			});

			client.Refresh(INDEX_NAME);
		}

		public void Demo() {
			var searchResponse = client.Search<CategoryObject>(s => s
				.Suggest(su => su
					.Completion("lang", cs => cs
						.Field(f => f.Suggest)
						.Prefix("t")
						.Contexts(ctx => ctx
							.Context("material", 
								//cd => cd.Context("steel"), 
								cd => cd.Context("wood"))
							.Context("size", 
								//cd => cd.Context("big"),
								cd => cd.Context("medium")
							)
						)
					)
				)
			);

			Console.WriteLine("Took: " + searchResponse.Took + "\n");
			Console.WriteLine("Result:");

			var suggestions = searchResponse.Suggest["lang"];
			var index = 0;
			foreach (var option in suggestions[0].Options) {
				Console.WriteLine("{0} {1}", index, option.Text);
				index++;
			}

			Console.ReadLine();
		}
	}
}
