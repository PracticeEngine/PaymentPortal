using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace PEPaymentProvider.Receipting
{
    public class ReceiptingService : IRegisteredObject
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly Timer timer;

        /// <summary>
        /// Setup our Basic Service that runs the ReceiptSync process on a background thread every 24 hours
        /// </summary>
        public ReceiptingService()
        {
            cancellationTokenSource = new CancellationTokenSource();
            var runTime = ConfigurationManager.AppSettings["QuickFeeReceiptingRunTime"];
            timer = new Timer(RunReceiptSync, cancellationTokenSource.Token, CalculateNextRun(runTime), TimeSpan.FromHours(24));

            // Attempt to restart any paused processing
            HostingEnvironment.QueueBackgroundWorkItem(CheckForUnfinishedWork);
        }

        private async Task CheckForUnfinishedWork(CancellationToken token)
        {
            var receiptSync = new ReceiptSync();
            await receiptSync.ResumeInterruptedFile(token);
        }

        /// <summary>
        /// Calculates how long until the next time to run
        /// </summary>
        /// <param name="runTime">string of hh:mm</param>
        /// <returns>TimeSpan representing how long it is until that time</returns>
        public static TimeSpan CalculateNextRun(string runTime)
        {
            var timeToRun = DateTime.ParseExact(runTime, "HH:mm", CultureInfo.InvariantCulture).TimeOfDay;
            var tillNextRun = DateTime.Now.TimeOfDay < timeToRun ? timeToRun.Subtract(DateTime.Now.TimeOfDay) : TimeSpan.FromDays(1).Subtract(DateTime.Now.TimeOfDay.Subtract(timeToRun));
            return tillNextRun;
        }

        /// <summary>
        /// Called by Backgroun Thread - Starts the Process to Handle Receipts
        /// </summary>
        /// <param name="state"></param>
        private void RunReceiptSync(object state)
        {
            CancellationToken hostingToken = (CancellationToken)state;
            var receiptSync = new ReceiptSync();
            receiptSync.RunAsync(hostingToken).Wait();
        }

        /// <summary>
        /// IRegisteredObject Implementation
        /// </summary>
        /// <param name="immediate">is the stop immediate?</param>
        public void Stop(bool immediate)
        {
            // Request Cancellation if not already requested
            if (!cancellationTokenSource.IsCancellationRequested)
                cancellationTokenSource.Cancel();

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
