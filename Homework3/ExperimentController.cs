using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LabApp.Data;
using LabApp.Models;

namespace LabApp.Controllers;

/// <summary>
/// Контроллер для управления экспериментами (detail).
/// Полный CRUD с выпадающим списком лабораторий и валидацией числового поля.
/// </summary>
public class ExperimentController : Controller
{
    // ── GET /Experiment ──────────────────────────────────────────────────────

    /// <summary>Список всех экспериментов с названиями лабораторий (Include).</summary>
    public async Task<IActionResult> Index()
    {
        using var ctx = Ctx();
        var list = await ctx.Experiments
            .Include(e => e.Laboratory)
            .OrderBy(e => e.Name)
            .ToListAsync();
        return View(list);
    }

    // ── GET /Experiment/Create ───────────────────────────────────────────────

    /// <summary>Форма добавления нового эксперимента.</summary>
    public async Task<IActionResult> Create()
    {
        using var ctx = Ctx();
        await FillLabs(ctx);
        return View(new Experiment());
    }

    // ── POST /Experiment/Create ──────────────────────────────────────────────

    /// <summary>Сохраняет новый эксперимент. Валидирует DurationHours ≥ 0.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Experiment exp)
    {
        ValidateDuration(exp);
        if (!ModelState.IsValid)
        {
            using var ctx2 = Ctx();
            await FillLabs(ctx2, exp.LaboratoryId);
            return View(exp);
        }
        using var ctx = Ctx();
        ctx.Experiments.Add(exp);
        await ctx.SaveChangesAsync();
        TempData["Success"] = $"Эксперимент «{exp.Name}» добавлен.";
        return RedirectToAction(nameof(Index));
    }

    // ── GET /Experiment/Edit/{id} ────────────────────────────────────────────

    /// <summary>Форма редактирования эксперимента.</summary>
    public async Task<IActionResult> Edit(int id)
    {
        using var ctx = Ctx();
        var exp = await ctx.Experiments.FindAsync(id);
        if (exp == null) return NotFound();
        await FillLabs(ctx, exp.LaboratoryId);
        return View(exp);
    }

    // ── POST /Experiment/Edit/{id} ───────────────────────────────────────────

    /// <summary>Сохраняет изменения эксперимента.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Experiment exp)
    {
        ValidateDuration(exp);
        if (!ModelState.IsValid)
        {
            using var ctx2 = Ctx();
            await FillLabs(ctx2, exp.LaboratoryId);
            return View(exp);
        }
        using var ctx = Ctx();
        ctx.Experiments.Update(exp);
        await ctx.SaveChangesAsync();
        TempData["Success"] = $"Эксперимент «{exp.Name}» обновлён.";
        return RedirectToAction(nameof(Index));
    }

    // ── GET /Experiment/Delete/{id} ──────────────────────────────────────────

    /// <summary>Страница подтверждения удаления.</summary>
    public async Task<IActionResult> Delete(int id)
    {
        using var ctx = Ctx();
        var exp = await ctx.Experiments
            .Include(e => e.Laboratory)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (exp == null) return NotFound();
        return View(exp);
    }

    // ── POST /Experiment/Delete/{id} ─────────────────────────────────────────

    /// <summary>Удаляет эксперимент из БД.</summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        using var ctx = Ctx();
        var exp = await ctx.Experiments.FindAsync(id);
        if (exp == null) return NotFound();
        ctx.Experiments.Remove(exp);
        await ctx.SaveChangesAsync();
        TempData["Success"] = $"Эксперимент «{exp.Name}» удалён.";
        return RedirectToAction(nameof(Index));
    }

    // ── private helpers ──────────────────────────────────────────────────────

    /// <summary>Создаёт новый контекст (using-паттерн в каждом действии).</summary>
    private static AppDbContext Ctx() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=lab.db").Options);

    /// <summary>Заполняет ViewBag.Laboratories для выпадающего списка.</summary>
    private async Task FillLabs(AppDbContext ctx, int selectedId = 0)
    {
        var labs = await ctx.Laboratories.OrderBy(l => l.Name).ToListAsync();
        ViewBag.Laboratories = new SelectList(labs, "Id", "Name", selectedId);
    }

    /// <summary>Добавляет ошибку валидации, если длительность отрицательна.</summary>
    private void ValidateDuration(Experiment exp)
    {
        if (exp.DurationHours < 0)
            ModelState.AddModelError(nameof(exp.DurationHours),
                "Длительность опыта не может быть отрицательной.");
    }
}
