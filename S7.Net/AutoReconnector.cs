using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace S7.Net
{
    public class AutoReconnector
    {
        private readonly Plc _plc;
        private readonly Timer _timer;
        public AutoReconnector(Plc plc, double interval = 3000)
        {
            _plc = plc;
            _timer = new Timer(interval);
            _timer.Elapsed += _timer_Elapsed;
        }
        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!_plc.IsConnected)
            {
                _plc?.Open();
            }
        }
        
    }
}
