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
using Microsoft.VisualBasic.FileIO;

namespace FileMatcher
{
    public partial class FileMatchForm : Form
    {
        private bool isRecursive;
        private Bitmap bitmapMatch1;
        private Bitmap bitmapMatch2;
        private int totalMatch;

        public FileMatchForm()
        {
            InitializeComponent();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
        }

        /**
         * Handles selecting a directory path
         */
        private void chooseFolder_Click(object sender, EventArgs e)
        {
            if (dlogDirSelect.ShowDialog() == DialogResult.OK)
            {
                txtDirPath.Text = dlogDirSelect.SelectedPath;
                btnFindMatches.Enabled = true;
            }
        }

        /**
         * Starts the search process, placing the main bulk of the processing
         * in to a background process.
         */
        private void findMatches_Click(object sender, EventArgs e)
        {
            cleanupBitmaps();
            if (treeResults.Nodes.Count > 0)
            {
                treeResults.BeginUpdate();
                treeResults.Nodes.Clear();
                treeResults.Enabled = false;
                treeResults.EndUpdate();
            }
            totalMatch = 0;
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

        /**
         * The background process thread.
         */
        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Dictionary<string, string> hashed = new Dictionary<string, string> { };
            Dictionary<string, List<string>> grouped = new Dictionary<string, List<string>> { };
            List<string> failed = new List<string> { };

            var files = from file
                        in Directory.EnumerateFiles(txtDirPath.Text, "*.jpg", (isRecursive
                            ? System.IO.SearchOption.AllDirectories
                            : System.IO.SearchOption.TopDirectoryOnly)
                        )
                        orderby file.Length
                        select new { File = file };
            totalMatch = files.Count();

            try
            {
                if (totalMatch > 2)
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
                            string md5 = generateFileHash(f.File);
                            if (md5 == "") {
                                failed.Add(f.File);
                            } else {
                                if (grouped.ContainsKey(md5))
                                {
                                    grouped[md5].Add(f.File);
                                }
                                else
                                {
                                    hashed[md5] = (string)f.File;
                                    grouped[md5] = new List<string>();
                                }
                            }
                            worker.ReportProgress(i);
                            ++i;
                        }
                    }
                }
                FileMatchResults fmr = new FileMatchResults();
                fmr.failed = failed;
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

        /**
         * Change the information and progress bar when the background process
         * makes an update.
         */
        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            prgMatching.Value = (int)(e.ProgressPercentage * 100 / totalMatch);
            labelInfoLine.Text = String.Format("Attempting to process file {0} out of {1}",
                    e.ProgressPercentage, totalMatch);
        }

        /**
         * Handle the building of the results tree when the background process
         * has gathered all required information.
         */
        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            FileMatchResults fmr = e.Result as FileMatchResults;

            if (e.Cancelled == true) {
                labelInfoLine.Text = "Search canceled";
            } else if (e.Error != null) {
                labelInfoLine.Text = "Error: " + e.Error.Message;
            } else {
                labelInfoLine.Text = String.Format("{0} unique file{1}, {2} duplicate{3}",
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

        /**
         * Handle clicking on tree view nodes.
         */
        private void treeResults_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeResults.SelectedNode = treeResults.GetNodeAt(e.X, e.Y);
                if (treeResults.SelectedNode != null)
                {
                    //deleteMenuItem.Enabled = (treeResults.SelectedNode.Level > 0);
                    treeResults.ContextMenuStrip.Show(treeResults, e.Location);
                }
            }
        }

        /**
         * Handle selection of tree view nodes.
         */
        private void treeResults_AfterSelect(object sender, TreeViewEventArgs e)
        {
            cleanupBitmaps();

            if (treeResults.SelectedNode.Level == 0)
            {
                picMatch1.SizeMode = PictureBoxSizeMode.Zoom;
                picMatch1.Image = Image.FromFile(treeResults.SelectedNode.Text);
                FileInfo fi1 = new FileInfo(treeResults.SelectedNode.Text);
                picMatchInfo1.Text = String.Format("Size: {0} / Modified: {1}", bytesToString(fi1.Length), fi1.LastWriteTime);
            }
            else
            {
                picMatch1.SizeMode = PictureBoxSizeMode.Zoom;
                picMatch2.SizeMode = PictureBoxSizeMode.Zoom;
                picMatch1.Image = Image.FromFile(treeResults.SelectedNode.Text);
                picMatch2.Image = Image.FromFile(treeResults.SelectedNode.Text);
                FileInfo fi1 = new FileInfo(treeResults.SelectedNode.Parent.Text);
                picMatchInfo1.Text = String.Format("Size: {0} / Modified: {1}", bytesToString(fi1.Length), fi1.LastWriteTime);
                FileInfo fi2 = new FileInfo(treeResults.SelectedNode.Text);
                picMatchInfo2.Text = String.Format("Size: {0} / Modified: {1}", bytesToString(fi2.Length), fi2.LastWriteTime);
            }
        }

        /**
         * Open a file from the context menu
         */
        private void openFile_ContextClick(object sender, EventArgs e)
        {
            TreeNode treeNode = this.treeResults.SelectedNode;
            if (File.Exists(treeNode.Text))
            {
                Process.Start("explorer.exe", "/select, \"" + treeNode.Text + "\"");
            }
        }

        /**
         * Delete a file from the context menu
         */
        private void deleteFile_ContextClick(object sender, EventArgs e)
        {
            try
            {
                cleanupBitmaps();
                FileSystem.DeleteFile(
                    treeResults.SelectedNode.Text,
                    UIOption.AllDialogs,
                    RecycleOption.SendToRecycleBin,
                    UICancelOption.ThrowException
                );
                if (treeResults.SelectedNode.Level > 0)
                {
                    TreeNode parent = treeResults.SelectedNode.Parent;
                    treeResults.Nodes.Remove(treeResults.SelectedNode);
                    if (parent.Nodes.Count < 1)
                    {
                        treeResults.Nodes.Remove(parent);
                    }
                }
                else
                {
                    treeResults.Nodes.Remove(treeResults.SelectedNode);
                }
            }
            catch (OperationCanceledException ex) { }
        }

        /**
         * Read the given file (if possible) and generate an MD5 hash of the
         * file contents.  Returns an empty string if the hash cannot be 
         * generated for some reason.
         */
        private string generateFileHash(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    FileStream file = new FileStream(path, FileMode.Open);
                    MD5 md5 = new MD5CryptoServiceProvider();
                    byte[] retVal = md5.ComputeHash(file);
                    file.Close();
                    file = null;

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < retVal.Length; i++)
                    {
                        sb.Append(retVal[i].ToString("x2"));
                    }
                    Debug.Print("Processed file '{0}'.", path);
                    return sb.ToString();
                }
                else
                {
                    return "";
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return "";
            }
        }

        /**
         * Clean up bitmap images and related labels
         */
        private void cleanupBitmaps()
        {
            if (bitmapMatch1 != null)
            {
                bitmapMatch1.Dispose();
                bitmapMatch1 = null;
            }
            if (bitmapMatch2 != null)
            {
                bitmapMatch2.Dispose();
                bitmapMatch2 = null;
            }
            if (picMatch1.Image != null) {
                picMatch1.Image.Dispose();
                picMatch1.Image = null;
            }
            if (picMatch2.Image != null)
            {
                picMatch2.Image.Dispose();
                picMatch2.Image = null;
            }
            picMatchInfo1.Text = null;
            picMatchInfo2.Text = null;
        }

        /**
         * Convert byte number to a more readable string
         */
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
    }
}
