using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPortfolyoWebSite.Models
{
    public class SmtpSettings
    {
        public string? Server { get; set; }
        public int Port { get; set; }
        public string? Username { get; set; }
        public string? Receiver { get; set; }
        public string? Password { get; set; }
    }
}