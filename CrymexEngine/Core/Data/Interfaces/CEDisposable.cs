namespace CrymexEngine.Data
{
    public partial class CEDisposable : IDisposable
    {
        public bool Disposed => _disposed;

        private bool _disposed;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            OnDispose();
            _disposed = true;
        }

        protected virtual void OnDispose() { }
    }
}
