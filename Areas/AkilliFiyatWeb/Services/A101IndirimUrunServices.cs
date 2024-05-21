using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AkilliFiyatWeb.Models;
using AkilliFiyatWeb.Services;
using AkilliFiyatWeb.Data;
using Microsoft.EntityFrameworkCore;
using AkilliFiyatWeb.Entity;
using HtmlAgilityPack;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Urunler = AkilliFiyatWeb.Entity.Urunler;
using System.Net;
using MyPortfolyoWebSite.Entity;
using System.Net.Http.Headers;

namespace AkilliFiyatWeb.Services
{
	public class A101IndirimUrunServices
	{
		private readonly DataContext _context;
		private readonly ApiService _apiService;
		private readonly KelimeKontrol _kelimeKontrol;
		private readonly MyLogger _log;
		public A101IndirimUrunServices(DataContext context, ApiService apiService, KelimeKontrol kelimeKontrol, MyLogger logger)
		{
			_context = context;
			_apiService = apiService;
			_kelimeKontrol = kelimeKontrol;
			_log = logger;
		}
		public async Task<List<Urunler>> IndirimA101Kayit()
		{
			var urunlerList = new List<Urunler>();

			try
			{
				var url = "https://www.a101.com.tr/ekstra/haftanin-yildizlari";
				var web = new HtmlWeb();
				var doc = web.Load(url);
				var articles = doc.DocumentNode.SelectNodes("//div[@class='product-container']");
				if (articles != null)
				{
					foreach (var article in articles)
					{
						try
						{
							var itemNameElement = article.SelectSingleNode(".//header/hgroup/h3");
							var itemPriceElement = article.SelectSingleNode(".//section/span");
							var itemEskiFiyat = article.SelectSingleNode(".//section/s");
							var imgElement = article.SelectSingleNode(".//figure/div/div/img");
							var ayrintiLink = article.SelectSingleNode(".//a");

							if (itemNameElement != null && itemPriceElement != null && imgElement != null && ayrintiLink != null)
							{
								var itemName = itemNameElement.InnerText.Trim();
								var itemPrice = itemPriceElement.InnerText;
								var ayrintLinkString = ayrintiLink.GetAttributeValue("href", "");

								string eskiFiyat = "";
								if (itemEskiFiyat != null)
								{
									eskiFiyat = itemEskiFiyat.InnerText.Replace("\u20BA", "");
								}

								var dataSrc = imgElement.GetAttributeValue("src", "");

								double indirimOran = 0.0;

								if (!string.IsNullOrEmpty(eskiFiyat) && double.TryParse(itemPrice, out double yeniFiyat) && double.TryParse(eskiFiyat.Replace(" \u20BA", ""), out double eskiFiyatDouble))
								{
									indirimOran = ((eskiFiyatDouble - yeniFiyat) / eskiFiyatDouble) * 100;
								}

								var urun = new Urunler(itemName, itemPrice + " \u20BA", dataSrc, "A-101", "/img/A-101.png", 0.0, ayrintLinkString, 0, eskiFiyat + " \u20BA", indirimOran);
								urunlerList.Add(urun);
								await _context.Urunler.AddAsync(urun);
							}


							else
							{
								Console.WriteLine("Bir veya daha fazla özellik null");
								if (itemNameElement == null)
								{
									Console.WriteLine("itemNameElement null");
								}
								if (itemPriceElement == null)
								{
									Console.WriteLine("itemPriceElement null");
								}
								if (imgElement == null)
								{
									Console.WriteLine("imgElement null");
								}
								if (ayrintiLink == null)
								{
									Console.WriteLine("ayrintiLink null");
								}
							}
						}
						catch (Exception e)
						{
							Console.WriteLine(e.Message);
						}
					}
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			await _context.SaveChangesAsync();
			return urunlerList;
		}

		public async Task<List<Urunler>> A101Kayit(string query)
		{
			List<Urunler> a101Urunler = new List<Urunler>();

			var a101 = await _apiService.A101ApiAsync(query, "100", "1");

			if (a101 != null)
			{
				dynamic jsonResponse = JObject.Parse(a101);
				Console.WriteLine(jsonResponse.ToString());
				if (jsonResponse != null && jsonResponse.res != null && jsonResponse.res.Count > 0 && jsonResponse.res[0].page_content != null)
				{
					foreach (var eleman in jsonResponse.res[0].page_content)
					{
						string formatliDeger = $"{eleman.price:#.##} \u20BA";

						var urunAdi = eleman.title.ToString();
						if (urunAdi != null)
						{
							await Console.Out.WriteLineAsync(urunAdi);
						}
						else
						{
							await Console.Out.WriteLineAsync("name null");
						}
						var benzerlikOrani = _kelimeKontrol.BenzerlikHesapla(_kelimeKontrol.ConvertTurkishToEnglish(query), _kelimeKontrol.ConvertTurkishToEnglish(urunAdi));
						var katSayi = _kelimeKontrol.IkinciKelime2(query, urunAdi);
						if (katSayi > 0)
						{
							benzerlikOrani += katSayi;
						}
						string imageUrl = "";
						foreach (var item in eleman.image)
						{
							if (item.imageType == "product")
							{
								imageUrl = item.url;
							}
						}
						if (benzerlikOrani > 0.1)
						{
							var eklenecekUrun = new Urunler(urunAdi, formatliDeger,
								imageUrl.ToString(), "A-101",
								"/img/A-101.png",
								benzerlikOrani, eleman.seoUrl.ToString());
							eklenecekUrun.EskiFiyat = ($"{eleman.Price} \u20BA");
							eklenecekUrun.UrunlerId = eleman.id;

							a101Urunler.Add(eklenecekUrun);
						}
					}
				}
				else
				{
					if (jsonResponse == null)
					{
						Console.WriteLine("--------------- jsonResponse null döndü");
					}
					else if (jsonResponse.res == null)
					{
						Console.WriteLine("--------------- jsonResponse.res null döndü");
					}
					else if (jsonResponse.res.Count == 0)
					{
						Console.WriteLine("--------------- jsonResponse.res boş döndü");
					}
					else if (jsonResponse.res[0] == null)
					{
						Console.WriteLine("--------------- jsonResponse.res[0] null döndü");
					}
					else if (jsonResponse.res[0].page_content == null)
					{
						Console.WriteLine("--------------- jsonResponse.res[0].page_content null döndü");
					}

				}
			}
			else
			{
				Console.WriteLine("başarsız");
			}


			return a101Urunler;
		}



		private static readonly string[] Categories = { "sut-urunleri-kahvaltilik", "meyve-sebze", "et-balik-tavuk", "firindan",
		"temel-gida", "atistirmalik", "icecek", "donuk-hazir-yemek-meze", "tatli", "dondurma",
		"temizlik-urunleri", "kisisel-bakim", "kagit-urunleri", "elektronik", "anne-bebek", "ev-yasam",
		"kitap-kirtasiye-oyuncak", "evcil-hayvan", "bayram" };

		public async Task<List<All_Products>> GetA101AllProductsAsync()
		{
			var allProductsA101 = new List<All_Products>();

			foreach (var category in Categories)
			{
				try
				{
					var url = $"https://www.a101.com.tr/kapida/{category}";
					var htmlContent = await GetHtmlContentAsync(url);

					if (!string.IsNullOrEmpty(htmlContent))
					{
						var htmlDocument = new HtmlDocument();
						htmlDocument.LoadHtml(htmlContent);

						var scriptNode = htmlDocument.DocumentNode.SelectSingleNode("//script[@id='__NEXT_DATA__']");
						if (scriptNode != null)
						{
							var json = scriptNode.InnerText;
							var jsonObject = JObject.Parse(json);

							var allProducts = new JArray();
							var children = jsonObject["props"]["pageProps"]["productsByCategoryOutput"]["children"] as JArray;

							foreach (var child in children)
							{
								var products = child["products"] as JArray;
								allProducts.Merge(products);
							}

							foreach (var product in allProducts)
							{
								try
								{
									var attributes = product["attributes"];
									var productName = attributes["name"].ToString();
									var price = product["price"]["discountedStr"].ToString().Replace("₺", "");
									var image = product["images"] as JArray;
									var imageURL = GetImageURL(image);
									var normalPrice = product["price"]["normalStr"].ToString().Replace("₺", "");
									var normalPriceFloat = float.Parse(normalPrice);

									All_Products a101Product2 = new All_Products
									{
										UrunAdi = productName,
										Fiyat = Convert.ToString(price),
										UrunResmi = imageURL,
										MarketAdi = "A-101",
										MarketResmi = "/img/A101.png"
									};
									allProductsA101.Add(a101Product2);
								}
								catch (Exception e)
								{
									_log.Log("1", $"Hata: {e.Message}"); // Hata loglama
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					_log.Log("1", $"Hata: {e.Message}"); // Hata loglama
				}

				// İstekler arasında rastgele gecikme ekleyerek bot benzeri davranışı azaltın
				Random rnd = new Random();
				Thread.Sleep(rnd.Next(1000, 5000));
			}

			return allProductsA101;
		}

		private async Task<string> GetHtmlContentAsync(string url)
		{
			try
			{
				var handler = new HttpClientHandler
				{
					AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
					UseCookies = true,
					CookieContainer = new CookieContainer()
				};

				// Proxy ayarlarını burada ekleyebilirsiniz
				// handler.Proxy = new WebProxy("http://myproxy:myport", false);
				// handler.UseProxy = true;

				using (var httpClient = new HttpClient(handler))
				{
					httpClient.DefaultRequestHeaders.Add("User-Agent", GetRandomUserAgent());
					httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
					httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
					httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9));
					httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
					httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
					httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
					httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
					httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
					httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
					httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
					{
						NoCache = true
					};

					// Çerezleri ekleyin (manuel olarak elde edilen)
					httpClient.DefaultRequestHeaders.Add("Cookie", "your-cookie-value");

					var htmlContent = await httpClient.GetStringAsync(url);
					return htmlContent;
				}
			}
			catch (Exception e)
			{
				_log.Log("1", e.Message);
				Console.WriteLine(e.Message);
				return null;
			}
		}

		private string GetImageURL(JArray images)
		{
			foreach (var image in images)
			{
				if (image["imageType"].ToString() == "product")
				{
					return image["url"].ToString();
				}
			}
			return string.Empty;
		}

		private string GetRandomUserAgent()
		{
			var userAgents = new[]
			{
			"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36",
			"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:89.0) Gecko/20100101 Firefox/89.0",
			"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36",
			"Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:89.0) Gecko/20100101 Firefox/89.0",
			"Mozilla/5.0 (iPhone; CPU iPhone OS 14_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0 Mobile/15E148 Safari/604.1"
		};

			Random rnd = new Random();
			return userAgents[rnd.Next(userAgents.Length)];
		}







	}
}