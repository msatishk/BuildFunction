using System;
using System.Linq;

namespace ToIVUMultipleFromOracle.Interfaces
{
    public interface IWCFClient<out T> : IDisposable
    {
        void Execute(Action<T> action);

       TResult Execute<TResult>(Func<T, TResult> function);
    }

}
