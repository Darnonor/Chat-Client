using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace ChatClient
{
    class Chat
    {
        static string userName;
        private const string host = "127.0.0.1";
        private const int port = 8888;
        static TcpClient client;
        static NetworkStream stream;
        static TextBox chatBox;

        public Chat(TextBox tb)
        {
            chatBox = tb;
        }


        protected internal void Connection(string name)
        {
            userName = name;
            client = new TcpClient();
            try
            {
                client.Connect(host, port); 
                stream = client.GetStream(); 

                string message = userName;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);

                
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start(); 
                
                PrintMessage("Welcome!");
                SendMessage("");
            }
            catch (Exception e)
            {
                PrintMessage(e.Message);
            }
            finally
            {
                Disconnect();
            }
        }

        protected internal void SendMessage(string message)
        {
            while (true)
            {
                string _message = message;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
        }

        void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; 
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    PrintMessage(message);
                }
                catch
                {
                    PrintMessage("Connection lost");
                    Disconnect();
                }
            }
        }

        protected internal void Disconnect()
        {
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
            Environment.Exit(0);
        }

        static void PrintMessage(string message)
        {
            chatBox.Text += message + Environment.NewLine;
        }
    }
}
