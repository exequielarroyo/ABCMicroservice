using ABC.Core.Business;
using ABC.Core.Contract;
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ABC.Core
{
    public class ServiceResolverModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Domain.ABCEntities>().AsSelf();
            builder.RegisterType<ItemsBL>().As<IItemsBL>();

            var assembly = Assembly.GetExecutingAssembly();
            builder.RegisterAssemblyTypes(assembly)
                .Where(t => t.Name.EndsWith("BL"))
                .As(t => t.GetInterfaces()[0]);
        }
    }
}
