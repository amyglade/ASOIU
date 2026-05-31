using Microsoft.Data.Sqlite;

/// <summary>
/// Управление базой данных SQLite.
/// Инкапсулирует все операции с БД: создание таблиц,
/// импорт CSV, CRUD-операции, выполнение запросов для отчётов.
/// </summary>
class DatabaseManager
{
    private string _connectionString;

    /// <summary>
    /// Конструктор. Принимает путь к файлу БД.
    /// </summary>
    public DatabaseManager(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
    }

    // ──────────── Инициализация ────────────

    /// <summary>
    /// Создаёт таблицы (если не существуют) и загружает CSV при первом запуске
    /// </summary>
    public void InitializeDatabase(string labCsvPath, string expCsvPath)
    {
        CreateTables();

        if (GetAllLaboratories().Count == 0 && File.Exists(labCsvPath))
        {
            ImportLaboratoriesFromCsv(labCsvPath);
            Console.WriteLine($"[OK] Загружены лаборатории из {labCsvPath}");
        }

        if (GetAllExperiments().Count == 0 && File.Exists(expCsvPath))
        {
            ImportExperimentsFromCsv(expCsvPath);
            Console.WriteLine($"[OK] Загружены эксперименты из {expCsvPath}");
        }
    }

    /// <summary>Создание таблиц</summary>
    private void CreateTables()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS lab (
                lab_id   INTEGER PRIMARY KEY AUTOINCREMENT,
                lab_name TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS exp (
                exp_id         INTEGER PRIMARY KEY AUTOINCREMENT,
                lab_id         INTEGER NOT NULL,
                exp_name       TEXT NOT NULL,
                duration_hours REAL NOT NULL,
                FOREIGN KEY (lab_id) REFERENCES lab(lab_id)
            );";
        cmd.ExecuteNonQuery();
    }

    /// <summary>Импорт лабораторий из CSV</summary>
    private void ImportLaboratoriesFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 2) continue;
            var cmd = conn.CreateCommand();
            cmd.CommandText =
                "INSERT INTO lab (lab_id, lab_name) VALUES (@id, @name)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@name", parts[1].Trim());
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>Импорт экспериментов из CSV</summary>
    private void ImportExperimentsFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 4) continue;
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO exp (exp_id, lab_id, exp_name, duration_hours)
                VALUES (@id, @labId, @name, @hours)";
            cmd.Parameters.AddWithValue("@id",    int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@labId", int.Parse(parts[1]));
            cmd.Parameters.AddWithValue("@name",  parts[2].Trim());
            cmd.Parameters.AddWithValue("@hours", double.Parse(parts[3],
                System.Globalization.CultureInfo.InvariantCulture));
            cmd.ExecuteNonQuery();
        }
    }

    // ──────────── Чтение данных ────────────

    /// <summary>Получить все лаборатории</summary>
    public List<Laboratory> GetAllLaboratories()
    {
        var result = new List<Laboratory>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT lab_id, lab_name FROM lab ORDER BY lab_id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Laboratory(
                reader.GetInt32(0),
                reader.GetString(1)));
        }
        return result;
    }

    /// <summary>Получить все эксперименты</summary>
    public List<Experiment> GetAllExperiments()
    {
        var result = new List<Experiment>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText =
            "SELECT exp_id, lab_id, exp_name, duration_hours FROM exp ORDER BY exp_id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Experiment(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetDouble(3)));
        }
        return result;
    }

    /// <summary>Получить эксперимент по Id</summary>
    public Experiment? GetExperimentById(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText =
            "SELECT exp_id, lab_id, exp_name, duration_hours FROM exp WHERE exp_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Experiment(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetDouble(3));
        }
        return null;
    }

    // ──────────── Изменение данных ────────────

    /// <summary>Добавить эксперимент (Id генерируется автоматически)</summary>
    public void AddExperiment(Experiment exp)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO exp (lab_id, exp_name, duration_hours)
            VALUES (@labId, @name, @hours)";
        cmd.Parameters.AddWithValue("@labId", exp.LaboratoryId);
        cmd.Parameters.AddWithValue("@name",  exp.Name);
        cmd.Parameters.AddWithValue("@hours", exp.DurationHours);
        cmd.ExecuteNonQuery();
    }

    /// <summary>Обновить данные эксперимента</summary>
    public void UpdateExperiment(Experiment exp)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE exp
            SET lab_id = @labId, exp_name = @name, duration_hours = @hours
            WHERE exp_id = @id";
        cmd.Parameters.AddWithValue("@id",    exp.Id);
        cmd.Parameters.AddWithValue("@labId", exp.LaboratoryId);
        cmd.Parameters.AddWithValue("@name",  exp.Name);
        cmd.Parameters.AddWithValue("@hours", exp.DurationHours);
        cmd.ExecuteNonQuery();
    }

    /// <summary>Удалить эксперимент по Id</summary>
    public void DeleteExperiment(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM exp WHERE exp_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    // ──────────── Выполнение произвольного запроса (для отчётов) ────────────

    /// <summary>
    /// Выполняет SQL-запрос и возвращает имена столбцов и строки результата.
    /// Используется классом ReportBuilder.
    /// </summary>
    public (string[] columns, List<string[]> rows) ExecuteQuery(string sql)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        using var reader = cmd.ExecuteReader();

        string[] columns = new string[reader.FieldCount];
        for (int i = 0; i < reader.FieldCount; i++)
            columns[i] = reader.GetName(i);

        var rows = new List<string[]>();
        while (reader.Read())
        {
            string[] row = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
                row[i] = reader.GetValue(i)?.ToString() ?? "";
            rows.Add(row);
        }
        return (columns, rows);
    }

    // ──────────── [ГРУППА Г] Фильтр по лаборатории ────────────

    /// <summary>Получить эксперименты конкретной лаборатории</summary>
    public List<Experiment> GetExperimentsByLaboratory(int labId)
    {
        var result = new List<Experiment>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT exp_id, lab_id, exp_name, duration_hours
            FROM exp WHERE lab_id = @labId ORDER BY exp_name";
        cmd.Parameters.AddWithValue("@labId", labId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Experiment(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetDouble(3)));
        }
        return result;
    }
}
