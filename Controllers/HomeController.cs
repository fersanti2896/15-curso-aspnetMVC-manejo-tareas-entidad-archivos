﻿using ManejoTareas.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Diagnostics;

namespace ManejoTareas.Controllers {
    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;
        private readonly IStringLocalizer<HomeController> stringLocalizer;

        public HomeController(ILogger<HomeController> logger, 
                              IStringLocalizer<HomeController> stringLocalizer) {
            _logger = logger;
            this.stringLocalizer = stringLocalizer;
        }

        public IActionResult Index() {
            return View();
        }

        [HttpPost]
        public IActionResult CambiarIdioma(string cultura, string urlRetorno) {
            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(cultura)),
                                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(5) });

            return LocalRedirect(urlRetorno);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}