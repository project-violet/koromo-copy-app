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
            catch (Exception e)
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
                catch (Exception ex)
                {
                    return toSerialize.ToString();
                }
            }
        }

        public delegate void NotifyEvent(object sender, NotifyCollectionChangedEventArgs e);
        public ObservableCollection<Tuple<DateTime, string, bool>> Log { get; } = new ObservableCollection<Tuple<DateTime, string, bool>>();

        public Logs()
        {
            AddLogNotify(Logs_Notify);
        }

        /// <summary>
        /// Attach your own notify event.
        /// </summary>
        /// <param name="notify_event"></param>
        public void AddLogNotify(NotifyEvent notify_event)
        {
            Log.CollectionChanged += new NotifyCollectionChangedEventHandler(notify_event);
        }

        /// <summary>
        /// Push some message to log.
        /// </summary>
        /// <param name="str"></param>
        public void Push(string str)
        {
            lock (Log)
            {
                Log.Add(Tuple.Create(DateTime.Now, str, false));
            }
        }

        /// <summary>
        /// Push some object to log.
        /// </summary>
        /// <param name="obj"></param>
        public void Push(object obj)
        {
            lock (Log)
            {
                Log.Add(Tuple.Create(DateTime.Now, obj.ToString(), false));
                Log.Add(Tuple.Create(DateTime.Now, SerializeObject(obj), true));
            }
        }

        private void Logs_Notify(object sender, NotifyCollectionChangedEventArgs e)
        {
            lock (Log)
            {
                CultureInfo en = new CultureInfo("en-US");
                File.AppendAllText("log.txt", $"[{Log.Last().Item1.ToString(en)}] {Log.Last().Item2}\r\n");
            }
        }
    }
}
