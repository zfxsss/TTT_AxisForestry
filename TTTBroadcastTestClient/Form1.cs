using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TTTBroadcastTestClient
{
    public partial class Form1 : Form
    {
        TcpClient t1;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Query_Click(object sender, EventArgs e)
        {
            using (var client = new UdpClient(10080))
            {
                var remoteEndPoint = default(IPEndPoint);
                var bytes = client.Receive(ref remoteEndPoint);
            }
        }

        private void button_Send_Click(object sender, EventArgs e)
        {
            using (var client = new UdpClient())
            {
                var remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, 10080);
                client.Send(new byte[] { 0xcc, 0xdd }, 2, remoteEndPoint);
            }
        }

        private void button_StartTcpSvr_Click(object sender, EventArgs e)
        {
            var listener = new TcpListener(new IPEndPoint(IPAddress.Any, 10080));

            listener.Start();

            t1 = listener.AcceptTcpClient();
        }

        private void button_ConnectRmtSvr_Click(object sender, EventArgs e)
        {
            var conn = new TcpClient();
            conn.Connect("127.0.0.1", 10080);
        }
    }
}
