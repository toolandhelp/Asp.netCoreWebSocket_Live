using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp
{
    public abstract class WebSocketHandler
    {
        public WebSocketConnectionManager WebSocketConnectionManager { get; set; }

        public WebSocketHandler(WebSocketConnectionManager webSocketConnectionManager)
        {
            WebSocketConnectionManager = webSocketConnectionManager;
        }

        public virtual void OnConnected(WebSocket socket, string key)
        {
            //var ServerSocket = WebSocketConnectionManager.GetWebSocket(key);
            //if (ServerSocket != null)
            //{
            //    WebSocketConnectionManager.AddSocket();
            //    Console.WriteLine("已经存在当前的连接，断开。。");
            //}
            WebSocketConnectionManager.AddSocket(socket, key);
        }

        public virtual async Task OnDisconnected(WebSocket socket)
        {
            Console.WriteLine("Socket 断开了");
            await WebSocketConnectionManager.RemoveSocket(WebSocketConnectionManager.GetId(socket));
        }

        public async Task SendMessageAsync(WebSocket socket, string message)
        {
            if (socket.State != WebSocketState.Open)
                return;
            var bytes = Encoding.UTF8.GetBytes(message);
            await socket.SendAsync(buffer: new ArraySegment<byte>(array: bytes, offset: 0, count: bytes.Length), messageType: WebSocketMessageType.Text, endOfMessage: true, cancellationToken: CancellationToken.None);
        }

        public async Task SendMessageAsync(string socketId, string message)
        {
            try
            {
                await SendMessageAsync(WebSocketConnectionManager.GetSocketById(socketId), message);
            }
            catch (Exception)
            {

            }

        }

        public async Task SendMessageToAllAsync(string message)
        {
            foreach (var pair in WebSocketConnectionManager.GetAll())
            {
                if (pair.Value.State == WebSocketState.Open)
                    await SendMessageAsync(pair.Value, message);
            }
        }
        /// <summary>
        /// 获取一些连接
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public IEnumerable<WebSocket> GetSomeWebsocket(string[] keys)
        {
            foreach (var key in keys)
            {
                yield return WebSocketConnectionManager.GetWebSocket(key);
            }
        }

        /// <summary>
        /// 给一堆人发消息
        /// </summary>
        /// <param name="webSockets"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessageToSome(WebSocket[] webSockets, string message)
        {
            webSockets.ToList().ForEach(async a => { await SendMessageAsync(a, message); });
        }

        public abstract Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer);
    }
}
