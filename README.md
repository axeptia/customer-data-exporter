# Introduction

The motivation behind this project is to let customers of Axeptia have a way to easy export and transfer data to Axeptia.

If you just want to download the program you find it here:

[Win64 self-contained exe (in a zip file)](https://github.com/axeptia/customer-data-exporter/releases/download/v1.0.1/AxeptiaExporter.ConsoleApp_Win_x64.zip)

## Getting started

Just clone this project.

## Build and deploy

There are multiple ways to publish the program. You can build it as a self-contained app, which produces a platform-specific executable file, or you can just use the .exe file (framework-dependent build), where the required .net core is already installed.
You will find more details about pubilshing [here](https://docs.microsoft.com/en-us/dotnet/core/deploying/).
Below is an example of how to publish the app as a self-contained for Win x64 platform to the location `C:\temp\Axeptia Dataexporter\publish`.

```powershell
dotnet publish -o 'C:\temp\Axeptia Dataexporter\publish' -r win-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true
```

## Use of the program

See the [readme file](src/AxeptiaExporter.Console/Readme.md) located in the src directory.
