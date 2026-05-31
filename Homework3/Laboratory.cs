using System.ComponentModel.DataAnnotations;

namespace LabApp.Models;

/// <summary>
/// Лаборатория — справочная таблица (сторона «один»)
/// </summary>
public class Laboratory
{
    /// <summary>Идентификатор лаборатории (первичный ключ)</summary>
    public int Id { get; set; }

    /// <summary>Название лаборатории</summary>
    [Required(ErrorMessage = "Название не может быть пустым")]
    [StringLength(200, ErrorMessage = "Не более 200 символов")]
    public string Name { get; set; } = "";

    /// <summary>Навигационное свойство: эксперименты этой лаборатории</summary>
    public ICollection<Experiment> Experiments { get; set; } = new List<Experiment>();
}
