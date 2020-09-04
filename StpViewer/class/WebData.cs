using System.Collections;
using System.Collections.Generic;
using WebSocketSharp;
using System;



public class WebData
{
    /// <summary>  
    /// The WebSocket address to connect  
    /// </summary>  
    public string address = "ws://127.0.0.1:1822";
    /// <summary>  
    /// Saved WebSocket instance  
    /// </summary>  
    public WebSocket _webSocket;
    public bool isConnected { get { return _webSocket.ReadyState == WebSocketState.Open; } }
    
    private Queue<string> _msgQueue = new Queue<string>();
    private Queue<byte[]> _binQueue = new Queue<byte[]>();
    //public Queue<DataInfo> MsgQueue { get { return _msgQueue; } }
    public Queue<string> MsgQueue { get { return _msgQueue; } }
    public Queue<byte[]> BinQueue { get { return _binQueue; } }
    public Queue<string> msgOutQueue = new Queue<string>();
    
   
    public WebData(string url)
    {
        address = url;
    }
    

    public void OpenWebSocket()
    {
        
        if (_webSocket == null)
        {
            // Create the WebSocket instance  
            _webSocket = new WebSocket(address);
            _webSocket.OnError += (sender, e) =>
            {
                Console.Write("OnError:" + e.Message);

            };
            _webSocket.OnClose += (sender, e) =>
            {
                Console.Write("Closed because:" + e.Reason);

            };
            _webSocket.OnOpen += (sender, e) =>
            {
                Console.Write("open");
            };
            _webSocket.OnMessage += (sender, e) =>
            {
                if (e.IsText)
                {
                    _msgQueue.Enqueue(e.Data);
                }
                else if (e.IsBinary)
                {
                    _binQueue.Enqueue(e.RawData);
                }
            };
            _webSocket.Connect();


        }
        //else
        //{
        //    _webSocket.Connect();
        //}
    }

    public void Send(string mess)
    {
        if (_webSocket!=null)
        {
            switch (_webSocket.ReadyState)
            {
                case WebSocketState.Connecting:
                    _webSocket.Connect();
                    break;
                case WebSocketState.Open:
                    _webSocket.Send(mess);
                    break;
                case WebSocketState.Closing:
                    _webSocket.Connect();
                    break;
                case WebSocketState.Closed:
                    _webSocket.Connect();
                    break;
                default:
                    break;
            }
        }
       
    }
}

