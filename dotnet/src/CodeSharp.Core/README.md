# codesharp Core

infrastructure for server side

==============

合并编译：
-Common.Logging2.0 开源项目中常用log库引用广泛且稳定，仅编译无修改
-NVelocity
	1.修正RuntimeConstants中对于NVelocity程序集的指定
	2.修正runtime/default/*.properties文件中对于NVelocity程序集的指定
-SmartThreadPool
-Dapper
-Quartz
	<EmbeddedResource Include="Quartz\Impl\AdoJobStore\Common\dbproviders.properties" />
    <EmbeddedResource Include="Quartz\quartz.config" />
    <EmbeddedResource Include="Quartz\quartz.properties" />
	对程序集名称的修改
	1.Quartz\quartz.config
	2.Quartz\Impl\StdSchedulerFactory
	3.Quartz\Impl\AdoJobStore\Common\DbProvider
-ZookeeperNet[取消，该为ilmerge]
	git fork: https://github.com/wsky/zookeeper/tree/trunk/src/dotnet/ZooKeeperNet
	直接git维护
	取消Log4net的依赖
	修正cpu100%问题


