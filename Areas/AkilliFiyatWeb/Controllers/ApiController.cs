using AkilliFiyatWeb.Data;
using AkilliFiyatWeb.Entity;
using AkilliFiyatWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPortfolyoWebSite.Areas.AkilliFiyatWeb.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPortfolyoWebSite.Areas.AkilliFiyatWeb.Controllers
{
	[Area("AkilliFiyatWeb")]
	[Route("akilli-fiyat/[controller]")]
	public class ApiController : Controller
	{
		private readonly MigrosIndirimUrunServices _migrosIndirimUrunServices;
		private readonly DataContext _context;
		private readonly CarfoursaIndirimUrunServices _carfoursaIndirimUrunServices;
		private readonly SokUrunServices _sokUrunServices;
		private readonly A101IndirimUrunServices _a101IndirimServices;
		private readonly KelimeKontrol _kelimeKontrol;
		private readonly ApiService _apiService;
		private readonly MyLogger _log;
		private readonly LuceneIndexer _luceneIndexer;



		[ActivatorUtilitiesConstructor]
		public ApiController(MigrosIndirimUrunServices migrosIndirimUrunServices, DataContext context, CarfoursaIndirimUrunServices carfoursaIndirimUrunServices,
							 SokUrunServices sokUrunServices, KelimeKontrol KelimeKontrol, A101IndirimUrunServices a101IndirimServices, ApiService apiService, MyLogger myLogger, LuceneIndexer luceneIndexer)
		{
			_migrosIndirimUrunServices = migrosIndirimUrunServices;
			_context = context;
			_carfoursaIndirimUrunServices = carfoursaIndirimUrunServices;
			_sokUrunServices = sokUrunServices;
			_kelimeKontrol = KelimeKontrol;
			_a101IndirimServices = a101IndirimServices;
			_apiService = apiService;

			_log = myLogger;
			_luceneIndexer = luceneIndexer;
		}

		[HttpGet]
		public IActionResult Index()
		{
			return View();
		}

		[HttpGet("search")]
		public async Task<ActionResult<IEnumerable<All_Products>>> SearchProducts(string query)
		{
			if (string.IsNullOrEmpty(query))
			{
				return BadRequest("Query parameter is required.");
			}

			var products = _luceneIndexer.SearchIndex(query);
			return Ok(products);
		}

		[HttpGet("search2")]
		public async Task<ActionResult<IEnumerable<All_Products>>> SearchProducts2(string query)
		{
			if (string.IsNullOrEmpty(query))
			{
				return BadRequest("Query parameter is required.");
			}

			var products = await _context.All_Products.Where(item => EF.Functions.Like(item.UrunAdi, $"%{query}%")).ToListAsync();
			return Ok(products);
		}

	}
}
