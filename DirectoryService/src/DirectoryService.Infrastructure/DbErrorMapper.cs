using Microsoft.EntityFrameworkCore;
using Npgsql;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Infrastructure
{
    public static class DbErrorMapper
    {
        public static Error Map(Exception exception)
        {
            // Concurrency conflict oптимистичная блокировка EF Core
            if (exception is DbUpdateConcurrencyException)
            {
                return Error.Conflict("concurrency.conflict", "Данные были изменены другим пользователем. Обновите страницу.");
            }

            // ошибки самого Postgresql
            if (exception is DbUpdateException dbUpdateEx && dbUpdateEx.InnerException is PostgresException pgEx)
            {
                return pgEx.SqlState switch
                {
                    // Unique violation (Нарушение уникальности / Дубликат)
                    "23505" => MapUniqueViolation(pgEx.ConstraintName),

                    // Foreign key violation (Несуществующий ID связи)
                    "23503" => MapForeignKeyViolation(pgEx.ConstraintName),

                    // Блокировки/дедлоки в самой СУБД
                    "40001" or "40P01" => Error.Conflict("database.lock", "Конфликт параллельного доступа к базе данных."),

                    // Любая другая ошибка Postgres - 500
                    _ => GeneralErrors.DataBase()
                };
            }

            // Пропала сеть, упал сервер БД - 500
            return GeneralErrors.DataBase();
        }

        private static Error MapUniqueViolation(string? constraintName)
        {
            if (constraintName != null)
            {
                // проверка дубликата привязки локации к департаменту
                if (constraintName.Contains("department_locations"))
                {
                    return Error.Conflict("department.location.duplicate", "Данная локация уже привязана к этому департаменту.");
                }

                // проверка уникальности имени департамента
                if (constraintName.Contains("departments_name"))
                {
                    return Error.Conflict("department.name.duplicate", "Департамент с таким названием уже существует.");
                }
            }

            return Error.Conflict("database.unique.violation", "Запись с таким уникальным значением уже существует.");
        }

        private static Error MapForeignKeyViolation(string? constraintName)
        {
            if (constraintName != null)
            {
                // Нарушен ключ к таблице локаций
                if (constraintName.Contains("fk_department_locations_location_id"))
                {
                    return Error.NotFound("location.not.found", "Указанная локация не существует в системе.");
                }

                // Нарушен ключ к таблице департаментов
                if (constraintName.Contains("fk_department_locations_department_id"))
                {
                    return Error.NotFound("department.not.found", "Указанное подразделение не существует в системе.");
                }

                // Нарушен иерархический ключ parent_id в самой таблице departments
                if (constraintName.Contains("FK_departments_departments_parent_id"))
                {
                    return Error.NotFound("parent.department.not.found", "Указанное родительское подразделение не существует.");
                }
            }

            return Error.Validation("database.foreign.key.violation", "Указанная связанная запись не найдена.");
        }
    }
}
