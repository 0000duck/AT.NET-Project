using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyWatcher
{
    // Simple queue for uploading, downloading -> handling ivoked events
    public class SyncQueue
    {
        private readonly ConcurrentQueue<SyncQueueItem> _syncQueue;
        private readonly List<FtpClient> _ftpClients;

        public SyncQueue() { }

        public SyncQueue(List<FtpClient> ftpClients)
        {
            _syncQueue = new ConcurrentQueue<SyncQueueItem>();
            //Clients = new FtpClient(ConfigForm.ConfigManager.SelectedConnection.Host,
            //    ConfigForm.ConfigManager.SelectedConnection.LocalDir,
            //    ConfigForm.ConfigManager.Username,
            //    ConfigForm.ConfigManager.Password);
            _ftpClients = ftpClients;

        }

        /// <summary>
        /// Ads item to queue
        /// <param name="item">Item to add</param>
        /// <param name="indexing">True if indexing, false if not</param>
        /// </summary>
        public void Add(SyncQueueItem item, bool indexing)
        {
            item.AddedOn = DateTime.Now;
            _syncQueue.Enqueue(item);
            StartQueue(indexing);

        }

        /// <summary>
        /// Starts processing queue
        /// <param name="indexing">True if indexing, false if not</param>
        /// </summary>
        public void StartQueue(bool indexing)
        {
            while (!_syncQueue.IsEmpty)
            {
                SyncQueueItem item;
                if (!_syncQueue.TryDequeue(out item)) continue;
                if (!indexing)
                {
                    foreach (var client in _ftpClients)
                    {
                        Process(item, client);
                    }
                      
                }
                else
                {
                    foreach (var client in _ftpClients)
                    {
                        ProcessIndexing(item, client);
                    }
                    
                }
            }
        }


        
        /// <summary>
        /// Calls method according to item properties (if file should be uploaded to ftp, it will be..)
        /// <param name="item">Item to process</param>
        /// <param name="ftpClient">Client to process</param>
        /// </summary>
        public void Process(SyncQueueItem item, FtpClient ftpClient)
        {

            switch (item.ChangeType)
            {
                case WatcherChangeTypes.Deleted:
                    DeleteItem(item, ftpClient);
                    break;
                case WatcherChangeTypes.Renamed:
                    RenameItem(item, ftpClient);
                    break;
                case WatcherChangeTypes.Changed:
                case WatcherChangeTypes.Created:
                    ChangeItem(item, ftpClient);
                    break;
            }

        }

        /// <summary>
        /// Handle files when indexing, not invoked by any event
        /// <param name="item">Item to process</param>
        /// <param name="ftpClient">Client to process</param>
        /// </summary>
        public void ProcessIndexing(SyncQueueItem item, FtpClient ftpClient)
        {

            switch (item.SyncTo)
            {
                case ToSync.ToFtp:
                    Task.Run(() => { ftpClient.UploadFile(item); });
                    break;
                case ToSync.ToLocal:
                    Task.Run(() => { ftpClient.DownloadFile(item); });
                    break;
                case ToSync.IsIdentical:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Handles file rename event and executes ftp client task for specific item
        /// <param name="item">Item to process</param>
        /// <param name="ftpClient">Client to process</param>
        /// </summary>
        public void RenameItem(SyncQueueItem item, FtpClient ftpClient)
        {
            Task.Run(() => { ftpClient.Rename(item); });
        }

        /// <summary>
        /// Handles file delete event and executes ftp client task for specific item
        /// <param name="item">Item to process</param>
        /// <param name="ftpClient">Client to process</param>
        /// </summary>
        public void DeleteItem(SyncQueueItem item, FtpClient ftpClient)
        {
            Task.Run(() => { ftpClient.DeleteFile(item); });
        }

        /// <summary>
        /// Handles file change event and executes ftp client task for specific item
        /// <param name="item">Item to process</param>
        /// <param name="ftpClient">Client to process</param>
        /// </summary>
        public void ChangeItem(SyncQueueItem item, FtpClient ftpClient)
        {
            Task.Run(() => { ftpClient.UploadFile(item); });

        }

        /// <summary>
        /// Compare local file hash and ftp file hash, newer is uploaded/downloaded
        /// <param name="items">Items to process</param>
        /// <param name="ftpClient">Client to process</param>
        /// </summary>
        public void SyncTo(List<SyncQueueItem> items, FtpClient ftpClient)
        {
            var root = ftpClient.GetAllHashFiles();

            var ftpFiles = (from f in root.Elements("File")
                            select new SyncQueueItem
                            {
                                Name = f.Attribute("FileName").Value,
                                AddedOn = DateTime.Parse(f.Attribute("AddedOn").Value),
                                Md5 = f.Attribute("MD5").Value
                            }).ToList();

            var toLocal = (from f in ftpFiles
                           let files = items.FirstOrDefault(i => i.Name == f.Name)
                           where files == null || (files != null && files.Md5 != f.Md5 && f.AddedOn > files.AddedOn)
                           select f).ToList();

            var toFtp = (from f in items
                         let files = ftpFiles.FirstOrDefault(i => i.Name == f.Name)
                         where files == null || (files != null && files.Md5 != f.Md5 && f.AddedOn > files.AddedOn)
                         select f).ToList();

            foreach (var item in toLocal)
            {
                item.SyncTo = ToSync.ToLocal;
                this.Add(item, true);
            }

            foreach (var item in toFtp)
            {
                item.SyncTo = ToSync.ToFtp;
                this.Add(item, true);
            }
        }

    }
}
