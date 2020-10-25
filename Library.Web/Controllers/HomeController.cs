using CommonData.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Library.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly MassTransit.IPublishEndpoint _publishEndpoint;
        private readonly MassTransit.IBus _bus;

        private const string SessionStatus = "_Status";
        private const string SessionName = "_Name";
        private const string SessionUserEmail = "_Email";
        private const string SessionUserId = "_UserID";

        public HomeController(MassTransit.IPublishEndpoint publishEndpoint, MassTransit.IBus bus)
        {
            _publishEndpoint = publishEndpoint;
            _bus = bus;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Login", "Users");
        }

        public IActionResult About()
        {
            ViewBag.fullName = HttpContext.Session.GetString(SessionName);
            ViewBag.Email = HttpContext.Session.GetString(SessionUserEmail);
            ViewBag.status = HttpContext.Session.GetInt32(SessionStatus);

            return View();
        }

        public IActionResult Privacy()
        {
            ViewBag.fullName = HttpContext.Session.GetString(SessionName);
            ViewBag.Email = HttpContext.Session.GetString(SessionUserEmail);
            ViewBag.status = HttpContext.Session.GetInt32(SessionStatus);

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
