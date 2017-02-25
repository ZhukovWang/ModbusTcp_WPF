using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModbusTcpDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public Socket client;
        public Thread MyThread;
        public String recvdata;
        public IPEndPoint ie;
        public bool isclose;

        public delegate void MyInvoke(string str,string str1);


        private void OnClick_connect(object sender, RoutedEventArgs e)
        {
            isclose = true;
            initsocket();
            connect();
        }


        private void OnClick_send(object sender, RoutedEventArgs e)
        {
            send();
        }

        public void initsocket()
        {
            string ipadd = "192.168.1.223";//IP
            int port = 502;//port

            //make a socket
            ie = new IPEndPoint(IPAddress.Parse(ipadd), port);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint bindip = new IPEndPoint(IPAddress.Parse("192.168.1.120"), port);
            if (client.IsBound == false)
            {
                client.Bind(bindip);
            }
        }

        public void connect()
        {
            
            byte[] data = new byte[1024];

            //connect
            try
            {
                client.Connect(ie);
            }
            catch (SocketException e)
            {
                MessageBox.Show("Connect Fail \n" + e.Message);
                return;
            }

            ThreadStart myThreaddelegate = new ThreadStart(ReceiveMsg);
            MyThread = new Thread(myThreaddelegate);
            MyThread.Start();
        }

        public void send()
        {
            byte[] data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x01, 0x03, 0x00, 0x00, 0x00, 0x01 };
            if (client == null)
            {
                ;
            }
            else
            {
                client.Send(data);
            }
        }

        public void ReceiveMsg()
        {
            while (isclose)
            {
                byte[] data = new byte[1024];
                client.Receive(data);
                int length = data[5];
                Byte[] datashow = new byte[length+6];
                for (int i = 0; i <= length + 5; i++)
                {
                    datashow[i] = data[i];
                }
                recvdata = "";
                string temp;
                for (int i = 0; i <= length + 5 ; i++)
                {
                    temp = Convert.ToString(datashow[i], 16);
                    switch (temp.Length)
                    {
                        case 1:
                            temp = "0" + temp;
                            break;
                        default:
                            break;
                    }
                    recvdata = recvdata + temp.ToUpper() + " ";
                }
                recvdata = recvdata + "\n";

                string numvalue = "Receive value: " +
                Convert.ToString(
                        Convert.ToUInt16((Convert.ToString(datashow[9], 16) + Convert.ToString(datashow[10], 16)), 16)) +
                    "\n";


                showMsg(recvdata,numvalue);
            }
        }

        public void showMsg(string msg,string num)
        {
            if (TextBox_out.CheckAccess() && TextBox_value.CheckAccess())
            {
                TextBox_out.AppendText(msg);
                TextBox_out.ScrollToEnd();

                TextBox_value.AppendText(num);
                TextBox_value.ScrollToEnd();
            }
            else
            {
                MyInvoke _myInvoke = new MyInvoke(showMsg);
                this.Dispatcher.Invoke(_myInvoke, new object[] { msg,num });

            }
        }

        private void MainWindow_Onclosed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
