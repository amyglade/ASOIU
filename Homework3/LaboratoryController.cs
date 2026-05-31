using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabApp.Data;
using LabApp.Models;

namespace LabApp.Controllers;

/// <summary>
/// Контроллер справочника лабораторий (master).
/// Полный CRUD; запрещает удаление при наличии связанных экспериментов.
/// Каждое действие открывает собственный AppDbContext через using-паттерн.
/// </summary>
public class LaboratoryController : Controller
{
    // ── GET /Laboratory ──────────────────────────────────────────────────────

    /// <summary>Список всех лабораторий с количеством экспериментов.</summary>
    public async Task<IActionResult> Index()
    {
        using var ctx = Ctx();
        var list = await ctx.Laboratories
            .OrderBy(l => l.Name)
            .Select(l => new LaboratoryListItem
            {
                Id              = l.Id,
                Name            = l.Name,
                ExperimentCount = l.Experiments.Count()
            })
            .ToListAsync();
        return View(list);
    }

    // ── GET /Laboratory/Create ───────────────────────────────────────────────

    /// <summary>Форма создания новой лаборатории.</summary>
    public IActionResult Create() => View(new Laboratory());

    // ── POST /Laboratory/Create ──────────────────────────────────────────────

    /// <summary>Сохраняет новую лабораторию.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Laboratory lab)
    {
        if (string.IsNullOrWhiteSpace(lab.Name))
            ModelState.AddModelError(nameof(lab.Name), "Название не может быть пустым.");

        if (!ModelState.IsValid) return View(lab);

        using var ctx = Ctx();
        ctx.Laboratories.Add(lab);
        await ctx.SaveChangesAsync();
        TempData["Success"] = $"Лаборатория «{lab.Name}» добавлена.";
        return RedirectToAction(nameof(Index));
    }

    // ── GET /Laboratory/Edit/{id} ────────────────────────────────────────────

    /// <summary>Форма редактирования лаборатории.</summary>
    public async Task<IActionResult> Edit(int id)
    {
        using var ctx = Ctx();
        var lab = await ctx.Laboratories.FindAsync(id);
        if (lab == null) return NotFound();
        return View(lab);
    }

    // ── POST /Laboratory/Edit/{id} ───────────────────────────────────────────

    /// <summary>Сохраняет изменения лаборатории.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Laboratory lab)
    {
        if (string.IsNullOrWhiteSpace(lab.Name))
            ModelState.AddModelError(nameof(lab.Name), "Название не может быть пустым.");

        if (!ModelState.IsValid) return View(lab);

        using var ctx = Ctx();
        ctx.Laboratories.Update(lab);
        await ctx.SaveChangesAsync();
        TempData["Success"] = $"Лаборатория «{lab.Name}» обновлена.";
        return RedirectToAction(nameof(Index));
    }

    // ── GET /Laboratory/Delete/{id} ──────────────────────────────────────────

    /// <summary>Страница подтверждения удаления.</summary>
    public async Task<IActionResult> Delete(int id)
    {
        using var ctx = Ctx();
        var lab = await ctx.Laboratories
            .Include(l => l.Experiments)
            .FirstOrDefaultAsync(l => l.Id == id);
        if (lab == null) return NotFound();
        return View(lab);
    }

    // ── POST /Laboratory/Delete/{id} ─────────────────────────────────────────

    /// <summary>
    /// Удаляет лабораторию. Запрещает, если есть связанные эксперименты.
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        using var ctx = Ctx();
        var lab = await ctx.Laboratories
            .Include(l => l.Experiments)
            .FirstOrDefaultAsync(l => l.Id == id);
        if (lab == null) return NotFound();

        if (lab.Experiments.Any())
        {
            TempData["Error"] =
                $"Невозможно удалить «{lab.Name}»: связано " +
                $"{lab.Experiments.Count} эксперимент(ов). Сначала удалите их.";
            return RedirectToAction(nameof(Index));
        }

        ctx.Laboratories.Remove(lab);
        await ctx.SaveChangesAsync();
        TempData["Success"] = $"Лаборатория «{lab.Name}» удалена.";
        return RedirectToAction(nameof(Index));
    }

    // ── helper ───────────────────────────────────────────────────────────────

    /// <summary>Создаёт новый контекст БД (using-паттерн).</summary>
    private static AppDbContext Ctx() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=lab.db").Options);
}

/// <summary>ViewModel для строки в списке лабораторий.</summary>
public class LaboratoryListItem
{
    /// <summary>Идентификатор лаборатории.</summary>
    public int    Id              { get; set; }
    /// <summary>Название лаборатории.</summary>
    public string Name            { get; set; } = "";
    /// <summary>Количество связанных экспериментов.</summary>
    public int    ExperimentCount { get; set; }
}
