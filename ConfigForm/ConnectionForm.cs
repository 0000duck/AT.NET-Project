using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConfigForm
{
    public partial class ConnectionForm : Form
    {
        public ConnectionPair SelectedConnectionPair { get; set; }
        private FolderBrowserDialog _fbd;
        public List<ConnectionPair> ConnectionPairs { get; set; }
        private Form1 _rootForm;
        public ConnectionForm(ConnectionPair pair, Form1 f)
        {
            InitializeComponent();
            SelectedConnectionPair = pair;
            _fbd = new FolderBrowserDialog();
            textBox1.Text = SelectedConnectionPair.Host;
            textBox2.Text = SelectedConnectionPair.LocalDir;
            _rootForm = f;
        }

        public ConnectionForm()
        {
            InitializeComponent();
            _fbd = new FolderBrowserDialog();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            DialogResult result = _fbd.ShowDialog();
            if (_fbd.SelectedPath == null)
                _fbd.SelectedPath = textBox2.Text;
            else textBox2.Text = @"" + _fbd.SelectedPath;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            SelectedConnectionPair.Host = "ftp://" + textBox1.Text + "//";
            if (_fbd.SelectedPath == null)
                _fbd.SelectedPath = textBox2.Text;
            else SelectedConnectionPair.LocalDir = @""+_fbd.SelectedPath + "\\";
            SelectedConnectionPair.Password = textBox4.Text;
            SelectedConnectionPair.User = textBox3.Text;
            ConfigManager.Save();
            ConfigManager.Load();
            _rootForm.PutValues();
            Close();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ConfigManager.Connections.Remove(SelectedConnectionPair);
            ConfigManager.Save();
            ConfigManager.Load();
            Close();
            _rootForm.PutValues();
        }
    }
}
