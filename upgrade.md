Infrastructure Upgrade History

====

## dotnet changes

* 201207 by wsky
====

original dotnet infrastructure from tb upgrade from v1.x to v2.0, and rename to CodeSharp.Core.*

license change to LGPL?

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
	- Kernel[key] obsolete
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
	
- NHibernate 2.1 -> 3.3.1
	- remove Antlr3.Runtime
	- nuget: NHibernate 3.3.1.4000
	- nuget: Iesi.Collections 3.2.0.4000

- FluentNHibernate 1.1 -> 1.3
	- nuget: FluentNHibernate 1.3.0.733

- log4net 1.2 -> 1.2.11
	- nuget: log4net 2.0.0
	- see http://logging.apache.org/log4net/release/release-notes.html

- NServiceBus

- 

# obj-c





