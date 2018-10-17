@echo off
nuget pack -Prop Configuration=Release
copy *.nupkg \\exiii-server\public\NugetPackageSource
move *.nupkg .\..\Nuget
pause