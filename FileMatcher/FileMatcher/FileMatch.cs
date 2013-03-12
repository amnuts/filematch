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
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Dictionary<string, string> hashed = new Dictionary<string, string> { };
            Dictionary<string, List<string>> grouped = new Dictionary<string, List<string>> { };

            var files = from file
                        in Directory.EnumerateFiles(txtDirPath.Text, "*.jpg", (isRecursive
                            ? SearchOption.AllDirectories
                            : SearchOption.TopDirectoryOnly)
                        )
                        orderby file.Length
                        select new { File = file };
            int total = files.Count();

            try
            {
                if (total > 2)
                {
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
                            if (grouped.ContainsKey(md5))
                            {
                                grouped[md5].Add(f.File);
                            }
                            else
                            {
                                hashed[md5] = (string)f.File;
                                grouped[md5] = new List<string>();
                            }
                            worker.ReportProgress((int)(i * 100 / total));
                            ++i;
                        }
                    }
                }
                FileMatchResults fmr = new FileMatchResults();
                fmr.hashed = hashed;
                fmr.grouped = grouped.Where(pair => pair.Value.Count() > 0)
                                 .ToDictionary(pair => pair.Key,
                                               pair => pair.Value);
                e.Result = fmr;
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

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            prgMatching.Value = e.ProgressPercentage;
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.Print("{0}", e.Result);

            FileMatchResults fmr = e.Result as FileMatchResults;

            treeResults.BeginUpdate();

            treeResults.Nodes.Clear();

            int i = 0;
            foreach (var same in fmr.grouped)
            {
                treeResults.Nodes.Add(new TreeNode(fmr.hashed[same.Key]));
                foreach (var f in fmr.grouped[same.Key])
                {
                    treeResults.Nodes[i].Nodes.Add(new TreeNode(f));
                }
                ++i;
            }

            Cursor.Current = Cursors.Default;
            treeResults.EndUpdate();
            treeResults.Enabled = true;


            foreach (var same in fmr.grouped)
            {
                Debug.Print("k = {0}", same.Key);
                foreach (var f in fmr.grouped[same.Key])
                {
                    Debug.Print("\tf = {0}", f);
                }
            }

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
                labelInfoLine.Text = String.Format("{0} files processed, {1} duplicate(s)", fmr.hashed.Count(), fmr.grouped.Count());
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
            Debug.Print("Processed file '{0}'.", path);
            return sb.ToString();
        }

    }
}
