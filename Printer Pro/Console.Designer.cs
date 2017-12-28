namespace PrinterPro
{
    partial class Console
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
                //components.Dispose();
            }
            //base.Dispose(disposing);
            base.Hide();
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Console));
            this.MotorX = new AxMG17MotorLib.AxMG17Motor();
            this.MotorY = new AxMG17MotorLib.AxMG17Motor();
            this.MotorZ = new AxMG17MotorLib.AxMG17Motor();
            ((System.ComponentModel.ISupportInitialize)(this.MotorX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MotorY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MotorZ)).BeginInit();
            this.SuspendLayout();
            // 
            // MotorX
            // 
            this.MotorX.Enabled = true;
            this.MotorX.Location = new System.Drawing.Point(23, 63);
            this.MotorX.Name = "MotorX";
            this.MotorX.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("MotorX.OcxState")));
            this.MotorX.Size = new System.Drawing.Size(283, 195);
            this.MotorX.TabIndex = 3;
            // 
            // MotorY
            // 
            this.MotorY.Enabled = true;
            this.MotorY.Location = new System.Drawing.Point(312, 63);
            this.MotorY.Name = "MotorY";
            this.MotorY.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("MotorY.OcxState")));
            this.MotorY.Size = new System.Drawing.Size(283, 195);
            this.MotorY.TabIndex = 4;
            // 
            // MotorZ
            // 
            this.MotorZ.Enabled = true;
            this.MotorZ.Location = new System.Drawing.Point(601, 63);
            this.MotorZ.Name = "MotorZ";
            this.MotorZ.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("MotorZ.OcxState")));
            this.MotorZ.Size = new System.Drawing.Size(283, 195);
            this.MotorZ.TabIndex = 5;
            // 
            // Console
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(907, 279);
            this.Controls.Add(this.MotorZ);
            this.Controls.Add(this.MotorY);
            this.Controls.Add(this.MotorX);
            this.Name = "Console";
            this.Resizable = false;
            this.Text = "Console";
            ((System.ComponentModel.ISupportInitialize)(this.MotorX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MotorY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MotorZ)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public AxMG17MotorLib.AxMG17Motor MotorX;
        public AxMG17MotorLib.AxMG17Motor MotorY;
        public AxMG17MotorLib.AxMG17Motor MotorZ;
    }
}