using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ConfigForm
{
    public partial class Form1 : Form
    {
        private ConnectionForm _form;
        public Form1()
        {
            InitializeComponent();
            ConfigManager.Load();
            PutValues();
            _form = new ConnectionForm();
        }


        public void PutValues()
        {
            try
            {
                textBox1.Text = ConfigManager.IndexInterval.ToString();
                comboBox1.DataSource = ConfigManager.Connections;

                comboBox1.DisplayMember = "Host";
                comboBox1.ValueMember = "Host";
                comboBox1.SelectedIndex = 0;

            }
            catch (Exception)
            {

                throw;
            }

        }

        private void GetValues()
        {
            try
            {
                ConfigManager.IndexInterval = Double.Parse(textBox1.Text);
            }
            catch (Exception)
            {

                throw;
            }

        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            GetValues();
            ConfigManager.Save();
            ConfigManager.SelectedConnection = (ConnectionPair) comboBox1.SelectedItem;
            Close();

        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newConn = new ConnectionPair();
            ConfigManager.Connections.Add(newConn);
            _form = new ConnectionForm(newConn, this);
            _form.Show();
            PutValues();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var x = comboBox1.SelectedItem;
            _form = new ConnectionForm(((ConnectionPair) comboBox1.SelectedItem), this);
            _form.Show();
        }
     
    }
}
