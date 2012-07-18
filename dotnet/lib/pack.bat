..\..\external-work\tools\nuget pack CodeSharp.Package.AspNetMvc3.3.0.nuspec
..\..\external-work\tools\nuget pack CodeSharp.Package.AspNetWebPage.1.0.nuspec

..\..\external-work\tools\nuget setApiKey 639c61cf-6e89-4a98-a69a-31b6fa0e2378

..\..\external-work\tools\nuget push CodeSharp.Package.AspNetMvc3.3.0.nupkg
..\..\external-work\tools\nuget push CodeSharp.Package.AspNetWebPage.1.0.nupkg

..\..\external-work\tools\nuget push CodeSharp.Package.AspNetMvc3.3.0.nupkg -s http://nuget.incooper.net/ codesharp
..\..\external-work\tools\nuget push CodeSharp.Package.AspNetWebPage.1.0.nupkg -s http://nuget.incooper.net/ codesharp
