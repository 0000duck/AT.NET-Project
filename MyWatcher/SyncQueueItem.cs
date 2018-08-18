using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System;
using System.Reflection;

namespace MyWatcher
{
    // Class that represents file, all necessary properties are here
    public class SyncQueueItem
    {
        public SyncQueueItem(string name)
        {
            Name = name;
        }

        public SyncQueueItem(string name, string oldName, string path, string oldPath, WatcherChangeTypes changeType)
        {
            Name = name;
            OldName = oldName;
            FullPath = path;
            OldFullPath = oldPath;
            ChangeType = changeType;
        }

        public SyncQueueItem(string name, string path, WatcherChangeTypes changeType)
        {
            Name = name;
            FullPath = path;
            ChangeType = changeType;
        }

        public SyncQueueItem(string name, string path)
        {
            Name = name;
            FullPath = path;
        }

        public SyncQueueItem()
        {
        }

        #region Properties

        public string Name { get; set; }

        public string OldName { get; set; }

        public string FullPath { get; set; }

        public string OldFullPath { get; set; }

        public string Md5 { get; set; }

        public WatcherChangeTypes ChangeType { get; set; }

        public DateTime AddedOn { get; set; }

        public ToSync SyncTo { get; set; }

        public decimal Size { get; set; }

        #endregion
    }
}
