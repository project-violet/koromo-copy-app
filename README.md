# Koromo Copy

`Koromo Copy` is a cross platform image downloader.

## How to use?

Put the URL of the site you want to download.

```
./Koromo_Copy.Console https://www.instagram.com/taylorswift/
```

If you want to see the download process, add `-p` option.

```
./Koromo_Copy.Console https://www.instagram.com/taylorswift/ -p
```

If you want to format the download path, enter `--list-extractor` to see the supported options. 
`%(file)s` and `%(ext)s` are provided by default.

```
./Koromo_Copy.Console --list-extractor

...
[InstagramExtractor]
[HostName] www\.instagram\.com
[Checker] ^https?://www\.instagram\.com/(?:p\/)?(?<id>.*?)/?.*?$
[Information] Instagram extactor info
   user:             Full-name.
   account:          User-name
[Options]
   --only-images               Extract only images.
   --only-thumbnail            Extract only thumbnails.
   --include-thumbnail         Include thumbnail extracting video.
   --limit-posts               Limit read posts count. [use --limit-posts <Number of post>]
...

./Koromo_Copy.Console -o "[%(account)s] %(user)s/%(file)s.%(ext)s" https://www.instagram.com/taylorswift/
```

## Documents

[Development Manual](Document/Development.md)

[Apply to my project](Document/Embedding.md)

[Custom Crawler](Document/CustomCrawler.md)

[General](Document/General.md)

## Supports

### Now support

```
Danbooru
DCInside
EHentai
ExHentai
Funbe
Gelbooru
Manamoa
Naver
Pixiv
Hitomi
Hiyobi
Imgur
Instagram (supports Video)
Twitter (supports Video)
```

### Plan to support

```
4Chan
ArtsAndCulture
Daum
Derpibooru
DeviantArt
Fakku
GoogleImage
Konachan
Namuwiki
Nozomi
Pixabay
Reddit
Rule34
SankakuComplex
SMPeople
Tistory
Zerochan
```

## Contribution

Welcome to any form of contribution!

If you are interested in this project or have any suggestions, feel free to open the issue!

## Thanks

```
dotnet: https://dotnet.microsoft.com/
youtube-dl: https://github.com/ytdl-org/youtube-dl
pixivpy: https://github.com/upbit/pixivpy
Pixeez: https://github.com/cucmberium/Pixeez
ImageSharp: https://github.com/SixLabors/ImageSharp
HtmlAgilityPack: https://html-agility-pack.net/
Json.NET: https://www.newtonsoft.com/json
FFmpeg: https://www.ffmpeg.org/
```