..\..\..\work-tool\nuget pack CodeSharp.Package.AspNetMvc3.3.0.nuspec
..\..\..\work-tool\nuget pack CodeSharp.Package.AspNetWebPage.1.0.nuspec
..\..\..\work-tool\nuget pack CodeSharp.Package.MSTest.10.0.nuspec

..\..\..\work-tool\nuget  setApiKey 639c61cf-6e89-4a98-a69a-31b6fa0e2378 
..\..\..\work-tool\nuget push CodeSharp.Package.AspNetMvc3.3.0.nupkg
..\..\..\work-tool\nuget push CodeSharp.Package.AspNetWebPage.1.0.nupkg
..\..\..\work-tool\nuget push CodeSharp.Package.MSTest.10.0.nupkg

..\..\..\work-tool\nuget push CodeSharp.Package.AspNetMvc3.3.0.nupkg -s http://nuget.icodesharp.com/ codesharp
..\..\..\work-tool\nuget push CodeSharp.Package.AspNetWebPage.1.0.nupkg -s http://nuget.icodesharp.com/ codesharp
..\..\..\work-tool\nuget push CodeSharp.Package.MSTest.10.0.nupkg -s http://nuget.icodesharp.com/ codesharp
