using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPortfolyoWebSite.Entity
{
    public class LinkIcon
    {
        public int LinkIconId { get; set; }
        public string? Name { get; set; } // Icon'un adı veya tanımı
        public string? IconClass { get; set; } // Bootstrap veya başka bir frameworkte kullanılacak CSS class'ı
        public string? Url { get; set; } // Bağlantının URL'si
    }
}