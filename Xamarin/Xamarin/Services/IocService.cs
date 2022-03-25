//using Autofac;
//using OpenWallet.Logic.Abstraction.Interfaces;
//using System;

//namespace OpentWallet.Logic
//{
//    public class IocService : IIocService
//    {
//        private readonly IContainer _container;

//        public IocService(IContainer container)
//        {
//            _container = container;
//        }

//        public T Resolve<T>()
//        {
//            using (var scope = _container.BeginLifetimeScope())
//            {
//                return scope.Resolve<T>();
//            }
//        }

//        public object Resolve(Type t)
//        {
//            using (var scope = _container.BeginLifetimeScope())
//            {
//                return scope.Resolve(t);
//            }
//        }
//    }
//}
