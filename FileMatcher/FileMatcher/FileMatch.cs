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
            string path = txtDirPath.Text;
            prgMatching.Value = 0;
            labelInfoLine.Text = "Searching may take a long time - please be patient!";

            try
            {
                var files = from file 
                    in Directory.EnumerateFiles(path, "*.jpg", (isRecursive 
                        ? SearchOption.AllDirectories 
                        : SearchOption.TopDirectoryOnly)
                    )
                    select new { File = file };
                int total = files.Count();
                prgMatching.Maximum = total;
                prgMatching.Visible = true;
                labelInfoLine.Text = String.Format("{0} file{1} found", total.ToString(), (total == 1 ? "" : "s"));
                if (total < 2)
                {
                    labelInfoLine.Text += " - nothing with which to compare";
                }
                else
                {
                    labelInfoLine.Text += " - processing...";
                    foreach (var f in files)
                    {
                        string md5 = ProcessFile(f.File);
                        prgMatching.PerformStep();
                    }
                    labelInfoLine.Text = String.Format("{0} files processed", total.ToString());
                }
            }
            catch (UnauthorizedAccessException UAEx)
            {
                Debug.WriteLine(UAEx.Message);
            }
            catch (PathTooLongException PathEx)
            {
                Debug.WriteLine(PathEx.Message);
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
