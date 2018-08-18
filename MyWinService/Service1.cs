using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using ConfigForm;
using System.Threading;
using MyWatcher;

namespace MyWinService
{
    public partial class Service1 : ServiceBase
    {

        private System.Timers.Timer _indexingTimer;
        private MyWatcherManager _watcherManager;

        /// <summary>
        /// Initialize timer and specify timer method witch should be invoked by timer
        /// </summary>
        public Service1()
        {
            InitializeComponent();
            //_indexingTimer.AutoReset = true;
            //_indexingTimer.Elapsed += indexingTimerTick;
        }

        public void OnDebug()
        {
            OnStart(null);
        }

        /// <summary>
        /// Timer tick method, stops watching dir, starts indexing, starts watching dir and resets timer
        /// </summary>
        void indexingTimerTick(object sender, ElapsedEventArgs e)
        {
            _watcherManager.StopWatching();
            _watcherManager.DoIndexing();
            _indexingTimer.Start();
            _watcherManager.StartWatching();
        }

        /// <summary>
        /// Initialize service
        /// </summary>
        protected override void OnStart(string[] args)
        {
            //System.IO.File.Create(AppDomain.CurrentDomain.BaseDirectory + "OnStart.txt");
            ConfigManager.Load();
            _watcherManager = new MyWatcherManager();

            _indexingTimer = new System.Timers.Timer(ConfigManager.IndexInterval);
            _indexingTimer.Elapsed += indexingTimerTick;
            _indexingTimer.Start();
        }

        /// <summary>
        /// Cleanup after service stop
        /// </summary>
        protected override void OnStop()
        {
            // cleanup
            //System.IO.File.Create(AppDomain.CurrentDomain.BaseDirectory + "OnStop.txt");

            _indexingTimer.Stop();
            _watcherManager.Clients = null;
            _watcherManager.Watchers = null;
        }
    }
}
