using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWatcher
{
    // Simple enum, which is used for sync items flaging -> where to sync them
    public enum ToSync
    {
        ToLocal,
        ToFtp,
        IsIdentical
    }
}
