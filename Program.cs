using System.Text;

// ══════════════════════════════════════════════════════════
// Точка входа — консольное меню
// ══════════════════════════════════════════════════════════
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding  = Encoding.UTF8;

// Пути к файлам
string dbPath = "experiments.db";
string labCsv = Path.Combine(AppContext.BaseDirectory, "lab.csv");
string expCsv = Path.Combine(AppContext.BaseDirectory, "exp.csv");

// Создаём менеджер БД и инициализируем данные
var db = new DatabaseManager(dbPath);
db.InitializeDatabase(labCsv, expCsv);

Console.WriteLine();

// Главный цикл меню
string choice;
do
{
    Console.WriteLine("╔══════════════════════════════════════════╗");
    Console.WriteLine("║     УПРАВЛЕНИЕ ЭКСПЕРИМЕНТАМИ            ║");
    Console.WriteLine("╠══════════════════════════════════════════╣");
    Console.WriteLine("║  1 — Показать все лаборатории            ║");
    Console.WriteLine("║  2 — Показать все эксперименты           ║");
    Console.WriteLine("║  3 — Добавить эксперимент                ║");
    Console.WriteLine("║  4 — Редактировать эксперимент           ║");
    Console.WriteLine("║  5 — Удалить эксперимент                 ║");
    Console.WriteLine("║  6 — Отчёты                              ║");
    Console.WriteLine("║  7 — Фильтр по лаборатории [ГРУППА Г]   ║");
    Console.WriteLine("║  0 — Выход                               ║");
    Console.WriteLine("╚══════════════════════════════════════════╝");
    Console.Write("Ваш выбор: ");
    choice = Console.ReadLine()?.Trim() ?? "";
    Console.WriteLine();

    switch (choice)
    {
        case "1": ShowLaboratories(db);          break;
        case "2": ShowExperiments(db);           break;
        case "3": AddExperiment(db);             break;
        case "4": EditExperiment(db);            break;
        case "5": DeleteExperiment(db);          break;
        case "6": ReportsMenu(db);               break;
        case "7": FilterByLaboratory(db);        break;  // [ГРУППА Г]
        case "0": Console.WriteLine("До свидания!"); break;
        default:  Console.WriteLine("Неверный пункт меню."); break;
    }

    Console.WriteLine();
}
while (choice != "0");

// ══════════════════════════════════════════════════════════
// Функции пунктов меню
// ══════════════════════════════════════════════════════════

static void ShowLaboratories(DatabaseManager db)
{
    Console.WriteLine("--- Все лаборатории ---");
    var labs = db.GetAllLaboratories();
    foreach (var lab in labs)
        Console.WriteLine("  " + lab);
    Console.WriteLine($"Итого: {labs.Count}");
}

static void ShowExperiments(DatabaseManager db)
{
    Console.WriteLine("--- Все эксперименты ---");
    var exps = db.GetAllExperiments();
    foreach (var exp in exps)
        Console.WriteLine("  " + exp);
    Console.WriteLine($"Итого: {exps.Count}");
}

static void AddExperiment(DatabaseManager db)
{
    Console.WriteLine("--- Добавление эксперимента ---");

    // Показываем лаборатории, чтобы пользователь выбрал
    Console.WriteLine("Доступные лаборатории:");
    var labs = db.GetAllLaboratories();
    foreach (var lab in labs)
        Console.WriteLine("  " + lab);

    Console.Write("ID лаборатории: ");
    if (!int.TryParse(Console.ReadLine(), out int labId))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    Console.Write("Название эксперимента: ");
    string name = Console.ReadLine()?.Trim() ?? "";
    if (name.Length == 0)
    {
        Console.WriteLine("Ошибка: название не может быть пустым.");
        return;
    }

    Console.Write("Длительность опыта (часов): ");
    if (!double.TryParse(Console.ReadLine(),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out double hours))
    {
        Console.WriteLine("Ошибка: введите числовое значение.");
        return;
    }

    try
    {
        var exp = new Experiment(0, labId, name, hours);
        db.AddExperiment(exp);
        Console.WriteLine("Эксперимент добавлен.");
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Ошибка: {ex.Message}");
    }
}

static void EditExperiment(DatabaseManager db)
{
    Console.WriteLine("--- Редактирование эксперимента ---");
    Console.Write("Введите ID эксперимента: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    var exp = db.GetExperimentById(id);
    if (exp == null)
    {
        Console.WriteLine($"Эксперимент с ID={id} не найден.");
        return;
    }

    Console.WriteLine($"Текущие данные: {exp}");
    Console.WriteLine("(нажмите Enter, чтобы оставить значение без изменений)");

    // Название
    Console.Write($"Название [{exp.Name}]: ");
    string input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0)
        exp.Name = input;

    // Лаборатория
    Console.Write($"ID лаборатории [{exp.LaboratoryId}]: ");
    input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0 && int.TryParse(input, out int newLabId))
        exp.LaboratoryId = newLabId;

    // Длительность
    Console.Write($"Длительность часов [{exp.DurationHours}]: ");
    input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0 && double.TryParse(input,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out double newHours))
    {
        try
        {
            exp.DurationHours = newHours; // валидация в set-аксессоре
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            return;
        }
    }

    db.UpdateExperiment(exp);
    Console.WriteLine("Данные обновлены.");
}

static void DeleteExperiment(DatabaseManager db)
{
    Console.WriteLine("--- Удаление эксперимента ---");
    Console.Write("Введите ID эксперимента: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    var exp = db.GetExperimentById(id);
    if (exp == null)
    {
        Console.WriteLine($"Эксперимент с ID={id} не найден.");
        return;
    }

    Console.Write($"Удалить «{exp.Name}»? (да/нет): ");
    string confirm = Console.ReadLine()?.Trim().ToLower() ?? "";
    if (confirm == "да")
    {
        db.DeleteExperiment(id);
        Console.WriteLine("Эксперимент удалён.");
    }
    else
    {
        Console.WriteLine("Удаление отменено.");
    }
}

// ══════════════════════════════════════════════════════════
// Подменю отчётов
// ══════════════════════════════════════════════════════════

static void ReportsMenu(DatabaseManager db)
{
    string choice;
    do
    {
        Console.WriteLine("--- Отчёты ---");
        Console.WriteLine("  1 — Эксперименты по лабораториям");
        Console.WriteLine("  2 — Количество экспериментов в лабораториях");
        Console.WriteLine("  3 — Средняя длительность опытов по лабораториям");
        Console.WriteLine("  0 — Назад");
        Console.Write("Ваш выбор: ");
        choice = Console.ReadLine()?.Trim() ?? "";

        switch (choice)
        {
            case "1": Report1_ExperimentsWithLabs(db);    break;
            case "2": Report2_CountByLab(db);             break;
            case "3": Report3_AvgDurationByLab(db);       break;
            case "0": break;
            default:  Console.WriteLine("Неверный пункт."); break;
        }

        Console.WriteLine();
    }
    while (choice != "0");
}

// ─────── Отчёт 1: Эксперименты с названиями лабораторий (JOIN) ───────
static void Report1_ExperimentsWithLabs(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT e.exp_name, l.lab_name, e.duration_hours
                 FROM exp e
                 JOIN lab l ON e.lab_id = l.lab_id
                 ORDER BY e.exp_name")
        .Title("Эксперименты по лабораториям")
        .Header("Название эксперимента", "Лаборатория", "Длит. (ч.)")
        .ColumnWidths(35, 30, 12)
        .Numbered()            // [ГРУППА А] нумерация строк
        .Footer("Всего записей") // [ГРУППА В] итоговая строка
        .Print();
}

// ─────── Отчёт 2: Количество экспериментов по лабораториям (GROUP BY + COUNT) ───
static void Report2_CountByLab(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT l.lab_name, COUNT(*) AS cnt
                 FROM exp e
                 JOIN lab l ON e.lab_id = l.lab_id
                 GROUP BY l.lab_name
                 ORDER BY l.lab_name")
        .Title("Количество экспериментов по лабораториям")
        .Header("Лаборатория", "Кол-во")
        .ColumnWidths(35, 10)
        .Print();
}

// ─────── Отчёт 3: Средняя длительность опытов по лабораториям (GROUP BY + AVG) ───
static void Report3_AvgDurationByLab(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT l.lab_name,
                        ROUND(AVG(e.duration_hours), 1) AS avg_hours
                 FROM exp e
                 JOIN lab l ON e.lab_id = l.lab_id
                 GROUP BY l.lab_name
                 ORDER BY avg_hours DESC")
        .Title("Средняя длительность опытов по лабораториям")
        .Header("Лаборатория", "Средн. длит. (ч.)")
        .ColumnWidths(35, 20)
        .Print();
}

// ══════════════════════════════════════════════════════════
// [ГРУППА Г] Фильтр по лаборатории
// ══════════════════════════════════════════════════════════

static void FilterByLaboratory(DatabaseManager db)
{
    Console.WriteLine("--- Фильтр по лаборатории ---");
    Console.WriteLine("Доступные лаборатории:");
    var labs = db.GetAllLaboratories();
    foreach (var lab in labs)
        Console.WriteLine("  " + lab);

    Console.Write("Введите ID лаборатории: ");
    if (!int.TryParse(Console.ReadLine(), out int labId))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    var experiments = db.GetExperimentsByLaboratory(labId);
    if (experiments.Count == 0)
    {
        Console.WriteLine("В этой лаборатории нет экспериментов.");
        return;
    }

    Console.WriteLine($"\nЭксперименты лаборатории #{labId}:");
    foreach (var exp in experiments)
        Console.WriteLine("  " + exp);
    Console.WriteLine($"Итого: {experiments.Count}");
}
