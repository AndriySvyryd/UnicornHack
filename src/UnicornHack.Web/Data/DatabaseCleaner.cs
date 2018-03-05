using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace UnicornHack.Data
{
    public class DatabaseCleaner
    {
        public virtual void Clean(DatabaseFacade facade)
        {
            var creator = facade.GetService<IRelationalDatabaseCreator>();
            var sqlGenerator = facade.GetService<IMigrationsSqlGenerator>();
            var executor = facade.GetService<IMigrationCommandExecutor>();
            var connection = facade.GetService<IRelationalConnection>();
            var sqlBuilder = facade.GetService<IRawSqlCommandBuilder>();
            var loggerFactory = facade.GetService<ILoggerFactory>();

            if (!creator.Exists())
            {
                creator.Create();
            }
            else
            {
                var databaseModelFactory = CreateDatabaseModelFactory(loggerFactory);
                var databaseModel = databaseModelFactory.Create(connection.DbConnection, Enumerable.Empty<string>(),
                    Enumerable.Empty<string>());

                var operations = new List<MigrationOperation>();

                foreach (var foreignKey in databaseModel.Tables
                    .SelectMany(t => t.ForeignKeys))
                {
                    operations.Add(Drop(foreignKey));
                }

                foreach (var table in databaseModel.Tables)
                {
                    operations.Add(Drop(table));
                }

                foreach (var sequence in databaseModel.Sequences)
                {
                    operations.Add(Drop(sequence));
                }

                connection.Open();

                try
                {
                    var customSql = BuildCustomSql();
                    if (!string.IsNullOrWhiteSpace(customSql))
                    {
                        sqlBuilder.Build(customSql).ExecuteNonQuery(connection);
                    }

                    if (operations.Any())
                    {
                        var commands = sqlGenerator.Generate(operations);
                        executor.ExecuteNonQuery(commands, connection);
                    }

                    customSql = BuildCustomEndingSql();
                    if (!string.IsNullOrWhiteSpace(customSql))
                    {
                        sqlBuilder.Build(customSql).ExecuteNonQuery(connection);
                    }
                }
                finally
                {
                    connection.Close();
                }
            }

            creator.CreateTables();
        }

        protected virtual DropSequenceOperation Drop(DatabaseSequence sequence)
            => new DropSequenceOperation
            {
                Name = sequence.Name,
                Schema = sequence.Schema
            };

        protected virtual DropTableOperation Drop(DatabaseTable table)
            => new DropTableOperation
            {
                Name = table.Name,
                Schema = table.Schema
            };

        protected virtual DropForeignKeyOperation Drop(DatabaseForeignKey foreignKey)
            => new DropForeignKeyOperation
            {
                Name = foreignKey.Name,
                Table = foreignKey.Table.Name,
                Schema = foreignKey.Table.Schema
            };

        protected virtual DropIndexOperation Drop(DatabaseIndex index)
            => new DropIndexOperation
            {
                Name = index.Name,
                Table = index.Table.Name,
                Schema = index.Table.Schema
            };

        protected IDatabaseModelFactory CreateDatabaseModelFactory(ILoggerFactory loggerFactory)
            => new SqlServerDatabaseModelFactory(
                new DiagnosticsLogger<DbLoggerCategory.Scaffolding>(
                    loggerFactory,
                    new LoggingOptions(),
                    new DiagnosticListener("Fake")));

        protected string BuildCustomSql()
            => @"
DECLARE @name VARCHAR(MAX) = '__dummy__', @SQL VARCHAR(MAX);

WHILE @name IS NOT NULL
BEGIN
    SELECT @name =
    (SELECT TOP 1 QUOTENAME(s.[name]) + '.' + QUOTENAME(o.[name])
     FROM sysobjects o
     INNER JOIN sys.views v ON o.id = v.object_id
     INNER JOIN sys.schemas s ON s.schema_id = v.schema_id
     WHERE (s.name = 'dbo' OR s.principal_id <> s.schema_id) AND o.[type] = 'V' AND o.category = 0 AND o.[name] NOT IN
     (
        SELECT referenced_entity_name
        FROM sys.sql_expression_dependencies AS sed
        INNER JOIN sys.objects AS o ON sed.referencing_id = o.object_id
     )
     ORDER BY v.[name])

    SELECT @SQL = 'DROP VIEW ' + @name
    EXEC (@SQL)
END";

        protected string BuildCustomEndingSql()
            => @"
DECLARE @SQL VARCHAR(MAX) = '';
SELECT @SQL = @SQL + 'DROP FUNCTION ' + QUOTENAME(ROUTINE_SCHEMA) + '.' + QUOTENAME(ROUTINE_NAME) + ';'
  FROM [INFORMATION_SCHEMA].[ROUTINES] WHERE ROUTINE_TYPE = 'FUNCTION' AND ROUTINE_BODY = 'SQL';
EXEC (@SQL);

SET @SQL ='';
SELECT @SQL = @SQL + 'DROP AGGREGATE ' + QUOTENAME(ROUTINE_SCHEMA) + '.' + QUOTENAME(ROUTINE_NAME) + ';'
  FROM [INFORMATION_SCHEMA].[ROUTINES] WHERE ROUTINE_TYPE = 'FUNCTION' AND ROUTINE_BODY = 'EXTERNAL';
EXEC (@SQL);

SET @SQL ='';
SELECT @SQL = @SQL + 'DROP PROC ' + QUOTENAME(schema_name(schema_id)) + '.' + QUOTENAME(name) + ';' FROM sys.procedures;
EXEC (@SQL);

SET @SQL ='';
SELECT @SQL = @SQL + 'DROP TYPE ' + QUOTENAME(schema_name(schema_id)) + '.' + QUOTENAME(name) + ';' FROM sys.types WHERE is_user_defined = 1;
EXEC (@SQL);

SET @SQL ='';
SELECT @SQL = @SQL + 'DROP SCHEMA ' + QUOTENAME(name) + ';' FROM sys.schemas WHERE principal_id <> schema_id;
EXEC (@SQL);";
    }
}