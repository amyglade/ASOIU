using Microsoft.AspNetCore.Mvc;

namespace LabApp.Controllers;

/// <summary>
/// Контроллер главной страницы приложения
/// </summary>
public class HomeController : Controller
{
    /// <summary>Отображает главную страницу с навигацией</summary>
    public IActionResult Index() => View();
}
