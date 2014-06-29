using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace ApplicationWindowsService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var service = new AppWinService();

            if (Environment.UserInteractive)
            {
                service.StartService();
                Console.WriteLine("Application server started. Press Ctrl+C for exit.");

                var cancel = new AutoResetEvent(false);
                Console.CancelKeyPress += (sender, e) =>
                {
                    service.StopService();
                    service.Dispose();
                    cancel.Set();
                };

                cancel.WaitOne();
            }
            else
            {
                RunAsService(service);    
            }
        }

        private static void RunAsService(ServiceBase service)
        {
            var servicesToRun = new ServiceBase[] 
            { 
                service
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
