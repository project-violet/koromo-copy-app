// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace Koromo_Copy.Framework.SQLite.DataBase
{
    /// <summary>
    /// SQLite Helper Class
    /// </summary>
    public class SQLiteWrapper
    {
        string conn;

        public SQLiteWrapper(string ConnectionString)
        {
            conn = ConnectionString;
        }

        /// <summary>
        /// Open new database from file
        /// </summary>
        /// <param name="filename">Full path of SQLite database file.</param>
        /// <returns></returns>
        public static SQLiteWrapper Open(string filename) =>
            new SQLiteWrapper($"Data Source={filename};Version=3;");

        /// <summary>
        /// Create new sqlite database file.
        /// </summary>
        /// <param name="filename"></param>
        public static void CreateNew(string filename)
        {
            SQLiteConnection.CreateFile(filename);
        }

        /// <summary>
        /// Get a count of rows by specific table.
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="attr">Attribute name</param>
        /// <returns></returns>
        public int EvalCount(string table, string attr = "*")
        {
            using (SQLiteConnection connection = new SQLiteConnection(this.conn))
            {
                connection.Open();
                return Convert.ToInt32(new SQLiteCommand($"select count({attr}) from {table}", connection).ExecuteScalar());
            }
        }

        /// <summary>
        /// Evaluate non-query.
        /// </summary>
        /// <param name="sql">SQL sentence</param>
        public void EvalNqSql(string sql)
        {
            using (SQLiteConnection connection = new SQLiteConnection(this.conn))
            {
                connection.Open();
                new SQLiteCommand(sql, connection).ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Evaluate query.
        /// </summary>
        /// <param name="sql">SQL sentence</param>
        /// <param name="cp">Delegate of reader.</param>
        public void EvalReadSql(string sql, Action<SQLiteDataReader> cp)
        {
            using (SQLiteConnection connection = new SQLiteConnection(this.conn))
            {
                connection.Open();
                using (SQLiteDataReader reader = new SQLiteCommand(sql, connection).ExecuteReader())
                {
                    cp(reader);
                }
            }
        }


    }
}
