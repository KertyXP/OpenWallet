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
            this.bt_refreshBalance = new System.Windows.Forms.Button();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.dgv_Balance = new System.Windows.Forms.DataGridView();
            this.obj = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Exchange = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Crypto = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Count = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CustomValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pb_chart = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.dgv_archive = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn13 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgv_trade_day = new System.Windows.Forms.DataGridView();
            this.obj2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.price = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CurrentPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Delta = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lbl_avg_sell = new System.Windows.Forms.Label();
            this.lbl_AdvgBuy = new System.Windows.Forms.Label();
            this.lbl_prev_2 = new System.Windows.Forms.Label();
            this.bt_refreshTrade = new System.Windows.Forms.Button();
            this.lbl_preview_archive = new System.Windows.Forms.Label();
            this.cb_Pair = new System.Windows.Forms.ComboBox();
            this.cb_HideArchived = new System.Windows.Forms.CheckBox();
            this.bt_unarchive = new System.Windows.Forms.Button();
            this.bt_archive = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Balance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_chart)).BeginInit();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_archive)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_trade_day)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // bt_refreshBalance
            // 
            this.bt_refreshBalance.Location = new System.Drawing.Point(3, 9);
            this.bt_refreshBalance.Name = "bt_refreshBalance";
            this.bt_refreshBalance.Size = new System.Drawing.Size(58, 23);
            this.bt_refreshBalance.TabIndex = 1;
            this.bt_refreshBalance.Text = "Refresh";
            this.bt_refreshBalance.UseVisualStyleBackColor = true;
            this.bt_refreshBalance.Click += new System.EventHandler(this.bt_refreshBalance_Click);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel1, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.panel2, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1185, 569);
            this.tableLayoutPanel2.TabIndex = 3;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.dgv_Balance, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pb_chart, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 78);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(468, 488);
            this.tableLayoutPanel1.TabIndex = 2;
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
            this.CustomValue});
            this.dgv_Balance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_Balance.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgv_Balance.Location = new System.Drawing.Point(3, 3);
            this.dgv_Balance.MultiSelect = false;
            this.dgv_Balance.Name = "dgv_Balance";
            this.dgv_Balance.ReadOnly = true;
            this.dgv_Balance.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_Balance.Size = new System.Drawing.Size(462, 238);
            this.dgv_Balance.TabIndex = 5;
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
            // CustomValue
            // 
            this.CustomValue.HeaderText = "CustomValue";
            this.CustomValue.Name = "CustomValue";
            this.CustomValue.ReadOnly = true;
            // 
            // pb_chart
            // 
            this.pb_chart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pb_chart.Location = new System.Drawing.Point(3, 247);
            this.pb_chart.Name = "pb_chart";
            this.pb_chart.Size = new System.Drawing.Size(462, 238);
            this.pb_chart.TabIndex = 6;
            this.pb_chart.TabStop = false;
            this.pb_chart.Paint += new System.Windows.Forms.PaintEventHandler(this.pb_chart_Paint);
            this.pb_chart.Resize += new System.EventHandler(this.pb_chart_Resize);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.dgv_archive, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.dgv_trade_day, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(474, 75);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(711, 494);
            this.tableLayoutPanel3.TabIndex = 3;
            // 
            // dgv_archive
            // 
            this.dgv_archive.AllowUserToAddRows = false;
            this.dgv_archive.AllowUserToDeleteRows = false;
            this.dgv_archive.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_archive.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewTextBoxColumn5,
            this.dataGridViewTextBoxColumn12,
            this.dataGridViewTextBoxColumn13});
            this.dgv_archive.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_archive.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgv_archive.Location = new System.Drawing.Point(3, 250);
            this.dgv_archive.Name = "dgv_archive";
            this.dgv_archive.ReadOnly = true;
            this.dgv_archive.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_archive.Size = new System.Drawing.Size(705, 241);
            this.dgv_archive.TabIndex = 6;
            this.dgv_archive.SelectionChanged += new System.EventHandler(this.dgv_archive_SelectionChanged);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "obj2";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.Visible = false;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.HeaderText = "CryptoFrom";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.HeaderText = "CountFrom";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn12
            // 
            this.dataGridViewTextBoxColumn12.HeaderText = "CryptoTo";
            this.dataGridViewTextBoxColumn12.Name = "dataGridViewTextBoxColumn12";
            this.dataGridViewTextBoxColumn12.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn13
            // 
            this.dataGridViewTextBoxColumn13.HeaderText = "ValueTo";
            this.dataGridViewTextBoxColumn13.Name = "dataGridViewTextBoxColumn13";
            this.dataGridViewTextBoxColumn13.ReadOnly = true;
            // 
            // dgv_trade_day
            // 
            this.dgv_trade_day.AllowUserToAddRows = false;
            this.dgv_trade_day.AllowUserToDeleteRows = false;
            this.dgv_trade_day.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_trade_day.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.obj2,
            this.dataGridViewTextBoxColumn6,
            this.dataGridViewTextBoxColumn7,
            this.dataGridViewTextBoxColumn8,
            this.dataGridViewTextBoxColumn9,
            this.dataGridViewTextBoxColumn10,
            this.dataGridViewTextBoxColumn11,
            this.price,
            this.CurrentPrice,
            this.Delta,
            this.Date});
            this.dgv_trade_day.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_trade_day.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgv_trade_day.Location = new System.Drawing.Point(3, 3);
            this.dgv_trade_day.Name = "dgv_trade_day";
            this.dgv_trade_day.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_trade_day.Size = new System.Drawing.Size(705, 241);
            this.dgv_trade_day.TabIndex = 5;
            this.dgv_trade_day.SelectionChanged += new System.EventHandler(this.dgv_trade_day_SelectionChanged_1);
            this.dgv_trade_day.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dgv_trade_day_MouseClick);
            this.dgv_trade_day.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.dgv_trade_day_MouseDoubleClick);
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
            this.dataGridViewTextBoxColumn8.Width = 75;
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
            this.dataGridViewTextBoxColumn10.Width = 75;
            // 
            // dataGridViewTextBoxColumn11
            // 
            this.dataGridViewTextBoxColumn11.HeaderText = "ValueTo";
            this.dataGridViewTextBoxColumn11.Name = "dataGridViewTextBoxColumn11";
            // 
            // price
            // 
            this.price.HeaderText = "price";
            this.price.Name = "price";
            this.price.Width = 75;
            // 
            // CurrentPrice
            // 
            this.CurrentPrice.HeaderText = "Cur. Price";
            this.CurrentPrice.Name = "CurrentPrice";
            this.CurrentPrice.Width = 85;
            // 
            // Delta
            // 
            this.Delta.HeaderText = "Delta";
            this.Delta.Name = "Delta";
            this.Delta.Width = 50;
            // 
            // Date
            // 
            this.Date.HeaderText = "Date";
            this.Date.Name = "Date";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lbl_avg_sell);
            this.panel2.Controls.Add(this.lbl_AdvgBuy);
            this.panel2.Controls.Add(this.lbl_prev_2);
            this.panel2.Controls.Add(this.bt_refreshTrade);
            this.panel2.Controls.Add(this.lbl_preview_archive);
            this.panel2.Controls.Add(this.cb_Pair);
            this.panel2.Controls.Add(this.cb_HideArchived);
            this.panel2.Controls.Add(this.bt_unarchive);
            this.panel2.Controls.Add(this.bt_archive);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(477, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(705, 69);
            this.panel2.TabIndex = 5;
            // 
            // lbl_avg_sell
            // 
            this.lbl_avg_sell.AutoSize = true;
            this.lbl_avg_sell.Location = new System.Drawing.Point(3, 49);
            this.lbl_avg_sell.Name = "lbl_avg_sell";
            this.lbl_avg_sell.Size = new System.Drawing.Size(46, 13);
            this.lbl_avg_sell.TabIndex = 14;
            this.lbl_avg_sell.Text = "Avg Sell";
            // 
            // lbl_AdvgBuy
            // 
            this.lbl_AdvgBuy.AutoSize = true;
            this.lbl_AdvgBuy.Location = new System.Drawing.Point(3, 29);
            this.lbl_AdvgBuy.Name = "lbl_AdvgBuy";
            this.lbl_AdvgBuy.Size = new System.Drawing.Size(47, 13);
            this.lbl_AdvgBuy.TabIndex = 13;
            this.lbl_AdvgBuy.Text = "Avg Buy";
            // 
            // lbl_prev_2
            // 
            this.lbl_prev_2.AutoSize = true;
            this.lbl_prev_2.Location = new System.Drawing.Point(236, 49);
            this.lbl_prev_2.Name = "lbl_prev_2";
            this.lbl_prev_2.Size = new System.Drawing.Size(25, 13);
            this.lbl_prev_2.TabIndex = 12;
            this.lbl_prev_2.Text = "      ";
            // 
            // bt_refreshTrade
            // 
            this.bt_refreshTrade.Location = new System.Drawing.Point(608, 9);
            this.bt_refreshTrade.Name = "bt_refreshTrade";
            this.bt_refreshTrade.Size = new System.Drawing.Size(58, 23);
            this.bt_refreshTrade.TabIndex = 2;
            this.bt_refreshTrade.Text = "Refresh";
            this.bt_refreshTrade.UseVisualStyleBackColor = true;
            this.bt_refreshTrade.Click += new System.EventHandler(this.bt_refreshTrade_Click);
            // 
            // lbl_preview_archive
            // 
            this.lbl_preview_archive.AutoSize = true;
            this.lbl_preview_archive.Location = new System.Drawing.Point(236, 29);
            this.lbl_preview_archive.Name = "lbl_preview_archive";
            this.lbl_preview_archive.Size = new System.Drawing.Size(25, 13);
            this.lbl_preview_archive.TabIndex = 11;
            this.lbl_preview_archive.Text = "      ";
            // 
            // cb_Pair
            // 
            this.cb_Pair.FormattingEnabled = true;
            this.cb_Pair.Location = new System.Drawing.Point(3, 3);
            this.cb_Pair.Name = "cb_Pair";
            this.cb_Pair.Size = new System.Drawing.Size(121, 21);
            this.cb_Pair.TabIndex = 10;
            this.cb_Pair.SelectedIndexChanged += new System.EventHandler(this.cb_Pair_SelectedIndexChanged);
            // 
            // cb_HideArchived
            // 
            this.cb_HideArchived.AutoSize = true;
            this.cb_HideArchived.Checked = true;
            this.cb_HideArchived.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_HideArchived.Location = new System.Drawing.Point(130, 5);
            this.cb_HideArchived.Name = "cb_HideArchived";
            this.cb_HideArchived.Size = new System.Drawing.Size(93, 17);
            this.cb_HideArchived.TabIndex = 9;
            this.cb_HideArchived.Text = "Hide Archived";
            this.cb_HideArchived.UseVisualStyleBackColor = true;
            this.cb_HideArchived.CheckedChanged += new System.EventHandler(this.cb_HideArchive_CheckedChanged);
            // 
            // bt_unarchive
            // 
            this.bt_unarchive.Location = new System.Drawing.Point(539, 9);
            this.bt_unarchive.Name = "bt_unarchive";
            this.bt_unarchive.Size = new System.Drawing.Size(67, 22);
            this.bt_unarchive.TabIndex = 8;
            this.bt_unarchive.Text = "UnArchive";
            this.bt_unarchive.UseVisualStyleBackColor = true;
            this.bt_unarchive.Click += new System.EventHandler(this.bt_unarchive_Click);
            // 
            // bt_archive
            // 
            this.bt_archive.Location = new System.Drawing.Point(229, 1);
            this.bt_archive.Name = "bt_archive";
            this.bt_archive.Size = new System.Drawing.Size(58, 23);
            this.bt_archive.TabIndex = 3;
            this.bt_archive.Text = "Archive";
            this.bt_archive.UseVisualStyleBackColor = true;
            this.bt_archive.Click += new System.EventHandler(this.bt_archive_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.bt_refreshBalance);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(468, 69);
            this.panel1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1185, 569);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Balance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_chart)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_archive)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_trade_day)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button bt_refreshBalance;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.DataGridView dgv_archive;
        private System.Windows.Forms.DataGridView dgv_trade_day;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ComboBox cb_Pair;
        private System.Windows.Forms.CheckBox cb_HideArchived;
        private System.Windows.Forms.Button bt_unarchive;
        private System.Windows.Forms.Button bt_archive;
        private System.Windows.Forms.Label lbl_preview_archive;
        private System.Windows.Forms.Button bt_refreshTrade;
        private System.Windows.Forms.Label lbl_prev_2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn12;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn13;
        private System.Windows.Forms.Label lbl_avg_sell;
        private System.Windows.Forms.Label lbl_AdvgBuy;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridView dgv_Balance;
        private System.Windows.Forms.DataGridViewTextBoxColumn obj;
        private System.Windows.Forms.DataGridViewTextBoxColumn Exchange;
        private System.Windows.Forms.DataGridViewTextBoxColumn Crypto;
        private System.Windows.Forms.DataGridViewTextBoxColumn Count;
        private System.Windows.Forms.DataGridViewTextBoxColumn CustomValue;
        private System.Windows.Forms.PictureBox pb_chart;
        private System.Windows.Forms.DataGridViewTextBoxColumn obj2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn11;
        private System.Windows.Forms.DataGridViewTextBoxColumn price;
        private System.Windows.Forms.DataGridViewTextBoxColumn CurrentPrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn Delta;
        private System.Windows.Forms.DataGridViewTextBoxColumn Date;
    }
}

