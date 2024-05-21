using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AkilliFiyatWeb.Entity
{
    public class Log
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
    }
}