//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.ComponentActivator;

namespace CodeSharp.Core.Castles
{
    [Obsolete]
    public class MultiTransientLifestyleManager : TransientLifestyleManager
    {
        public override bool Release(object instance)
        {
            //var r = CodeSharp.Core.Services.DependencyResolver.Resolver as MultiWindsorResolver;
            //if (!r.Global.Kernel.Equals(this.Kernel))
            //    r.Global.Kernel.ReleaseComponent(instance);
            //if (!r.Local.Kernel.Equals(this.Kernel))
            //    r.Global.Kernel.ReleaseComponent(instance);
            //if (!r.Container.Kernel.Equals(this.Kernel))
            //    r.Global.Kernel.ReleaseComponent(instance);
            //var b1 = r.Global.Kernel.Equals(this.Kernel);
            //var b2 = r.Local.Kernel.Equals(this.Kernel);
            //var b3 = r.Container.Kernel.Equals(this.Kernel);
            //var b4 = (this.ComponentActivator as AbstractComponentActivator).Kernel.Equals(this.Kernel);
            this.ComponentActivator.Destroy(instance);
            return true;
        }
    }
}