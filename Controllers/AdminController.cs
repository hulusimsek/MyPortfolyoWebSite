using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyPortfolyoWebSite.Entity;
using MyPortfolyoWebSite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyPortfolyoWebSite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IdentityContext _context;
        private const string DefaultProfilePicturePath = "aboutme.jpg";

        public AdminController(IdentityContext context)
        {
            _context = context;
        }

        // Admin panel ana sayfası
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Header()
        {
            // Retrieve the single Header entry from the database
            Header header = await _context.Header.FirstOrDefaultAsync() ?? new Header();
            return View(header);
        }

        // POST action for handling form submission to edit Header
        [HttpPost]
        public async Task<IActionResult> Header(Header model, IFormFile CvPath, IFormFile imageFile)
        {
            try
            {

                var extension = "";
                if (imageFile != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    extension = Path.GetExtension(imageFile.FileName); // abc.jpg

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Geçerli bir resim seçiniz.");
                    }
                }
                // Resim dosyası yüklendiyse
                if (imageFile != null)
                {
                    var randomFileName = "profile.png";
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/", randomFileName);

                    // Resim dosyasını wwwroot/img/ klasörüne kaydet
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Profil resminin yolunu modelde güncelle
                    model.ProfilePicturePath = "/img/" + randomFileName;
                }


                // Handle CV file upload if a file is provided
                if (CvPath != null && CvPath.Length > 0)
                {
                    extension = Path.GetExtension(CvPath.FileName).ToLower();
                    var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Please select a valid CV file.");
                        return View(model);
                    }

                    var fileName = "cv" + extension; // Or generate a random file name
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/cv/", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await CvPath.CopyToAsync(stream);
                    }

                    // Update the CV path in the model
                    model.CvPath = "/cv/" + fileName;
                }

                // Check if Header entry already exists in the database
                var existingHeader = await _context.Header.FirstOrDefaultAsync();
                if (existingHeader != null)
                {
                    // Update existing Header entry
                    existingHeader.Name = model.Name;
                    existingHeader.TypedText = model.TypedText;

                    // Update the CV path only if a new file was uploaded
                    if (CvPath != null && CvPath.Length > 0)
                    {
                        existingHeader.CvPath = model.CvPath; // Use the updated path
                    }

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        existingHeader.ProfilePicturePath = model.ProfilePicturePath; // Use the updated path
                    }
                }
                else
                {
                    // Add new Header entry
                    _context.Header.Add(model);
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Redirect to the admin index page or any other appropriate action
                return RedirectToAction("Index", "Admin");
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                string errorMessage = ex.Message;
                string stackTrace = ex.StackTrace ?? "";

                // Hata kaydını veritabanına ekle
                _context.ErrorLogs.Add(new ErrorLogs
                {
                    ErrorMessage = errorMessage,
                    StackTrace = stackTrace?? ""
                });
                await _context.SaveChangesAsync();

                return NotFound();

            }
        }

        // About Me için GET action methodu
        public IActionResult AboutMe()
        {
            // Assuming you have a single AboutMe entry in the database
            var aboutMe = _context.AboutMe.Include(x => x.Skills).FirstOrDefault();
            if (aboutMe == null)
            {
                aboutMe = new AboutMe(); // Varsayılan bir AboutMe nesnesi oluştur
            }
            return View(aboutMe);
        }

        // POST: AboutMe/Edit
        [HttpPost]
        public async Task<IActionResult> AboutMe(AboutMe model, IFormFile imageFile, List<Skill> skills)
        {
            try
            {
                var extension = "";
                if (imageFile != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    extension = Path.GetExtension(imageFile.FileName); // abc.jpg

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Geçerli bir resim seçiniz.");
                    }
                }
                // Resim dosyası yüklendiyse
                if (imageFile != null)
                {
                    var randomFileName = "about.png";
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/", randomFileName);

                    // Resim dosyasını wwwroot/img/ klasörüne kaydet
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Profil resminin yolunu modelde güncelle
                    model.ProfilePicturePath = "/img/" + randomFileName;
                }

                // Mevcut AboutMe kaydını güncelle veya ekle
                var existingAboutMe = await _context.AboutMe.FirstOrDefaultAsync();
                if (existingAboutMe != null)
                {
                    existingAboutMe.Name = model.Name;
                    existingAboutMe.Description = model.Description;
                    existingAboutMe.ProfilePicturePath = "/img/about.png";
                    existingAboutMe.Skills = skills;
                }
                else
                {
                    _context.AboutMe.Add(model);
                }

                await _context.SaveChangesAsync();

                // Mevcut becerileri güncelle veya ekle
                foreach (var skill in skills)
                {
                    var existingSkill = await _context.Skills.FirstOrDefaultAsync(s => s.SkillId == skill.SkillId);
                    if (existingSkill != null)
                    {
                        existingSkill.Value = skill.Value;
                    }
                    else
                    {
                        _context.Skills.Add(skill);
                    }
                }
                await _context.SaveChangesAsync();

                // Silinmiş becerileri veritabanından kaldır
                // Mevcut becerilerin benzersiz kimliklerini alın
                var existingSkillIds = skills.Select(s => s.SkillId);

                // Mevcut becerilerden silinecek becerilerin listesini belirleyin
                var skillsToRemove = new List<Skill>();




                // Değişiklikleri kaydedin
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Admin");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.ToString()}");
                // Hata durumunda ana sayfaya yönlendir ve hata mesajını TempData üzerinden aktar
                TempData["ErrorMessage"] = "An error occurred while saving your data.";
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveSkill(int skillId)
        {
            try
            {
                // Silinecek beceriyi bul
                var skillToRemove = await _context.Skills.FindAsync(skillId);

                if (skillToRemove == null)
                {
                    return NotFound();
                }

                // Silinecek beceriyi veritabanından kaldır
                _context.Skills.Remove(skillToRemove);
                await _context.SaveChangesAsync();

                return Ok(); // Başarı durumunu döndür
            }
            catch (Exception ex)
            {
                // Hata durumunda istemciye hata mesajını döndür
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public async Task<IActionResult> Education()
        {
            var educations = await _context.Educations.ToListAsync();
            return View(educations);
        }
        public IActionResult EducationCreate()
        {
            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EducationCreate(Education education)
        {
            await _context.Educations.AddAsync(education);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> EducationEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var education = await _context.Educations.FindAsync(id);
            if (education == null)
            {
                return NotFound();
            }
            return View(education);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EducationEdit(int id, Education education)
        {
            if (id != education.EducationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(education);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EducationExists(education.EducationId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(education);
        }

        // GET: Admin/Delete/5
        public async Task<IActionResult> EducationDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var education = await _context.Educations
                .FirstOrDefaultAsync(m => m.EducationId == id);
            if (education == null)
            {
                return NotFound();
            }
            _context.Educations.Remove(education);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Education));
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var education = await _context.Educations.FindAsync(id);
            _context.Educations.Remove(education);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EducationExists(int id)
        {
            return _context.Educations.Any(e => e.EducationId == id);
        }


        public async Task<IActionResult> Experience()
        {
            List<Experience> experiences = await _context.Experiences.ToListAsync();
            if (experiences == null)
            {
                experiences = new List<Experience>();
            }
            return View(experiences);
        }

        // Belirli bir deneyimi düzenlemek için get işlemi
        public async Task<IActionResult> ExperienceEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var experience = await _context.Experiences.FindAsync(id);
            if (experience == null)
            {
                return NotFound();
            }

            return View(experience);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExperienceEdit(int id, [Bind("ExperienceId,CompanyName,Position,City,StartDate,EndDate,Description")] Experience experience)
        {
            if (id != experience.ExperienceId)
            {
                System.Console.WriteLine("hata 1");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(experience);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (_context.Experiences.Any(e => e.ExperienceId == id))
                    {
                        System.Console.WriteLine("hata 2");
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(experience);
        }

        // Yeni bir deneyim eklemek için get işlemi
        public IActionResult ExperienceCreate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExperienceCreate([Bind("ExperienceId,CompanyName,Position,City,StartDate,EndDate,Description")] Experience experience)
        {

            _context.Add(experience);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ExperienceDelete(int id)
        {
            var experience = await _context.Experiences.FindAsync(id);
            if (experience == null)
            {
                return NotFound();
            }

            _context.Experiences.Remove(experience);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Service()
        {
            var services = _context.Services.ToList();
            return View(services);
        }

        // GET: /Service/ServiceCreate
        public IActionResult ServiceCreate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ServiceCreate(Service model, IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length < 1)
            {
                return View(model);
            }
            try
            {
                var extension = "";
                if (imageFile != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".ico" };
                    extension = Path.GetExtension(imageFile.FileName); // abc.jpg

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Geçerli bir resim seçiniz.");
                    }
                }
                // Resim dosyası yüklendiyse
                if (imageFile != null)
                {
                    var randomFileName = string.Format($"{Guid.NewGuid().ToString()}{extension}");
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", randomFileName);

                    // Resim dosyasını wwwroot/img/ klasörüne kaydet
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Profil resminin yolunu modelde güncelle
                    model.IconPicturePath = "/img/" + randomFileName;
                }

                // Mevcut AboutMe kaydını güncelle veya ekle
                _context.Services.Add(model);

                await _context.SaveChangesAsync();

                // Mevcut becerileri güncelle veya ekle
                return RedirectToAction("Service", "Admin");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.ToString()}");
                // Hata durumunda ana sayfaya yönlendir ve hata mesajını TempData üzerinden aktar
                TempData["ErrorMessage"] = "An error occurred while saving your data.";
                return View("Error");
            }
        }

        // GET: /Service/ServiceEdit/5
        public IActionResult ServiceEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = _context.Services.Find(id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ServiceEdit(int id, Service model, IFormFile imageFile)
        {
            if (id != model.ServiceId)
            {
                return NotFound();
            }

            if (imageFile == null || imageFile.Length == 0)
            {
                // Resim dosyası yüklenmediği için modelin geçerli olmasını sağla
                ModelState.Remove("IconPicturePath");
            }

            try
            {
                var service = await _context.Services.FindAsync(id);
                if (service == null)
                {
                    return NotFound();
                }

                service.ServiceName = model.ServiceName;
                service.Description = model.Description;

                if (imageFile != null && imageFile.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".ico" };
                    var extension = Path.GetExtension(imageFile.FileName);

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Please select a valid image.");
                        return View(model);
                    }

                    var randomFileName = $"{Guid.NewGuid().ToString()}{extension}";
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", randomFileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Eğer yeni resim yüklendiyse, eski resmi sil
                    if (!string.IsNullOrEmpty(service.IconPicturePath))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", service.IconPicturePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    service.IconPicturePath = "/img/" + randomFileName;
                }

                _context.Services.Update(service);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Service));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.ToString()}");
                TempData["ErrorMessage"] = "An error occurred while saving your data.";
                return View("Error");
            }
        }


        // GET: /Service/ServiceDelete/5
        [HttpPost]
        public async Task<IActionResult> ServiceDelete(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            try
            {
                // Resim dosyasını sil
                if (!string.IsNullOrEmpty(service.IconPicturePath))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", service.IconPicturePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.ToString()}");
                TempData["ErrorMessage"] = "An error occurred while deleting the service.";
                return View("Error");
            }
        }


        public async Task<IActionResult> Category()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Category(List<Category> categories, int id)
        {
            if (ModelState.IsValid)
            {
                foreach (var category in categories)
                {
                    if (category.CategoryId == 0)
                    {
                        _context.Categories.Add(category);
                    }
                    else
                    {
                        _context.Categories.Update(category);
                    }
                }

                await _context.SaveChangesAsync();
                // Edit action'ına ve gerekli id parametresine yönlendir
                return RedirectToAction("Portfolio");
            }

            // ModelState geçerli değilse, formu tekrar gösterin
            return View(categories);
        }


        [HttpPost]
        public async Task<IActionResult> RemoveCategory(int categoryId)
        {
            var categoryToRemove = await _context.Categories.FindAsync(categoryId);
            if (categoryToRemove == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(categoryToRemove);
            await _context.SaveChangesAsync();

            return Ok(); // Başarılı olduğunda 200 OK yanıtı gönderin
        }

        [HttpGet]
        public IActionResult CategoryCreate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryCreate(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Category));
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryDelete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Category));
        }
        public async Task<IActionResult> Portfolio()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync() ?? new List<Category>();
            return View(await _context.Portfolios.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> PortfolioCreate()
        {
            var categories = await _context.Categories.ToListAsync();
            if (categories != null && categories.Count > 0)
            {
                ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");
            }
            else
            {
                // Eğer kategoriler null veya boş ise, bu durumu ele almak için bir şeyler yapabilirsiniz.
                // Örneğin, ViewBag.Categories'i boş bir SelectList ile doldurabilir veya bir hata mesajı gösterebilirsiniz.
                ViewBag.Categories = new SelectList(new List<Category>(), "CategoryId", "Name");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PortfolioCreate(Portfolio model, IFormFile imageFile)
        {
            try
            {
                var extension = "";
                if (imageFile != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    extension = Path.GetExtension(imageFile.FileName); // abc.jpg

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Geçerli bir resim seçiniz.");
                    }
                }
                // Resim dosyası yüklendiyse
                if (imageFile != null)
                {
                    var randomFileName = string.Format($"{Guid.NewGuid().ToString()}{extension}");
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", randomFileName);

                    // Resim dosyasını wwwroot/img/ klasörüne kaydet
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Profil resminin yolunu modelde güncelle
                    model.ImageUrl = "/img/" + randomFileName;
                }

                // URL kontrolü yap
                var isURLExist = await _context.Portfolios.AnyAsync(p => p.URL == model.URL);
                if (isURLExist)
                {
                    ModelState.AddModelError("", "Bu URL zaten kullanılıyor. Başka bir URL seçin.");
                    return StatusCode(500, "This url is already used");
                }

                // Portfolio modelini veritabanına ekle
                await _context.Portfolios.AddAsync(model);
                await _context.SaveChangesAsync();

                return RedirectToAction("Portfolio");

            }
            catch (Exception ex)
            {
                // Hata durumunda hata mesajını logla ve hata sayfasına yönlendir
                Console.WriteLine($"Error occurred: {ex.ToString()}");
                TempData["ErrorMessage"] = "An error occurred while creating portfolio.";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PortfolioEdit(int id)
        {
            var portfolio = await _context.Portfolios.FindAsync(id);
            var categories = await _context.Categories.ToListAsync();

            // Eğer categories null değilse, SelectList'i oluştur ve ViewBag.Categories'e ata
            if (categories != null)
            {
                ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");
            }
            else
            {
                // Categories null ise, bir hata mesajı göster
                ViewBag.ErrorMessage = "Categories data is null.";
            }

            // PortfolioCreate view'ünü döndür ve SelectList'i ile birlikte gönder
            if (portfolio == null)
            {
                return NotFound();
            }
            return View(portfolio);
        }

        public async Task<IActionResult> PortfolioEdit(int id, Portfolio model, IFormFile imageFile)
        {
            if (id != model.PortfolioId)
            {
                return NotFound();
            }

            if (imageFile == null || imageFile.Length == 0)
            {
                // Resim dosyası yüklenmediği için modelin geçerli olmasını sağla
                ModelState.Remove("ImageUrl");
            }

            try
            {
                var portfolio = await _context.Portfolios.FindAsync(id);
                if (portfolio == null)
                {
                    return NotFound();
                }

                portfolio.ProjectName = model.ProjectName;
                portfolio.Category = model.Category;
                portfolio.URL = model.URL;
                portfolio.Description = model.Description;

                if (imageFile != null && imageFile.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var extension = Path.GetExtension(imageFile.FileName);

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Please select a valid image.");
                        return View(model);
                    }

                    var randomFileName = $"{Guid.NewGuid().ToString()}{extension}";
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", randomFileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Eğer yeni resim yüklendiyse, eski resmi sil
                    if (!string.IsNullOrEmpty(portfolio.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", portfolio.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    portfolio.ImageUrl = "/img/" + randomFileName;
                }

                _context.Portfolios.Update(portfolio);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Portfolio));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.ToString()}");
                TempData["ErrorMessage"] = "An error occurred while saving your data.";
                return View("Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> PortfolioDelete(int id)
        {
            var portfolio = await _context.Portfolios.FindAsync(id);
            if (portfolio == null)
            {
                return NotFound();
            }

            try
            {
                // Resim dosyasını sil
                if (!string.IsNullOrEmpty(portfolio.ImageUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", portfolio.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Portfolios.Remove(portfolio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.ToString()}");
                TempData["ErrorMessage"] = "An error occurred while deleting the portfolio.";
                return View("Error");
            }
        }





        public IActionResult Contact()
        {
            // Assuming you have a single AboutMe entry in the database
            var contact = _context.Contacts.Include(x => x.LinkIcons).FirstOrDefault();
            if (contact == null)
            {
                contact = new Contact(); // Varsayılan bir AboutMe nesnesi oluştur
            }
            return View(contact);
        }

        [HttpPost]
        public async Task<IActionResult> Contact(Contact contact, List<LinkIcon> linkIcons)
        {
            try
            {
                // Eğer gelen contact null ise, BadRequest yanıtı döndür
                if (contact == null)
                {
                    return BadRequest("Invalid contact object");
                }

                // Eğer ModelState geçerli değilse, BadRequest yanıtı döndür
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Mevcut Contact kaydını güncelle veya ekle
                var existingContact = await _context.Contacts.FirstOrDefaultAsync();
                if (existingContact != null)
                {
                    existingContact.Name = contact.Name;
                    existingContact.Job = contact.Job;
                    existingContact.Email = contact.Email;
                    existingContact.Telephone = contact.Telephone;
                    existingContact.Localization = contact.Localization;

                    // LinkIcons listesini güncelle
                    existingContact.LinkIcons = linkIcons;
                }
                else
                {
                    // Yeni bir Contact oluştur
                    var newContact = new Contact
                    {
                        Name = contact.Name,
                        Job = contact.Job,
                        Email = contact.Email,
                        Telephone = contact.Telephone,
                        Localization = contact.Localization,
                        LinkIcons = linkIcons
                    };

                    // Yeni Contact'i ekle
                    _context.Contacts.Add(newContact);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Admin"); // Başarı durumunda Admin Index sayfasına yönlendir
            }
            catch (Exception ex)
            {
                // Hata durumunda ana sayfaya yönlendir ve hata mesajını TempData üzerinden aktar
                TempData["ErrorMessage"] = "An error occurred while saving your data.";
                return RedirectToAction("Error", "Home");
            }
        }





        [HttpPost]
        public IActionResult RemoveLinkIcon(int linkIconId)
        {
            try
            {
                // Link ikonunu veritabanından bul
                var linkIcon = _context.LinkIcons.Find(linkIconId);

                // Eğer link ikonu bulunamazsa hata döndür
                if (linkIcon == null)
                {
                    return NotFound();
                }

                // Link ikonunu veritabanından kaldır
                _context.LinkIcons.Remove(linkIcon);
                _context.SaveChanges();

                // Başarı durumunda 200 OK yanıtı döndür
                return Ok();
            }
            catch (Exception ex)
            {
                // Hata durumunda 500 Internal Server Error yanıtı döndür
                return StatusCode(500, ex.Message);
            }
        }

        public async Task<IActionResult> Messages()
        {
            var messages = await _context.Messages.ToListAsync();
            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> MessagesDelete(int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
            {
                return NotFound();
            }

            _context.Messages.Remove(message);
            _context.SaveChanges();

            return RedirectToAction(nameof(Messages));
        }














    }
}
