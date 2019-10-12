// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Koromo_Copy.Framework.Log
{
    public class Logs : ILazy<Logs>
    {
        /// <summary>
        /// Serialize an object.
        /// </summary>
        /// <param name="toSerialize"></param>
        /// <returns></returns>
        public static string SerializeObject(object toSerialize)
        {
            try
            {
                return JsonConvert.SerializeObject(toSerialize, Formatting.Indented, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            }
            catch
            {
                try
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

                    using (StringWriter textWriter = new StringWriter())
                    {
                        xmlSerializer.Serialize(textWriter, toSerialize);
                        return textWriter.ToString();
                    }
                }
                catch
                {
                    return toSerialize.ToString();
                }
            }
        }

        public delegate void NotifyEvent(object sender, EventArgs e);
        event EventHandler LogCollectionChange;
        event EventHandler LogErrorCollectionChange;
        object event_lock = new object();

        /// <summary>
        /// Attach your own notify event.
        /// </summary>
        /// <param name="notify_event"></param>
        public void AddLogNotify(NotifyEvent notify_event)
        {
            LogCollectionChange += new EventHandler(notify_event);
        }

        /// <summary>
        /// Attach your own notify event.
        /// </summary>
        /// <param name="notify_event"></param>
        public void AddLogErrorNotify(NotifyEvent notify_event)
        {
            LogErrorCollectionChange += new EventHandler(notify_event);
        }

        /// <summary>
        /// Push some message to log.
        /// </summary>
        /// <param name="str"></param>
        public void Push(string str)
        {
            write_log(DateTime.Now, str);
            lock (event_lock) LogCollectionChange?.Invoke(Tuple.Create(DateTime.Now, str, false), null);
        }

        /// <summary>
        /// Push some object to log.
        /// </summary>
        /// <param name="obj"></param>
        public void Push(object obj)
        {
            write_log(DateTime.Now, obj.ToString());
            write_log(DateTime.Now, SerializeObject(obj));
            lock (event_lock)
            {
                LogCollectionChange?.Invoke(Tuple.Create(DateTime.Now, obj.ToString(), false), null);
                LogCollectionChange?.Invoke(Tuple.Create(DateTime.Now, SerializeObject(obj), true), null);
            }
        }

        /// <summary>
        /// Push some message to log.
        /// </summary>
        /// <param name="str"></param>
        public void PushError(string str)
        {
            write_error_log(DateTime.Now, str);
            lock (event_lock) LogErrorCollectionChange?.Invoke(Tuple.Create(DateTime.Now, str, false), null);
        }

        /// <summary>
        /// Push some object to log.
        /// </summary>
        /// <param name="obj"></param>
        public void PushError(object obj)
        {
            write_error_log(DateTime.Now, obj.ToString());
            write_error_log(DateTime.Now, SerializeObject(obj));
            lock (event_lock)
            {
                LogErrorCollectionChange?.Invoke(Tuple.Create(DateTime.Now, obj.ToString(), false), null);
                LogErrorCollectionChange?.Invoke(Tuple.Create(DateTime.Now, SerializeObject(obj), true), null);
            }
        }

        object log_lock = new object();

        private void write_log(DateTime dt, string message)
        {
            CultureInfo en = new CultureInfo("en-US");
            lock (log_lock)
            {
                File.AppendAllText(Path.Combine(AppProvider.ApplicationPath, "log.txt"), $"[{dt.ToString(en)}] {message}\r\n");
            }
        }

        private void write_error_log(DateTime dt, string message)
        {
            CultureInfo en = new CultureInfo("en-US");
            lock (log_lock)
            {
                File.AppendAllText(Path.Combine(AppProvider.ApplicationPath, "log.txt"), $"[{dt.ToString(en)}] [Error] {message}\r\n");
            }
        }
    }
}
