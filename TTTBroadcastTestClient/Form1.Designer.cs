namespace TTTBroadcastTestClient
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.button_Query = new System.Windows.Forms.Button();
            this.button_Send = new System.Windows.Forms.Button();
            this.button_StartTcpSvr = new System.Windows.Forms.Button();
            this.button_ConnectRmtSvr = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_Query
            // 
            this.button_Query.Location = new System.Drawing.Point(81, 12);
            this.button_Query.Name = "button_Query";
            this.button_Query.Size = new System.Drawing.Size(133, 23);
            this.button_Query.TabIndex = 0;
            this.button_Query.Text = "Receive Broadcast";
            this.button_Query.UseVisualStyleBackColor = true;
            this.button_Query.Click += new System.EventHandler(this.button_Query_Click);
            // 
            // button_Send
            // 
            this.button_Send.Location = new System.Drawing.Point(81, 41);
            this.button_Send.Name = "button_Send";
            this.button_Send.Size = new System.Drawing.Size(133, 23);
            this.button_Send.TabIndex = 1;
            this.button_Send.Text = "Send Broadcast";
            this.button_Send.UseVisualStyleBackColor = true;
            this.button_Send.Click += new System.EventHandler(this.button_Send_Click);
            // 
            // button_StartTcpSvr
            // 
            this.button_StartTcpSvr.Location = new System.Drawing.Point(81, 71);
            this.button_StartTcpSvr.Name = "button_StartTcpSvr";
            this.button_StartTcpSvr.Size = new System.Drawing.Size(133, 23);
            this.button_StartTcpSvr.TabIndex = 2;
            this.button_StartTcpSvr.Text = "Start Tcp Svr";
            this.button_StartTcpSvr.UseVisualStyleBackColor = true;
            this.button_StartTcpSvr.Click += new System.EventHandler(this.button_StartTcpSvr_Click);
            // 
            // button_ConnectRmtSvr
            // 
            this.button_ConnectRmtSvr.Location = new System.Drawing.Point(81, 101);
            this.button_ConnectRmtSvr.Name = "button_ConnectRmtSvr";
            this.button_ConnectRmtSvr.Size = new System.Drawing.Size(133, 23);
            this.button_ConnectRmtSvr.TabIndex = 3;
            this.button_ConnectRmtSvr.Text = "Connect Remote Svr";
            this.button_ConnectRmtSvr.UseVisualStyleBackColor = true;
            this.button_ConnectRmtSvr.Click += new System.EventHandler(this.button_ConnectRmtSvr_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 135);
            this.Controls.Add(this.button_ConnectRmtSvr);
            this.Controls.Add(this.button_StartTcpSvr);
            this.Controls.Add(this.button_Send);
            this.Controls.Add(this.button_Query);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Query;
        private System.Windows.Forms.Button button_Send;
        private System.Windows.Forms.Button button_StartTcpSvr;
        private System.Windows.Forms.Button button_ConnectRmtSvr;
    }
}

