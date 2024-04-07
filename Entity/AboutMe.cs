using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyPortfolyoWebSite.Entity
{
    public class AboutMe
    {
        public int? AboutMeId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ProfilePicturePath { get; set; } // Sabit yol
    }
}