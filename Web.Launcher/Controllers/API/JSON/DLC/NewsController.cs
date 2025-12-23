using Microsoft.AspNetCore.Mvc;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Bundles.Internal;
using System.Xml;

namespace Web.Launcher.Controllers.API.JSON.DLC;

[Route("api/json/dlc/news")]
public class NewsController(InternalNews internalNews) : Controller
{
    [HttpGet]
    public IActionResult GetNews()
    {
        var sb = new SeparatedStringBuilder('\n');

        foreach (var notes in internalNews.News)
        {
            if (!string.IsNullOrEmpty(sb.ToString()))
                break;

            if (!notes.DefaultNews && !string.IsNullOrEmpty(notes.StartDate) &&
                !string.IsNullOrEmpty(notes.EndDate) &&
                DateTime.Now >= XmlConvert.ToDateTime(notes.StartDate, "MM/dd/yyyy") &&
                DateTime.Now <= XmlConvert.ToDateTime(notes.EndDate, "MM/dd/yyyy"))
            {
                sb.Append(notes.NewsDate);

                foreach (var line in notes.Notes)
                    sb.Append(line);
            }
            else if (notes.DefaultNews)
            {
                sb.Append(notes.NewsDate);

                foreach (var line in notes.Notes)
                    sb.Append(line);
            }
        }

        return Ok(sb.ToString());
    }
}
