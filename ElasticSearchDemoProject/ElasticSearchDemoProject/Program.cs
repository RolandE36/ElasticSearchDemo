using ElasticSearchDemoProject.Aggregations;
using ElasticSearchDemoProject.Search;
using ElasticSearchDemoProject.Suggest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchDemoProject {
	class Program {
		static void Main(string[] args)
		{
			// Search
			//(new DemoSearch()).Demo();
			//(new MultiFieldSearchCriteria()).Demo();
			//(new DeclarativeCodeSearch()).Demo();

			// Suggest
			//(new SimpleSuggest()).Demo();
			//(new WeightedSuggest()).Demo();
			//(new CategorySuggest()).Demo();

			// Aggregations
			//(new SimpleAggregation()).Demo();
		}
	}
}
