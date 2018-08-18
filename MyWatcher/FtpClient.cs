using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Xml;
using System.Xml.Linq;


namespace MyWatcher
{
    // FTP Client class, most found on internet, customized for my needs
    public class FtpClient
    {
        #region Fields
        private string _user;
        private string _password;
        private string _host;
        private string _dir;
        private int _bufferSize = 2048;

        public bool Passive = true;
        public bool Binary = true;
        public bool EnableSsl = false;
        public bool Hash = false;

        // Number of concurent uploads, downloads
        private readonly SemaphoreSlim _semaphoreUp;
        private readonly SemaphoreSlim _semaphoreDown;

        //private string _configFile = "XML/ServiceConfig.xml";
        #endregion

        #region FtpMethods
        public FtpClient(string host, string local, string user, string password)
        {
            _host = host;
            _dir = local;
            _user = user;
            _password = password;

            _semaphoreUp = new SemaphoreSlim(10, 10);
            _semaphoreDown = new SemaphoreSlim(10, 10);
        }

        public string Host
        {
            get
            {
                return _host;
            }
        }

        public string Dir
        {
            get
            {
                return _dir;
            }
        }

        
        /// <summary>
        /// Method checks if connection is OK
        /// </summary>
        public bool CheckConnection()
        {
            try
            {
                var r = (FtpWebRequest)FtpWebRequest.Create(_host);
                r.Credentials = new NetworkCredential(_user, _password);
                r.GetResponse();
                return true;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ConnectFailure || ex.Status == WebExceptionStatus.ConnectionClosed ||
                    ex.Status == WebExceptionStatus.ServerProtocolViolation)
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Method for appending content to file
        /// <param name="source">Sourcle file</param>
        /// <param name="destination">Destination file</param>
        /// </summary>
        public void AppendFile(string source, string destination)
        {
            try
            {
                var request = createRequest(combine(_host, destination), WebRequestMethods.Ftp.AppendFile);

                using (var stream = request.GetRequestStream())
                using (var fileStream = System.IO.File.Open(source, FileMode.Open))
                {
                    int num;

                    byte[] buffer = new byte[_bufferSize];

                    while ((num = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        if (Hash)
                            Console.Write("#");

                        stream.Write(buffer, 0, num);
                    }

                }

                //return getStatusDescription(request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


        }


        /// <summary>
        /// Delete file method
        /// <param name="item">SyncQueueItem to delete</param>
        /// </summary>
        public string DeleteFile(SyncQueueItem item)
        {
            try
            {
                var request = createRequest(combine(_host, item.Name), WebRequestMethods.Ftp.DeleteFile);
                DeleteHashFile(item.Name);

                return getStatusDescription(request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

        }


        /// <summary>
        /// Download file method
        /// <param name="item">SyncQueueItem to download</param>
        /// </summary>
        public void DownloadFile(SyncQueueItem item)
        {
            try
            {
                _semaphoreDown.Wait();
                var request = createRequest(combine(_host, item.Name), WebRequestMethods.Ftp.DownloadFile);

                byte[] buffer = new byte[_bufferSize];

                using (var response = (FtpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var fs = new FileStream(combine(_dir, item.Name), FileMode.OpenOrCreate))
                {
                    int readCount = stream.Read(buffer, 0, _bufferSize);

                    while (readCount > 0)
                    {
                        fs.Write(buffer, 0, readCount);
                        readCount = stream.Read(buffer, 0, _bufferSize);
                    }
                    //return response.StatusDescription;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                _semaphoreDown.Release();
            }

        }

        /// <summary>
        /// Upload file method
        /// <param name="item">SyncQueueItem to Uplaod</param>
        /// </summary>
        public void UploadFile(SyncQueueItem item)
        {
            try
            {
                _semaphoreUp.Wait();
                var request = createRequest(combine(_host, item.Name), WebRequestMethods.Ftp.UploadFile);

                using (var stream = request.GetRequestStream())
                using (var fileStream = File.Open(combine(_dir, item.Name), FileMode.Open))
                {
                    int num;

                    byte[] buffer = new byte[_bufferSize];

                    while ((num = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        stream.Write(buffer, 0, num);
                    }
                }
                UploadHashFile(item);
                //return getStatusDescription(request);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _semaphoreUp.Release();
            }
        }

        /// <summary>
        /// Method returns list directory - list of files in directory
        /// </summary>
        public List<string> ListDirectory()
        {
            var list = new List<string>();
            try
            {
                var request = createRequest(WebRequestMethods.Ftp.ListDirectory);

                using (var response = (FtpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream, true))
                {
                    while (!reader.EndOfStream)
                    {
                        list.Add(reader.ReadLine());
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return list;
        }

        
        /// <summary>
        /// Method returns list directory details - detailed list of files in directory (size, last time used, etc.)
        /// </summary>
        public List<string> ListDirectoryDetails()
        {
            var list = new List<string>();
            try
            {
                var request = createRequest(WebRequestMethods.Ftp.ListDirectoryDetails);

                using (var response = (FtpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream, true))
                {
                    while (!reader.EndOfStream)
                    {
                        list.Add(reader.ReadLine());
                    }
                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return list;

        }

        /// <summary>
        /// Rename file method
        /// <param name="item">SyncQueueItem to Rename</param>
        /// </summary>
        public string Rename(SyncQueueItem item)
        {
            var result = "";
            try
            {
                var request = createRequest(combine(_host, item.OldName), WebRequestMethods.Ftp.Rename);

                request.RenameTo = item.Name;
                RenameHashFile(item);
                result = getStatusDescription(request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }


        /// <summary>
        /// Method to create ftp request
        /// <param name="method">method to create</param>
        /// </summary>
        private FtpWebRequest createRequest(string method)
        {
            var request = (FtpWebRequest)WebRequest.Create(_host);
            request.Method = method;
            request.Credentials = new NetworkCredential(_user, _password);

            return request;
        }

        /// <summary>
        /// Method to create ftp request
        /// <param name="uri">Uri to use</param>
        /// <param name="method">Method to create</param>
        /// </summary>
        private FtpWebRequest createRequest(string uri, string method)
        {
            var request = (FtpWebRequest)WebRequest.Create(uri);

            request.Credentials = new NetworkCredential(_user, _password);
            request.Method = method;
            request.UseBinary = Binary;
            request.EnableSsl = EnableSsl;
            request.UsePassive = Passive;

            return request;
        }

        /// <summary>
        /// Method to get response from ftp server
        /// <param name="path">Path to get response from(file)</param>
        /// </summary>
        public FtpWebResponse GetResponse(string path)
        {
            var request = createRequest(combine(_host, path), WebRequestMethods.Ftp.DownloadFile);
            return (FtpWebResponse)request.GetResponse();
        }


        /// <summary>
        /// Help method to get status description of request
        /// <param name="request">Request to get response from</param>
        /// </summary>
        private string getStatusDescription(FtpWebRequest request)
        {
            var result = "";
            try
            {
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    result = response.StatusDescription;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        /// <summary>
        /// Help method to combine path from URLs
        /// <param name="path1">Path 1 to combine</param>
        /// <param name="path2">Path 2 to combine</param>
        /// </summary>
        private string combine(string path1, string path2)
        {
            var x = Path.Combine(path1, path2).Replace("\\", "/");
            return x;
        }
        #endregion
        #region HashFile
        
        /// <summary>
        /// Returns name of hash
        /// <param name="fileName">Name of file to create hash to</param>
        /// </summary>
        private string GetHashFileName(string fileName)
        {
            var result = "";
            try
            {
                result = Path.GetFileNameWithoutExtension(fileName) + ConfigForm.ConfigManager.HASH_FILE_SUFIX;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;

        }

        /// <summary>
        /// Renames name of hash file
        /// <param name="item">Item to rename</param>
        /// </summary>
        public string RenameHashFile(SyncQueueItem item)
        {
            var result = "";
            try
            {
                var request = createRequest(combine(_host, GetHashFileName(item.OldName)), WebRequestMethods.Ftp.Rename);

                request.RenameTo = GetHashFileName(item.Name);

                result = getStatusDescription(request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        /// <summary>
        /// Deletes hash file
        /// <param name="fileName">Name of file to delete hash from</param>
        /// </summary>
        private void DeleteHashFile(string fileName)
        {
            try
            {
                var request = createRequest(combine(_host, GetHashFileName(fileName)), WebRequestMethods.Ftp.DeleteFile);
                getStatusDescription(request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        /// <summary>
        /// Creates structure of hash file, xml structure
        /// <param name="item">Item to create hash structure to</param>
        /// </summary>
        public XElement CreateHashFile(SyncQueueItem item)
        {
            var result = new XElement("File");
            try
            {
                result = new XElement("File", new XAttribute("FileName", item.Name),
                    new XAttribute("AddedOn", item.AddedOn.ToString()), new XAttribute("MD5", item.Md5));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return result;
        }

        /// <summary>
        /// Uploads hash file
        /// <param name="item">Item to uploade hash file to</param>
        /// </summary>
        public void UploadHashFile(SyncQueueItem item)
        {
            try
            {
                var request = createRequest(combine(_host, GetHashFileName(item.Name)), WebRequestMethods.Ftp.UploadFile);
                using (var s = request.GetRequestStream())
                {
                    this.CreateHashFile(item).Save(s);
                }
                getStatusDescription(request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }


        /// <summary>
        /// Returns all hash files in xml structure
        /// </summary>
        public XElement GetAllHashFiles()
        {
            XElement root = new XElement("HashFiles");

            try
            {
                // comparing specific file
                //var hashFiles = ListDirectory().Where(file => Path.GetExtension(file) == Constants.HASH_FILE_SUFIX
                //                                             &&
                //                                             Path.GetFileNameWithoutExtension(localItem.Name) + Constants.HASH_FILE_SUFIX ==
                //                                             Path.GetFileNameWithoutExtension(file) + Constants.HASH_FILE_SUFIX).ToList();

                var hashFiles =
                    ListDirectory().Where(file => Path.GetExtension(file) == ConfigForm.ConfigManager.HASH_FILE_SUFIX).ToList();

                foreach (var f in hashFiles)
                {
                    var request = createRequest(combine(_host, f), WebRequestMethods.Ftp.DownloadFile);
                    using (var response = (FtpWebResponse)request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    {
                        var elem = XElement.Load(stream);
                        root.Add(elem);
                    }
                }

            }
            catch (XmlException e)
            {
                Console.WriteLine(e.Message);
            }
            return root;
        }


        #endregion

    }
}