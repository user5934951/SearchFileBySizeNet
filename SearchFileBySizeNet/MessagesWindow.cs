using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SearchFileBySizeNet
{
    public partial class MessagesWindow : Form
    {
        string _messageToShow;
        public MessagesWindow(string message)
        {
            InitializeComponent();
            _messageToShow = message;
        }

        private void MessagesWindow_Load(object sender, EventArgs e)
        {
            txtMain.Text = _messageToShow;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
