using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using ApplicationService.Common;
using log4net;

namespace ApplicationWindowsService
{
    public partial class AppWinService : ServiceBase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ServiceHost _serviceHost;

        public AppWinService()
        {
            InitializeComponent();
        }

        public void StartService()
        {
            OnStart(null);
        }

        public void StopService()
        {
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            //Debugger.Break();
            try
            {
                StopAll();

                _serviceHost = new ServiceHost(typeof(ApplicationService.Common.ApplicationService));
                _serviceHost.Open();
            }
            catch (Exception e)
            {
                log.Error("Error on start", e);
                throw;
            }
        }

        protected override void OnStop()
        {
            StopAll();
        }

        private void StopAll()
        {
            if (_serviceHost == null)
                return;

            if (_serviceHost.State == CommunicationState.Opened)
            {
                try
                {
                    _serviceHost.Close();
                }
                catch (Exception e)
                {
                    log.Error("Error on stop", e);
                }
            }
            _serviceHost = null;
        }
    }
}
