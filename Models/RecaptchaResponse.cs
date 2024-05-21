using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MyPortfolyoWebSite.Models
{
    public class CaptchaResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }
 
    [JsonProperty("error-codes")]
    public List<string> ErrorCodes { get; set; }
}
}