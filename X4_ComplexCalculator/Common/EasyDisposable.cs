using System;

namespace X4_ComplexCalculator.Common
{
    /// <summary>
    /// 簡易 <see cref="IDisposable"/> 実装
    /// </summary>
    public class EasyDisposable : IDisposable
    {
        public Action? DisposeAction { init; private get; }


        #region IDisposable
        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    DisposeAction?.Invoke();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
