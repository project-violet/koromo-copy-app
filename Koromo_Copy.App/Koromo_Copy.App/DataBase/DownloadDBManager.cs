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
        Forbidden,
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
        public int CountOfFiles { get; set; }
        public long SizeOfContents { get; set; }
        public DownloadDBState State { get; set; }
    }

    public class DownloadDBManager : ILazy<DownloadDBManager>
    {
        object db_lock = new object();
        string db_path;

        public DownloadDBManager()
        {
            db_path = Path.Combine(AppProvider.ApplicationPath, "download.db");

            var db = new SQLiteConnection(db_path);
            var info = db.GetTableInfo("DownloadDBModel");
            if (!info.Any())
                db.CreateTable<DownloadDBModel>();
            db.Close();
        }

        public void Add(DownloadDBModel dbm)
        {
            lock (db_lock)
            {
                var db = new SQLiteConnection(db_path);
                var count = db.ExecuteScalar<int>("select count(*) from DownloadDBModel");
                dbm.Id = count;
                db.Insert(dbm);
                db.Close();
            }
        }

        public void Update(DownloadDBModel dbm)
        {
            lock (db_lock)
            {
                var db = new SQLiteConnection(db_path);
                db.Update(dbm);
                db.Close();
            }
        }

        public List<DownloadDBModel> QueryAll()
        {
            lock (db_lock)
            {
                using (var db = new SQLiteConnection(db_path))
                    return db.Table<DownloadDBModel>().ToList();
            }
        }
    }
}
