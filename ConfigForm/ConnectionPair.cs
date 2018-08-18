using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigForm
{
    // Class representing connection, host, local indexing directory etc.
    public class ConnectionPair
    {
        private string _host;
        private string _localDir;
        public string User { get; set; }
        public string Password { get; set; }

        public ConnectionPair()
        {
            
        }

        public ConnectionPair(string host, string local, string user, string pass)
        {
            _host = host;
            _localDir = local;
            User = user;
            Password = pass;
        }

        public string Host
        {
            get { return _host; }
            set { _host = @"" + value; } 
        }

        public string LocalDir
        {
            get
            {
                return _localDir;
            }
            set { _localDir = @"" + value; }
        }


        public ConnectionPair(string host, string dir)
        {
            Host = @""+host;
            LocalDir = @""+dir;
        }
    }
}
