# codesharp Core

infrastructure for server side

==============

### ServiceFramework

Merged in Core.

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

### �ϲ����룺

-Common.Logging2.0 ��Դ��Ŀ�г���log�����ù㷺���ȶ������������޸�

-NVelocity
	1.����RuntimeConstants�ж���NVelocity���򼯵�ָ��
	2.����runtime/default/*.properties�ļ��ж���NVelocity���򼯵�ָ��

-SmartThreadPool

-Dapper

-Quartz
	<EmbeddedResource Include="Quartz\Impl\AdoJobStore\Common\dbproviders.properties" />
    <EmbeddedResource Include="Quartz\quartz.config" />
    <EmbeddedResource Include="Quartz\quartz.properties" />
	�Գ������Ƶ��޸�
	1.Quartz\quartz.config
	2.Quartz\Impl\StdSchedulerFactory
	3.Quartz\Impl\AdoJobStore\Common\DbProvider

-ZookeeperNet[ȡ������Ϊilmerge]
	git fork: https://github.com/wsky/zookeeper/tree/trunk/src/dotnet/ZooKeeperNet
	ֱ��gitά��
	ȡ��Log4net������
	����cpu100%����

