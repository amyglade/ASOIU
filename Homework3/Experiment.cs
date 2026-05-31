using System.ComponentModel.DataAnnotations;

namespace LabApp.Models;

/// <summary>
/// Эксперимент — основная таблица (сторона «много»)
/// </summary>
public class Experiment
{
    /// <summary>Идентификатор эксперимента (первичный ключ)</summary>
    public int Id { get; set; }

    /// <summary>Идентификатор лаборатории (внешний ключ)</summary>
    public int LaboratoryId { get; set; }

    /// <summary>Лаборатория (навигационное свойство)</summary>
    public Laboratory? Laboratory { get; set; }

    /// <summary>Название эксперимента</summary>
    [Required(ErrorMessage = "Название обязательно")]
    [StringLength(200, ErrorMessage = "Не более 200 символов")]
    public string Name { get; set; } = "";

    /// <summary>Длительность опыта в часах (≥ 0)</summary>
    [Range(0, double.MaxValue, ErrorMessage = "Длительность не может быть отрицательной")]
    public double DurationHours { get; set; }
}
