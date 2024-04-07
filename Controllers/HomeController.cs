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
            var aboutMe = await _context.AboutMe.FirstOrDefaultAsync();
            if (aboutMe == null)
            {
                return View();
            }

            ViewBag.AboutMe = aboutMe;

            // Diğer modeli de ViewBag'e ekle

            return View();
        }
}