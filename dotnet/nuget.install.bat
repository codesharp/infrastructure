echo off
@forfiles /s /m packages.config /c "cmd /c %1\nuget install @file -o %2 -source http://incooper.cloudapp.net/nuget/nuget;https://nuget.org/api/v2/"
echo on