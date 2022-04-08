using App.Models;
using Business;
using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAutorService _autorService;

        public HomeController(ILogger<HomeController> logger, IAutorService autorService)
        {
            _logger = logger;
            _autorService = autorService;
        }

        public IActionResult Index()
        {
            _autorService.Adicionar(new Autor());

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}