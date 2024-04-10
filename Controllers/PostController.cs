using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MyPortfolyoWebSite.Entity;
using MyPortfolyoWebSite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogAppProjesi.Controllers
{
    public class PostController : Controller
    {
        private readonly IdentityContext _context;
        public PostController(IdentityContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Details(string url)
        {
            ViewBag.NewPosts = await _context.Portfolios.OrderByDescending(p=>p.PortfolioId).Take(5).ToListAsync();
            return View(
                await _context
                        .Portfolios
                        .FirstOrDefaultAsync(p => p.URL == url)
                );
        }

    }
}