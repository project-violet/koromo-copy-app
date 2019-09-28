# Koromo Copy

`Koromo Copy` is a cross platform image downloader, crawler collection, and file management system.

## Projects

### Koromo_Copy.Framework

`Koromo_Copy.Framework` project is the core library for koromo-copy that uses `.netstandard`. 
This library implements all the major functions such as script analysis, 
configuration, download, crawling and file management.

### Koromo_Copy.Console

`Koromo_Copy.Console` project is cross platform console application for `Windows`, `Linux`, and `Mac OS`.
In this project, some command are implemented to properly use the main functions of the `Koromo_Copy.Framework`.
Also, a dialog has implemented, so you can handle `Koromo_Copy.Framework`.

### Koromo_Copy.Tool.CustomCrawler

`Koromo_Copy.Framework` project supports to script embedding based html-parsing command line called `Html Toolkit`.
This project will increases development productivity and eases maintenance.

[Click here for more informations.](Document/CustomCrawler.md)

## Documents

[Development Manual](Document/Development.md)

## How to build?

### Linux

First, you must download and install .net-sdk for mono.

```
https://dotnet.microsoft.com/download/dotnet-core/3.0

Ubuntu 19.04 - x64
wget -q https://packages.microsoft.com/config/ubuntu/19.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install apt-transport-https
sudo apt-get install dotnet-sdk-3.0
```

Second, build project.

```
git clone https://github.com/dc-koromo/koromo-copy2
cd koromo-copy2/Koromo_Copy.Console
nuget restore Koromo_Copy.Console.csproj
dotnet build -c Release
```

Last, run program!

```
cd bin/Release/netcoreapp3
./Koromo_Copy.Console
```

### Windows

Install VisualStudio 2019 and .NetCore 3.0.

Open `Koromo Copy.sln` and build solutions.

## Supports

```
DCInside
Danbooru
E-Hentai
Ex-Hentai
Facebook
Gellbooru
Hitomi.la
Imgur
Instagram
Naver Blog
Pixiv
...
```