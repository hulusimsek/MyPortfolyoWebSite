using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AkilliFiyatWeb.Data;
using AkilliFiyatWeb.Entity;

namespace AkilliFiyatWeb.Services
{
    public class MyLogger
{
    private readonly DataContext _context;

    public MyLogger(DataContext context)
    {
        _context = context;
    }

    public void Log(string level, string message)
    {
        _context.Logs.Add(new Log { Level = level, Message = message, Timestamp = DateTime.Now, Exception = "" });
        _context.SaveChanges();
    }
    public void Log(string level, string message, string exception)
    {
        _context.Logs.Add(new Log { Level = level, Message = message, Timestamp = DateTime.Now, Exception = exception });
        _context.SaveChanges();
    }
}
}