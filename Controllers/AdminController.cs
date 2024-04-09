using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        [HttpDelete]
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














    }
}
