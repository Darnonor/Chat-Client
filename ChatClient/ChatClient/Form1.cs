using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections;
using System.Linq;

namespace ChatClient
{
    public partial class Form1 : Form
    {
        static string userName;       
        private string receivedCommand;
        private const string host = "127.0.0.1";
        private const int port = 8888;
        private delegate void UpdateLogCallback(string strMessage);
        private delegate void ClearOnlineList();
        private delegate void UpdateOnlineList(string onlineList);
        private delegate void UpdatePrivateChat(string message);
        private TcpClient client;       
        private StreamWriter sender;
        private StreamReader receiver;
        private Thread receiveThread;
        private IPAddress ipAddr;
        private bool connected;       
        
        public Form1()
        {
            InitializeComponent();
            this.Text = "ChatClient";                
        }

        private void Connect(string name)
        {
            try
            {
                ipAddr = IPAddress.Parse(host);
                client = new TcpClient();
                client.Connect(ipAddr, port);

                connected = true;
                userName = tbName.Text;

                sender = new StreamWriter(client.GetStream());
                sender.WriteLine(userName);
                sender.Flush();

                receiveThread = new Thread(new ThreadStart(ReceiveMessages));
                receiveThread.Start();
            }
            
            catch
            {
                this.Invoke(new UpdateLogCallback(this.UpdateChat), new object[] { "No connection to the server" });
            }
        }

        private void ReceiveMessages()
        {
            receiver = new StreamReader(client.GetStream());
            bool endUpdate = false;
            try
            {
                while (connected)
                {
                    receivedCommand = receiver.ReadLine();

                    if (receivedCommand == "#sendAll")
                    {
                        receivedCommand = receiver.ReadLine();
                        this.Invoke(new UpdateLogCallback(this.UpdateChat), new object[] { receivedCommand });
                    }

                    if (receivedCommand == "#update_onlineList")
                    {
                        this.Invoke(new ClearOnlineList(this.ClearOnlineUser), new object[] { });
                        endUpdate = true;
                        while (endUpdate)
                        {
                            receivedCommand = receiver.ReadLine();
                            if (receivedCommand != "#end_update_onlineList")
                            {
                                this.Invoke(new UpdateOnlineList(this.UpdateOnlineUsers), new object[] { receivedCommand });
                            }

                            else { endUpdate = false; }
                        }
                    }

                    if (receivedCommand == "#input_private_message")
                    {
                        receivedCommand = receiver.ReadLine();
                        this.Invoke(new UpdatePrivateChat(this.UpdatePrivateChatLog), new object[] { receivedCommand });
                    }
                }
            }

            catch
            {
                this.Invoke(new UpdateLogCallback(this.UpdateChat), new object[] { "Connection lost" });
            }
            
        }       

        private void UpdateChat(string message)
        {
            tbChat.AppendText(message + "\r\n");
        }
        
        private void UpdateOnlineUsers(string list)
        {
            tbUsers.AppendText(list + "\r\n");
        }

        private void UpdatePrivateChatLog(string message)
        {
            tbPrivateChat.AppendText(message + "\r\n");
        }

        private void ClearOnlineUser()
        {
            tbUsers.Text = "";
        }

        private void SendMessage(string message)
        {
            sender.WriteLine(tbMessage.Text);
            sender.Flush();
            tbMessage.Text = "";
        }

        private void SendPrivateMessage(string userName, string message)
        {
            sender.WriteLine("#output_private_message");
            sender.WriteLine(userName);
            sender.WriteLine(message);
            sender.Flush();
            //sender
            tbPrivateMessage.Text = "";
        }

        private void EnableChat()
        {
            exitButton.Enabled = true;
            tbPrivateMessage.Enabled = true;
            tbNameRec.Enabled = true;
            SendPrivateMessButton.Enabled = true;
            tbMessage.Enabled = true;
            SendButton.Enabled = true;

            tbName.Enabled = false;
            joinButton.Enabled = false;
        }

        private void DisableChat()
        {
            exitButton.Enabled = false;
            tbPrivateMessage.Enabled = false;
            tbNameRec.Enabled = false;
            SendPrivateMessButton.Enabled = false;
            tbMessage.Enabled = false;
            SendButton.Enabled = false;

            tbName.Enabled = true;
            joinButton.Enabled = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
          
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Connect(tbName.Text);
            if (connected == true) { EnableChat(); }
        }

        private void ExitButton_Click(object send, EventArgs e)
        {
            connected = false;
            client.Close();
            receiver.Close();
            sender.Close();
            DisableChat();
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            SendMessage(tbMessage.Text);
        }

        private void SendPrivateMessButton_Click(object sender, EventArgs e)
        {
            this.Invoke(new UpdatePrivateChat(this.UpdatePrivateChatLog), new object[] { "you: " + tbNameRec.Text + ", " + tbPrivateMessage.Text + "\r\n" });
            SendPrivateMessage(tbNameRec.Text, tbPrivateMessage.Text);       
        }

        public void OnApplicationExit(object send, EventArgs e)
        {
            if (connected == true)
            {
                connected = false;
                sender.Close();
                receiver.Close();
                client.Close();
            }
        }
    }
}

