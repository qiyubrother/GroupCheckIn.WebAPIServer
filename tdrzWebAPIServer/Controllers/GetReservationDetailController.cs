using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Transaction.Definition;
using NNHuman.Cipher.Standard;
namespace tdrzWebAPIServer.Controllers
{
    [EnableCors("AllowCors"), Route("api/[controller]")]
    [ApiController]
    public class GetReservationDetailController : ControllerBase
    {
        // GET: api/GetReservationDetail
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // POST: api/GetReservationDetail
        [HttpPost]
        [EnableCors("any")]
        public JsonResult Post([FromBody] AuthenticationsInput value)
        {
            var txGetReservationDetail = new GetReservationDetailTx
            {
                HotelId = value.HotelId,
                ReservationNo = value.ReservationNo
            };

            var socketServerIP = "192.168.0.6";
            var socketServerPort = 5060;

            //设定服务器IP地址
            var ip = IPAddress.Parse(socketServerIP);
            var aesHelper = new AESUtil();
            using (var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                byte[] recvBuffer = new byte[1024 * 50]; // 50KB
                try
                {
                    clientSocket.Connect(new IPEndPoint(ip, socketServerPort)); //配置服务器IP与端口

                    #region 首次连接，客户端生成公钥，并将公钥发给服务端
                    var clientKey = ECDHUtil.CreatAliceKeyType();
                    var tx = new ECDHTx { ClientPublicKey = clientKey.PublicKey };
                    #region Base64加密后，追加#作为结束符
                    var j = JsonConvert.SerializeObject(tx);
                    var js = qiyubrother.extend.Base64.EncodeString(j, Encoding.UTF8) + "#";
                    #endregion
                    #region 100000 发送公钥到云端SocketServer
                    try
                    {
                        clientSocket.Send(Encoding.Default.GetBytes(js)); // 发送
                        //LogHelper.WriteLogAsync($"[{DateTime.Now}][SocketClient][已经发送数据到云端SocketServer]{js}", LogType.All);
                        #region 接收响应报文，并生成客户端私钥
                        var len = clientSocket.Receive(recvBuffer); // 接收回文
                        if (len > 0)
                        {
                            var buffer = recvBuffer;
                            var rx = Encoding.UTF8.GetString(buffer, 0, len);
                            //LogHelper.WriteLogAsync($"[{DateTime.Now}][SocketClient][收到SocketServer响应数据]{rx}", LogType.All);
                            #region Base64解密
                            var rxd = qiyubrother.extend.Base64.DecodeString(rx);
                            #endregion
                            var rxObj = JsonConvert.DeserializeObject<ECDHRx>(rxd);
                            // 客户端根据服务端传来的公钥，生成自己的私钥
                            ECDHUtil.CreatAlicePriKey(ref clientKey, rxObj.ServerPublicKey);
                        }
                        else
                        {
                            throw new Exception($"[异常][SocketClient]收到字节为0，疑似连接断开。");
                        }
                        #endregion
                    }
                    catch (Exception xcp)
                    {
                        throw new Exception($"[异常][SocketClient]{xcp.Message}");
                    }
                    #endregion
                    #endregion

                    #region AES加密数据
                    var data = JsonConvert.SerializeObject(txGetReservationDetail);
                    var aesData = aesHelper.AESEncrypt(data, clientKey.PrivateKey);
                    #endregion
                    #region Base64加密后，追加#作为结束符
                    var base64Data = qiyubrother.extend.Base64.EncodeBytes(aesData) + "#";
                    #endregion
                    clientSocket.Send(Encoding.Default.GetBytes(base64Data)); // 发送
                    var recvLen = clientSocket.Receive(recvBuffer);
                    if (recvLen == 0)
                    {
                        throw new Exception("Connection disconnect.");
                    }
                    #region AES解密数据
                    var aesBuffer = new byte[recvLen];
                    Array.Copy(recvBuffer, aesBuffer, recvLen);
                    var rxData = aesHelper.AESDecrypt(aesBuffer, clientKey.PrivateKey);
                    #endregion

                    return new JsonResult(Encoding.Default.GetString(rxData));
                }
                catch (Exception ex)
                {
                    var rx = new GetReservationDetailRx
                    {
                        ErrorCode = "-1",
                        ErrorMessage = ex.Message
                    };
                    return new JsonResult(JsonConvert.SerializeObject(rx));
                }
            }
        }

    }
}
