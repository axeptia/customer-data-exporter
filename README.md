# Introduction

The motivation behind this project is to let customers of Axeptia have a way to easy export and transfere the exported content to Axepta from their Data Resourses

## Getting Started

Just clone this project.

## Build and Deploy

There are multiple ways to publish the program. You can build it as a self-contained, who produces a platform-specific executable file, or you can just use the Exe file (Framework-dependent build), where the required .net core already is installed.

More details about pubilshing will you find [here](https://docs.microsoft.com/en-us/dotnet/core/deploying/)

Here a sample how to publish a self-contained for Win x64 platform to the location C:\temp\Axeptia Dataexporter\publish

```powershell
dotnet publish -o 'C:\temp\Axeptia Dataexporter\publish' -r win-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true
```

## Use of the program

See the Readme.md file, located in the src directory