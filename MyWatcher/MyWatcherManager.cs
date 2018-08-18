using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConfigForm;

namespace MyWatcher
{
    // Main sync class, handles sync items, puts them in queue, filters event etc.
    public class MyWatcherManager
    {
#region properties
        public List<FtpClient> Clients { get; set; }
        public List<FileSystemWatcher> Watchers { get; set; }
        public SyncQueue SyncQueue { get; set; }
        public MyMd5 MyMd5 { get; set; }
#endregion
#region logic
        public MyWatcherManager()
        {
            Clients = new List<FtpClient>();
            Watchers = new List<FileSystemWatcher>();
            MyMd5 = new MyMd5();

            Watchers = new List<FileSystemWatcher>();
            foreach (var c in ConfigManager.Connections)
            {
                var w = new FileSystemWatcher(c.LocalDir);
                w.NotifyFilter = (NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName);
                w.Changed += OnChanged;
                w.Created += OnChanged;
                w.Deleted += OnDeleted;
                w.Renamed += OnRenamed;
                w.EnableRaisingEvents = true;
                w.IncludeSubdirectories = true;
                Watchers.Add(w);

                Clients.Add(new FtpClient(c.Host, c.LocalDir, c.User, c.Password));
            }
            
            SyncQueue = new SyncQueue(Clients);

            Task.Run(() => { DoIndexing(); });

        }

        /// <summary>
        /// Change event handling
        /// </summary>
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // Check if there is file and directory else return
            if ((!File.Exists(e.FullPath) && !Directory.Exists(e.FullPath))) return;

            // Check if file has allowed name
            if (!FileHelper.IsAllowedFilename(e.Name)) return;

            var retries = 0;
            if (File.Exists(e.FullPath))
            {
                // avoid queuing the same file multiple times
                while (true)
                {
                    // Check if file is used
                    if (!FileHelper.FileIsUsed(e.FullPath)) break;
                    // Exit after 5 retries
                    if (retries > 5) return;
                    // Sleep for a 10th of a second, then check again
                    Thread.Sleep(100);
                    retries++;
                }
            }
            // Add to queue
            var actionType = e.ChangeType == WatcherChangeTypes.Changed ? WatcherChangeTypes.Changed : WatcherChangeTypes.Created;

            var toSync = new SyncQueueItem
            {
                Name = e.Name,
                FullPath = e.FullPath,
                ChangeType = actionType,
                Md5 = MyMd5.CalcMd5(e.FullPath),
                SyncTo = ToSync.ToFtp
            };

            SyncQueue.Add(toSync, false);
        }

        /// <summary>
        /// Delete item handling
        /// </summary>
        private void OnDeleted(object source, FileSystemEventArgs e)
        {
            // Add to queue
            SyncQueue.Add(new SyncQueueItem { Name = e.Name, FullPath = e.FullPath, ChangeType = e.ChangeType }, false);
        }

        /// <summary>
        /// Rename item handling
        /// </summary>
        private void OnRenamed(object source, RenamedEventArgs e)
        {
            // Check if file has allowed name
            if (!FileHelper.IsAllowedFilename(e.Name)) return;

            // Find if renamed from/to temporary file
            var renamedFromTempFile = new FileInfo(e.OldFullPath).Attributes.HasFlag(FileAttributes.Temporary);
            var renamedToTempFile = new FileInfo(e.FullPath).Attributes.HasFlag(FileAttributes.Temporary);

            // Add to queue if not temporary file
            if (!renamedFromTempFile || renamedToTempFile) return;

            var toSync = new SyncQueueItem
            {
                Name = e.Name,
                OldName = e.OldName,
                FullPath = e.FullPath,
                OldFullPath = e.OldFullPath,
                ChangeType = WatcherChangeTypes.Renamed,
                Md5 = MyMd5.CalcMd5(e.FullPath),
                SyncTo = ToSync.ToFtp
            };
            SyncQueue.Add(toSync, false);
            //else
            //  AddToQueue(e, WatcherChangeTypes.Changed);
        }


        /// <summary>
        /// Starts watching of sync folder
        /// </summary>
        public void StartWatching()
        {
            foreach (var watcher in Watchers)
            {
                watcher.EnableRaisingEvents = true;
            }
            
        }

        /// <summary>
        /// Stops watching of sync folder
        /// </summary>
        public void StopWatching()
        {
            foreach (var watcher in Watchers)
            {
                watcher.EnableRaisingEvents = false;
            }
            
        }


        /// <summary>
        /// Takes all local files and puts them into queue as queue items
        /// </summary>
        public void DoIndexing()
        {
            foreach (var client in Clients)
            {
                // Local files
                foreach (var c in ConfigManager.Connections)
                {
                    var dirInfo = new DirectoryInfo(c.LocalDir);
                    var localFiles = dirInfo.GetFiles("*.*");
                    var items = localFiles.Select(f => new SyncQueueItem
                    {
                        Name = f.Name,
                        FullPath = f.FullName,
                        AddedOn = DateTime.Now,
                        Md5 = new MyMd5().CalcMd5(f.FullName)
                    }).ToList();

                    SyncQueue.SyncTo(items, client);
                } 
            }
            


        }
#endregion
    }
}
