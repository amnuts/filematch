using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;

namespace FileMatcher
{
    public partial class FileMatchForm : Form
    {
        private bool isRecursive;

        public FileMatchForm()
        {
            InitializeComponent();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
        }

        private void ChooseFolder(object sender, EventArgs e)
        {
            if (dlogDirSelect.ShowDialog() == DialogResult.OK)
            {
                txtDirPath.Text = dlogDirSelect.SelectedPath;
                btnFindMatches.Enabled = true;
            }
        }

        private void FindMatches(object sender, EventArgs e)
        {
            isRecursive = cbkMatchRecursive.Checked;
            prgMatching.Maximum = 100;
            prgMatching.Value = 0;
            prgMatching.Visible = true;
            labelInfoLine.Text = "Searching may take a long time - please be patient!";


            if (bgWorker.IsBusy != true)
            {
                bgWorker.RunWorkerAsync();
            }



            /*
            if (Directory.Exists(path))
            {
                // This path is a directory
                ProcessDirectory(path);
            }
            else
            {
                Debug.WriteLine("{0} is not a valid file or directory.", txtDirPath);
            }
            */
        }

        /*
        public void ProcessDirectory(string targetDirectory)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                ProcessFile(fileName);
            }

            if (isRecursive)
            {
                string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
                foreach (string subdirectory in subdirectoryEntries)
                {
                    ProcessDirectory(subdirectory);
                }
            }
        }
        */

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            var files = from file
                        in Directory.EnumerateFiles(txtDirPath.Text, "*.jpg", (isRecursive
                            ? SearchOption.AllDirectories
                            : SearchOption.TopDirectoryOnly)
                        )
                        select new { File = file };
            int total = files.Count();

            try
            {
                //labelInfoLine.Text = String.Format("{0} file{1} found", total.ToString(), (total == 1 ? "" : "s"));
                if (total < 2)
                {
                    //labelInfoLine.Text += " - nothing with which to compare";
                }
                else
                {
                    //labelInfoLine.Text += " - processing...";
                    int i = 0;
                    e.Result = 0;
                    foreach (var f in files)
                    {
                        if (worker.CancellationPending == true)
                        {
                            e.Cancel = true;
                            break;
                        }
                        else
                        {
                            string md5 = ProcessFile(f.File);
                            worker.ReportProgress((int)(i * 100 / total));
                            ++i;
                        }
                    }
                }
                e.Result = total;
            }
            catch (UnauthorizedAccessException UAEx)
            {
                Debug.WriteLine(UAEx.Message);
            }
            catch (PathTooLongException PathEx)
            {
                Debug.WriteLine(PathEx.Message);
            }
        }

        // This event handler updates the progress. 
        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            prgMatching.Value = e.ProgressPercentage;
        }

        // This event handler deals with the results of the background operation. 
        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            prgMatching.Visible = false;
            if (e.Cancelled == true)
            {
                labelInfoLine.Text = "Canceled!";
            }
            else if (e.Error != null)
            {
                labelInfoLine.Text = "Error: " + e.Error.Message;
            }
            else
            {
                labelInfoLine.Text = String.Format("{0} files processed", e.Result.ToString());
            }
        }

        public string ProcessFile(string path)
        {
            FileStream file = new FileStream(path, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            Debug.WriteLine("Processed file '{0}'.", path);
            return sb.ToString();
        }

    }
}
