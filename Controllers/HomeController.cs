using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyPortfolyoWebSite.Entity;
using MyPortfolyoWebSite.Models;
using Newtonsoft.Json;

namespace MyPortfolyoWebSite.Controllers;

public class HomeController : Controller
{
	private readonly IdentityContext _context;
	private readonly SmtpSettings _smtpSettings;
	public HomeController(IdentityContext context, IOptions<SmtpSettings> smtpSettings)
	{
		_context = context;
		_smtpSettings = smtpSettings.Value;
	}
	public async Task<IActionResult> Index()
	{
		try
		{
			var header = await _context.Header.FirstOrDefaultAsync();
			ViewBag.Header = header;


			var aboutMe = await _context.AboutMe.Include(x => x.Skills).FirstOrDefaultAsync();
			ViewBag.AboutMe = aboutMe;


			// Eğitim bilgilerini veritabanından alın veya başka bir kaynaktan alın
			var educations = await _context.Educations
										.OrderByDescending(e => e.EducationId)
										.ToListAsync();


			// Eğitim bilgilerini ikiye ayırma
			var secondHalf = educations.Take(educations.Count / 2).ToList();
			var firstHalf = educations.Skip(educations.Count / 2).ToList();

			ViewBag.FirstHalfEducations = firstHalf;
			ViewBag.SecondHalfEducations = secondHalf;

			var experience = await _context.Experiences
										.OrderByDescending(e => e.ExperienceId)
										.ToListAsync();
			ViewBag.Experiences = experience;

			var service = await _context.Services
										.OrderByDescending(e => e.ServiceId)
										.ToListAsync();

			ViewBag.Services = service;

			var portfolio = await _context.Portfolios
										.OrderByDescending(e => e.PortfolioId)
										.ToListAsync();

			ViewBag.Portfolio = portfolio;

			var category = await _context.Categories
										.OrderByDescending(e => e.CategoryId)
										.ToListAsync();

			ViewBag.Category = category;

			var contact = await _context.Contacts
										.Include(x => x.LinkIcons).FirstOrDefaultAsync();

			ViewBag.Contact = contact;
			// Diğer modeli de ViewBag'e ekle

			return View();
		}
		catch (Exception ex)
		{
			string errorMessage = ex.Message;
			string stackTrace = ex.StackTrace ?? "";

			// Hata kaydını veritabanına ekle
			_context.ErrorLogs.Add(new ErrorLogs
			{
				ErrorMessage = errorMessage,
				StackTrace = stackTrace ?? ""
			});
			await _context.SaveChangesAsync();
			return View("Error");
		}
	}

	[HttpPost]
	public IActionResult Contact(ContactViewModel model, [FromForm(Name = "g-recaptcha-response")] string recaptchaResponse)
	{
		const string secret = "6LeFBbgpAAAAAKy-mHiSk0haDC9Y7DhV-O5haGAc";

		var client = new WebClient();
		var reply = client.DownloadString($"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={recaptchaResponse}");

		var captchaResponse = JsonConvert.DeserializeObject<CaptchaResponse>(reply);

		if (!captchaResponse.Success)
		{
			TempData["Message"] = "Lütfen güvenliği doğrulayınız.";
			return BadRequest("reCAPTCHA doğrulaması başarısız.");
		}
		else
		{

			if (ModelState.IsValid)
			{
				// ContactViewModel'den bir ContactEntity oluştur
				var contact = new Messages
				{
					Name = model.Name,
					Email = model.Email,
					Subject = model.Subject,
					Message = model.Message
				};

				// ContactEntity'yi veritabanına ekle
				_context.Messages.Add(contact);
				_context.SaveChanges();

				// Başarılı bir şekilde kaydedildikten sonra başka bir sayfaya yönlendirme
				return Ok(); // 200 OK yanıtı döndür
			}
		}
		// ModelState geçerli değilse, uygun bir yanıt döndür
		return BadRequest(ModelState); // Geçersiz model durumunda BadRequest yanıtı döndür
	}

}
