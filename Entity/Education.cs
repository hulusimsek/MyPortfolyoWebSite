using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPortfolyoWebSite.Entity
{
    public class Education
    {
        public int EducationId { get; set; }
        public string? SchoolName { get; set; }
        public string? Degree { get; set; }
        public string? Field { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
}