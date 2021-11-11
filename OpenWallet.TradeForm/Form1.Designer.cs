
namespace OpenWallet.TradeForm
{
    partial class Form1
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
            this.cb_Test = new System.Windows.Forms.CheckBox();
            this.lbl_qtty_to = new System.Windows.Forms.Label();
            this.lbl_qtty_from = new System.Windows.Forms.Label();
            this.rad_to = new System.Windows.Forms.RadioButton();
            this.rad_from = new System.Windows.Forms.RadioButton();
            this.nud_To = new System.Windows.Forms.NumericUpDown();
            this.nud_From = new System.Windows.Forms.NumericUpDown();
            this.lbl_currentPrice = new System.Windows.Forms.Label();
            this.bt_swap = new System.Windows.Forms.Button();
            this.cb_to = new System.Windows.Forms.ComboBox();
            this.cb_From = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.nud_To)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_From)).BeginInit();
            this.SuspendLayout();
            // 
            // lbl_couple
            // 
            this.lbl_couple.AutoSize = true;
            this.lbl_couple.Location = new System.Drawing.Point(146, 93);
            this.lbl_couple.Name = "lbl_couple";
            this.lbl_couple.Size = new System.Drawing.Size(40, 13);
            this.lbl_couple.TabIndex = 24;
            this.lbl_couple.Text = "Couple";
            // 
            // cb_Test
            // 
            this.cb_Test.AutoSize = true;
            this.cb_Test.Checked = true;
            this.cb_Test.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_Test.Location = new System.Drawing.Point(5, 2);
            this.cb_Test.Name = "cb_Test";
            this.cb_Test.Size = new System.Drawing.Size(47, 17);
            this.cb_Test.TabIndex = 23;
            this.cb_Test.Text = "Test";
            this.cb_Test.UseVisualStyleBackColor = true;
            // 
            // lbl_qtty_to
            // 
            this.lbl_qtty_to.AutoSize = true;
            this.lbl_qtty_to.Location = new System.Drawing.Point(228, 149);
            this.lbl_qtty_to.Name = "lbl_qtty_to";
            this.lbl_qtty_to.Size = new System.Drawing.Size(16, 13);
            this.lbl_qtty_to.TabIndex = 22;
            this.lbl_qtty_to.Text = "---";
            // 
            // lbl_qtty_from
            // 
            this.lbl_qtty_from.AutoSize = true;
            this.lbl_qtty_from.Location = new System.Drawing.Point(228, 30);
            this.lbl_qtty_from.Name = "lbl_qtty_from";
            this.lbl_qtty_from.Size = new System.Drawing.Size(16, 13);
            this.lbl_qtty_from.TabIndex = 21;
            this.lbl_qtty_from.Text = "---";
            // 
            // rad_to
            // 
            this.rad_to.AutoSize = true;
            this.rad_to.Location = new System.Drawing.Point(10, 167);
            this.rad_to.Name = "rad_to";
            this.rad_to.Size = new System.Drawing.Size(14, 13);
            this.rad_to.TabIndex = 20;
            this.rad_to.TabStop = true;
            this.rad_to.UseVisualStyleBackColor = true;
            this.rad_to.CheckedChanged += new System.EventHandler(this.cb_to_TextUpdate);
            // 
            // rad_from
            // 
            this.rad_from.AutoSize = true;
            this.rad_from.Checked = true;
            this.rad_from.Location = new System.Drawing.Point(10, 43);
            this.rad_from.Name = "rad_from";
            this.rad_from.Size = new System.Drawing.Size(14, 13);
            this.rad_from.TabIndex = 19;
            this.rad_from.TabStop = true;
            this.rad_from.UseVisualStyleBackColor = true;
            this.rad_from.CheckedChanged += new System.EventHandler(this.rad_from_CheckedChanged);
            // 
            // nud_To
            // 
            this.nud_To.DecimalPlaces = 5;
            this.nud_To.Enabled = false;
            this.nud_To.Location = new System.Drawing.Point(48, 181);
            this.nud_To.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.nud_To.Name = "nud_To";
            this.nud_To.Size = new System.Drawing.Size(120, 20);
            this.nud_To.TabIndex = 18;
            this.nud_To.ValueChanged += new System.EventHandler(this.nud_To_ValueChanged);
            // 
            // nud_From
            // 
            this.nud_From.DecimalPlaces = 5;
            this.nud_From.Location = new System.Drawing.Point(48, 57);
            this.nud_From.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.nud_From.Name = "nud_From";
            this.nud_From.Size = new System.Drawing.Size(120, 20);
            this.nud_From.TabIndex = 17;
            this.nud_From.ValueChanged += new System.EventHandler(this.nud_From_ValueChanged);
            // 
            // lbl_currentPrice
            // 
            this.lbl_currentPrice.AutoSize = true;
            this.lbl_currentPrice.Location = new System.Drawing.Point(146, 119);
            this.lbl_currentPrice.Name = "lbl_currentPrice";
            this.lbl_currentPrice.Size = new System.Drawing.Size(80, 13);
            this.lbl_currentPrice.TabIndex = 16;
            this.lbl_currentPrice.Text = "lbl_currentPrice";
            // 
            // bt_swap
            // 
            this.bt_swap.Location = new System.Drawing.Point(48, 90);
            this.bt_swap.Name = "bt_swap";
            this.bt_swap.Size = new System.Drawing.Size(75, 45);
            this.bt_swap.TabIndex = 15;
            this.bt_swap.Text = "SWAP";
            this.bt_swap.UseVisualStyleBackColor = true;
            this.bt_swap.Click += new System.EventHandler(this.bt_swap_Click);
            // 
            // cb_to
            // 
            this.cb_to.FormattingEnabled = true;
            this.cb_to.Location = new System.Drawing.Point(48, 146);
            this.cb_to.Name = "cb_to";
            this.cb_to.Size = new System.Drawing.Size(164, 21);
            this.cb_to.TabIndex = 14;
            this.cb_to.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.comboBox1_DrawItem);
            this.cb_to.SelectedIndexChanged += new System.EventHandler(this.cb_to_SelectedIndexChanged);
            this.cb_to.TextChanged += new System.EventHandler(this.cb_TextChanged);
            this.cb_to.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cb_KeyDown);
            this.cb_to.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cb_KeyUp);
            // 
            // cb_From
            // 
            this.cb_From.FormattingEnabled = true;
            this.cb_From.Location = new System.Drawing.Point(48, 22);
            this.cb_From.Name = "cb_From";
            this.cb_From.Size = new System.Drawing.Size(164, 21);
            this.cb_From.TabIndex = 13;
            this.cb_From.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.comboBox1_DrawItem);
            this.cb_From.SelectedIndexChanged += new System.EventHandler(this.cb_From_SelectedIndexChanged);
            this.cb_From.TextChanged += new System.EventHandler(this.cb_TextChanged);
            this.cb_From.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cb_KeyDown);
            this.cb_From.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cb_KeyUp);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(359, 219);
            this.Controls.Add(this.lbl_couple);
            this.Controls.Add(this.cb_Test);
            this.Controls.Add(this.lbl_qtty_to);
            this.Controls.Add(this.lbl_qtty_from);
            this.Controls.Add(this.rad_to);
            this.Controls.Add(this.rad_from);
            this.Controls.Add(this.nud_To);
            this.Controls.Add(this.nud_From);
            this.Controls.Add(this.lbl_currentPrice);
            this.Controls.Add(this.bt_swap);
            this.Controls.Add(this.cb_to);
            this.Controls.Add(this.cb_From);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nud_To)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_From)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_couple;
        private System.Windows.Forms.CheckBox cb_Test;
        private System.Windows.Forms.Label lbl_qtty_to;
        private System.Windows.Forms.Label lbl_qtty_from;
        private System.Windows.Forms.RadioButton rad_to;
        private System.Windows.Forms.RadioButton rad_from;
        private System.Windows.Forms.NumericUpDown nud_To;
        private System.Windows.Forms.NumericUpDown nud_From;
        private System.Windows.Forms.Label lbl_currentPrice;
        private System.Windows.Forms.Button bt_swap;
        private System.Windows.Forms.ComboBox cb_to;
        private System.Windows.Forms.ComboBox cb_From;
    }
}

