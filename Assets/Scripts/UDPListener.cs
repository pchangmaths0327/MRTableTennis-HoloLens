using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


public class UDPListener : MonoBehaviour
{
    public scoreBoardHandler scoreBoard;
    public LoggerScript logger;
    public Socket _socket;
    // public Queue<trackPoint[]> tracks = new Queue<trackPoint[]>();
    public BallVisualizer visualizer;
    public GameObject table;
    public IntroMenuScript introMenu;

    private const int bufSize = 8 * 1024;
    private State state = new State();
    private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
    private AsyncCallback recv = null;
    private AsyncCallback snd = null;
    private static readonly Queue<Action> tasks = new Queue<Action>();
    private string serverIP = "127.0.0.1";
    private int serverPort = 2000;
    private Queue<byte[]> rcvdPackets = new Queue<byte[]>();
    private Queue<EventData> rcvdEvents = new Queue<EventData>();
    private Queue<int> rcvdPacketsLengths = new Queue<int>();
    private int trackCount = 0;

    public class State
    {
        public byte[] buffer = new byte[bufSize];
    }

    public void Server(string address, int port)
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        logger.log("New socket created");
        _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
        logger.log("Socket option set");
        _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
        logger.log("Socket bind");
        Receive();
    }

    public void Client(string address, int port)
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.Connect(IPAddress.Parse(address), port);
        Receive();
    }

    public void Send(string text)
    {
        byte[] data = Encoding.ASCII.GetBytes(text);
        _socket.BeginSend(data, 0, data.Length, SocketFlags.None, snd = (ar) =>
        {
            State so = (State)ar.AsyncState;
            int bytes = _socket.EndSend(ar);
            Console.WriteLine("SEND: {0}, {1}", bytes, text);
            //_socket.BeginSend(data, 0, data.Length, SocketFlags.None, snd, so);
            this.QueueOnMainThread(() => { logger.log("Send: " + text); });
        }, state);
    }

    private void Receive()
    {
        this.QueueOnMainThread(() => { logger.log("Enter Receive!"); });
        _socket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) =>
        {
            try
            {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndReceiveFrom(ar, ref epFrom);
                byte[] rcvBuffer = new byte[bufSize];
                _socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);
                rcvBuffer = so.buffer;
                //Console.WriteLine("RECV: {0}: {1}, {2}", epFrom.ToString(), bytes, Encoding.ASCII.GetString(so.buffer, 0, bytes));
                //this.QueueOnMainThread(() => { logger.log("Received from " + epFrom.ToString() + " at " + System.DateTime.Now + ":\n" + Encoding.ASCII.GetString(so.buffer, 1, bytes - 1)); });
                //this.QueueOnMainThread(() => { logger.log("Received from " + epFrom.ToString() + " at " + System.DateTime.Now + "Length: " + so.buffer.Length + "\n"); });
                rcvdPackets.Enqueue(rcvBuffer);
                rcvdPacketsLengths.Enqueue(bytes);
            }
            catch
            {
                this.QueueOnMainThread(() => { logger.log("Caught!"); });
            }
        }, state);
    }

    void HandleTasks()
    {
        while (tasks.Count > 0)
        {
            Action task = null;

            lock (tasks)
            {
                if (tasks.Count > 0)
                {
                    task = tasks.Dequeue();
                }
            }

            task();
        }
    }



    public void HandleJson()
    {
        while (rcvdPackets.Count > 0)
        {
            logger.log("Receiving Packet");
            byte[] packet = rcvdPackets.Dequeue();
            int byteLength = rcvdPacketsLengths.Dequeue();
            byte Header;
            int offset = 0;
            Header = packet[offset++];
            byte stringHeader = (byte)0xfc;
            byte trackHeader = (byte)0xfd;
            byte eventHeader = (byte)0xfe;

            if (Byte.Equals(Header, trackHeader))
            {
                //if (table.activeInHierarchy)
                //{
                    /*Here we read in the Track*/
                    /*First we read in how many packages there are*/
                    logger.log("Receiving Track");
                    byte[] bufferLength = new byte[4];
                    Array.Copy(packet, offset, bufferLength, 0, 4);
                    Array.Reverse(bufferLength);
                    int nPackets = BitConverter.ToInt32(bufferLength, 0);
                    logger.log("Buffer length: " + bufferLength);
                    offset += 4;
                    /*For all packages do:*/
                    trackPoint[] track = new trackPoint[nPackets];

                    for (int i = 0; i < nPackets; i++)
                    {
                        /*Read JSONObject Length*/
                        bufferLength = new byte[4];
                        Array.Copy(packet, offset, bufferLength, 0, 4);
                        offset += 4;
                        Array.Reverse(bufferLength);
                        int packetLength = BitConverter.ToInt32(bufferLength, 0);
                        //logger.log("Buffer length of Json: " + bufferLength);

                        /*Read in JSONObject*/
                        string jsonString = Encoding.ASCII.GetString(packet, offset, packetLength);
                        offset += packetLength;
                        trackPoint trackpoint = trackPoint.CreateTrackFromJSON(jsonString);
                        track[i] = trackpoint;

                        visualizer.VisualizeTrackpoint(trackpoint);
                        visualizer.AddTrackPointToLine(trackpoint, i == 0);
                        visualizer.AddTrackpointToRecord(trackpoint);

                        logger.log("Track Point id: " + trackpoint.id.ToString() + "[" + trackpoint.positionX.ToString() + ", " + trackpoint.positionY.ToString() + ", " + trackpoint.positionZ.ToString() + "]");
                        /*Here we could put trackpoints into a queue ro so!*/
                    }
                    logger.log("Track: " + trackCount);
                    trackCount += 1;
                    // tracks.Enqueue(track);
                //}
            }

            else if (Byte.Equals(Header, eventHeader))
            {
                logger.log("Receiving Event");
                string message = Encoding.ASCII.GetString(packet, 1, byteLength - 1);
                logger.log("Json Event: " + message);
                EventData myData = EventData.CreateEventFromJSON(message);
                logger.log("My Data: EventType: " + myData.eventType + " Side: "+ myData.side);
                handleEvent(myData);
            }
            else if (Byte.Equals(Header, stringHeader))
            {
                logger.log("Receiving String");
                string message = Encoding.ASCII.GetString(packet, 1, byteLength - 1);
                logger.log("Message : " + message);
            }
            else
            {
                logger.log("Header not recognized: " + Header.ToString());
            }

        }
    }

    public void QueueOnMainThread(Action task)
    {
        lock (tasks)
        {
            tasks.Enqueue(task);
        }
    }

    public void showServerIP()
    {
        logger.log("Server is at " + serverIP + ":" + serverPort);
    }

    public void logString(string s)
    {
        logger.log(s);
    }

   
    void Start()
    {

        try
        {
            // Retrieve the Name of HOST
            string hostName = Dns.GetHostName();
            logger.log("Server Hostname: " + hostName);
            // Get the IP
            IPAddress[] ipaddress = Dns.GetHostAddresses(hostName);
            foreach (IPAddress ip in ipaddress)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    serverIP = ip.ToString();
                    break;
                }
            }
            logger.log("Server IP & port: " + serverIP + ":" + serverPort);
        }
        catch (Exception e)
        {
            logger.log("Failed to find server IP, using " + serverIP + ":" + serverPort);
        }

        Server(serverIP, serverPort);
        logger.log("Server created! (" + serverIP + ":" + serverPort + ")");
        introMenu.AddServerIPToUnlockText(serverIP, serverPort);

        //Client("192.168.1.116", 2000);
        //Send("TEST!");

        //_socket.Close(); //Fixed closing bug (System.ObjectDisposedException)
        //Bugfix allows to relaunch server
    }

    // Update is called once per frame
    void Update()
    {
        HandleJson();
        HandleTasks();
    }
    [System.Serializable]
    public class EventData
    {
        public string side;
        public int wins;
        public int score;
        public string eventType;
        public string nextServer;
        public string lastServer;
        public string role;
        public static EventData CreateEventFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<EventData>(jsonString);
        }
    }

    public void handleEvent(EventData mrEvent)
    {
        if (mrEvent.eventType != null)
        {
            if(mrEvent.eventType == "onReadyToServe")
            {
                // we only record the latest exchange
                visualizer.ClearRecord();
            }
            scoreBoard.updateScoreBoard(mrEvent);
        }
        else
        {
            logger.log("Event is null Object!");
        }
        return;
    }


    [System.Serializable]
    public class TrackData
    {
        Queue<Track> Tracks;

        public TrackData()
        {
            this.Tracks = null;
        }

    }
    public class trackPoint
    {
        public int id;
        public int positionX;
        public int positionY;
        public double positionZ;
        public int color;
        //public float velocityX;
        //public float velocityY;
        public static trackPoint CreateTrackFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<trackPoint>(jsonString);
        }
    }
    public class Track
    {

        public trackPoint[] trackPoints;
        public Track()
        {
            this.trackPoints = null;
        }


    }
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }
}