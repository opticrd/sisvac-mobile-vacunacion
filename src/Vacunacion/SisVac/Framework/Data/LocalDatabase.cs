﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using SisVac.Framework.Domain;
using SQLite;

namespace SisVac.Framework.Data
{
    public class LocalDatabase
    {
        static SQLiteAsyncConnection _db;

        public SQLiteAsyncConnection Connection
        {
            get
            {
                return _db;
            }
        }

        public async Task Initialize()
        {
            if(_db == null)
            {
                var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LocalDatabase.db");

                _db = new SQLiteAsyncConnection(databasePath);

                await _db.CreateTableAsync<ClinicLocation>();
                await _db.CreateTableAsync<VaccineBrand>();
                await _db.CreateTableAsync<VaccineLot>();
                await LoadSeeds();
            }
        }

        async Task LoadSeeds()
        {
            if(await _db.Table<ClinicLocation>().CountAsync() == 0)
            {
                var locationsScript = ReadResourceFile("SisVac.Framework.Data.Scripts.ClinicLocations.sql");
                await _db.ExecuteAsync(locationsScript);
            }
            if (await _db.Table<VaccineBrand>().CountAsync() == 0)
            {
                var vaccineBrandId = await _db.InsertAsync(new VaccineBrand { Name = "AstraZeneca" });
                await _db.InsertAllAsync(new List<VaccineLot>{
                    new VaccineLot { Name="4120Z001", VaccineBrandLocalId=vaccineBrandId },
                    new VaccineLot { Name="4120Z023", VaccineBrandLocalId=vaccineBrandId },
                });
                vaccineBrandId = await _db.InsertAsync(new VaccineBrand { Name = "SINOVAC" });
                await _db.InsertAllAsync(new List<VaccineLot>{
                    new VaccineLot { Name="A2021010034", VaccineBrandLocalId=vaccineBrandId },
                    new VaccineLot { Name="A2021010039", VaccineBrandLocalId=vaccineBrandId },
                    new VaccineLot { Name="A2021010041", VaccineBrandLocalId=vaccineBrandId },
                });
            }
        }

        private string ReadResourceFile(string filename)
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            using (var stream = thisAssembly.GetManifestResourceStream(filename))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
