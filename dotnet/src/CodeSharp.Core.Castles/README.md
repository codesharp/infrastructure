# CodeSharp Core base Castle

infrastructure for server side

==============

合并编译：集成型三方库，解决版本、升级至castle3.0后兼容等问题
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


