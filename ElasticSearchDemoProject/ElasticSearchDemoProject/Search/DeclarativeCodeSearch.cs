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
	class DeclarativeCodeSearch {
		private const string INDEX_NAME = "intsearch";
		private readonly ElasticClient client;

		public DeclarativeCodeSearch()
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

				// ((0 < rate && rate < 3) || (7 < rate && rate < 10) && comment.Contains(search))

				var r1 = new NumericRangeQuery() {Field = "rate", GreaterThan = 0};
				var r2 = new NumericRangeQuery() {Field = "rate", LessThan = 3};

				var r3 = new NumericRangeQuery() {Field = "rate", GreaterThan = 7};
				var r4 = new NumericRangeQuery() {Field = "rate", LessThan = 10};

				var s1 = new MultiMatchQuery() {Fields = "comment", Query = search};

				var r12 = r1 & r2;
				var r34 = r3 & r4;

				var query = (r12 | r34) & s1;

				var result = client.Search<Imdb>(s => s
					.Query(q => query)
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
