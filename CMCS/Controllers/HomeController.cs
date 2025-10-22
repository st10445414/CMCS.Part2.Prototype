using System.Diagnostics;
using CMCS.Models;
using Microsoft.AspNetCore.Mvc;

namespace CMCS.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
