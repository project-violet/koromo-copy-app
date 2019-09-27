// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Koromo_Copy.Framework.Cache
{
    public class ProgramLock
    {
        public const string Name = "koromo-copy.lock";
        public static FileStream LockStream;
        public static bool ProgramCrashed = false;

        /// <summary>
        /// Create Lock File
        /// </summary>
        /// <returns></returns>
        public static bool Lock()
        {
            try
            {
                var full_path = Path.Combine(Directory.GetCurrentDirectory(), Name);
                if (!File.Exists(full_path))
                    using (File.Create(full_path)) { }
                else
                    ProgramCrashed = true;
                LockStream = new FileStream(full_path, FileMode.Open, FileAccess.Read, FileShare.None);
                return true;
            }
            catch (FileNotFoundException e)
            {
                // Write Permission Error
                Logs.Instance.Push("[Program Lock] Locking pp-error - " + e.Message);
                return false;
            }
            catch (IOException e)
            {
                Logs.Instance.Push("[Program Lock] Locking io-error - " + e.Message);
                return false;
            }
            catch (Exception e)
            {
                Logs.Instance.Push("[Program Lock] Locking unhandled-error - " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Delete Lock File
        /// </summary>
        public static void UnLock()
        {
            LockStream.Close();
            var full_path = Path.Combine(Directory.GetCurrentDirectory(), Name);
            File.Delete(full_path);
        }
    }
}
