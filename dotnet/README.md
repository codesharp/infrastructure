# CodeSharp Infrastructure .Net/Mono c#

Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

[http://codesharp.github.com/infrastructure/](http://codesharp.github.com/infrastructure/)

![logo](http://www.icodesharp.com/codesharp.png)

=====

## Library

* Core
	- CodeSharp.Core
		- merge ServiceFramework
	- CodeSharp.Core.Castles
		- merge ServiceFramework.Castles
	- CodeSharp.Core.Web
	- CodeSharp.Core

* BusinessFramework
	- CodeSharp.Framework
	- CodeSharp.Framework.Casltes

## NET40

* Build

```shell
build.bat nuget
build.bat all [pack]
build.bat test_core
build.bat test_framework
```

## MONO

just use dll under mono 2.10.8+

or build:
```shell
sh build.sh nuget
```
or
```shell
sh nuget.install.sh
```
then
```shell
sh build.sh all [pack]
sh build.sh test_core
sh build.sh test_framework
```

## License

	Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

	Licensed under the Apache License, Version 2.0 (the "License");

	you may not use this file except in compliance with the License.

	You may obtain a copy of the License at
 
		 http://www.apache.org/licenses/LICENSE-2.0
 
	Unless required by applicable law or agreed to in writing, 

	software distributed under the License is distributed on an "AS IS" BASIS, 
	
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	
	See the License for the specific language governing permissions and limitations under the License.