using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorWith3d.Shared
{
    public class DisposableContainer: IAsyncDisposable
    {
        
        private readonly List<object> _disposables=new ();
        
        
        public async ValueTask DisposeAsync()
        {
            foreach (var o in _disposables)
            {
                if (o is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                else if (o is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                }
            }
        }

        public void TrackDisposable(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        public void TrackDisposable(IAsyncDisposable disposable)
        {
            _disposables.Add(disposable);
        }
    }
}