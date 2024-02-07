﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Packages;

namespace ClientBase
{
    public class Client : IDisposable
    {
        public string ip { get; private set; }
        private int _port;
        //ToDo: Make this a config var
        private float _timeout = 10;
        private DateTime _connectionExparation;
        private bool _isConnected;
        public int id { get; private set; }
        public Tcp? _tcp;
        public Queue<Package> recivedQueue;
        private Queue<Package> sendingQueue;
        private bool _isSending;
        private bool _stopSending;

        public Client(string ip, int port)
        {
            this.ip = ip;
            _port = port;
            recivedQueue = new Queue<Package>();
            sendingQueue = new Queue<Package>();
            _isSending = false;
            _stopSending = false;

        }

        public void Connect()
        {
            _isConnected = false;
            StartSendingProcess();
            _tcp = new Tcp();
            _tcp.handler.onPacketRecived += RecevedPackage;
            _tcp.Connect(ip, _port);
        }
        

        public void RecevedPackage(Package package) 
        {
            switch (package.key)
            {
                case PackageFactory.Keys.Welcome:
                    ProcessPackage(package as Welcome);
                    break;
                case PackageFactory.Keys.Heartbeat:
                    ProcessPackage(package as Heartbeat);
                    break;
                default:
                    recivedQueue.Enqueue(package);
                    break;
            }
        }

        public void Dispose()
        {
            if (_tcp != null)
            _tcp.handler.onPacketRecived -= RecevedPackage;
        }

        private void ProcessPackage(Heartbeat? package)
        {
            if (package == null) return;
            _connectionExparation = DateTime.Now.AddSeconds(_timeout);
            sendingQueue.Enqueue(new Heartbeat());
            Console.WriteLine("Processed Heartbeat");
        }

        private void ProcessPackage(Welcome? package) 
        {
            if(package == null) return;

            id = package._id;
            if(_tcp != null)
            {
                sendingQueue.Enqueue(new WelcomeReceved());
                _isConnected = true;
                Console.WriteLine($"Processed Welcome {package._message}");
                return;
            }
            Console.WriteLine("Welcome message could not be responed to");
        }

        public void Send(Package package)
        {   
            sendingQueue.Enqueue(package);
        }

        private void SendingPackage()
        {
            if(sendingQueue.Count <= 0 || _tcp == null) return;
            
            Package package = sendingQueue.Dequeue();
            switch (package.protocoll)
            {
                case PackageFactory.Protocoll.tcp:
                    _tcp.SendPackage(package);
                    break;
                case PackageFactory.Protocoll.udp:
                    throw new NotImplementedException();
                default:
                    Console.WriteLine($"Protocoll not supported {package.protocoll}");
                    break;
            }
        }


        public void StartSendingProcess()
        {
            if(_isSending) return;
            _isSending = true;

            new Thread(() =>
            {
                while (true)
                {
                    if (_stopSending) break;
                    SendingPackage();
                }
            }).Start();
        }

        public bool IsConnected()
        {
            if (_isConnected && _connectionExparation >= DateTime.Now)
                return true;

            return false;
        }
    }
}
