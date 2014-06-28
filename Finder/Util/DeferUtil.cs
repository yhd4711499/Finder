using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Finder.Util
{
    static class DeferUtil
    {
        private readonly static Dictionary<object, DispatcherTimer> Timers = new Dictionary<object, DispatcherTimer>();

        public static void StopAll()
        {
            foreach (var timer in Timers.Values)
            {
                timer.Stop();
            }
            Timers.Clear();
        }

        public static void Relay(this object o, Action a, long msInterval)
        {
            DispatcherTimer existedTimer;
            if (Timers.TryGetValue(o, out existedTimer))
            {
                existedTimer.Stop();
                existedTimer = null;
            }
            if (msInterval == 0)
            {
                a();
            }
            else
            {
                var timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(msInterval);
                var handler = new EventHandler((s, e) =>
                {
                    ((DispatcherTimer)s).Stop();
                    Timers.Remove(o);
                    a();
                });
                timer.Tick += handler;

                Timers[o] = timer;
                timer.Start();
            }

        }
    }
}