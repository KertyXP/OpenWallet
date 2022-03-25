using System;

namespace OpenWallet.Logic.Abstraction.Interfaces
{
    public interface IIocService
    {
        T Resolve<T>();
        object Resolve(Type t);
    }
}
