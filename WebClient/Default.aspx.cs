using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MyWatcher;

namespace FtpViewer
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        
        /// <summary>
        /// Method for handling all files in ftp directories, and shows them on webpage
        /// </summary>
        public void BindFtp()
        {
            try
            {
                var dtFiles = new DataTable();
                dtFiles.Columns.AddRange(new DataColumn[3] { new DataColumn("Name", typeof(string)), new DataColumn("Last modified", typeof(DateTime)), new DataColumn("Size", typeof(decimal)) });

                var files = new FtpClient(tbFtpHost.Text, "", tbUser.Text, tbPassword.Text).ListDirectoryDetails();
                
                // Regex for parsing only info We need from direftorydetails
                Regex regex = new Regex(@"^([d-])([rwxt-]{3}){3}\s+\d{1,}\s+.*?(\d{1,})\s+(\w+\s+\d{1,2}\s+(?:\d{4})?)(\d{1,2}:\d{2})?\s+(.+?)\s?$",
    RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

                // Handle all files
                foreach (var f in files)
                {
                    var size = Decimal.Parse(regex.Match(f).Groups[3].Value);
                    var lastModified = DateTime.Parse(regex.Match(f).Groups[4].Value);
                    var name = regex.Match(f).Groups[6].Value;
                    var extension = Path.GetExtension(name);
                    if (extension == ConfigForm.ConfigManager.HASH_FILE_SUFIX || extension == "") continue;
                    dtFiles.Rows.Add(name, lastModified, size);
                }

                ftpFiles.DataSource = dtFiles;
                ftpFiles.DataBind();
            }
            catch (WebException ex)
            {
                throw new Exception((ex.Response as FtpWebResponse).StatusDescription);
            }
        }

       
        /// <summary>
        /// Method for downloading file from web client
        /// </summary>
        protected void DownloadFile(object sender, EventArgs e)
        {
            string fileName = (sender as LinkButton).CommandArgument;

            try
            {
                var c = new FtpClient(tbFtpHost.Text, "", tbUser.Text, tbPassword.Text);
                
                using (var response = c.GetResponse(fileName))
                using (MemoryStream stream = new MemoryStream())
                {
                    response.GetResponseStream().CopyTo(stream);
                    Response.AddHeader("content-disposition", "attachment;filename=" + fileName);
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.BinaryWrite(stream.ToArray());
                    Response.End();
                }
            }
            catch (WebException ex)
            {
                throw new Exception((ex.Response as FtpWebResponse).StatusDescription);
            }
        }

        protected void btnLoadFtp_Click(object sender, EventArgs e)
        {
            this.BindFtp();
        }

    }
}