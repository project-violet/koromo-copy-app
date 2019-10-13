using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Lang;
using Environment = System.Environment;

namespace Koromo_Copy.App.Droid
{
    public class YoutubeDL
    {
        public static void test_run()
        {
            //var app_path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var path = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath, "KoromoCopy", "youtube_dl");
            using (var br = new BinaryReader(Application.Context.Assets.Open("youtube_dl")))
            {
                using (var bw = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    byte[] buffer = new byte[2048];
                    int length = 0;
                    while ((length = br.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        bw.Write(buffer, 0, length);
                    }
                }
            }

            //if (File.Exists(Path.Combine(app_path, "youtube_dl")))
            //    ;

            Java.Lang.Process p = Runtime.GetRuntime().Exec("chmod u+x " + path);
            //Java.Lang.Process p = Runtime.GetRuntime().Exec(new string[] { "/system/bin/chmod", "744", Path.Combine(app_path, "youtube_dl") });

            BufferedReader reader = new BufferedReader(new InputStreamReader(p.InputStream));
            int read;
            char[] bbuffer = new char[4096];
            StringBuffer output = new StringBuffer();
            while ((read = reader.Read(bbuffer)) > 0)
            {
                output.Append(bbuffer, 0, read);
            }
            reader.Close();


            p.WaitFor();

            var st = output.ToString();
        }
    }
}