using Microsoft.EntityFrameworkCore;
using LabApp.Models;

namespace LabApp.Data;

/// <summary>
/// Контекст базы данных приложения.
/// Управляет подключением к SQLite и содержит DbSet для обеих таблиц.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>Инициализирует контекст с переданными параметрами</summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>Таблица лабораторий (справочник)</summary>
    public DbSet<Laboratory> Laboratories { get; set; }

    /// <summary>Таблица экспериментов (основная)</summary>
    public DbSet<Experiment> Experiments { get; set; }

    /// <summary>
    /// Инициализирует БД и заполняет начальными данными, если таблицы пусты.
    /// Вызывается один раз при старте приложения.
    /// </summary>
    public void EnsureSeeded()
    {
        Database.EnsureCreated();

        if (Laboratories.Any()) return;

        var labs = new[]
        {
            new Laboratory { Name = "Лаборатория биохимии" },
            new Laboratory { Name = "Лаборатория физики плазмы" },
            new Laboratory { Name = "Лаборатория молекулярной биологии" },
            new Laboratory { Name = "Лаборатория нанотехнологий" },
        };
        Laboratories.AddRange(labs);
        SaveChanges();

        var experiments = new[]
        {
            new Experiment { LaboratoryId = labs[0].Id, Name = "Синтез белковых структур",          DurationHours = 12 },
            new Experiment { LaboratoryId = labs[0].Id, Name = "Анализ ферментативной активности",   DurationHours = 8  },
            new Experiment { LaboratoryId = labs[0].Id, Name = "Хроматография аминокислот",          DurationHours = 6  },
            new Experiment { LaboratoryId = labs[0].Id, Name = "Электрофорез белков",                DurationHours = 4  },
            new Experiment { LaboratoryId = labs[1].Id, Name = "Спектроскопия плазмы",               DurationHours = 24 },
            new Experiment { LaboratoryId = labs[1].Id, Name = "Ионизация газов",                    DurationHours = 18 },
            new Experiment { LaboratoryId = labs[1].Id, Name = "Магнитное удержание плазмы",         DurationHours = 36 },
            new Experiment { LaboratoryId = labs[2].Id, Name = "ПЦР-диагностика",                    DurationHours = 3  },
            new Experiment { LaboratoryId = labs[2].Id, Name = "Секвенирование ДНК",                 DurationHours = 10 },
            new Experiment { LaboratoryId = labs[2].Id, Name = "Клонирование генов",                 DurationHours = 48 },
            new Experiment { LaboratoryId = labs[2].Id, Name = "Гель-электрофорез",                  DurationHours = 5  },
            new Experiment { LaboratoryId = labs[3].Id, Name = "Синтез наночастиц",                  DurationHours = 16 },
            new Experiment { LaboratoryId = labs[3].Id, Name = "Атомно-силовая микроскопия",         DurationHours = 9  },
            new Experiment { LaboratoryId = labs[3].Id, Name = "Нанолитография",                     DurationHours = 20 },
        };
        Experiments.AddRange(experiments);
        SaveChanges();
    }
}
