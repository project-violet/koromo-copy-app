// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework;
using Koromo_Copy.Framework.Utils;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Koromo_Copy.App.DataBase
{
    public enum DownloadDBState
    {
        Downloading,
        Downloaded,
        Aborted,
        ErrorOccured,
        Deleted, // Deleted From DB but still alive in databse.
    }

    public class DownloadDBModel
    {
        [PrimaryKey]
        public int Id { get; set; }
        public DateTime StartsTime { get; set; }
        public DateTime EndsTime { get; set; }
        public string Url { get; set; }
        public string Directory { get; set; }
        public string ShortInfo { get; set; }
        public string InfoCache { get; set; }
        public string ThumbnailCahce { get; set; }
        public string LogCache { get; set; }
        public DownloadDBState State { get; set; }
    }

    public class DownloadDBManager : ILazy<DownloadDBManager>
    {
        SQLiteConnection db;

        public DownloadDBManager()
        {
            var db_path = Path.Combine(AppProvider.ApplicationPath, "download.db");

            db = new SQLiteConnection(db_path);
            var info = db.GetTableInfo("DownloadDBModel");
            if (!info.Any())
                db.CreateTable<DownloadDBModel>();
        }

        public void Add(DownloadDBModel dbm)
        {
            var count = db.ExecuteScalar<int>("select count(*) from DownloadDBModel");
            dbm.Id = count;
            db.Insert(dbm);
        }

        public void Update(DownloadDBModel dbm)
            => db.Update(dbm);

        public List<DownloadDBModel> QueryAll()
            => db.Table<DownloadDBModel>().ToList();
    }
}
