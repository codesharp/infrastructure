# CodeSharp Core base Castle

infrastructure for server side

==============

### ServiceFramework.Castles

Merged in Core.Castles.

- Update to Castle 3.x, exclude NServiceBus support.
- Remove NamingSubSystem with castle 2.x

	Copyright 2011-2012 houkun
 
	Licensed under the Apache License, Version 2.0 (the "License");
	you may not use this file except in compliance with the License.
	You may obtain a copy of the License at
 
		 http://www.apache.org/licenses/LICENSE-2.0
 
	Unless required by applicable law or agreed to in writing, software
	distributed under the License is distributed on an "AS IS" BASIS,
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	See the License for the specific language governing permissions and
	limitations under the License.


### 合并编译：集成型三方库，解决版本、升级至castle3.0后兼容等问题
- Castle.Services.Transaction
	- Attributes、TransactionMode.cs等，移至Core中，转移Core依赖问题
- Castle.Facilities.Logging
	- bugfix
		- AddComponentInstance出现iloggerfactory组件名称被占用问题
- Castle.Facilities.AutoTx
	- bugfix
		- AddComponentInstance
		- AddComponent
		- kernel[key]
		- new InterceptorReference(typeof(TransactionInterceptor) -> new InterceptorReference("transaction.interceptor")
- Castle.Facilities.NHibernateIntegration
	- bugfix see changes.txt
	- add feature "encrypt=true" for connection.connection_string at DefaultConfigurationBuilder.cs


