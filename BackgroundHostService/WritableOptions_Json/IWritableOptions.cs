using Microsoft.Extensions.Options;
using System;

namespace WritableOptions_Json
{
    public interface IWritableOptions<out T> :
        IOptionsSnapshot<T> where T : class, new()
    {
        void Update(Action<T> applyChanges);
    }
}
