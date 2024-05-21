using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPortfolyoWebSite.Entity
{
    public class ErrorLogs
    {
        public int Id { get; set; }
        public DateTime LogDate { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
    }
}