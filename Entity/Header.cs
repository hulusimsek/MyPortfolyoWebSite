using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyPortfolyoWebSite.Entity
{
    public class Header
    {
        public int HeaderId { get; set; }
        public string? Name { get; set; }
        public string? TypedText { get; set; }
        public string? ProfilePicturePath { get; set; }
        public string? CvPath { get; set; } // Sabit yol
    }
}