using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SearchFileBySizeNet
{
    public partial class Form1 : Form
    {
        #region Private Fields
        private string _errorMessages = "";
        private List<FileData> _gridsSource = new List<FileData>();
        private int _filesCount = 0;
        #endregion

        #region Constructor
        public Form1()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Handlers
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(txtPath.Text))
            {
                MessageBox.Show("Choose existing directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (backgroundWorker1.IsBusy)
            {
                MessageBox.Show("Aleardy searching. Click stop search to stop.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            toolStripProgressBar1.Visible = true;
            _errorMessages = "";
            toolStripLblFilesCount.Text = "0";
            _filesCount = 0;
            //if path is c: or d: change it to c:\ or d:\
            if (txtPath.Text.Length == 2 && txtPath.Text.Contains(":") && !txtPath.Text.Contains("\\"))
                txtPath.Text += "\\";

            _gridsSource = new List<FileData>();
            backgroundWorker1.RunWorkerAsync(txtPath.Text);
        }

        private void btnStopSearch_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy && !backgroundWorker1.CancellationPending)
                backgroundWorker1.CancelAsync();
        }


        private void btnShowInExplorer_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select row", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            foreach (DataGridViewRow item in dataGridView1.SelectedRows)
            {
                try
                {
                    //Process.Start(Path.GetDirectoryName(item.Cells["cFilepath"].Value.ToString()));
                    string argument = @"/select, " + item.Cells["Filepath"].Value.ToString();
                    System.Diagnostics.Process.Start("explorer.exe", argument);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (_gridsSource.Count == 0)
                return;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel Documents (*.xls)|*.xls";
            saveFileDialog.FileName = "export.xls";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                ToCsV(saveFileDialog.FileName);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            GetFiles(e.Argument.ToString());
            if (_gridsSource.Count == 0)
            {
                _errorMessages += string.Format("{0} Directory is empty", e.Argument.ToString());
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _filesCount += e.ProgressPercentage;
            toolStripLblFilesCount.Text = _filesCount.ToString();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _gridsSource.Sort((a, b) => b.Filesizebytes.CompareTo(a.Filesizebytes));
            dataGridView1.DataSource = _gridsSource;
            //toolStripFilesCount.Text = _gridsSource.Count.ToString();
            toolStripProgressBar1.Visible = false;

            if (_errorMessages != "")
            {
                MessagesWindow m = new MessagesWindow(_errorMessages);
                m.Show();
            }
        }

        #endregion

        #region  Methods
        private void GetFiles(string dirPath)
        {
            string[] files;
            try
            {
                if (backgroundWorker1.CancellationPending)
                    return;

                files = Directory.GetFiles(dirPath);
                _gridsSource.AddRange(DataSourceFromStringList(files));
                backgroundWorker1.ReportProgress(files.Length);
                foreach (string item in Directory.GetDirectories(dirPath))
                {
                    GetFiles(item);
                }
            }
            catch (Exception ex)
            {
                _errorMessages += "cant access " + dirPath + "\r\n";
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Get file size string
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public string GetFileSize(string filepath)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = new FileInfo(filepath).Length;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }

        /// <summary>
        /// get grid data source from files array
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public List<FileData> DataSourceFromStringList(string[] files)
        {
            List<FileData> dataSource = new List<FileData>();
            for (int i = 0; i < files.Length; i++)
            {
                int id = i + 1;
                FileInfo fileInfo = new FileInfo(files[i]);
                FileData file = new FileData();
                file.Filepath = files[i];
                file.Filename = Path.GetFileName(files[i]);
                file.Filesizebytes = fileInfo.Length;
                file.Filesize = GetFileSize(files[i]);
                file.Id = id;
                dataSource.Add(file);
            }

            return dataSource;
        }

        private void ToCsV(string filename)
        {
            string stOutput = "";
            // Export titles:
            string sHeaders = "";

            sHeaders = Convert.ToString(dataGridView1.Columns["Filename"].HeaderText) + "\t"
            + Convert.ToString(dataGridView1.Columns["Filepath"].HeaderText) + "\t"
            + Convert.ToString(dataGridView1.Columns["Filesize"].HeaderText) + "\t";


            stOutput += sHeaders + "\r\n";

            // Export data.
            for (int i = 0; i < dataGridView1.RowCount - 1; i++)
            {
                if (!dataGridView1.Rows[i].HeaderCell.Visible)
                    continue;
                string stLine = "";

                stLine = stLine.ToString() + Convert.ToString(dataGridView1.Rows[i].Cells["Filename"].Value) + "\t"
                + stLine.ToString() + Convert.ToString(dataGridView1.Rows[i].Cells["Filepath"].Value) + "\t"
                + stLine.ToString() + Convert.ToString(dataGridView1.Rows[i].Cells["Filesize"].Value) + "\t";


                stOutput += stLine + "\r\n";
            }
            Encoding utf16 = Encoding.GetEncoding(1254);
            byte[] output = utf16.GetBytes(stOutput);
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(output, 0, output.Length); //write the encoded file
                bw.Flush();
                bw.Close();
            }
        }

        #endregion
    }
}
