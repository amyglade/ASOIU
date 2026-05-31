/// <summary>
/// Лаборатория (справочная таблица, сторона «один»)
/// </summary>
class Laboratory
{
    /// <summary>Идентификатор лаборатории</summary>
    public int Id { get; set; }

    /// <summary>Название лаборатории</summary>
    public string Name { get; set; }

    /// <summary>Конструктор с параметрами</summary>
    public Laboratory(int id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>Конструктор по умолчанию</summary>
    public Laboratory() : this(0, "") { }

    /// <summary>Строковое представление объекта</summary>
    public override string ToString() => $"[{Id}] {Name}";
}
