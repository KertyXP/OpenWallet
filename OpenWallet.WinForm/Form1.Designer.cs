namespace OpenWallet.WinForm
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.dgv_trade_day = new System.Windows.Forms.DataGridView();
            this.obj2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Back = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgv_Balance = new System.Windows.Forms.DataGridView();
            this.obj = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Exchange = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Crypto = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Count = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ValueBtc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CustomValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dgv_Trades = new System.Windows.Forms.DataGridView();
            this.obj1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Couple = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tb_3 = new System.Windows.Forms.TabPage();
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
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_trade_day)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Balance)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Trades)).BeginInit();
            this.tb_3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_To)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_From)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tb_3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1076, 450);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tableLayoutPanel1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1068, 424);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tableLayoutPanel1.Controls.Add(this.dgv_trade_day, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dgv_Balance, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1062, 418);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // dgv_trade_day
            // 
            this.dgv_trade_day.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_trade_day.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.obj2,
            this.dataGridViewTextBoxColumn6,
            this.dataGridViewTextBoxColumn7,
            this.dataGridViewTextBoxColumn8,
            this.dataGridViewTextBoxColumn9,
            this.dataGridViewTextBoxColumn10,
            this.dataGridViewTextBoxColumn11,
            this.Date,
            this.Back});
            this.dgv_trade_day.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_trade_day.Location = new System.Drawing.Point(480, 3);
            this.dgv_trade_day.Name = "dgv_trade_day";
            this.dgv_trade_day.Size = new System.Drawing.Size(579, 412);
            this.dgv_trade_day.TabIndex = 3;
            // 
            // obj2
            // 
            this.obj2.HeaderText = "obj2";
            this.obj2.Name = "obj2";
            this.obj2.Visible = false;
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.HeaderText = "Exchange";
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            this.dataGridViewTextBoxColumn6.Visible = false;
            // 
            // dataGridViewTextBoxColumn7
            // 
            this.dataGridViewTextBoxColumn7.HeaderText = "Couple";
            this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            this.dataGridViewTextBoxColumn7.Visible = false;
            // 
            // dataGridViewTextBoxColumn8
            // 
            this.dataGridViewTextBoxColumn8.HeaderText = "CryptoFrom";
            this.dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
            // 
            // dataGridViewTextBoxColumn9
            // 
            this.dataGridViewTextBoxColumn9.HeaderText = "CountFrom";
            this.dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
            // 
            // dataGridViewTextBoxColumn10
            // 
            this.dataGridViewTextBoxColumn10.HeaderText = "CryptoTo";
            this.dataGridViewTextBoxColumn10.Name = "dataGridViewTextBoxColumn10";
            // 
            // dataGridViewTextBoxColumn11
            // 
            this.dataGridViewTextBoxColumn11.HeaderText = "ValueTo";
            this.dataGridViewTextBoxColumn11.Name = "dataGridViewTextBoxColumn11";
            // 
            // Date
            // 
            this.Date.HeaderText = "Date";
            this.Date.Name = "Date";
            // 
            // Back
            // 
            this.Back.HeaderText = "Back";
            this.Back.Name = "Back";
            // 
            // dgv_Balance
            // 
            this.dgv_Balance.AllowUserToAddRows = false;
            this.dgv_Balance.AllowUserToDeleteRows = false;
            this.dgv_Balance.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_Balance.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.obj,
            this.Exchange,
            this.Crypto,
            this.Count,
            this.ValueBtc,
            this.CustomValue});
            this.dgv_Balance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_Balance.Location = new System.Drawing.Point(3, 3);
            this.dgv_Balance.MultiSelect = false;
            this.dgv_Balance.Name = "dgv_Balance";
            this.dgv_Balance.ReadOnly = true;
            this.dgv_Balance.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_Balance.Size = new System.Drawing.Size(471, 412);
            this.dgv_Balance.TabIndex = 1;
            this.dgv_Balance.SelectionChanged += new System.EventHandler(this.dataGridView1_SelectionChanged);
            // 
            // obj
            // 
            this.obj.HeaderText = "obj";
            this.obj.Name = "obj";
            this.obj.ReadOnly = true;
            this.obj.Visible = false;
            // 
            // Exchange
            // 
            this.Exchange.HeaderText = "Exchange";
            this.Exchange.Name = "Exchange";
            this.Exchange.ReadOnly = true;
            // 
            // Crypto
            // 
            this.Crypto.HeaderText = "Crypto";
            this.Crypto.Name = "Crypto";
            this.Crypto.ReadOnly = true;
            // 
            // Count
            // 
            this.Count.HeaderText = "Count";
            this.Count.Name = "Count";
            this.Count.ReadOnly = true;
            // 
            // ValueBtc
            // 
            this.ValueBtc.HeaderText = "Value Btc";
            this.ValueBtc.Name = "ValueBtc";
            this.ValueBtc.ReadOnly = true;
            // 
            // CustomValue
            // 
            this.CustomValue.HeaderText = "CustomValue";
            this.CustomValue.Name = "CustomValue";
            this.CustomValue.ReadOnly = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dgv_Trades);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1068, 424);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dgv_Trades
            // 
            this.dgv_Trades.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_Trades.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.obj1,
            this.dataGridViewTextBoxColumn1,
            this.Couple,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewTextBoxColumn5});
            this.dgv_Trades.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_Trades.Location = new System.Drawing.Point(3, 3);
            this.dgv_Trades.Name = "dgv_Trades";
            this.dgv_Trades.Size = new System.Drawing.Size(1062, 418);
            this.dgv_Trades.TabIndex = 3;
            // 
            // obj1
            // 
            this.obj1.HeaderText = "obj1";
            this.obj1.Name = "obj1";
            this.obj1.Visible = false;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "Exchange";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // Couple
            // 
            this.Couple.HeaderText = "Couple";
            this.Couple.Name = "Couple";
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "CryptoFrom";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.HeaderText = "CountFrom";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.HeaderText = "CryptoTo";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.HeaderText = "ValueTo";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            // 
            // tb_3
            // 
            this.tb_3.Controls.Add(this.lbl_qtty_to);
            this.tb_3.Controls.Add(this.lbl_qtty_from);
            this.tb_3.Controls.Add(this.rad_to);
            this.tb_3.Controls.Add(this.rad_from);
            this.tb_3.Controls.Add(this.nud_To);
            this.tb_3.Controls.Add(this.nud_From);
            this.tb_3.Controls.Add(this.lbl_currentPrice);
            this.tb_3.Controls.Add(this.bt_swap);
            this.tb_3.Controls.Add(this.cb_to);
            this.tb_3.Controls.Add(this.cb_From);
            this.tb_3.Location = new System.Drawing.Point(4, 22);
            this.tb_3.Name = "tb_3";
            this.tb_3.Padding = new System.Windows.Forms.Padding(3);
            this.tb_3.Size = new System.Drawing.Size(1068, 424);
            this.tb_3.TabIndex = 2;
            this.tb_3.Text = "---";
            this.tb_3.UseVisualStyleBackColor = true;
            // 
            // lbl_qtty_to
            // 
            this.lbl_qtty_to.AutoSize = true;
            this.lbl_qtty_to.Location = new System.Drawing.Point(314, 176);
            this.lbl_qtty_to.Name = "lbl_qtty_to";
            this.lbl_qtty_to.Size = new System.Drawing.Size(16, 13);
            this.lbl_qtty_to.TabIndex = 10;
            this.lbl_qtty_to.Text = "---";
            // 
            // lbl_qtty_from
            // 
            this.lbl_qtty_from.AutoSize = true;
            this.lbl_qtty_from.Location = new System.Drawing.Point(81, 176);
            this.lbl_qtty_from.Name = "lbl_qtty_from";
            this.lbl_qtty_from.Size = new System.Drawing.Size(16, 13);
            this.lbl_qtty_from.TabIndex = 9;
            this.lbl_qtty_from.Text = "---";
            // 
            // rad_to
            // 
            this.rad_to.AutoSize = true;
            this.rad_to.Location = new System.Drawing.Point(314, 206);
            this.rad_to.Name = "rad_to";
            this.rad_to.Size = new System.Drawing.Size(43, 17);
            this.rad_to.TabIndex = 8;
            this.rad_to.TabStop = true;
            this.rad_to.Text = "Buy";
            this.rad_to.UseVisualStyleBackColor = true;
            this.rad_to.CheckedChanged += new System.EventHandler(this.rad_to_CheckedChanged);
            // 
            // rad_from
            // 
            this.rad_from.AutoSize = true;
            this.rad_from.Checked = true;
            this.rad_from.Location = new System.Drawing.Point(81, 206);
            this.rad_from.Name = "rad_from";
            this.rad_from.Size = new System.Drawing.Size(43, 17);
            this.rad_from.TabIndex = 7;
            this.rad_from.TabStop = true;
            this.rad_from.Text = "Buy";
            this.rad_from.UseVisualStyleBackColor = true;
            this.rad_from.CheckedChanged += new System.EventHandler(this.rad_from_CheckedChanged);
            // 
            // nud_To
            // 
            this.nud_To.DecimalPlaces = 5;
            this.nud_To.Enabled = false;
            this.nud_To.Location = new System.Drawing.Point(314, 153);
            this.nud_To.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.nud_To.Name = "nud_To";
            this.nud_To.Size = new System.Drawing.Size(120, 20);
            this.nud_To.TabIndex = 6;
            this.nud_To.ValueChanged += new System.EventHandler(this.nud_To_ValueChanged);
            // 
            // nud_From
            // 
            this.nud_From.DecimalPlaces = 5;
            this.nud_From.Location = new System.Drawing.Point(81, 153);
            this.nud_From.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.nud_From.Name = "nud_From";
            this.nud_From.Size = new System.Drawing.Size(120, 20);
            this.nud_From.TabIndex = 5;
            this.nud_From.ValueChanged += new System.EventHandler(this.nud_From_ValueChanged);
            // 
            // lbl_currentPrice
            // 
            this.lbl_currentPrice.AutoSize = true;
            this.lbl_currentPrice.Location = new System.Drawing.Point(480, 122);
            this.lbl_currentPrice.Name = "lbl_currentPrice";
            this.lbl_currentPrice.Size = new System.Drawing.Size(80, 13);
            this.lbl_currentPrice.TabIndex = 4;
            this.lbl_currentPrice.Text = "lbl_currentPrice";
            // 
            // bt_swap
            // 
            this.bt_swap.Location = new System.Drawing.Point(222, 118);
            this.bt_swap.Name = "bt_swap";
            this.bt_swap.Size = new System.Drawing.Size(75, 55);
            this.bt_swap.TabIndex = 2;
            this.bt_swap.Text = "SWAP";
            this.bt_swap.UseVisualStyleBackColor = true;
            this.bt_swap.Click += new System.EventHandler(this.bt_swap_Click);
            // 
            // cb_to
            // 
            this.cb_to.FormattingEnabled = true;
            this.cb_to.Location = new System.Drawing.Point(314, 118);
            this.cb_to.Name = "cb_to";
            this.cb_to.Size = new System.Drawing.Size(121, 21);
            this.cb_to.TabIndex = 1;
            this.cb_to.SelectedIndexChanged += new System.EventHandler(this.cb_to_SelectedIndexChanged);
            this.cb_to.TextUpdate += new System.EventHandler(this.cb_to_TextUpdate);
            // 
            // cb_From
            // 
            this.cb_From.FormattingEnabled = true;
            this.cb_From.Location = new System.Drawing.Point(81, 118);
            this.cb_From.Name = "cb_From";
            this.cb_From.Size = new System.Drawing.Size(121, 21);
            this.cb_From.TabIndex = 0;
            this.cb_From.SelectedIndexChanged += new System.EventHandler(this.cb_From_SelectedIndexChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1076, 450);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_trade_day)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Balance)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Trades)).EndInit();
            this.tb_3.ResumeLayout(false);
            this.tb_3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_To)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_From)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tb_3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridView dgv_Balance;
        private System.Windows.Forms.DataGridViewTextBoxColumn obj;
        private System.Windows.Forms.DataGridViewTextBoxColumn Exchange;
        private System.Windows.Forms.DataGridViewTextBoxColumn Crypto;
        private System.Windows.Forms.DataGridViewTextBoxColumn Count;
        private System.Windows.Forms.DataGridViewTextBoxColumn ValueBtc;
        private System.Windows.Forms.DataGridViewTextBoxColumn CustomValue;
        private System.Windows.Forms.DataGridView dgv_trade_day;
        private System.Windows.Forms.DataGridView dgv_Trades;
        private System.Windows.Forms.DataGridViewTextBoxColumn obj1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Couple;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn obj2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn11;
        private System.Windows.Forms.DataGridViewTextBoxColumn Date;
        private System.Windows.Forms.DataGridViewTextBoxColumn Back;
        private System.Windows.Forms.Button bt_swap;
        private System.Windows.Forms.ComboBox cb_to;
        private System.Windows.Forms.ComboBox cb_From;
        private System.Windows.Forms.Label lbl_currentPrice;
        private System.Windows.Forms.NumericUpDown nud_To;
        private System.Windows.Forms.NumericUpDown nud_From;
        private System.Windows.Forms.RadioButton rad_to;
        private System.Windows.Forms.RadioButton rad_from;
        private System.Windows.Forms.Label lbl_qtty_to;
        private System.Windows.Forms.Label lbl_qtty_from;
    }
}

