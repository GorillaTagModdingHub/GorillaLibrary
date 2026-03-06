using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace GorillaLibrary.Models;

public class WebRequest
{
    public RequestMethod Method { get; set; }
    public JObject PostData { get; set; }
    public string ContentType { get; set; } = "application/json";
    public Dictionary<string, string> Headers { get; set; }
}
