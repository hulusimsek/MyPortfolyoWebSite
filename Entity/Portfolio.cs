using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPortfolyoWebSite.Entity
{
    public class Portfolio
    {
        public int PortfolioId { get; set; }
        public string? ProjectName { get; set; }
        public string? Category { get; set; }
        public string? URL { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

    }
}