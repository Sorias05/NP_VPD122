﻿using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ConsoleServerChat
{
    class Program
    {
        static readonly object _lock = new object();
        static readonly Dictionary<int, TcpClient> list_clients = new Dictionary<int, TcpClient>();
        public static void Main(string[] args)
        {
            // Створення змінних
            string fileName = "setting.txt";
            int count = 1;
            IPAddress ip;
            int port;
            // Зчитування з файла налаштувань порт, ip
            using(FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using(StreamReader sr = new StreamReader(fs))
                {
                    ip=IPAddress.Parse(sr.ReadLine());
                    port=int.Parse(sr.ReadLine());
                }
            }
            // Створення нового сервера і запуск
            TcpListener serverSocket = new TcpListener(ip, port);
            serverSocket.Start();
            Console.WriteLine("Запуск сервера {0}:{1}", ip, port);
            // Відслідковування подій
            while (true)
            {
                TcpClient client = serverSocket.AcceptTcpClient();
                lock(_lock) { list_clients.Add(count, client); }
                Console.WriteLine("Нова подія в чаті :)");
                Thread t = new Thread(handle_clients);
                t.Start(count);
                count++;
            }
        }
        // Обслуговування клієнта
        public static void handle_clients(object c)
        {
            int id = (int)c;
            // Обрання клієнта за id зі списку
            TcpClient client;
            lock (_lock) { client = list_clients[id]; }
            // Підключення й обробка клієнта
            while (true)
            {
                NetworkStream stream = client.GetStream();
                Console.WriteLine("Підключився клієнт {0}", client.Client.RemoteEndPoint);
                byte[] buffer = new byte[4096];
                int byte_count = stream.Read(buffer, 0, buffer.Length);
                if (byte_count == 0) break;
                string data = Encoding.UTF8.GetString(buffer);
                broadcast(data);
                Console.WriteLine(data);
            }
            // Відключення клієнта
            lock(_lock) { list_clients.Remove(id); }
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
        // Передача й запис даних
        public static void broadcast(string data)
        {
            byte[] buffer=Encoding.UTF8.GetBytes(data);
            lock(_lock)
            {
                foreach (TcpClient c in list_clients.Values)
                {
                    NetworkStream stream = c.GetStream();
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}

