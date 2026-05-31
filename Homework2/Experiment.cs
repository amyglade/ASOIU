/// <summary>
/// Эксперимент (основная таблица, сторона «много»)
/// </summary>
class Experiment
{
    /// <summary>Идентификатор эксперимента</summary>
    public int Id { get; set; }

    /// <summary>Идентификатор лаборатории (внешний ключ)</summary>
    public int LaboratoryId { get; set; }

    /// <summary>Название эксперимента</summary>
    public string Name { get; set; }

    private double _durationHours;

    /// <summary>
    /// Длительность опыта в часах (не может быть отрицательной)
    /// </summary>
    public double DurationHours
    {
        get => _durationHours;
        set
        {
            if (value < 0)
                throw new ArgumentException(
                    "Длительность опыта не может быть отрицательной");
            _durationHours = value;
        }
    }

    /// <summary>Конструктор с параметрами</summary>
    public Experiment(int id, int laboratoryId, string name, double durationHours)
    {
        Id = id;
        LaboratoryId = laboratoryId;
        Name = name;
        DurationHours = durationHours; // валидация сработает здесь
    }

    /// <summary>Конструктор по умолчанию</summary>
    public Experiment() : this(0, 0, "", 0) { }

    /// <summary>Строковое представление объекта</summary>
    public override string ToString()
        => $"[{Id}] {Name}, лаб. #{LaboratoryId}, длительность: {DurationHours} ч.";
}
