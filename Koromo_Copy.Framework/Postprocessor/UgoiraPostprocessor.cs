// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Extractor;
using Koromo_Copy.Framework.Network;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Koromo_Copy.Framework.Postprocessor
{
    /// <summary>
    /// Postprocessor for pixiv-ugoria(zip) to gif.
    /// </summary>
    public class UgoiraPostprocessor : IPostprocessor
    {
        public List<PixivExtractor.PixivAPI.UgoiraFrames> Frames;

        public override void Run(NetTask task)
        {
            ugoira2gif(task);
        }

        private void ugoira2gif(NetTask task)
        {
            using (var file = File.OpenRead(task.Filename))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
            using (var entry = zip.GetEntry(Frames[0].File).Open())
            using (var first_image = Image.Load(entry))
            {
                for (int i = 1; i < Frames.Count; i++)
                {
                    using (var ientry = zip.GetEntry(Frames[i].File).Open())
                    using (var iimage = Image.Load(ientry))
                    {
                        var frame = iimage.Frames.RootFrame;

                        frame.Metadata.GetFormatMetadata(GifFormat.Instance).FrameDelay = Frames[i].Delay.Value / 10;
                        first_image.Frames.AddFrame(frame);
                    }
                }

                first_image.Save(Path.Combine(Path.GetDirectoryName(task.Filename), Path.GetFileName(task.Filename).Split('_')[0] + ".gif"), new GifEncoder());
            }

            File.Delete(task.Filename);
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        }

        private void ugoira2webp(NetTask task)
        {

        }
    }
}
