using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPortfolyoWebSite.Entity
{
    public class Experience
    {
        public int ExperienceId { get; set; }
        public string? CompanyName { get; set; }
        public string? Position { get; set; }
        public string? City { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }
    }
}