using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADAT_Project.Utilities
{
    public static class TimeUtilities
    {
        /// <summary>
        /// Runs the given action and returns how long it took to execute.
        /// </summary>
        public static TimeSpan RunWithStopwatch(Action action)
        {
            Stopwatch sw = Stopwatch.StartNew();
            action.Invoke();
            sw.Stop();
            return sw.Elapsed;
        }

        /// <summary>
        /// Compares multiple labeled TimeSpan results and returns a summary
        /// showing which operation was the fastest and how much slower the others were.
        /// </summary>
        public static string GetFastest(params KeyValuePair<string, TimeSpan>[] timeSpans)
        {
            if (timeSpans == null || timeSpans.Length == 0)
            {
                return "No TimeSpans provided.";
            }

            var fastest = timeSpans.OrderBy(ts => ts.Value).First();

            StringBuilder sb = new StringBuilder();
            sb.Append($"Fastest: {fastest.Key} - {fastest.Value}");

            foreach (var ts in timeSpans)
            {
                if (ts.Key == fastest.Key) continue;

                var difference = ts.Value - fastest.Value;
                sb.Append($"\n{fastest.Key} was faster than {ts.Key} by {difference}");
            }

            return sb.ToString();
        }
    }
}
