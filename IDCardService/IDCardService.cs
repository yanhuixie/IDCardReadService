using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Collections;

namespace IDCardService
{
    public partial class IDCardService : ServiceBase
    {
        private SortedList lstMZ = new SortedList();

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct IDCardData
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string Name; //姓名   
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
            public string Sex;   //性别
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
            public string Nation; //民族
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
            public string Born; //出生日期
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 72)]
            public string Address; //住址
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 38)]
            public string IDCardNo; //身份证号
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string GrantDept; //发证机关
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
            public string ValidBegin; // 有效开始日期
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
            public string ValidEnd;  // 有效截止日期
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 38)]
            public string Reserved; // 保留
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string PhotoFileName; // 照片路径

            public string NationName;
            public string SexName;
        }

        /************************端口类API *************************/
        [DllImport("Syn_IDCardRead.dll", EntryPoint = "Syn_GetCOMBaud", CharSet = CharSet.Ansi)]
        public static extern int Syn_GetCOMBaud(int iComID, ref uint puiBaud);
        [DllImport("Syn_IDCardRead.dll", EntryPoint = "Syn_SetCOMBaud", CharSet = CharSet.Ansi)]
        public static extern int Syn_SetCOMBaud(int iComID, uint uiCurrBaud, uint uiSetBaud);
        [DllImport("Syn_IDCardRead.dll", EntryPoint = "Syn_OpenPort", CharSet = CharSet.Ansi)]
        public static extern int Syn_OpenPort(int iPortID);
        [DllImport("Syn_IDCardRead.dll", EntryPoint = "Syn_ClosePort", CharSet = CharSet.Ansi)]
        public static extern int Syn_ClosePort(int iPortID);

        /************************ SAM类API *************************/
        [DllImport("Syn_IDCardRead.dll", EntryPoint = "Syn_GetSAMStatus", CharSet = CharSet.Ansi)]
        public static extern int Syn_GetSAMStatus(int iPortID, int iIfOpen);
        [DllImport("Syn_IDCardRead.dll", EntryPoint = "Syn_ResetSAM", CharSet = CharSet.Ansi)]
        public static extern int Syn_ResetSAM(int iPortID, int iIfOpen);
        [DllImport("Syn_IDCardRead.dll", EntryPoint = "Syn_GetSAMID", CharSet = CharSet.Ansi)]
        public static extern int Syn_GetSAMID(int iPortID, ref byte pucSAMID, int iIfOpen);
        [DllImport("Syn_IDCardRead.dll", EntryPoint = "Syn_GetSAMIDToStr", CharSet = CharSet.Ansi)]
        public static extern int Syn_GetSAMIDToStr(int iPortID, ref byte pcSAMID, int iIfOpen);

        /********************身份证卡类API *************************/
        [DllImport("Syn_IDCardRead.dll", EntryPoint = "Syn_StartFindIDCard", CharSet = CharSet.Ansi)]
        public static extern int Syn_StartFindIDCard(int iPortID, ref byte pucManaInfo, int iIfOpen);
        [DllImport("Syn_IDCardRead.dll", EntryPoint = "Syn_SelectIDCard", CharSet = CharSet.Ansi)]
        public static extern int Syn_SelectIDCard(int iPortID, ref byte pucManaMsg, int iIfOpen);
        [DllImport("Syn_IDCardRead.dll", EntryPoint = "Syn_ReadMsg", CharSet = CharSet.Ansi)]
        public static extern int Syn_ReadMsg(int iPortID, int iIfOpen, ref IDCardData pIDCardData);
        [DllImport("Syn_IDCardRead.dll", EntryPoint = "Syn_ReadBaseMsg", CharSet = CharSet.Ansi)]
        public static extern int Syn_ReadBaseMsg(int iPort, IntPtr pucCHMsg, ref uint puiCHMsgLen, IntPtr pucPHMsg, ref uint puiPHMsgLen, int iIfOpen);

        /********************附加类API *****************************/
        [DllImport("Syn_IDCardRead.dll", EntryPoint = "Syn_SendSound", CharSet = CharSet.Ansi)]
        public static extern int Syn_SendSound(int iCmdNo);
        [DllImport("Syn_IDCardRead.dll", EntryPoint = "Syn_DelPhotoFile", CharSet = CharSet.Ansi)]
        public static extern void Syn_DelPhotoFile();

        private int lstPort = 0;

        private HttpListener HttpListener {get; set;}

        private EventLog logger = new EventLog("Application");
        private string port = null;

        public IDCardService()
        {
            InitializeComponent();
            logger.Source = "IDCardService";

            this.port = ConfigurationManager.AppSettings["ServerPort"];
            string prefix = String.Format("http://+:{0}/read/", this.port);
            HttpListener = new HttpListener();
            HttpListener.Prefixes.Add(prefix);

            lstMZ.Add("01", "汉族");
            lstMZ.Add("02", "蒙古族");
            lstMZ.Add("03", "回族");
            lstMZ.Add("04", "藏族");
            lstMZ.Add("05", "维吾尔族");
            lstMZ.Add("06", "苗族");
            lstMZ.Add("07", "彝族");
            lstMZ.Add("08", "壮族");
            lstMZ.Add("09", "布依族");
            lstMZ.Add("10", "朝鲜族");
            lstMZ.Add("11", "满族");
            lstMZ.Add("12", "侗族");
            lstMZ.Add("13", "瑶族");
            lstMZ.Add("14", "白族");
            lstMZ.Add("15", "土家族");
            lstMZ.Add("16", "哈尼族");
            lstMZ.Add("17", "哈萨克族");
            lstMZ.Add("18", "傣族");
            lstMZ.Add("19", "黎族");
            lstMZ.Add("20", "傈僳族");
            lstMZ.Add("21", "佤族");
            lstMZ.Add("22", "畲族");
            lstMZ.Add("23", "高山族");
            lstMZ.Add("24", "拉祜族");
            lstMZ.Add("25", "水族");
            lstMZ.Add("26", "东乡族");
            lstMZ.Add("27", "纳西族");
            lstMZ.Add("28", "景颇族");
            lstMZ.Add("29", "柯尔克孜族");
            lstMZ.Add("30", "土族");
            lstMZ.Add("31", "达翰尔族");
            lstMZ.Add("32", "仫佬族");
            lstMZ.Add("33", "羌族");
            lstMZ.Add("34", "布朗族");
            lstMZ.Add("35", "撒拉族");
            lstMZ.Add("36", "毛南族");
            lstMZ.Add("37", "仡佬族");
            lstMZ.Add("38", "锡伯族");
            lstMZ.Add("39", "阿昌族");
            lstMZ.Add("40", "普米族");
            lstMZ.Add("41", "塔吉克族");
            lstMZ.Add("42", "怒族");
            lstMZ.Add("43", "乌孜别克族");
            lstMZ.Add("44", "俄罗斯族");
            lstMZ.Add("45", "鄂温克族");
            lstMZ.Add("46", "德昂族");
            lstMZ.Add("47", "保安族");
            lstMZ.Add("48", "裕固族");
            lstMZ.Add("49", "京族");
            lstMZ.Add("50", "塔塔尔族");
            lstMZ.Add("51", "独龙族");
            lstMZ.Add("52", "鄂伦春族");
            lstMZ.Add("53", "赫哲族");
            lstMZ.Add("54", "门巴族");
            lstMZ.Add("55", "珞巴族");
            lstMZ.Add("56", "基诺族");
            lstMZ.Add("57", "其它");
            lstMZ.Add("98", "外国人入籍");
        }

        protected override void OnStart(string[] args)
        {
            HttpListener.Start();
            HttpListener.BeginGetContext(new AsyncCallback(RequestCallback), null);
            logger.WriteEntry("Service started at port "+this.port);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        private void RequestCallback(IAsyncResult result)
        {
            Encoding encoding = new UTF8Encoding(false); // Whenever I have UTF8 problems it's BOM's fault
            HttpListenerContext context = null;
            try
            {
                context = HttpListener.EndGetContext(result);
                IDCardData obj = DirectReadCard();
                string json = GetJsonByObject(obj);
                
                byte[] outputBytes = encoding.GetBytes(json);

                context.Response.ContentType = "application/json";
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.ContentLength64 = outputBytes.Length;
                context.Response.OutputStream.Write(outputBytes, 0, outputBytes.Length);
                //logger.WriteEntry("完成HTTP请求，已应答。");
            }
            catch(Exception ex)
            {
                byte[] outputBytes = encoding.GetBytes(ex.Message);
                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/plain";
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.ContentLength64 = outputBytes.Length;
                context.Response.OutputStream.Write(outputBytes, 0, outputBytes.Length);

                logger.WriteEntry("读取身份证出错："+ ex.Message);
            }
            finally
            {
                if (context != null && context.Response != null)
                {
                    context.Response.Close();
                }

                HttpListener.BeginGetContext(new AsyncCallback(RequestCallback), null);
            }
        }

        protected override void OnStop()
        {
            HttpListener.Close();
            logger.WriteEntry("HttpListener Closed.");
        }

        protected override void OnShutdown()
        {
            HttpListener.Close();
            logger.WriteEntry("HttpListener Closed.");
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool TryOpenUSB()
        {
            bool valid = false;
            while (true)
            {
                if (this.lstPort >= 1001 && Syn_OpenPort(this.lstPort) == 0)
                {
                    valid = true;
                    break;
                }
                else
                {
                    int iPort = 0;
                    for (iPort = 1001; iPort < 1017; iPort++)
                    {
                        if (iPort != this.lstPort && Syn_OpenPort(iPort) == 0)
                        {
                            valid = true;
                            this.lstPort = iPort;
                            break;
                        }
                    }
                }
                break;
            }
            if (!valid)
            {
                return false;
            }

            byte[] cSAMID = new byte[128];
            if (Syn_GetSAMStatus(this.lstPort, 0) == 0)
            {
                if (Syn_GetSAMIDToStr(this.lstPort, ref cSAMID[0], 0) == 0)
                {
                    //Syn_ClosePort(this.lstPort);
                    return true;
                }
            }

            Syn_ClosePort(this.lstPort);
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool TryOpenSerial()
        {
            bool valid = false;
            while (true)
            {
                if (this.lstPort >= 1 && this.lstPort <= 17 && Syn_OpenPort(this.lstPort) == 0)
                {
                    valid = true;
                    break;
                }
                else
                {
                    int iPort = 0;
                    for (iPort = 1; iPort < 17; iPort++)
                    {
                        if (iPort != this.lstPort && Syn_OpenPort(iPort) == 0)
                        {
                            valid = true;
                            this.lstPort = iPort;
                            break;
                        }
                    }
                }
                break;
            }
            if (!valid)
            {
                return false;
            }

            byte[] cSAMID = new byte[128];
            if (Syn_GetSAMStatus(this.lstPort, 0) == 0)
            {
                if (Syn_GetSAMIDToStr(this.lstPort, ref cSAMID[0], 0) == 0)
                {
                    //Syn_ClosePort(this.lstPort);
                    return true;
                }
            }

            Syn_ClosePort(this.lstPort);
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IDCardData DirectReadCard()
        {
            if (!TryOpenUSB())
            {
                if (!TryOpenSerial())
                {
                    throw new ApplicationException("连接读卡器失败");
                }
            }

            byte[] pucIIN = new byte[4];
            byte[] pucSN = new byte[8];
            IDCardData cardMsg = new IDCardData();

            int nRet = 0;
            nRet = Syn_StartFindIDCard(this.lstPort, ref pucIIN[0], 0);
            nRet = Syn_SelectIDCard(this.lstPort, ref pucSN[0], 0);

            string baseMsg = new string(' ', 256);  //身份证基本信息返回长度为256
            string imgMsg = new string(' ', 1024);  //身份证图片信息返回长度为1024
            IntPtr msg = Marshal.StringToHGlobalAnsi(baseMsg);  //身份证基本信息
            IntPtr img = Marshal.StringToHGlobalAnsi(imgMsg);   //身份证图片
            try
            {
                uint mLen = 0;
                uint iLen = 0;
                nRet = Syn_ReadBaseMsg(this.lstPort, msg, ref mLen, img, ref iLen, 1);
                if (nRet == 0)
                {
                    string card = Marshal.PtrToStringUni(msg);
                    char[] cartb = card.ToCharArray();
                    cardMsg.Name = (new string(cartb, 0, 15)).Trim();
                    cardMsg.Sex = new string(cartb, 15, 1);
                    cardMsg.Nation = new string(cartb, 16, 2);
                    cardMsg.Born = new string(cartb, 18, 8);
                    cardMsg.Address = (new string(cartb, 26, 35)).Trim();
                    cardMsg.IDCardNo = new string(cartb, 61, 18);
                    cardMsg.GrantDept = (new string(cartb, 79, 15)).Trim();
                    cardMsg.ValidBegin = new string(cartb, 94, 8);
                    cardMsg.ValidEnd = new string(cartb, 102, 8);
                    switch (cardMsg.Sex)
                    {
                        case "1":
                            cardMsg.SexName = "男";
                            break;
                        case "2":
                            cardMsg.SexName = "女";
                            break;
                    }
                    if (lstMZ.Contains(cardMsg.Nation))
                    {
                        cardMsg.NationName = lstMZ[cardMsg.Nation].ToString();
                    }
                    return cardMsg;
                }
                else
                {
                    throw new ApplicationException("读取身份证信息错误");
                }
            }
            catch (Exception e2)
            {
                throw new ApplicationException(e2.Message);
            }
            finally
            {
                Syn_ClosePort(this.lstPort);
                Marshal.FreeHGlobal(msg);
                Marshal.FreeHGlobal(img);
            }
        }

        private static string GetJsonByObject(Object obj)
        {
            //实例化DataContractJsonSerializer对象，需要待序列化的对象类型
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            //实例化一个内存流，用于存放序列化后的数据
            MemoryStream stream = new MemoryStream();
            //使用WriteObject序列化对象
            serializer.WriteObject(stream, obj);
            //写入内存流中
            byte[] dataBytes = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(dataBytes, 0, (int)stream.Length);
            //通过UTF8格式转换为字符串
            return Encoding.UTF8.GetString(dataBytes);
        }
    }
}
