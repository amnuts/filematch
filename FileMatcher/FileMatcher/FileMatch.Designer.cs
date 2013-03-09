namespace FileMatcher
{
    partial class FileMatchForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.prgMatching = new System.Windows.Forms.ProgressBar();
            this.dlogDirSelect = new System.Windows.Forms.FolderBrowserDialog();
            this.btnDirSelect = new System.Windows.Forms.Button();
            this.txtDirPath = new System.Windows.Forms.TextBox();
            this.cbkMatchRecursive = new System.Windows.Forms.CheckBox();
            this.btnFindMatches = new System.Windows.Forms.Button();
            this.treeResults = new System.Windows.Forms.TreeView();
            this.picMatch1 = new System.Windows.Forms.PictureBox();
            this.picMatch2 = new System.Windows.Forms.PictureBox();
            this.labelInfoLine = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picMatch1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMatch2)).BeginInit();
            this.SuspendLayout();
            // 
            // prgMatching
            // 
            this.prgMatching.Location = new System.Drawing.Point(622, 443);
            this.prgMatching.Name = "prgMatching";
            this.prgMatching.Size = new System.Drawing.Size(100, 17);
            this.prgMatching.Step = 1;
            this.prgMatching.TabIndex = 1;
            this.prgMatching.Visible = false;
            // 
            // btnDirSelect
            // 
            this.btnDirSelect.Location = new System.Drawing.Point(13, 13);
            this.btnDirSelect.Name = "btnDirSelect";
            this.btnDirSelect.Size = new System.Drawing.Size(86, 23);
            this.btnDirSelect.TabIndex = 2;
            this.btnDirSelect.Text = "Select a folder";
            this.btnDirSelect.UseVisualStyleBackColor = true;
            this.btnDirSelect.Click += new System.EventHandler(this.ChooseFolder);
            // 
            // txtDirPath
            // 
            this.txtDirPath.Location = new System.Drawing.Point(13, 42);
            this.txtDirPath.Name = "txtDirPath";
            this.txtDirPath.Size = new System.Drawing.Size(482, 20);
            this.txtDirPath.TabIndex = 3;
            // 
            // cbkMatchRecursive
            // 
            this.cbkMatchRecursive.AutoSize = true;
            this.cbkMatchRecursive.Location = new System.Drawing.Point(426, 17);
            this.cbkMatchRecursive.Name = "cbkMatchRecursive";
            this.cbkMatchRecursive.Size = new System.Drawing.Size(69, 17);
            this.cbkMatchRecursive.TabIndex = 4;
            this.cbkMatchRecursive.Text = "recursive";
            this.cbkMatchRecursive.UseVisualStyleBackColor = true;
            // 
            // btnFindMatches
            // 
            this.btnFindMatches.Enabled = false;
            this.btnFindMatches.Location = new System.Drawing.Point(509, 13);
            this.btnFindMatches.Name = "btnFindMatches";
            this.btnFindMatches.Size = new System.Drawing.Size(212, 49);
            this.btnFindMatches.TabIndex = 5;
            this.btnFindMatches.Text = "Find matches";
            this.btnFindMatches.UseVisualStyleBackColor = true;
            this.btnFindMatches.Click += new System.EventHandler(this.FindMatches);
            // 
            // treeResults
            // 
            this.treeResults.Enabled = false;
            this.treeResults.Location = new System.Drawing.Point(13, 69);
            this.treeResults.Name = "treeResults";
            this.treeResults.Size = new System.Drawing.Size(482, 361);
            this.treeResults.TabIndex = 6;
            // 
            // picMatch1
            // 
            this.picMatch1.Enabled = false;
            this.picMatch1.InitialImage = null;
            this.picMatch1.Location = new System.Drawing.Point(509, 69);
            this.picMatch1.Name = "picMatch1";
            this.picMatch1.Size = new System.Drawing.Size(212, 178);
            this.picMatch1.TabIndex = 7;
            this.picMatch1.TabStop = false;
            // 
            // picMatch2
            // 
            this.picMatch2.Enabled = false;
            this.picMatch2.InitialImage = null;
            this.picMatch2.Location = new System.Drawing.Point(509, 252);
            this.picMatch2.Name = "picMatch2";
            this.picMatch2.Size = new System.Drawing.Size(212, 178);
            this.picMatch2.TabIndex = 8;
            this.picMatch2.TabStop = false;
            // 
            // labelInfoLine
            // 
            this.labelInfoLine.AutoSize = true;
            this.labelInfoLine.Location = new System.Drawing.Point(11, 445);
            this.labelInfoLine.MinimumSize = new System.Drawing.Size(248, 13);
            this.labelInfoLine.Name = "labelInfoLine";
            this.labelInfoLine.Size = new System.Drawing.Size(248, 13);
            this.labelInfoLine.TabIndex = 9;
            this.labelInfoLine.Text = "Searching may take a long time - please be patient!";
            // 
            // FileMatchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(734, 472);
            this.Controls.Add(this.labelInfoLine);
            this.Controls.Add(this.picMatch2);
            this.Controls.Add(this.picMatch1);
            this.Controls.Add(this.treeResults);
            this.Controls.Add(this.btnFindMatches);
            this.Controls.Add(this.cbkMatchRecursive);
            this.Controls.Add(this.txtDirPath);
            this.Controls.Add(this.btnDirSelect);
            this.Controls.Add(this.prgMatching);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FileMatchForm";
            this.Text = "File Matcher";
            ((System.ComponentModel.ISupportInitialize)(this.picMatch1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMatch2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar prgMatching;
        private System.Windows.Forms.FolderBrowserDialog dlogDirSelect;
        private System.Windows.Forms.Button btnDirSelect;
        private System.Windows.Forms.TextBox txtDirPath;
        private System.Windows.Forms.CheckBox cbkMatchRecursive;
        private System.Windows.Forms.Button btnFindMatches;
        private System.Windows.Forms.TreeView treeResults;
        private System.Windows.Forms.PictureBox picMatch1;
        private System.Windows.Forms.PictureBox picMatch2;
        private System.Windows.Forms.Label labelInfoLine;

    }
}

