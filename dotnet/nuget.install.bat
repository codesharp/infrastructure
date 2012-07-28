echo off
mkdir %2
@forfiles /s /m packages.config /c "cmd /c %1\nuget install @file -o %2"
echo on