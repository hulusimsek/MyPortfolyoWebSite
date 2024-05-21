using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AkilliFiyatWeb.Data;
using AkilliFiyatWeb.Entity;
using AkilliFiyatWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AkilliFiyatWeb.Controllers
{
    [Area("AkilliFiyatWeb")]
	[Authorize(Roles = "Admin")]
	public class AdminController : Controller
	{
		private readonly IServiceProvider _serviceProvider;

		public AdminController(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public async Task<IActionResult> Yenile()
		{
			using (var kapsam = _serviceProvider.CreateScope())
			{
				var veritabaniBaglantisi = kapsam.ServiceProvider.GetRequiredService<DataContext>();
				var migrosServisi = kapsam.ServiceProvider.GetRequiredService<MigrosIndirimUrunServices>();
				var carfoursaServisi = kapsam.ServiceProvider.GetRequiredService<CarfoursaIndirimUrunServices>();
				var bimServisi = kapsam.ServiceProvider.GetRequiredService<BimIndirimUrunServices>();
				var a101Servisi = kapsam.ServiceProvider.GetRequiredService<A101IndirimUrunServices>();
				var sokUrunServisi = kapsam.ServiceProvider.GetRequiredService<SokUrunServices>();
				var _log = kapsam.ServiceProvider.GetRequiredService<MyLogger>();

				try
				{
					// Mevcut tüm ürünleri kaldır
					veritabaniBaglantisi.Urunler.RemoveRange(veritabaniBaglantisi.Urunler);
					await veritabaniBaglantisi.SaveChangesAsync();

					// Yeni ürünleri kaydet
					await migrosServisi.IndirimMigrosKayit();
					await carfoursaServisi.IndirimCarfoursaKayit();
					await bimServisi.IndirimBimKayit();
					await a101Servisi.IndirimA101Kayit();



					veritabaniBaglantisi.All_Products.RemoveRange(veritabaniBaglantisi.All_Products);
					await veritabaniBaglantisi.SaveChangesAsync();

					// Yeni ürünleri al
					List<All_Products> tumMarketler = new List<All_Products>();
					List<All_Products> sok = await sokUrunServisi.GetAllProducts();
					List<All_Products> a101 = await a101Servisi.GetA101AllProductsAsync();

					tumMarketler.AddRange(sok);
					tumMarketler.AddRange(a101);


					// Her bir ürünü eklemeden önce kontrol et
					await veritabaniBaglantisi.All_Products.AddRangeAsync(tumMarketler);
					await veritabaniBaglantisi.SaveChangesAsync();

					Console.WriteLine("GeceLikGorevServisi: Görevler başarıyla tamamlandı.");
					_log.Log("1", "gece gorevi", "GeceLikGorevServisi: Görevler başarıyla tamamlandı");

					return Ok("Marketler başarıyla güncellendi.");
				}
				catch (Exception ex)
				{
					Console.WriteLine("GeceLikGorevServisi: Hata oluştu: " + ex.Message);
					_log.Log("1", ex.Message, ex.ToString());

					return StatusCode(500, "Bir hata oluştu. Lütfen tekrar deneyin." + ex.Message);
				}
			}
		}

	}
}