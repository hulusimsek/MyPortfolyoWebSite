using System;
using System.Threading;
using System.Threading.Tasks;
using AkilliFiyatWeb.Data;
using AkilliFiyatWeb.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AkilliFiyatWeb.Services
{
    public class NightlyTaskService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _calismaZamani;

        public NightlyTaskService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _calismaZamani = new TimeSpan(2, 0, 0);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var simdikiZaman = DateTime.Now;
                var beklemeSuresi = simdikiZaman.Date + _calismaZamani - simdikiZaman;

                if (beklemeSuresi < TimeSpan.Zero)
                {
                    beklemeSuresi = beklemeSuresi.Add(new TimeSpan(1, 0, 0, 0));
                }

                await Task.Delay(beklemeSuresi, stoppingToken);

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
                        veritabaniBaglantisi.Urunler.RemoveRange(veritabaniBaglantisi.Urunler);
                        await veritabaniBaglantisi.SaveChangesAsync();

                        await migrosServisi.IndirimMigrosKayit();

                        await carfoursaServisi.IndirimCarfoursaKayit();

                        await bimServisi.IndirimBimKayit();

                        await a101Servisi.IndirimA101Kayit();




						veritabaniBaglantisi.All_Products.RemoveRange(veritabaniBaglantisi.All_Products);
                        await veritabaniBaglantisi.SaveChangesAsync();

                        List<All_Products> tumMarketler = new List<All_Products>();
                        List<All_Products> sok = await sokUrunServisi.GetAllProducts();
						List<All_Products> a101 = await a101Servisi.GetA101AllProductsAsync();
						List<All_Products> car = await carfoursaServisi.GetAllProducts();

						tumMarketler.AddRange(sok);
						tumMarketler.AddRange(a101);
						tumMarketler.AddRange(car);


						await veritabaniBaglantisi.All_Products.AddRangeAsync(tumMarketler);
						await veritabaniBaglantisi.SaveChangesAsync();

						Console.WriteLine("GeceLikGorevServisi: Görevler başarıyla tamamlandı.");
                        _log.Log("1", "gece gorevi", "GeceLikGorevServisi: Görevler başarıyla tamamlandı");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("GeceLikGorevServisi: Hata oluştu: " + ex.Message);
                        _log.Log("1", ex.Message, ex.ToString());
                    }
                }
            }
        }
    }
}