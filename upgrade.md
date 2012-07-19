#Infrastructure Upgrade History

====

## .Net Changes

#### 201207 by wsky

Original dotnet infrastructure from "tb" upgrade, from v1.x to v2.0, and renamed to CodeSharp.*

License change to LGPL?

- Castle 2.x -> 3.x
	- remove Castle.DynamicProxy
	- remove Caslte.Microkernel
	- nuget: Castle.Core 3.0.0.4001
	- nuget: Castle.Windsor 3.0.0.4001
	- DynamicProxy, Microkernel "As part of changes in version 2.5 it was merged into Castle.Core.dll." 
	- see http://stw.castleproject.org/Tools.DynamicProxy.ashx
	- see http://devlicio.us/blogs/krzysztof_kozmic/archive/2010/07/05/castle-windsor-2-5-the-final-countdown-beta-1-released-and-core-dynamicproxy-dicitionary-adapter.aspx

- Facilities or about Castle changes
	- Castle.Core.Interceptor.IInterceptor -> Castle.DynamicProxy.IInterceptor
	- Castle.Core.Interceptor.IInvocation -> Castle.DynamicProxy.IInvocation
	- Castle.Core.Logging.Factories -> X
	- Castle.Core.Logging.WebLoggerFactory -> X
	- Castle.MicroKernel.CreationContext -> Castle.MicroKernel.Context.CreationContext
	- Castle.Core.Logging.ILogger interface change, more methods
	- NHibernate.ISession interface change, more methods
	- IWindsorContainer.Resolve(string) -> Resolve(string key, Type type)
	- NHibernate.Bytecode.IProxyFactoryFactory changes, more methods: IsInstrumented, IsProxy
		- IsInstrumented return true, enable lazyload
		- see http://www.digipedia.pl/usenet/thread/17921/600/
		- see http://mausch.github.com/nhibernate-3.2.0GA/html/d92f91a7-5663-f481-1bd7-3625741962c2.htm
	- ComponentUnregistered -> X
	- RemoveComponent -> X
	- Castle.Facilities.FactorySupport.FactorySupportFacility -> X
	- Kernel[key] obsolete
	- Kernel.AddComponentInstance obsolete, must change to "Use Register(Component.For(serviceType).Named(key).Instance(instance)) or generic version instead."
		- see https://github.com/codesharp/infrastructure/commit/cc5d88fd47b85df417c1abff9f2360bef1b62da4
	- Multi service exposed: ComponentModel.Service -> ComponentModel.Services
	- improved: 
		- DefaultComponent Change use IsDefault(), "You can force the later-registered component to become the default instance via the method IsDefault."
		- http://stw.castleproject.org/Windsor.Registering-components-one-by-one.ashx
		- http://docs.castleproject.org/Windsor.Whats-New-In-Windsor-3.ashx
		- http://kozmic.pl/2011/03/20/working-with-nhibernate-without-default-constructors/

- Castle.Facilities.NHibernateIntegration
	- update to new version sourcecode and need patch
	- https://github.com/castleproject/Castle.Facilities.NHibernateIntegration-READONLY
	- http://issues.castleproject.org/issue/FACILITIES-156
	- "No ISessionFactory implementation associated with the given alias: nh.facility.default"
		- Castle.Facilities.NHibernateIntegration.Internal.SessionFactoryActivator not work well 
		- override Create(CreationContext context) -> override Create(CreationContext context, Burden burden)
		- need add "burden.SetRootInstance(instance);"
		- see https://gist.github.com/3137006
		- see changes https://github.com/castleproject/Castle.Windsor-READONLY/blob/master/src/Castle.Windsor/MicroKernel/ComponentActivator/AbstractComponentActivator.cs
		- 2.x is https://github.com/castleproject/Castle.Windsor-READONLY/blob/2.5.x/src/Castle.Windsor/MicroKernel/ComponentActivator/AbstractComponentActivator.cs
		- see our bugfix commits https://github.com/codesharp/infrastructure/commit/e081608718c57fab3b6dc482263c719157b053d0
		
- Castle.Facilities.Logging
	- bugfix " There is already a component with that name". update "AddComponentInstance" to "Component.For"
	- kernel[key] -> Resolve()
	
- Castle.Facilities.AutoTx
	- bugfix " There is already a component with that name". update "AddComponentInstance" to "Component.For"
	
- NHibernate 2.1 -> 3.3.1
	- remove Antlr3.Runtime
	- nuget: NHibernate 3.3.1.4000
	- nuget: Iesi.Collections 3.2.0.4000
	- public virtual string Name { get; private set; } not work!
		- *use_proxy_validator*  Enabled by default in nh3.x
		- must *Not.LazyLoad()* or *use_proxy_validator = false* to avoid this
		- then, you can "public string Name { get; private set; }" without "virtual", and no proxy for lazyload?
		- see http://stackoverflow.com/questions/2339264/property-access-strategies-in-nhibernate
		- see http://nhforge.org/doc/nh/en/index.html#mapping-declaration-property
		- see http://stackoverflow.com/questions/741489/ignore-public-internal-fields-for-nhibernate-proxy
		- see https://github.com/search?langOverride=&q=use_proxy_validator&repo=&start_value=1&type=Code
		- see http://www.nhforge.org/doc/nh/en/index.html nh3.3 refer, search use_proxy_validator
		- https://www.google.com/webhp?sourceid=chrome-instant&ix=seb&ie=UTF-8&ion=1#hl=en&gs_nf=1&tok=YnGhj1tw8Ag-CtBRPDc_-Q&pq=nhibernate%20use_proxy_validator&cp=11&gs_id=17&xhr=t&q=nhibernate+proxy+validator&pf=p&newwindow=1&sclient=psy-ab&oq=nhibernate+proxy_validator&gs_l=&pbx=1&bav=on.2,or.r_gc.r_pw.r_cp.r_qf.,cf.osb&fp=f2e1c51fde2e6b7d&ix=seb&ion=1&biw=1920&bih=955&bs=1
		- http://stackoverflow.com/questions/1485127/nhibernate-proxy-validator-changes-in-2-1
		- http://davybrion.com/blog/2009/03/must-everything-be-virtual-with-nhibernate/
	- about logging
		-  NHibernate.SQL will print "Batch commands:"

- FluentNHibernate 1.1 -> 1.3
	- nuget: FluentNHibernate 1.3.0.733

- log4net 1.2 -> 1.2.11
	- nuget: log4net 2.0.0
	- see http://logging.apache.org/log4net/release/release-notes.html

- NServiceBus

- Quartz.Net

## obj-c





