using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MyMD5
{
    // Class that was used for testing MD5 hash, it is not used anymore...
    public class MyMD5Class
    {
        public MyMD5Class() { }

        /// <summary>
        /// Calculates hash for specific file
        /// </summary>
        public string CalcHash(string path)
        {
            using (var md5 = MD5.Create())
            {

                using (var stream = File.OpenRead(path))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }
    }
}
