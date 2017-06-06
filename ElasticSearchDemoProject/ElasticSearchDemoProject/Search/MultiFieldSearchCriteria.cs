using CsvHelper;
using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchDemoProject.Search {
	public class MultiFieldSearchCriteria {
		private const string INDEX_NAME = "intsearch";
		private readonly ElasticClient client;

		public MultiFieldSearchCriteria()
		{
			// Connection
			var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
			var connectionSettings = new ConnectionSettings(pool).DefaultIndex(INDEX_NAME);
			client = new ElasticClient(connectionSettings);

			// Index configuration
			if (client.IndexExists(INDEX_NAME).Exists) client.DeleteIndex(INDEX_NAME);

			client.CreateIndex(INDEX_NAME, ci => ci
				.Mappings(m => m
					.Map<Imdb>(mm => mm
						.AutoMap()
					)
				)
			);

			using (var csv = new CsvReader( new StreamReader( "../../data/data.csv" ) )) {
				csv.ReadHeader();
				while (csv.Read()) {
					client.Index(csv.GetRecord<Imdb>());
				}
			}
		}

		public void Demo()
		{
			while (true)
			{
				Console.Clear();
				Console.Write("Enter search string: ");
				var search = Console.ReadLine();

				var result = client.Search<Imdb>(s => s
					.Query(q => q
						.Bool(b => b
							.Must(m => m
								.MultiMatch(mm => mm
									.Query(search)
									.Fields(f => f.Field(f1 => f1.Comment))
								)
							)
							.Filter(f => f
								.Bool(fb => fb
									.Must(
										fm => fm.Range(r => r.Field(f1 => f1.Rate).GreaterThan(4))
										,fm => fm.Range(r => r.Field(f1 => f1.Rate).LessThan(8))
									)
								)
							)
						)
					)
					.From(0)
					.Size(1)
				);

				Console.WriteLine("Took: " + result.Took + "\n");
				Console.WriteLine("Total: " + result.Total + "\n");
				Console.WriteLine("MaxScore: " + result.MaxScore + "\n");

				var index = 0;
				foreach (var document in result.Documents) {
					Console.WriteLine("===========================================================\n {0} {1}", document.Rate, document.Comment);
					index++;
				}

				Console.ReadLine();
			}
		}
	}
}
