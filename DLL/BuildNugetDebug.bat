@echo off
nuget pack -Prop Configuration=Debug -suffix debug
rem copy *.nupkg \\exiii-server\public\NugetPackageSource\DebugBuild
move *.nupkg .\..\Nuget\DebugBuild
pause