# Koromo Copy Development Manual

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

## App Provider

`App Provider` is initializer and deinitializer of `Koromo Copy Framework`.
All parts of the `Koromo Copy Framework` are interrelated with each components, so you need to be initialized.

### Initializing Step

```
1. Initialize Logs class for logging.
2. Create Lock file.
3. Check program crashed when last running.
4. Check invalid instance initializing.
5. Set gc to intrusive mode.
6. Initialize scheduler.
```

### Deinitializing Step

Inverse step of `Initializing Step`.

## Network

`Network` namespace is collection of core utils for downloading.

### Net Task

### Net Scheduler

### Net Field

### Net Tools

`NetTools` provides some useful download methods like `WebClient.DownloadStringAsync`.
All network communication is handled by the scheduler,
you should not use interfaces provided by `.Net` directly, such as `WebClient` or `HttpRequest`.

## Extractor

## Log

## Cache

## Compiler