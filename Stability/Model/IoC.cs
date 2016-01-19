using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Parameters;

namespace Stability.Model
{
    public static class IoC
        {
            private static readonly IKernel _kernel = new StandardKernel();

            static IoC()
            {
                SetBindings();
            }

            public static void RegisterType<TInterface, TClass>() where TClass : TInterface
            {
                _kernel.Bind<TInterface>().To<TClass>();
            }

            public static void RegisterSingleton<TInterface, TClass>() where TClass : TInterface
            {
                _kernel.Bind<TInterface>().To<TClass>().InSingletonScope();
            }

            public static void RegisterInstance<TInterface, TClass>() where TClass : TInterface
            {
                _kernel.Bind<TInterface>().To<TClass>();
            }

            public static T Resolve<T>(params IParameter[] parameters)
            {
                return _kernel.Get<T>(parameters);
            }

            public static void SetBindings()
            {
            /*    RegisterSingleton<IDataExchange, DataExchangeCan>();
                RegisterSingleton<DataListener, DataListener>();
                RegisterSingleton<ArhivVm, ArhivVm>();
                RegisterSingleton<MineConfig, MineConfig>();
                RegisterSingleton<FormSettings, FormSettings>();
                RegisterSingleton<FormSettingsParol, FormSettingsParol>();
                RegisterSingleton<DataBaseService, DataBaseService>();
                RegisterSingleton<CanStateService, CanStateService>();*/
            }
        }
}
