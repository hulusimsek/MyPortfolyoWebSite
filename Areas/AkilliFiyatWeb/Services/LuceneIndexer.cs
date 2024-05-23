using AkilliFiyatWeb.Data;
using AkilliFiyatWeb.Entity;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers; // Bu satırı ekleyin
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;


namespace MyPortfolyoWebSite.Areas.AkilliFiyatWeb.Services
{

	public class LuceneIndexer
	{
		private static readonly LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
		private readonly string _indexPath;
		private readonly DataContext _context;


		public LuceneIndexer(DataContext context)
		{
			_indexPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "lucene_index");
			_context = context;
		}

		public LuceneIndexer()
		{
			_indexPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "lucene_index");

		}

		public void CreateIndex(List<All_Products> products)
		{
			using var directory = FSDirectory.Open(_indexPath);
			var analyzer = new StandardAnalyzer(AppLuceneVersion);
			var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyzer);
			using var writer = new IndexWriter(directory, indexConfig);

			foreach (var product in products)
			{
				var doc = new Document
			{
				new StringField("All_ProductsId", product.All_ProductsId.ToString(), Field.Store.YES),
				new TextField("UrunAdi", product.UrunAdi, Field.Store.YES),
				new TextField("Fiyat", product.Fiyat, Field.Store.YES),
				new StringField("UrunResmi", product.UrunResmi, Field.Store.YES),
				new TextField("MarketAdi", product.MarketAdi, Field.Store.YES),
				new StringField("EskiFiyat", product.EskiFiyat, Field.Store.YES),
				new StringField("MarketResmi", product.MarketResmi, Field.Store.YES)

			};
				writer.AddDocument(doc);
			}

			writer.Flush(triggerMerge: false, applyAllDeletes: false);
		}

		public List<All_Products> SearchIndex(string queryText)
		{
			using var directory = FSDirectory.Open(_indexPath);
			using var reader = DirectoryReader.Open(directory);
			var searcher = new IndexSearcher(reader);

			var analyzer = new StandardAnalyzer(AppLuceneVersion);
			var parser = new QueryParser(AppLuceneVersion, "UrunAdi", analyzer);
			var query = parser.Parse(queryText);

			var hits = searcher.Search(query, 10).ScoreDocs;
			var results = new List<All_Products>();

			foreach (var hit in hits)
			{
				var foundDoc = searcher.Doc(hit.Doc);
				results.Add(new All_Products
				{
					All_ProductsId = int.Parse(foundDoc.Get("All_ProductsId")),
					UrunAdi = foundDoc.Get("UrunAdi"),
					Fiyat = foundDoc.Get("Fiyat"),
					UrunResmi = foundDoc.Get("UrunResmi"),
					MarketAdi = foundDoc.Get("MarketAdi"),
					EskiFiyat = foundDoc.Get("EskiFiyat"),
					MarketResmi = foundDoc.Get("MarketResmi")
				});
			}

			return results;
		}

		public List<All_Products> GetProductsFromDatabase()
		{
			// Burada veritabanınızdan ürünleri almak için kullanacağınız mantığı ekleyin
			// Örnek olarak, Entity Framework Core kullanarak bir veritabanı bağlamı üzerinden ürünleri sorgulayabilirsiniz
			var products = _context.All_Products.ToList();

			return products;
		}
	}




}