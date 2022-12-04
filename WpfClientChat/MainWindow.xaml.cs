using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace WpfClientChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Створення змінних
        TcpClient client = new TcpClient();
        NetworkStream ns;
        Thread thread;
        private ChatMessage _message = new ChatMessage();

        public MainWindow()
        {
            InitializeComponent();
        }
        
        // Кнопка підключення до чату
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            // Спроба підключення до чату
            try
            {
                // Створення змінних
                string fileName = "config.txt";
                IPAddress ip;
                int port;
                // Зчитування з файла конфігурації порт, id
                using(FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    using(StreamReader sr = new StreamReader(fs))
                    {
                        ip=IPAddress.Parse(sr.ReadLine());
                        port = int.Parse(sr.ReadLine());
                    }
                }
                // Підключення клієнта до чату
                _message.UserName=txtUserName.Text;
                _message.UserId=Guid.NewGuid().ToString();
                client.Connect(ip,port);
                lbInfo.Items.Add("Підключення до сервера "+ip.ToString()+":"+port);
                ns=client.GetStream();
                thread = new Thread(o=>RecieveData((TcpClient)o));
                thread.Start(client);

                // Створення повідомлення про підключення клієнта до чату
                _message.MessageType = TypeMessage.Login;
                _message.Text="Приєднався до чату";
                byte[] bytes = _message.Serialize();
                ns.Write(bytes);
            }
            // Помилка
            catch (Exception ex)
            {
                MessageBox.Show("Problem Connection Sever "+ ex.Message);
            }
        }

        // Кнопка відправки повідомлення
        private void bntSend_Click(object sender, RoutedEventArgs e)
        {
            _message.MessageType = TypeMessage.Message;
            _message.Text = txtText.Text;
            var buffer = _message.Serialize();
            ns.Write(buffer, 0, buffer.Length);
        }
        
        // Відключення клієнта від чату
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _message.MessageType = TypeMessage.Message;
            _message.Text = "Відвалився козак із чату";
            var buffer = _message.Serialize();
            ns.Write(buffer, 0, buffer.Length);
            client.Client.Shutdown(SocketShutdown.Send);
            thread.Join();
            ns.Close();
            client.Close();
           
        }

        // Передача повідомлень
        private void RecieveData(TcpClient client)
        {
            // Створення змінних
            NetworkStream ns = client.GetStream();
            var receivedBytes = new byte[4128];
            int byte_count;
            string data = "";
            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                // Відправка повідомлення
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    // Спроба отримання повідомлення
                    try
                    {
                        ChatMessage message = ChatMessage.Desserialize(receivedBytes);
                        switch(message.MessageType)
                        {
                            case TypeMessage.Message:
                                {
                                    lbInfo.Items.Add(message.UserName+" -> "+message.Text);
                                    break;
                                }
                            case TypeMessage.Login:
                                {
                                    if(message.UserId != _message.UserId)
                                    {
                                        lbInfo.Items.Add(message.UserName + " -> " + message.Text);
                                    }
                                    break;
                                }
                            case TypeMessage.Logout:
                                {
                                    if (message.UserId != _message.UserId)
                                    {
                                        lbInfo.Items.Add(message.UserName + " -> " + message.Text);
                                    }
                                    break;
                                }
                        }
                        lbInfo.Items.MoveCurrentToLast();
                        lbInfo.ScrollIntoView(lbInfo.Items.CurrentItem);
                    }
                    // Помилка
                    catch(Exception ex)
                    { Console.WriteLine(ex.ToString()); }
                }));
            }
        }
    }
}
