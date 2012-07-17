echo off
mkdir assembly\packages
echo pack
forfiles /p assembly /m *.nuspec /c "cmd /c ..\tools\nuget pack @file -outputdirectory packages"
echo push
forfiles /p assembly\packages /m *.nupkg /c "cmd /c ..\..\tools\nuget push @file -s http://taobao-ops-base/feeds/ hello1234"
echo on