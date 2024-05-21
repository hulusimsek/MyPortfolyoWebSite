using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AkilliFiyatWeb.Data;
using AkilliFiyatWeb.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Newtonsoft.Json.Linq; // JSON.NET kütüphanesini ekleyin


namespace AkilliFiyatWeb.Services
{
	public class MigrosIndirimUrunServices
	{
		private readonly ApiService _apiService;
		private readonly DataContext _dataContext; // ApplicationDbContext'e bağlanacak bir context
		private readonly MyLogger _log;

		private readonly KelimeKontrol _kelimeKontrol;

		public MigrosIndirimUrunServices(ApiService apiService, DataContext dataContext, KelimeKontrol kelimeKontrol, MyLogger log)
		{
			_apiService = apiService;
			_dataContext = dataContext;
			_kelimeKontrol = kelimeKontrol;
			_log = log;
		}

		public async Task<List<Urunler>> IndirimMigrosKayit()
		{
			List<Urunler> indirimliMigrosUrunler = new List<Urunler>();

			for (int i = 5; i > 0; i = i - 2)
			{
				var indirimlerMigros = await _apiService.IndirimMigrosApiAsync(i);

				if (indirimlerMigros != null)
				{
					dynamic jsonResponse = JObject.Parse(indirimlerMigros);
					if (jsonResponse != null && jsonResponse.data != null && jsonResponse.data.searchInfo != null && jsonResponse.data.searchInfo.storeProductInfos != null)
					{
						foreach (var urun in jsonResponse.data.searchInfo.storeProductInfos)
						{
							decimal fiyat;
							decimal result = 0;
							if (decimal.TryParse(urun.shownPrice.ToString(), out fiyat))
							{
								result = fiyat / 100;
								string resultString = result.ToString();
							}
							else
							{
								fiyat = 0;
							}

							double fiyat2 = (double)fiyat / 100.0;
							string fiyat3 = fiyat2.ToString("0.00");

							Urunler eklenecekUrun = new Urunler
							{
								UrunAdi = urun.name,
								Fiyat = fiyat3 + " ₺",
								UrunResmi = urun.images[0].urls.PRODUCT_HD,
								MarketAdi = "Migros",
								MarketResmi = "/img/Migros.png",
								Benzerlik = 1.0,
								AyrintiLink = "https://www.migros.com.tr/" + urun.prettyName
							};

							if (urun.badges != null && urun.badges.Count > 0 && urun.badges[0].value != null)
							{
								eklenecekUrun.EskiFiyat = urun.badges[0].value;
								try
								{
									Decimal eskiFiyatDecimal = Convert.ToDecimal(eklenecekUrun.EskiFiyat.Replace("TL", "").Trim());
									Decimal indirimOran = (eskiFiyatDecimal - result) / eskiFiyatDecimal * 100;
									indirimOran = Math.Round(indirimOran, 0);
									eklenecekUrun.IndirimOran = Convert.ToDouble(indirimOran);
								}
								catch (Exception ex)
								{

								}
							}

							indirimliMigrosUrunler.Add(eklenecekUrun);
							await _dataContext.Urunler.AddAsync(eklenecekUrun);
						}
					}
				}
				else
				{
					// API çağrısı başarısız oldu.
				}
			}

			await _dataContext.SaveChangesAsync();
			return indirimliMigrosUrunler;
		}

		public async Task<List<Urunler>> MigrosKayit(string query, string reid)
		{
			List<Urunler> migrosUrunler = new List<Urunler>();


			var migros = await _apiService.MigrosApiAsync(query, reid);

			if (migros != null)
			{
				dynamic jsonResponse = JObject.Parse(migros);
				if (jsonResponse != null && jsonResponse.data != null && jsonResponse.data.searchInfo != null && jsonResponse.data.searchInfo.storeProductInfos != null)
				{
					foreach (var urun in jsonResponse.data.searchInfo.storeProductInfos)
					{
						decimal fiyat;
						decimal result = 0;
						if (decimal.TryParse(urun.shownPrice.ToString(), out fiyat))
						{
							result = fiyat / 100;
							string resultString = result.ToString();
						}
						else
						{
							fiyat = 0;
						}

						double fiyat2 = (double)fiyat / 100.0;
						string fiyat3 = fiyat2.ToString("0.00");

						double benzerlikOrani = _kelimeKontrol.BenzerlikHesapla(_kelimeKontrol.ConvertTurkishToEnglish(query.ToString()), _kelimeKontrol.ConvertTurkishToEnglish(urun.name.ToString()));
						int katSayi = _kelimeKontrol.IkinciKelime2(query, urun.name.ToString());
						if (katSayi > 0)
						{
							benzerlikOrani += katSayi;
						}

						Urunler eklenecekUrun = new Urunler
						{
							UrunAdi = urun.name,
							Fiyat = fiyat3 + " ₺",
							UrunResmi = urun.images[0].urls.PRODUCT_HD,
							MarketAdi = "Migros",
							MarketResmi = "/img/Migros.png",
							Benzerlik = benzerlikOrani,
							AyrintiLink = "https://www.migros.com.tr/" + urun.prettyName
						};

						if (urun.badges != null && urun.badges.Count > 0 && urun.badges[0].value != null)
						{
							eklenecekUrun.EskiFiyat = urun.badges[0].value;
							try
							{
								Decimal eskiFiyatDecimal = Convert.ToDecimal(eklenecekUrun.EskiFiyat.Replace("TL", "").Trim());
								Decimal indirimOran = (eskiFiyatDecimal - result) / eskiFiyatDecimal * 100;
								indirimOran = Math.Round(indirimOran, 0);
								eklenecekUrun.IndirimOran = Convert.ToDouble(indirimOran);
							}
							catch (Exception ex)
							{

							}
						}

						migrosUrunler.Add(eklenecekUrun);
					}
				}
			}
			else
			{
				// API çağrısı başarısız oldu.
			}


			return migrosUrunler;
		}


		private readonly HttpClient _httpClient;
		public async Task<ActionResult<List<All_Products>>> GetAllMigrosProducts()
		{
			var allMigrosProducts = new List<All_Products>();

			string[] categories = { "meyve-sebze-c-2", "et-tavuk-balik-c-3", "sut-kahvaltilik-c-4", "temel-gida-c-5",
			"meze-hazir-yemek-donuk-c-7d", "icecek-c-6", "dondurma-c-41b", "atistirmalik-c-113fb", "firin-pastane-c-7e",
			"deterjan-temizlik-c-7", "kagit-islak-mendil-c-8d", "kisisel-bakim-kozmetik-saglik-c-8", "bebek-c-9",
			"ev-yasam-c-a", "kitap-kirtasiye-oyuncak-c-118ec", "cicek-c-502", "pet-shop-c-a0", "elektronik-c-a6" };

			for (int i = 1; i <= 100; i++) // 100 sayfayla sınırlıyoruz
			{
				foreach (var category in categories)
				{
					var migros = await _apiService.MigrosApiAllProductAsync(category, i);

					if (migros != null)
					{
						dynamic jsonResponse = JObject.Parse(migros);
						if (jsonResponse != null && jsonResponse.data != null && jsonResponse.data.searchInfo != null && jsonResponse.data.searchInfo.storeProductInfos != null)
						{
							foreach (var urun in jsonResponse.data.searchInfo.storeProductInfos)
							{
								decimal fiyat;
								decimal result = 0;
								if (decimal.TryParse(urun.shownPrice.ToString(), out fiyat))
								{
									result = fiyat / 100;
									string resultString = result.ToString();
								}
								else
								{
									fiyat = 0;
								}

								double fiyat2 = (double)fiyat / 100.0;
								string fiyat3 = fiyat2.ToString("0.00");

								decimal eskiFiyat;
								decimal resultEski = 0;
								if (decimal.TryParse(urun.regularPrice.ToString(), out eskiFiyat))
								{
									resultEski = eskiFiyat / 100;
									string resultEskiString = result.ToString();
								}
								else
								{
									eskiFiyat = 0;
								}

								double fiyat4 = (double)eskiFiyat / 100.0;
								string fiyat5 = fiyat4.ToString("0.00");

								Double indirimOrani = ((Convert.ToDouble(fiyat5) - Convert.ToDouble(fiyat3)) / Convert.ToDouble(fiyat5)) * 100;


								All_Products eklenecekUrun = new All_Products
								{
									UrunAdi = urun.name,
									Fiyat = fiyat3,
									EskiFiyat = fiyat5,
									UrunResmi = urun.images[0].urls.PRODUCT_HD,
									MarketAdi = "Migros",
									IndirimOran = indirimOrani,	
									MarketResmi = "/img/Migros.png",
									AyrintiLink = "https://www.migros.com.tr/" + urun.prettyName
								};

								allMigrosProducts.Add(eklenecekUrun);

							}
						}
					}
					else
					{
						// API çağrısı başarısız oldu.
					}
				}
			}

			return allMigrosProducts;
		}


	}

}