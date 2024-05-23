using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AkilliFiyatWeb.Data;
using AkilliFiyatWeb.Entity;
using HtmlAgilityPack;
using AkilliFiyatWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace AkilliFiyatWeb.Services
{
	public class CarfoursaIndirimUrunServices
	{
		private readonly DataContext _context;
		private readonly KelimeKontrol _kelimeKontrol;
		private readonly MyLogger _log;


		public CarfoursaIndirimUrunServices(DataContext context, KelimeKontrol kelimeKontrol, MyLogger myLogger)
		{
			_context = context;
			_kelimeKontrol = kelimeKontrol;
			_log = myLogger;
		}

		public async Task<List<Urunler>> IndirimCarfoursaKayit()
		{
			var urunlerList = new List<Urunler>();

			try
			{
				var httpClient = new HttpClient();
				var response = await httpClient.GetAsync("https://www.carrefoursa.com/");
				var content = await response.Content.ReadAsStringAsync();

				var htmlDocument = new HtmlDocument();
				htmlDocument.LoadHtml(content);

				var elements = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'product_click')]");

				if (elements != null)
				{
					foreach (var element in elements)
					{
						try
						{
							var html = element.InnerHtml;
							var test = new HtmlDocument();
							test.LoadHtml(html);

							var itemNameElement = test.DocumentNode.SelectSingleNode(".//span[contains(@class, 'item-name')]");
							var itemPriceElement = test.DocumentNode.SelectSingleNode(".//span[contains(@class, 'item-price')]");
							var imgElement = test.DocumentNode.SelectSingleNode(".//img");
							var ayrintiLink = test.DocumentNode.SelectSingleNode(".//a");
							var itemEskiFiyat = test.DocumentNode.SelectSingleNode(".//span[contains(@class, 'priceLineThrough') and contains(@class, 'js-variant-price')]");

							if (itemNameElement != null && itemPriceElement != null && imgElement != null)
							{
								var itemName = itemNameElement.InnerText.Trim();
								var itemPrice = itemPriceElement.GetAttributeValue("content", "");



								int index = itemPrice.IndexOf('.') + 3; // Noktadan sonra 2 basamak
								itemPrice = itemPrice.Replace('.', ',');
								if (index < itemPrice.Length)
								{
									itemPrice = itemPrice.Substring(0, index);
								}

								double itemPriceDouble = Convert.ToDouble(itemPrice);
								itemPriceDouble = Math.Round(itemPriceDouble, 2);


								var dataSrc = imgElement.GetAttributeValue("data-src", "");

								// Virgülü noktaya çevir



								var ayrintLinkString = "https://www.carrefoursa.com" + ayrintiLink.GetAttributeValue("href", "");
								ayrintLinkString = RemovePartFromUrl(ayrintLinkString, "/quickView");

								if (itemEskiFiyat != null)
								{
									var eskiFiyatString = itemEskiFiyat.InnerText.Trim().Split(" TL")[0];
									Double EskiFiyatDouble = Convert.ToDouble(eskiFiyatString);

									Double indirimOran = (EskiFiyatDouble - itemPriceDouble) / EskiFiyatDouble * 100;
									indirimOran = Math.Round(indirimOran, 0);

									var urun = new Urunler(itemName, itemPriceDouble + " \u20BA", dataSrc, "Carrefour-SA", "/img/Carrefour-SA.png", 0.0, ayrintLinkString, 0, eskiFiyatString + " \u20BA", indirimOran);
									urunlerList.Add(urun);
									_context.Urunler.Add(urun);
								}
								else
								{
									var urun = new Urunler(itemName, itemPriceDouble + " \u20BA", dataSrc, "Carrefour-SA", "https://sdgmapturkey.com/wp-content/uploads/carrefoursa.png", 0.0, ayrintLinkString);
									urunlerList.Add(urun);
									_context.Urunler.Add(urun);
								}
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message);
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

		private string RemovePartFromUrl(string url, string partToRemove)
		{
			return url.Replace(partToRemove, "");
		}

		private string CarArama(string query)
		{
			string query2 = _kelimeKontrol.ConvertTurkishToEnglish2(query);

			try
			{
				query2 = WebUtility.UrlEncode(_kelimeKontrol.ConvertTurkishToEnglish2(query2));
				return "https://www.carrefoursa.com/search/?text=" + query2;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return "";
			}
		}

		public async Task<List<Urunler>> CarfoursaKayit(String query)
		{

			string searchUrl = CarArama(query);

			var urunlerList = new List<Urunler>();

			try
			{
				var httpClient = new HttpClient();
				var response = await httpClient.GetAsync(searchUrl);
				var content = await response.Content.ReadAsStringAsync();

				var htmlDocument = new HtmlDocument();
				htmlDocument.LoadHtml(content);

				var elements = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'product_click')]");

				if (elements != null)
				{
					foreach (var element in elements)
					{
						try
						{
							var html = element.InnerHtml;
							var test = new HtmlDocument();
							test.LoadHtml(html);

							var itemNameElement = test.DocumentNode.SelectSingleNode(".//span[contains(@class, 'item-name')]");
							var itemPriceElement = test.DocumentNode.SelectSingleNode(".//span[contains(@class, 'item-price')]");
							var imgElement = test.DocumentNode.SelectSingleNode(".//img");
							var ayrintiLink = test.DocumentNode.SelectSingleNode(".//a");
							var itemEskiFiyat = test.DocumentNode.SelectSingleNode(".//span[contains(@class, 'priceLineThrough') and contains(@class, 'js-variant-price')]");

							if (itemNameElement != null && itemPriceElement != null && imgElement != null)
							{
								var itemName = itemNameElement.InnerText.Trim();
								var itemPrice = itemPriceElement.GetAttributeValue("content", "");



								int index = itemPrice.IndexOf('.') + 3; // Noktadan sonra 2 basamak
								itemPrice = itemPrice.Replace('.', ',');
								if (index < itemPrice.Length)
								{
									itemPrice = itemPrice.Substring(0, index);
								}

								double benzerlikOrani = _kelimeKontrol.BenzerlikHesapla(_kelimeKontrol.ConvertTurkishToEnglish(query), _kelimeKontrol.ConvertTurkishToEnglish(itemName));
								int katSayi = _kelimeKontrol.IkinciKelime2(query, itemName);
								if (katSayi > 0)
								{
									benzerlikOrani += katSayi;
								}


								double itemPriceDouble = Convert.ToDouble(itemPrice);
								itemPriceDouble = Math.Round(itemPriceDouble, 2);


								var dataSrc = imgElement.GetAttributeValue("data-src", "");

								// Virgülü noktaya çevir



								var ayrintLinkString = "https://www.carrefoursa.com" + ayrintiLink.GetAttributeValue("href", "");
								ayrintLinkString = RemovePartFromUrl(ayrintLinkString, "/quickView");

								if (itemEskiFiyat != null)
								{
									var eskiFiyatString = itemEskiFiyat.InnerText.Trim().Split(" TL")[0];
									Double EskiFiyatDouble = Convert.ToDouble(eskiFiyatString);

									Double indirimOran = (EskiFiyatDouble - itemPriceDouble) / EskiFiyatDouble * 100;
									indirimOran = Math.Round(indirimOran, 0);

									var urun = new Urunler(itemName, itemPriceDouble + " \u20BA", dataSrc, "Carrefour-SA", "/img/Carrefour-SA.png", benzerlikOrani, ayrintLinkString, 0, eskiFiyatString + " \u20BA", indirimOran);
									urunlerList.Add(urun);
								}
								else
								{
									var urun = new Urunler(itemName, itemPriceDouble + " \u20BA", dataSrc, "Carrefour-SA", "/img/Carrefour-SA.png", benzerlikOrani, ayrintLinkString);
									urunlerList.Add(urun);
								}
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			return urunlerList;
		}


		private static readonly string[] numbers = {
	"1015", "1025", "1045", "1098", "1065", "1070", "1102", "1061", "1174", "1234",
	"1239", "1111", "1121", "1275", "1223", "1159", "1186", "1209", "1493", "1494",
	"1009","1939", "1948", "2538", "1940", "1963", "1454", "1484", "1412", "1418", "1411",
	"1481", "1040", "1311", "1318", "1356", "1390", "1389", "1349", "1348", "1385",
	"1038", "1962", "1875", "1873", "1899", "1858", "1847", "1915", "1613", "1591",
	"1602", "1627", "1557", "1658", "1652", "1598", "1675", "1710", "1736", "1757",
	"1772", "1785", "1805", "1831", "1838", "1729", "1821", "1820", "2305", "2313",
	"2341", "2287", "2330", "2202", "2234", "2195", "2076", "2239", "1964", "2025",
	"2149", "2127", "2054", "2342", "2115", "2190", "2105", "2186", "2537", "2138",
	"1261", "1270", "1266", "1110"
};
		public async Task<List<All_Products>> GetAllProducts()
		{
			var products = new List<All_Products>();

			foreach (var number in numbers)
			{
				try
				{
					using (HttpClient client = new HttpClient())
					{
						client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
						client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
						var url = $"https://www.carrefoursa.com/unlu-mamuller-ve-tatlilar/c/{number}?q=%3AbestSeller&show=All";
						var response = await client.GetStringAsync(url);

						var htmlDoc = new HtmlDocument();
						htmlDoc.LoadHtml(response);

						var productNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'product_click')]");

						if (productNodes != null)
						{
							foreach (var productNode in productNodes)
							{
								try
								{
									var itemNameElement = productNode.SelectSingleNode(".//span[contains(@class, 'item-name')]");
									var itemPriceElement = productNode.SelectSingleNode(".//span[contains(@class, 'item-price')]");
									var oldPriceElement = productNode.SelectSingleNode(".//span[contains(@class, 'priceLineThrough js-variant-price')]");
									var imgElement = productNode.SelectSingleNode(".//img");

									var itemName = "";
									if (itemNameElement != null)
									{
										itemName = itemNameElement.InnerText.Trim();
										// Diğer işlemleri buraya yazın
									}
									else
									{
										continue;
									}

									var itemPrice = itemPriceElement.GetAttributeValue("content", "");



									int index = itemPrice.IndexOf('.') + 3; // Noktadan sonra 2 basamak
									itemPrice = itemPrice.Replace('.', ',');
									if (index < itemPrice.Length)
									{
										itemPrice = itemPrice.Substring(0, index);
									}

									double itemPriceDouble = Convert.ToDouble(itemPrice);
									itemPriceDouble = Math.Round(itemPriceDouble, 2);

									string oldPrice = null;
									Double indirimOrani = 0.0;

									if (oldPriceElement != null)
									{
										var oldPriceText = oldPriceElement.InnerText.Trim();
										var priceParts = oldPriceText.Split(" TL");

										if (priceParts.Length > 0)
										{
											oldPrice = priceParts[0];
											indirimOrani = ((Convert.ToDouble(oldPrice) - Convert.ToDouble(itemPriceDouble)) / Convert.ToDouble(oldPrice)) * 100;
										}
									}


									var dataSrc = imgElement.GetAttributeValue("data-src", "");

									All_Products product = new All_Products
									{
										UrunAdi = itemName,
										Fiyat = Convert.ToString(itemPriceDouble),
										EskiFiyat = Convert.ToString(oldPrice),
										UrunResmi = dataSrc,
										MarketAdi = "Carrefour-SA",
										IndirimOran = indirimOrani,
										MarketResmi = "/img/Carrefour-SA.png"
									};

									products.Add(product);
								}
								catch (Exception ex)
								{
									Console.WriteLine($"Error processing product: {ex.Message}");
									_log.Log("1", ex.Message);
									// Hata oluştuğunda loglama yapabilirsiniz
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error fetching products for number {number}: {ex.Message}");
					// Hata oluştuğunda loglama yapabilirsiniz
					_log.Log("1", ex.Message);
				}
			}

			return products;
		}




	}

}