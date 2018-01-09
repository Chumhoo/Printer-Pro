namespace PrinterPro
{
    partial class AddMode
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
            this.metroPanel1 = new MetroFramework.Controls.MetroPanel();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.tbFileName = new MetroFramework.Controls.MetroTextBox();
            this.btnConfirm = new MetroFramework.Controls.MetroButton();
            this.btnCancel = new MetroFramework.Controls.MetroButton();
            this.metroLabel29 = new MetroFramework.Controls.MetroLabel();
            this.metroLabel27 = new MetroFramework.Controls.MetroLabel();
            this.tbDroplets = new MetroFramework.Controls.MetroTextBox();
            this.tbRows = new MetroFramework.Controls.MetroTextBox();
            this.metroLabel28 = new MetroFramework.Controls.MetroLabel();
            this.tbColumns = new MetroFramework.Controls.MetroTextBox();
            this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
            this.metroPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // metroPanel1
            // 
            this.metroPanel1.Controls.Add(this.metroLabel2);
            this.metroPanel1.Controls.Add(this.metroLabel1);
            this.metroPanel1.Controls.Add(this.tbFileName);
            this.metroPanel1.Controls.Add(this.btnConfirm);
            this.metroPanel1.Controls.Add(this.btnCancel);
            this.metroPanel1.Controls.Add(this.metroLabel29);
            this.metroPanel1.Controls.Add(this.metroLabel27);
            this.metroPanel1.Controls.Add(this.tbDroplets);
            this.metroPanel1.Controls.Add(this.tbRows);
            this.metroPanel1.Controls.Add(this.metroLabel28);
            this.metroPanel1.Controls.Add(this.tbColumns);
            this.metroPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroPanel1.HorizontalScrollbarBarColor = true;
            this.metroPanel1.HorizontalScrollbarHighlightOnWheel = false;
            this.metroPanel1.HorizontalScrollbarSize = 10;
            this.metroPanel1.Location = new System.Drawing.Point(20, 60);
            this.metroPanel1.Name = "metroPanel1";
            this.metroPanel1.Size = new System.Drawing.Size(399, 191);
            this.metroPanel1.TabIndex = 0;
            this.metroPanel1.VerticalScrollbarBarColor = true;
            this.metroPanel1.VerticalScrollbarHighlightOnWheel = false;
            this.metroPanel1.VerticalScrollbarSize = 10;
            // 
            // metroLabel1
            // 
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.Location = new System.Drawing.Point(35, 17);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(45, 19);
            this.metroLabel1.TabIndex = 26;
            this.metroLabel1.Text = "Name";
            // 
            // tbFileName
            // 
            // 
            // 
            // 
            this.tbFileName.CustomButton.Image = null;
            this.tbFileName.CustomButton.Location = new System.Drawing.Point(211, 1);
            this.tbFileName.CustomButton.Name = "";
            this.tbFileName.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.tbFileName.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.tbFileName.CustomButton.TabIndex = 1;
            this.tbFileName.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.tbFileName.CustomButton.UseSelectable = true;
            this.tbFileName.CustomButton.Visible = false;
            this.tbFileName.Lines = new string[0];
            this.tbFileName.Location = new System.Drawing.Point(83, 17);
            this.tbFileName.MaxLength = 32767;
            this.tbFileName.Name = "tbFileName";
            this.tbFileName.PasswordChar = '\0';
            this.tbFileName.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbFileName.SelectedText = "";
            this.tbFileName.SelectionLength = 0;
            this.tbFileName.SelectionStart = 0;
            this.tbFileName.ShortcutsEnabled = true;
            this.tbFileName.Size = new System.Drawing.Size(233, 23);
            this.tbFileName.TabIndex = 25;
            this.tbFileName.UseSelectable = true;
            this.tbFileName.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.tbFileName.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(312, 155);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(71, 23);
            this.btnConfirm.TabIndex = 24;
            this.btnConfirm.Text = "Confirm";
            this.btnConfirm.UseSelectable = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(235, 155);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(71, 23);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseSelectable = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // metroLabel29
            // 
            this.metroLabel29.AutoSize = true;
            this.metroLabel29.Location = new System.Drawing.Point(35, 104);
            this.metroLabel29.Name = "metroLabel29";
            this.metroLabel29.Size = new System.Drawing.Size(116, 19);
            this.metroLabel29.TabIndex = 23;
            this.metroLabel29.Text = "Droplets Per Point";
            // 
            // metroLabel27
            // 
            this.metroLabel27.AutoSize = true;
            this.metroLabel27.Location = new System.Drawing.Point(35, 59);
            this.metroLabel27.Name = "metroLabel27";
            this.metroLabel27.Size = new System.Drawing.Size(39, 19);
            this.metroLabel27.TabIndex = 19;
            this.metroLabel27.Text = "Rows";
            // 
            // tbDroplets
            // 
            // 
            // 
            // 
            this.tbDroplets.CustomButton.Image = null;
            this.tbDroplets.CustomButton.Location = new System.Drawing.Point(151, 1);
            this.tbDroplets.CustomButton.Name = "";
            this.tbDroplets.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.tbDroplets.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.tbDroplets.CustomButton.TabIndex = 1;
            this.tbDroplets.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.tbDroplets.CustomButton.UseSelectable = true;
            this.tbDroplets.CustomButton.Visible = false;
            this.tbDroplets.Lines = new string[0];
            this.tbDroplets.Location = new System.Drawing.Point(172, 104);
            this.tbDroplets.MaxLength = 32767;
            this.tbDroplets.Name = "tbDroplets";
            this.tbDroplets.PasswordChar = '\0';
            this.tbDroplets.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbDroplets.SelectedText = "";
            this.tbDroplets.SelectionLength = 0;
            this.tbDroplets.SelectionStart = 0;
            this.tbDroplets.ShortcutsEnabled = true;
            this.tbDroplets.Size = new System.Drawing.Size(173, 23);
            this.tbDroplets.TabIndex = 22;
            this.tbDroplets.UseSelectable = true;
            this.tbDroplets.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.tbDroplets.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // tbRows
            // 
            // 
            // 
            // 
            this.tbRows.CustomButton.Image = null;
            this.tbRows.CustomButton.Location = new System.Drawing.Point(53, 1);
            this.tbRows.CustomButton.Name = "";
            this.tbRows.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.tbRows.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.tbRows.CustomButton.TabIndex = 1;
            this.tbRows.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.tbRows.CustomButton.UseSelectable = true;
            this.tbRows.CustomButton.Visible = false;
            this.tbRows.Lines = new string[0];
            this.tbRows.Location = new System.Drawing.Point(83, 58);
            this.tbRows.MaxLength = 32767;
            this.tbRows.Name = "tbRows";
            this.tbRows.PasswordChar = '\0';
            this.tbRows.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbRows.SelectedText = "";
            this.tbRows.SelectionLength = 0;
            this.tbRows.SelectionStart = 0;
            this.tbRows.ShortcutsEnabled = true;
            this.tbRows.Size = new System.Drawing.Size(75, 23);
            this.tbRows.TabIndex = 18;
            this.tbRows.UseSelectable = true;
            this.tbRows.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.tbRows.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // metroLabel28
            // 
            this.metroLabel28.AutoSize = true;
            this.metroLabel28.Location = new System.Drawing.Point(203, 60);
            this.metroLabel28.Name = "metroLabel28";
            this.metroLabel28.Size = new System.Drawing.Size(60, 19);
            this.metroLabel28.TabIndex = 21;
            this.metroLabel28.Text = "Columns";
            // 
            // tbColumns
            // 
            // 
            // 
            // 
            this.tbColumns.CustomButton.Image = null;
            this.tbColumns.CustomButton.Location = new System.Drawing.Point(53, 1);
            this.tbColumns.CustomButton.Name = "";
            this.tbColumns.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.tbColumns.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.tbColumns.CustomButton.TabIndex = 1;
            this.tbColumns.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.tbColumns.CustomButton.UseSelectable = true;
            this.tbColumns.CustomButton.Visible = false;
            this.tbColumns.Lines = new string[0];
            this.tbColumns.Location = new System.Drawing.Point(270, 59);
            this.tbColumns.MaxLength = 32767;
            this.tbColumns.Name = "tbColumns";
            this.tbColumns.PasswordChar = '\0';
            this.tbColumns.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbColumns.SelectedText = "";
            this.tbColumns.SelectionLength = 0;
            this.tbColumns.SelectionStart = 0;
            this.tbColumns.ShortcutsEnabled = true;
            this.tbColumns.Size = new System.Drawing.Size(75, 23);
            this.tbColumns.TabIndex = 20;
            this.tbColumns.UseSelectable = true;
            this.tbColumns.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.tbColumns.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // metroLabel2
            // 
            this.metroLabel2.AutoSize = true;
            this.metroLabel2.Location = new System.Drawing.Point(316, 18);
            this.metroLabel2.Name = "metroLabel2";
            this.metroLabel2.Size = new System.Drawing.Size(29, 19);
            this.metroLabel2.TabIndex = 27;
            this.metroLabel2.Text = ".csv";
            // 
            // AddMode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(439, 271);
            this.Controls.Add(this.metroPanel1);
            this.Name = "AddMode";
            this.Resizable = false;
            this.Text = "AddMode";
            this.metroPanel1.ResumeLayout(false);
            this.metroPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroPanel metroPanel1;
        private MetroFramework.Controls.MetroButton btnConfirm;
        private MetroFramework.Controls.MetroButton btnCancel;
        private MetroFramework.Controls.MetroLabel metroLabel29;
        private MetroFramework.Controls.MetroLabel metroLabel27;
        private MetroFramework.Controls.MetroTextBox tbDroplets;
        private MetroFramework.Controls.MetroTextBox tbRows;
        private MetroFramework.Controls.MetroLabel metroLabel28;
        private MetroFramework.Controls.MetroTextBox tbColumns;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private MetroFramework.Controls.MetroTextBox tbFileName;
        private MetroFramework.Controls.MetroLabel metroLabel2;
    }
}