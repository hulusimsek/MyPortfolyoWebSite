using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPortfolyoWebSite.Models;

namespace MyPortfolyoWebSite.Controllers;

public class HomeController : Controller
{
    private readonly IdentityContext _context;
    public HomeController(IdentityContext context)
    {
        _context = context;
    }
    public async Task<IActionResult> Index()
    {
        var aboutMe = await _context.AboutMe.Include(x => x.Skills).FirstOrDefaultAsync();
        ViewBag.AboutMe = aboutMe;


        // Eğitim bilgilerini veritabanından alın veya başka bir kaynaktan alın
        var educations = _context.Educations
                                    .OrderByDescending(e => e.EducationId)
                                    .ToList();


        // Eğitim bilgilerini ikiye ayırma
        var secondHalf = educations.Take(educations.Count / 2).ToList();
        var firstHalf = educations.Skip(educations.Count / 2).ToList();

        ViewBag.FirstHalfEducations = firstHalf;
        ViewBag.SecondHalfEducations = secondHalf;

        var experience = _context.Experiences
                                    .OrderByDescending(e => e.ExperienceId)
                                    .ToList();
        ViewBag.Experiences = experience;
        // Diğer modeli de ViewBag'e ekle

        return View();
    }
}
