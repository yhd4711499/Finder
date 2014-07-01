using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Finder.Util
{
    static class DeferUtil
    {
        private readonly static Dictionary<Action, DispatcherTimer> Timers = new Dictionary<Action, DispatcherTimer>();

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
            if (Timers.TryGetValue(a, out existedTimer))
            {
                existedTimer.Stop();
            }
            if (msInterval <= 0)
            {
                a();
            }
            else
            {
                var timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(msInterval)};
                var handler = new EventHandler((s, e) =>
                {
                    ((DispatcherTimer)s).Stop();
                    Timers.Remove(a);
                    a();
                });
                timer.Tick += handler;

                Timers[a] = timer;
                timer.Start();
            }

        }
    }
}