using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading.Tasks;
using AkilliFiyatWeb.Data;
using AkilliFiyatWeb.Entity;
using HtmlAgilityPack;
using AkilliFiyatWeb.Services;
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium.Support.UI;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace AkilliFiyatWeb.Services
{
	public class SokUrunServices
	{
		private readonly KelimeKontrol _kelimeKontrol;
		private MyLogger _log;
		public SokUrunServices(KelimeKontrol kelimeKontrol, MyLogger myLogger)
		{
			_kelimeKontrol = kelimeKontrol;
			_log = myLogger;
		}
		private string SokArama(string query)
		{
			string query2 = _kelimeKontrol.ConvertTurkishToEnglish2(query);

			try
			{
				query2 = WebUtility.UrlEncode(_kelimeKontrol.ConvertTurkishToEnglish2(query2));
				return "https://www.sokmarket.com.tr/arama?q=" + query2;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return "";
			}
		}

		public async Task<List<Urunler>> SokKayit(String query)
		{

			string searchUrl = SokArama(query);

			var urunlerList = new List<Urunler>();

			try
			{
				var httpClient = new HttpClient();
				var response = await httpClient.GetAsync(searchUrl);
				var content = await response.Content.ReadAsStringAsync();

				var htmlDocument = new HtmlDocument();
				htmlDocument.LoadHtml(content);

				var elements = htmlDocument.DocumentNode.SelectNodes("//div[@class='PLPProductListing_PLPCardParent__GC2qb']");

				if (elements != null)
				{
					foreach (var element in elements)
					{
						try
						{
							string html = element.InnerHtml;
							HtmlDocument test = new HtmlDocument();
							test.LoadHtml(html);

							var itemNameElement = test.DocumentNode.SelectSingleNode(".//h2[@class='CProductCard-module_title__u8bMW']");
							var itemPriceElement = test.DocumentNode.SelectSingleNode(".//span[@class='CPriceBox-module_price__bYk-c']");
							var imgElement = test.DocumentNode.SelectSingleNode(".//div[@class='CProductCard-module_imageContainer__aTMdz']//img");
							var ayrintiLink = test.DocumentNode.SelectSingleNode(".//a");

							if (itemNameElement != null && itemPriceElement != null && imgElement != null && ayrintiLink != null)
							{
								string urunAdi = itemNameElement.InnerText;
								string fiyat = itemPriceElement.InnerText.Replace("\u20BA", "");
								string dataSrc = imgElement.GetAttributeValue("src", "");
								string ayrintLinkString = "https://www.sokmarket.com.tr/" + ayrintiLink.GetAttributeValue("href", "");

								double benzerlikOrani = _kelimeKontrol.BenzerlikHesapla(_kelimeKontrol.ConvertTurkishToEnglish(query), _kelimeKontrol.ConvertTurkishToEnglish(urunAdi));
								int katSayi = _kelimeKontrol.IkinciKelime2(query, urunAdi);
								if (katSayi > 0)
								{
									benzerlikOrani += katSayi;
								}

								var itemEskiFiyat = test.DocumentNode.SelectSingleNode(".//span[@class='priceLineThrough js-variant-price']");

								if (benzerlikOrani > 0.10)
								{
									urunlerList.Add(new Urunler(urunAdi, $"{fiyat} \u20BA", dataSrc, "Şok", "/img/Sok.png", benzerlikOrani, ayrintLinkString, 0, null, 0));
								}
								else
								{
									// Log or handle low similarity
								}
							}

							else
							{
								if (itemNameElement == null)
								{
									Console.WriteLine("Item name element is null.");
								}
								if (itemPriceElement == null)
								{
									Console.WriteLine("Item price element is null.");
								}
								if (imgElement == null)
								{
									Console.WriteLine("Image element is null.");
								}
								if (ayrintiLink == null)
								{
									Console.WriteLine("Detail link element is null.");
								}
							}
						}
						catch (Exception ex)
						{
							// Handle inner loop exception
							_log.Log("1", ex.Message);
						}
					}
				}
				else
				{
					System.Console.WriteLine();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			return urunlerList;
		}

		private string RemovePartFromUrl(string url, string partToRemove)
		{
			return url.Replace(partToRemove, "");
		}

		public async Task<List<All_Products>> GetAllProducts()
		{
			List<All_Products> allSokProducts = new List<All_Products>();
			string[] categories = { "meyve-ve-sebze-c-20", "et-ve-tavuk-ve-sarkuteri-c-160", "anne-bebek-ve-cocuk-c-20634",
			"atistirmaliklar-c-20376", "sut-ve-sut-urunleri-c-460", "kahvaltilik-c-890", "ekmek-ve-pastane-c-1250",
			"dondurma-c-31102", "dondurulmus-urunler-c-1550", "yemeklik-malzemeler-c-1770", "icecek-c-20505",
			"kisisel-bakim-ve-kozmetik-c-20395", "temizlik-c-20647", "kagit-urunler-c-20875", "evcil-dostlar-c-20880",
			"elektronik-c-22769", "giyim-ayakkabi-ve-aksesuar-c-20886", "ev-ve-yasam-c-20898"};

			using (HttpClient client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
				client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");

				foreach (var category in categories)
				{
					for (int i = 1; i <= 100; i++)
					{
						try
						{
							string url = $"https://www.sokmarket.com.tr/{category}?page={i}";
							string pageContent = await client.GetStringAsync(url);

							HtmlDocument document = new HtmlDocument();
							document.LoadHtml(pageContent);

							var scriptNodes = document.DocumentNode.SelectNodes("//script");

							var productNodes = document.DocumentNode.SelectNodes("//div[contains(@class, 'CProductCard-module_containerTop__35Ma3')]");

							if(productNodes==null || productNodes.Count <1 )
							{
								break;
							}


							var json = await GetScriptJson(scriptNodes);
							if (json != null)
							{
								var products = json[0]["14"][1]["initialSearchResult"]["results"];

								// Her bir ürün için bilgileri çek
								foreach (var product in products)
								{
									// Ürün adını al
									string productName = product["product"]["name"].ToString();

									// İndirimli fiyatı al
									Double discountedPrice = Double.Parse(product["prices"]["discounted"]["value"].ToString().Replace('.', '#').Replace(',', '.').Replace('#', ','));
									Double eskiFiyat = Double.Parse(product["prices"]["original"]["value"].ToString().Replace('.', '#').Replace(',', '.').Replace('#', ','));
									// Resim URL'sini al
									string imageHost = product["product"]["images"][0]["host"].ToString();
									string imagePath = product["product"]["images"][0]["path"].ToString();
									string imageUrl = $"{imageHost}/{imagePath}";

									Double indirimOrani = ((Convert.ToDouble(eskiFiyat) - Convert.ToDouble(discountedPrice)) / Convert.ToDouble(eskiFiyat)) * 100;

									// Ürün bilgilerini listeye ekle
									All_Products sokProduct2 = new All_Products
									{
										UrunAdi = productName,
										Fiyat = Convert.ToString(discountedPrice),
										EskiFiyat = Convert.ToString(eskiFiyat),
										UrunResmi = imageUrl,
										MarketAdi = "Şok",
										IndirimOran = indirimOrani,
										MarketResmi = "/img/Sok.png"

									};
									allSokProducts.Add(sokProduct2);
								}
							}
						}
						catch (HttpRequestException ex)
						{
							_log.Log("1", ex.Message);
						}
						catch (Exception ex)
						{
							_log.Log("6", ex.Message);
						}
					}
				}
			}

			return allSokProducts;
		}


		public async Task<JArray> GetScriptJson(HtmlNodeCollection scriptNodes)
		{
			if (scriptNodes != null && scriptNodes.Count > 0)
			{
				string longestScriptContent = string.Empty;

				// En uzun __next_f.push içeriğini bul
				foreach (var node in scriptNodes)
				{
					// JSON içeren script etiketlerini bul
					if (node.InnerText.Contains("__next_f.push"))
					{
						// Script etiketinin içeriğini al
						string scriptContent = node.InnerText;

						// En uzun __next_f.push içeriğini bul
						if (scriptContent.Length > longestScriptContent.Length)
						{
							longestScriptContent = scriptContent;
						}
					}
				}

				if (!string.IsNullOrEmpty(longestScriptContent))
				{
					// JSON içeriğini temizleme işlemi
					string cleanedData = longestScriptContent.Substring(longestScriptContent.IndexOf("["));
					cleanedData = cleanedData.Substring(0, cleanedData.LastIndexOf("]") - 1) + "}]";


					// Kaçış karakterlerini temizle
					cleanedData = Regex.Unescape(cleanedData);

					// Beklenmeyen karakterleri temizle
					cleanedData = cleanedData.Replace("\"$\",", "").Replace("\"$L16\",", "");

					// JSON içeriğinin boş olup olmadığını kontrol et
					if (!string.IsNullOrEmpty(cleanedData))
					{
						try
						{
							// "14:[" kısmını "14": olarak düzelt
							cleanedData = cleanedData.Replace("1,\"14:[", "{\"14\":[");
							// JSON dizisini parse et
							JArray jsonArray = JArray.Parse(cleanedData);
							return jsonArray;
						}
						catch (JsonReaderException ex)
						{
							// JSON parse hatası oluştuğunda buraya düşer
							_log.Log("1", "JSON parse hatası: " + ex.Message);
						}
					}
					else
					{
						// Temizlenmiş JSON içeriği boşsa
						_log.Log("1", "Temizlenmiş JSON içeriği boş.");
					}
				}
				else
				{
					// En uzun script içeriği bulunamadı
					_log.Log("1", "En uzun script içeriği bulunamadı.");
				}
			}
			else
			{
				// Script etiketleri bulunamadı veya koleksiyon boş
				_log.Log("1", "Script etiketleri bulunamadı veya koleksiyon boş.");
			}

			// Eğer hiçbir JSON bulunamazsa veya içerik boşsa null döndür
			return null;
		}

















		public async Task<List<Urunler>> GetAllProductsSelenium()
		{
			try
			{
				List<Urunler> allSokProducts = new List<Urunler>();

				string[] categories = { "meyve-ve-sebze-c-20", "et-ve-tavuk-ve-sarkuteri-c-160" /* diğer kategoriler buraya eklenebilir */ };

				var chromeOptions = new ChromeOptions();
				//chromeOptions.AddArgument("--headless");
				using (IWebDriver driver = new ChromeDriver(chromeOptions))
				{
					foreach (var category in categories)
					{
						for (int i = 1; i <= 100; i++)
						{
							try
							{
								string url = $"https://www.sokmarket.com.tr/{category}?page={i}";
								driver.Navigate().GoToUrl(url);

								WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
								wait.Until(d => d.FindElement(By.CssSelector(".CProductCard-module_containerTop__35Ma3")));

								var productElements = driver.FindElements(By.CssSelector(".CProductCard-module_containerTop__35Ma3"));

								foreach (var element in productElements)
								{
									try
									{
										string itemName = element.FindElement(By.CssSelector(".CProductCard-module_title__u8bMW")).Text.Trim();
										string itemPriceSalt = element.FindElement(By.CssSelector(".CPriceBox-module_price__bYk-c")).Text.Trim();
										string itemPhotoURL = element.FindElement(By.CssSelector(".CProductCard-module_imageContainer__aTMdz img")).GetAttribute("src");

										float itemPrice = float.Parse(itemPriceSalt.Replace("₺", "").Replace(",", "."));

										Urunler sokProduct = new Urunler
										{
											UrunAdi = itemName,
											Fiyat = itemPrice.ToString("0.00"),
											MarketResmi = itemPhotoURL,
											MarketAdi = "Şok",
										};

										allSokProducts.Add(sokProduct);
									}
									catch (Exception ex)
									{
										_log.Log("Error parsing product details", ex.Message);
									}
								}
							}
							catch (WebDriverTimeoutException ex)
							{
								break;
							}
							catch (Exception ex)
							{
								_log.Log("General error", ex.Message);
							}
						}
					}
				}

				return await Task.FromResult(allSokProducts);
			}
			catch (Exception ex)
			{
				_log.Log("General error", ex.Message);
				return null;
			}


		}








	}
}