using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPortfolyoWebSite.Entity
{
    public class Contact
    {
        public int ContactId { get; set; }
        public string? Name { get; set; }
        public string? Job { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Localization { get; set; }
        public List<LinkIcon> LinkIcons { get; set; } = new();
    }
}