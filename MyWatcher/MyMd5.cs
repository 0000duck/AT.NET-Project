using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyWatcher
{
    // Class for MD5 hash, logic is loaded via reflection
    public class MyMd5
    {
        public MyMd5() { }

        /// <summary>
        /// Method for calculating md5
        /// </summary>
        public string CalcMd5(string path)
        {
            var p = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MyMD5.dll");

            var myMd5Assembly = Assembly.LoadFile(p);
            var myMd5Type = myMd5Assembly.GetType("MyMD5.MyMD5");

            var ctr = myMd5Type.GetConstructors()[0];
            var md5 = ctr.Invoke(new object[] { });

            var calcHashMethod = myMd5Type.GetMethod("CalcHash");

            var x = (string)calcHashMethod.Invoke(md5, new object[] { path });
            return (string)calcHashMethod.Invoke(md5, new object[] { path });
        }
    }
}
