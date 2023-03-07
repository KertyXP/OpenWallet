namespace OpenWallet.WinForm
{
    partial class CoinSelector
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
            this.lbl_couple = new System.Windows.Forms.Label();
            this.cb_to = new System.Windows.Forms.ComboBox();
            this.cb_From = new System.Windows.Forms.ComboBox();
            this.bt_ok = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbl_couple
            // 
            this.lbl_couple.AutoSize = true;
            this.lbl_couple.Location = new System.Drawing.Point(56, 63);
            this.lbl_couple.Name = "lbl_couple";
            this.lbl_couple.Size = new System.Drawing.Size(40, 13);
            this.lbl_couple.TabIndex = 27;
            this.lbl_couple.Text = "Couple";
            // 
            // cb_to
            // 
            this.cb_to.FormattingEnabled = true;
            this.cb_to.Location = new System.Drawing.Point(12, 39);
            this.cb_to.Name = "cb_to";
            this.cb_to.Size = new System.Drawing.Size(164, 21);
            this.cb_to.TabIndex = 26;
            this.cb_to.SelectedIndexChanged += new System.EventHandler(this.cb_to_SelectedIndexChanged);
            // 
            // cb_From
            // 
            this.cb_From.FormattingEnabled = true;
            this.cb_From.Location = new System.Drawing.Point(12, 12);
            this.cb_From.Name = "cb_From";
            this.cb_From.Size = new System.Drawing.Size(164, 21);
            this.cb_From.TabIndex = 25;
            this.cb_From.SelectedIndexChanged += new System.EventHandler(this.cb_From_SelectedIndexChanged);
            // 
            // bt_ok
            // 
            this.bt_ok.Location = new System.Drawing.Point(39, 88);
            this.bt_ok.Name = "bt_ok";
            this.bt_ok.Size = new System.Drawing.Size(75, 45);
            this.bt_ok.TabIndex = 28;
            this.bt_ok.Text = "OK";
            this.bt_ok.UseVisualStyleBackColor = true;
            this.bt_ok.Click += new System.EventHandler(this.bt_ok_Click);
            // 
            // CoinSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(191, 151);
            this.Controls.Add(this.bt_ok);
            this.Controls.Add(this.lbl_couple);
            this.Controls.Add(this.cb_to);
            this.Controls.Add(this.cb_From);
            this.Name = "CoinSelector";
            this.Text = "CoinSelector";
            this.Load += new System.EventHandler(this.CoinSelector_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_couple;
        private System.Windows.Forms.ComboBox cb_to;
        private System.Windows.Forms.ComboBox cb_From;
        private System.Windows.Forms.Button bt_ok;
    }
}