/********************************************************************************
* Copyright (c) {2019 - 2024} Contributors to the Eclipse Foundation
*
* See the NOTICE file(s) distributed with this work for additional
* information regarding copyright ownership.
*
* This program and the accompanying materials are made available under the
* terms of the Apache License Version 2.0 which is available at
* https://www.apache.org/licenses/LICENSE-2.0
*
* SPDX-License-Identifier: Apache-2.0
********************************************************************************/

/*
 * https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli
 * 
 * Steps to modify the database
 * 1. Change the database context
 * 2. Open package management console
 * 3. Optional: if initial migration
 *      Add-Migration InitialCreate -Context SqliteAasContext -OutputDir Migrations\Sqlite -Project AasxServerDB
 *      Add-Migration InitialCreate -Context PostgreAasContext -OutputDir Migrations\Postgres -Project AasxServerDB
 * 4. Add a new migration
 *      Add-Migration <name of new migration> -Context SqliteAasContext -Project AasxServerDB
 *      Add-Migration <name of new migration> -Context PostgreAasContext -Project AasxServerDB
 * 5. Review the migration for accuracy
 * 6. Optional: Update database to new schema
 *      Update-Database -Context SqliteAasContext -Project AasxServerDB
 *      Update-Database -Context PostgreAasContext -Project AasxServerDB
 */

namespace AasxServerDB
{
    using System;
    using System.Text.Json.Nodes;
    using AasxServerDB.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using AasCore.Aas3_0;
    using AasxServerDB.Context;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public class AasContext : DbContext
    {
        public static IConfiguration? Config { get; set; }

        public static string? DataPath{ get; set; }

        public static bool IsPostgres { get; set; }

        public DbSet<EnvSet> EnvSets { get; set; }
        public DbSet<EnvCDSet> EnvCDSets { get; set; }
        public DbSet<CDSet> CDSets { get; set; }
        public DbSet<AASSet> AASSets { get; set; }
        public DbSet<SMSet> SMSets { get; set; }
        public DbSet<SMESet> SMESets { get; set; }
        public DbSet<SValueSet> SValueSets { get; set; }
        public DbSet<IValueSet> IValueSets { get; set; }
        public DbSet<DValueSet> DValueSets { get; set; }
        public DbSet<OValueSet> OValueSets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var connectionString = GetConnectionString();

            if (IsPostgres) // PostgreSQL
                options.UseNpgsql(connectionString);
            else // SQLite
                options.UseSqlite(connectionString);
        }

        protected static string GetConnectionString()
        {
            // Get configuration
            if (Config == null)
                Config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();
            if (Config == null)
                throw new Exception("No configuration");

            // Get connection string
            var connectionString = Config["DatabaseConnection:ConnectionString"];
            if (connectionString.IsNullOrEmpty())
                throw new Exception("No ConnectionString in appsettings");
            if (connectionString.Contains("$DATAPATH"))
            {
                connectionString = connectionString.Replace("$DATAPATH", DataPath);
            }
            IsPostgres = connectionString.ToLower().Contains("host");

            return connectionString;
        }
    }
}
