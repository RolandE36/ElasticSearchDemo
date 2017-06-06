using CsvHelper;
using Elasticsearch.Net;
using ElasticSearchDemoProject.Search;
using Nest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchDemoProject.Aggregations {
	public class SimpleAggregation {
		private const string INDEX_NAME = "intsearch";
		private readonly ElasticClient client;

		public SimpleAggregation()
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

				var result = client.Search<Imdb>(s => s
					.Aggregations(a => a
						.Terms("rate", r => r
							.Field(f => f.Rate)
							.Order(TermsOrder.TermAscending)
						)
					)
					.Size(0)
				);

				Console.WriteLine("Took: " + result.Took);
				Console.WriteLine("Total: " + result.Total);

				foreach (Nest.KeyedBucket<object> document in ((Nest.BucketAggregate)(result.Aggregations["rate"])).Items) {
					Console.WriteLine("Rate/Count: {0}/{1}", document.Key, document.DocCount);
				}

				Console.ReadLine();
			}
		}
	}
}
