using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabApp.Data;

namespace LabApp.Controllers;

/// <summary>
/// Контроллер отчётов. Формирует три раздела исключительно средствами LINQ.
/// </summary>
public class ReportController : Controller
{
    /// <summary>
    /// Строит три раздела отчёта и передаёт результаты в представление.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        using var ctx = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data Source=lab.db").Options);

        // Раздел 1: полный список экспериментов с названием лаборатории
        var report1 = await ctx.Experiments
            .Include(e => e.Laboratory)
            .OrderBy(e => e.Name)
            .Select(e => new Report1Row
            {
                Name          = e.Name,
                LaboratoryName = e.Laboratory!.Name,
                DurationHours = e.DurationHours
            })
            .ToListAsync();

        // Раздел 2: количество экспериментов по лабораториям
        var report2 = await ctx.Experiments
            .GroupBy(e => e.Laboratory!.Name)
            .Select(g => new Report2Row
            {
                LaboratoryName = g.Key,
                Count          = g.Count()
            })
            .OrderBy(r => r.LaboratoryName)
            .ToListAsync();

        // Раздел 3: средняя длительность по лабораториям, убывание
        var report3 = await ctx.Experiments
            .GroupBy(e => e.Laboratory!.Name)
            .Select(g => new Report3Row
            {
                LaboratoryName  = g.Key,
                AvgDurationHours = g.Average(e => e.DurationHours)
            })
            .OrderByDescending(r => r.AvgDurationHours)
            .ToListAsync();

        ViewBag.Report1 = report1;
        ViewBag.Report2 = report2;
        ViewBag.Report3 = report3;
        return View();
    }
}

/// <summary>Строка раздела 1: эксперимент + лаборатория</summary>
public class Report1Row
{
    /// <summary>Название эксперимента</summary>
    public string Name          { get; set; } = "";
    /// <summary>Название лаборатории</summary>
    public string LaboratoryName { get; set; } = "";
    /// <summary>Длительность в часах</summary>
    public double DurationHours { get; set; }
}

/// <summary>Строка раздела 2: лаборатория + количество</summary>
public class Report2Row
{
    /// <summary>Название лаборатории</summary>
    public string LaboratoryName { get; set; } = "";
    /// <summary>Количество экспериментов</summary>
    public int    Count          { get; set; }
}

/// <summary>Строка раздела 3: лаборатория + средняя длительность</summary>
public class Report3Row
{
    /// <summary>Название лаборатории</summary>
    public string LaboratoryName  { get; set; } = "";
    /// <summary>Средняя длительность опытов (часов)</summary>
    public double AvgDurationHours { get; set; }
}
