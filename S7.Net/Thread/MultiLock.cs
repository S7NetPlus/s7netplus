using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace S7.Net.Thread
{
    /// <summary>
    /// Muilti Lock
    /// </summary>
    public class MultiLock : IDisposable
    {
        #region Private variable

        private int _waiterCount = 0;
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private bool hasDisposed = false;

        #endregion

        #region Private method

        private void Dispose(bool disposing)
        {
            if (!hasDisposed)
            {
                if (disposing)
                {

                }
            }

            _autoResetEvent.Dispose();
            hasDisposed = true;
        }

        #endregion

        #region Public method

        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// If no thread access code between method Enter() and method Leave() currently,then go on. Or blocked.
        /// </summary>
        public void Enter()
        {
            if (Interlocked.Increment(ref _waiterCount) == 1) return;  // Indicate that no thread is waiting.

            _autoResetEvent.WaitOne();
        }

        /// <summary>
        /// Release control
        /// </summary>
        public void Leave()
        {
            if (Interlocked.Decrement(ref _waiterCount) == 0) return;  // Indicate that no thread is waiting.
            _autoResetEvent.Set();
        }

        #endregion



    }
}
