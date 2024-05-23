using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AkilliFiyatWeb.Models;
using AkilliFiyatWeb.Services;
using AkilliFiyatWeb.Data;
using Microsoft.EntityFrameworkCore;
using AkilliFiyatWeb.Entity;
using HtmlAgilityPack;
using System.Globalization;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
namespace AkilliFiyatWeb.Controllers;

[Area("AkilliFiyatWeb")]
public class HomeController : Controller
{
    private readonly MigrosIndirimUrunServices _migrosIndirimUrunServices;
    private readonly DataContext _context;
    private readonly CarfoursaIndirimUrunServices _carfoursaIndirimUrunServices;
    private readonly SokUrunServices _sokUrunServices;
    private readonly A101IndirimUrunServices _a101IndirimServices;
    private readonly KelimeKontrol _kelimeKontrol;
    private readonly ApiService _apiService;
    private MyLogger _log;

    [ActivatorUtilitiesConstructor]
    public HomeController(MigrosIndirimUrunServices migrosIndirimUrunServices, DataContext context, CarfoursaIndirimUrunServices carfoursaIndirimUrunServices,
                            SokUrunServices sokUrunServices, KelimeKontrol KelimeKontrol, A101IndirimUrunServices a101IndirimServices, ApiService apiService, MyLogger myLogger)
    {
        _migrosIndirimUrunServices = migrosIndirimUrunServices;
        _context = context;
        _carfoursaIndirimUrunServices = carfoursaIndirimUrunServices;
        _sokUrunServices = sokUrunServices;
        _kelimeKontrol = KelimeKontrol;
        _a101IndirimServices = a101IndirimServices;
        _apiService = apiService;
        _log = myLogger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var urunler = await _context.Urunler.ToListAsync();
            ViewBag.SiralanmisUrunler = urunler.Where(u => u.IndirimOran != null && u.IndirimOran != 0)
                                       .OrderByDescending(u => u.IndirimOran)
                                       .ToList();
            return View(urunler);
        }
        catch (Exception ex)
        {
            _log.Log("1", ex.Message);
            return View("Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Search(string query)
    {
        try
        {
			if (!string.IsNullOrEmpty(query))
			{
				// Arama sonuçlarını al ve ViewData üzerinden görünüme gönder
				ViewData["query"] = query;
				List<Urunler> _sonucUrunler = new List<Urunler>();
				_sonucUrunler.AddRange(await _migrosIndirimUrunServices.MigrosKayit(query, "1711827415305000067"));
				_sonucUrunler.AddRange(await _carfoursaIndirimUrunServices.CarfoursaKayit(query));
				_sonucUrunler.AddRange(await _sokUrunServices.SokKayit(query));
				_sonucUrunler.AddRange(await _a101IndirimServices.A101Kayit(query));

				var siraliUrunler = _sonucUrunler
											.Where(u => u.Benzerlik != null && u.Benzerlik != 0)
											.GroupBy(u => u.Benzerlik > 40 ? 1 : u.Benzerlik > 30 ? 2 : u.Benzerlik > 20 ? 3 : u.Benzerlik > 10 ? 4 : 5) // Benzerlik değerine göre grupla
											.OrderBy(g => g.Key)  // Grupları büyükten küçüğe göre sırala
											.SelectMany(g => g.OrderByDescending(u => u.Fiyat))  // Her grubu içindeki ürünleri fiyata göre sırala ve birleştir
											.ToList();
				foreach (var urun in siraliUrunler)
				{
					Console.WriteLine("Ürün Adı: " + urun.UrunAdi); // Varsayılan olarak ürün adını yazdırabilirsiniz
					Console.WriteLine("Benzerlik: " + urun.Benzerlik); // Benzerlik değerini yazdır
				}
				return View(siraliUrunler);
			}

			// Arama sayfasını göster
			return View();
		}
        catch (Exception ex)
        {
			_log.Log("1", ex.Message, ex.ToString());
            return Redirect("/akilli-fiyat/Home/Search");
        }
    }

    [HttpGet]
    public IActionResult GetServerTime()
    {
        DateTime serverTime = DateTime.Now;
        return Ok(serverTime);
    }

    [HttpGet]
    public async Task<IActionResult> Test()
    {
        try
        {
            var test = await _migrosIndirimUrunServices.GetAllMigrosProducts();
            string jsonResult = JsonConvert.SerializeObject(test);

            return Content(jsonResult, "application/json");
        }
        catch (Exception ex)
        {
            _log.Log("1", ex.Message, ex.ToString());
            return Redirect("/akilli-fiyat/Home");
        }

    }


	[HttpGet("SearchProducts")]
	public async Task<ActionResult<IEnumerable<All_Products>>> SearchProducts(string query)
	{
		if (string.IsNullOrEmpty(query))
		{
			return BadRequest("Query parameter is required.");
		}

		var products = await _context.All_Products
			.Where(p => p.UrunAdi.Contains(query) || p.MarketAdi.Contains(query))
			.ToListAsync();

		// Ürün bulunamasa bile boş bir liste döndür
		return products;
	}


}
