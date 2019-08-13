//TODO - Resilliance file

using System;
using System.Configuration;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Hosting;

namespace PEPaymentProvider.Receipting
{
    public class ReceiptingService : IRegisteredObject
    {
        private static CancellationTokenSource timerCancellationTokenSource;
        private static System.Timers.Timer startTimer;
        private static System.Timers.Timer frequencyTimer;

        /// <summary>
        /// Setup our Basic Service that runs the ReceiptSync process on a background thread every 24 hours
        /// </summary>
        public ReceiptingService()
        {
            System.Diagnostics.Trace.TraceInformation("Receipting Service current time {0:HH:mm:ss.fff}", DateTime.Now.ToString());

            timerCancellationTokenSource = new CancellationTokenSource();

            var runOnStartup = bool.Parse(ConfigurationManager.AppSettings["ReceiptingRunOnStartup"]);

            if (runOnStartup)
                RunReceiptSync();

            SetStartTimer();
        }

        private static void SetStartTimer()
        {
            var runTime = ConfigurationManager.AppSettings["ReceiptingRunTime"];

            var timeToRun = DateTime.ParseExact(runTime, "HH:mm", CultureInfo.InvariantCulture).TimeOfDay;

            var tillNextRun = DateTime.Now.TimeOfDay < timeToRun ? timeToRun.Subtract(DateTime.Now.TimeOfDay) : TimeSpan.FromDays(1).Subtract(DateTime.Now.TimeOfDay.Subtract(timeToRun));

            startTimer = new System.Timers.Timer(tillNextRun.TotalMilliseconds);

            startTimer.Elapsed += StartTimerElapsed;
            startTimer.AutoReset = false;
            startTimer.Enabled = true;
        }

        private static void StartTimerElapsed(object sender, ElapsedEventArgs e)
        {
            RunReceiptSync();

            SetFrequencyTimer();
        }

        private static void SetFrequencyTimer()
        {
            var runFrequency = int.Parse(ConfigurationManager.AppSettings["ReceiptingRunFrequencyMinutes"]);

            var runFrequencyMilliseconds = new TimeSpan(0, runFrequency, 0).TotalMilliseconds;

            frequencyTimer = new System.Timers.Timer(runFrequencyMilliseconds);

            frequencyTimer.Elapsed += FrequencyTimerElapsed;
            frequencyTimer.AutoReset = true;
            frequencyTimer.Enabled = true;
        }

        private static void FrequencyTimerElapsed(Object source, ElapsedEventArgs e)
        {
            RunReceiptSync();
        }

        private static void RunReceiptSync()
        {
            System.Diagnostics.Trace.TraceInformation("RunReceiptSync Started {0:HH:mm:ss.fff}", DateTime.Now.ToString());

            var receiptSync = new ReceiptSync();
            receiptSync.RunAsync(timerCancellationTokenSource.Token).Wait();

            System.Diagnostics.Trace.TraceInformation("RunReceiptSync Complete {0:HH:mm:ss.fff}", DateTime.Now.ToString());
        }

        //TODO - NEED TO IMPLEMENT
        //private async Task CheckForUnfinishedWork(CancellationToken token)
        //{
        //    var receiptSync = new ReceiptSync();
        //    await receiptSync.ResumeInterruptedFile(token);
        //}

        /// <summary>
        /// IRegisteredObject Implementation
        /// </summary>
        /// <param name="immediate">is the stop immediate?</param>
        public void Stop(bool immediate)
        {
            // Request Cancellation if not already requested
            if (!timerCancellationTokenSource.IsCancellationRequested)
                timerCancellationTokenSource.Cancel();

            // Unregister this Object
            HostingEnvironment.UnregisterObject(this);

            // Block as long as we can
            if (!immediate)
            {
                Task.Delay(TimeSpan.FromSeconds(30)).Wait();
            }
        }
    }
}
