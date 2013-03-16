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
        private Bitmap bitmapMatch1;
        private Bitmap bitmapMatch2;

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
                                 .ToDictionary(pair => pair.Key, pair => pair.Value);
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
            FileMatchResults fmr = e.Result as FileMatchResults;

            if (e.Cancelled == true) {
                labelInfoLine.Text = "Search canceled";
            } else if (e.Error != null) {
                labelInfoLine.Text = "Error: " + e.Error.Message;
            } else {
                labelInfoLine.Text = String.Format("{0} file{1} processed, {2} duplicate{3}",
                    fmr.hashed.Count(), (fmr.hashed.Count() == 1 ? "" : "s"),
                    fmr.grouped.Count(), (fmr.grouped.Count() == 1 ? "" : "s"));
            }

            treeResults.BeginUpdate();
            treeResults.Nodes.Clear();
            if (fmr.grouped.Count() > 0)
            {
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
                treeResults.ContextMenuStrip = resultsContextMenu;
                this.Controls.Add(treeResults);
                treeResults.AfterSelect += new TreeViewEventHandler(treeResults_AfterSelect);
                Cursor.Current = Cursors.Default;
                treeResults.Enabled = true;
            }
            else
            {
                treeResults.Enabled = false;
            }
            treeResults.EndUpdate();
            prgMatching.Visible = false;
        }

        public string ProcessFile(string path)
        {
            FileStream file = new FileStream(path, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();
            file.Dispose();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            Debug.Print("Processed file '{0}'.", path);
            return sb.ToString();
        }

        private void treeResults_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeResults.SelectedNode = treeResults.GetNodeAt(e.X, e.Y);
                if (treeResults.SelectedNode != null)
                {
                    treeResults.ContextMenuStrip.Show(treeResults, e.Location);
                }
            }
        }

        private void treeResults_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (bitmapMatch1 != null)
            {
                bitmapMatch1.Dispose();
            }
            if (bitmapMatch2 != null)
            {
                bitmapMatch2.Dispose();
            }

            picMatch1.Image = null;
            picMatch2.Image = null;
            picMatchInfo1.Text = null;
            picMatchInfo2.Text = null;

            if (treeResults.SelectedNode.Level == 0)
            {
                picMatch1.SizeMode = PictureBoxSizeMode.Zoom;
                bitmapMatch1 = new Bitmap(treeResults.SelectedNode.Text);
                picMatch1.Image = (Image)bitmapMatch1;
                FileInfo fi1 = new FileInfo(treeResults.SelectedNode.Text);
                picMatchInfo1.Text = String.Format("Size: {0} / Modified: {1}", bytesToString(fi1.Length), fi1.LastWriteTime);
            }
            else
            {
                picMatch1.SizeMode = PictureBoxSizeMode.Zoom;
                bitmapMatch1 = new Bitmap(treeResults.SelectedNode.Parent.Text);
                picMatch1.Image = (Image)bitmapMatch1;
                picMatch2.SizeMode = PictureBoxSizeMode.Zoom;
                bitmapMatch2 = new Bitmap(treeResults.SelectedNode.Text);
                picMatch2.Image = (Image)bitmapMatch2;
                FileInfo fi1 = new FileInfo(treeResults.SelectedNode.Parent.Text);
                picMatchInfo1.Text = String.Format("Size: {0} / Modified: {1}", bytesToString(fi1.Length), fi1.LastWriteTime);
                FileInfo fi2 = new FileInfo(treeResults.SelectedNode.Text);
                picMatchInfo2.Text = String.Format("Size: {0} / Modified: {1}", bytesToString(fi2.Length), fi2.LastWriteTime);
            }
        }

        private static string bytesToString(double bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB" };
            int order = 0;
            while (bytes >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                bytes = bytes / 1024;
            }
            return String.Format("{0:0.##}{1}", bytes, sizes[order]);
        }

        private void openFileLocation_Click(object sender, EventArgs e)
        {
            TreeNode treeNode = this.treeResults.SelectedNode;
            if (File.Exists(treeNode.Text))
            {
                Process.Start("explorer.exe", "/select, \"" + treeNode.Text + "\"");
            }
        }

        private void deleteFile_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete that file?", "Confirm delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                TreeNode parent = this.treeResults.SelectedNode.Parent;
                this.treeResults.Nodes.Remove(this.treeResults.SelectedNode);
                if (bitmapMatch1 != null)
                {
                    bitmapMatch1.Dispose();
                }
                if (bitmapMatch2 != null)
                {
                    bitmapMatch2.Dispose();
                }

                picMatch1.Image = null;
                picMatch2.Image = null;
                picMatchInfo1.Text = null;
                picMatchInfo2.Text = null;

                /** @todo implement recycle bin delete */

                if (parent.Nodes.Count < 1)
                {
                    this.treeResults.Nodes.Remove(parent);
                }
            }
        }

    }
}
