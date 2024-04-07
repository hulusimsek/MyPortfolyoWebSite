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
            var aboutMe = _context.AboutMe.FirstOrDefault();
            if (aboutMe == null)
            {
                aboutMe = new AboutMe(); // Varsayılan bir AboutMe nesnesi oluştur
            }
            return View(aboutMe);
        }

        // POST: AboutMe/Edit
        [HttpPost]
        public async Task<IActionResult> AboutMe(AboutMe model, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
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


                    // Check if AboutMe already exists in database
                    var existingAboutMe = await _context.AboutMe.FirstOrDefaultAsync(a => a.AboutMeId == model.AboutMeId);
                    if (existingAboutMe != null)
                    {
                        // Update existing AboutMe
                        existingAboutMe.Name = model.Name;
                        existingAboutMe.Description = model.Description;
                        existingAboutMe.ProfilePicturePath = model.ProfilePicturePath;
                    }
                    else
                    {
                        // Add new AboutMe
                        await _context.AboutMe.AddAsync(model);
                    }

                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred: {ex.ToString()}");
                    // Hata durumunda ana sayfaya yönlendir ve hata mesajını TempData üzerinden aktar
                    TempData["ErrorMessage"] = "An error occurred while saving your data.";
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Skills()
        {
            var skills = await _context.Skills.ToListAsync();
            if (skills == null || !skills.Any())
            {
                skills = new List<Skill> { new Skill() }; // Varsayılan bir Skill nesnesi oluştur
            }
            return View(skills);
        }

        [HttpPost]
        public async Task<IActionResult> Skills(List<Skill> skills)
        {
            // Assuming you have a single AboutMe entry in the database
            if (ModelState.IsValid)
            {
                try
                {
                    // Veritabanındaki tüm becerileri çek
                    var existingSkills = _context.Skills.ToList();

                    // Formda olmayan becerileri sil
                    foreach (var existingSkill in existingSkills)
                    {
                        if (!skills.Any(s => s.SkillId == existingSkill.SkillId))
                        {
                            _context.Skills.Remove(existingSkill);
                        }
                    }

                    // Formdan gelen her beceri için kontrol yap
                    foreach (var skill in skills)
                    {
                        var existingSkill = existingSkills.FirstOrDefault(s => s.SkillId == skill.SkillId);
                        if (existingSkill != null)
                        {
                            // Eğer beceri veritabanında varsa güncelle
                            existingSkill.Name = skill.Name;
                            existingSkill.Value = skill.Value;
                        }
                        else
                        {
                            // Eğer beceri veritabanında yoksa ekle
                            _context.Skills.Add(skill);
                        }
                    }

                    // Değişiklikleri kaydet
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred: {ex.ToString()}");
                    // Hata durumunda ana sayfaya yönlendir ve hata mesajını TempData üzerinden aktar
                    TempData["ErrorMessage"] = "An error occurred while saving your data.";
                    return View("Error");
                }
            }
            else
            {
                return View("Error");
            }
        }





    }
}
