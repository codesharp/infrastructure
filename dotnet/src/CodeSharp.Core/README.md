# codesharp Core

infrastructure for server side

==============

�ϲ����룺
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


