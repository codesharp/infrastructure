# CodeSharp Core base Castle

infrastructure for server side

==============

�ϲ����룺�����������⣬����汾��������castle3.0����ݵ�����
- Castle.Services.Transaction
	- Attributes��TransactionMode.cs�ȣ�����Core�У�ת��Core��������
- Castle.Facilities.Logging
	- bugfix
		- AddComponentInstance����iloggerfactory������Ʊ�ռ������
- Castle.Facilities.AutoTx
	- bugfix
		- AddComponentInstance
		- AddComponent
		- kernel[key]
		- new InterceptorReference(typeof(TransactionInterceptor) -> new InterceptorReference("transaction.interceptor")
- Castle.Facilities.NHibernateIntegration
	- bugfix see changes.txt


