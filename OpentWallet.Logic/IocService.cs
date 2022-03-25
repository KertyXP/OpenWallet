using OpenWallet.Logic.Abstraction.Interfaces;
using System;
using Unity;

namespace OpentWallet.Logic
{
    public class IocService : IIocService
    {
        private readonly IUnityContainer _unityContainer;

        public IocService(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
        }

        public T Resolve<T>()
        {
            return _unityContainer.Resolve<T>();
        }

        public object Resolve(Type t)
        {
            return _unityContainer.Resolve(t);
        }
    }
}