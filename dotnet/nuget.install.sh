rm -rf packages
mono --runtime=v4.0 ../../work-tool/NuGet.exe install src/CodeSharp.Core.Castles/packages.config		-NoCache -o packages
mono --runtime=v4.0 ../../work-tool/NuGet.exe install src/CodeSharp.Core.Castles.Test/packages.config		-NoCache -o packages
mono --runtime=v4.0 ../../work-tool/NuGet.exe install src/CodeSharp.Framework.Castles/packages.config		-NoCache -o packages
mono --runtime=v4.0 ../../work-tool/NuGet.exe install src/CodeSharp.Framework.Castles.Test/packages.config	-NoCache -o packages
