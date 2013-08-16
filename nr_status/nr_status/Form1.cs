// CS1591.cs
// compile with: /W:4 /doc:headof.xml
using System;
using System.ComponentModel;
using System.IO.Ports;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Threading;
using GSM;
using SendMail;
namespace nr_status
{
    /// <summary>
    /// 主界面
    /// </summary>
    public partial class Form1 : Form
    {

        string sReadline;
        private bool starWarnFlag = false;
        private bool finWarnFlag = false;
        private bool sendWarnFlag = false;
        string WarningTXT;

        /// <summary>
        /// 端口实例
        /// </summary>
        public SerialPort sPort = new SerialPort();

        /// <summary>
        /// 端口打开标志
        /// </summary>
        public bool openFlag = false;

        /// <summary>
        /// 主界面
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            AllComNameTwo();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
        }

        /// <summary>
        /// 主界面载入触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            nrDate();
            starWarningInit();
            finWarningInit();
            comboBox1.SelectedIndex = 4; //默认ping延迟             
        }

        /// <summary>
        /// 端口参数设置
        /// </summary>
        private void portConfigration()
        {
            sPort.PortName = cbSerialPort.SelectedItem.ToString();
            sPort.BaudRate = 9600;
            sPort.StopBits = StopBits.One;
            sPort.Parity = Parity.None;
            sPort.DataBits = 8;
            sPort.ReadTimeout = -1; 
        }

        private void button1_Click(object sender, EventArgs e)
        { 
            if (button1.Text == "连接成功")
            {
                sPort.Close();
                if (!sPort.IsOpen)
                {
                    button1.Text = "连接设备";
                    cbSerialPort.Enabled = true;
                    ovalShape1.BackColor = System.Drawing.Color.Gray;
                    openFlag = false;
                    Msg("设备关闭");
                    return;
                }

            }
            else
            {
                portConfigration();
            }

            try
            {
                sPort.Open();
                openFlag = true;
                Msg("端口打开成功");
            }
            catch (UnauthorizedAccessException ex)
            {
                textBox1.Text += ex.Message;
                openFlag = false;
                //throw;
            }

            try
            {
                sPort.WriteLine("AT+CGMI\r");
                //sPort.WriteLine("ATD13810199085;\r");
            }
            catch (InvalidOperationException ex)
            {
                textBox1.Text += ex.Message;
                openFlag = false;

            }

            if (openFlag)
            {
                try
                {
                    while (true)
                    {
                        sPort.ReadTimeout = 2000;
                        sReadline = sPort.ReadLine();
                        Msg(sReadline);
                        if (sReadline.Trim() == "OK")
                        {
                            break;
                        }
                    }
                }
                catch (TimeoutException ex)
                {
                    Msg(ex.Message);
                    Msg("设备没有相应请检查端口");
                    openFlag = false;
                    sPort.Close();
                    //throw;
                }
            }

            if (openFlag)
            {
                ovalShape1.BackColor = System.Drawing.Color.Green;
                button1.Text = "连接成功";
                cbSerialPort.Enabled = false;

            }
            else
            {
                ovalShape1.BackColor = System.Drawing.Color.Red;
                sPort.Close();
            }
            //Console.WriteLine(sPort.IsOpen.ToString());
            //sPort.WriteLine("AT+CGMI\r");
            //sPort.WriteLine("ATD18311016537;\r");
            /*
            try
            {
                while (true)
                {
                    Console.WriteLine(sPort.ReadLine());
                }
            }
            catch (TimeoutException e)
            {
                Console.WriteLine(e.Message);
                //throw;
            }

            */
            //sPort.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 获取系统所有com接口
        /// </summary>
        public void AllComNameTwo()
        {
            //2,读取本机所有Com端口
            this.cbSerialPort.Items.Clear();
            string[] comNames = SerialPort.GetPortNames();
            //comNames = comNames.OrderBy(x=>x).ToArray();
            if (comNames.Length == 0)
            {
                Msg("No SerailPort exists");
                return;
            }
            foreach (string comName in comNames)
            {
                this.cbSerialPort.Items.Add(comName);
            }
            if (cbSerialPort.SelectedItem == null)
            {
                cbSerialPort.SelectedItem = cbSerialPort.Items[0];
            }
        }
        /// <summary>
        /// 在textbox1中显示
        /// </summary>
        /// <param name="text">内容</param>
        public void Msg(string text)
        {
            string txt="[" + DateTime.Now.ToString() + "]" + text + "\r\n" + textBox1.Text;
            textBox1.Text = txt;
            writeLog(txt);
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="log"></param>
        public void writeLog(string log)
        {
            FileStream fs = new FileStream("status.log", FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.Flush();
            sw.BaseStream.Seek(0, SeekOrigin.Begin);
            sw.WriteLine(log);
            sw.Flush();
            sw.Close();
        }
        
        private void ListView_Load()
        {
            //DES加密
            DES des = new DES();
            /*
            Msg(des.EncryptDES("storep", Properties.Resources.Seckey));
            Msg(des.EncryptDES("sister", Properties.Resources.Seckey));
            Msg(des.EncryptDES("oracle", Properties.Resources.Seckey));
            Msg(des.EncryptDES("change_storep", Properties.Resources.Seckey));
            Msg(des.DecryptDES("tu88IIYxbCM=", Properties.Resources.Seckey));
            */

            //载入总部
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("XML/headof.xml");
            XmlNode xn = xmlDoc.SelectSingleNode("headof");
            XmlNodeList xnl = xn.ChildNodes;
            int i = xn.ChildNodes.Count;
            //string[,] arr=new string[i,4];
            int x = 0;
            int y = 0;

            //清理lv
            //listView1.Groups.Clear();
            //listView1.Items.Clear();
           
            foreach (XmlNode xnf in xnl)
            {
                XmlElement xe = (XmlElement)xnf;
                string[] str = new string[7];          
                XmlNodeList xnf1 = xe.ChildNodes;
                int z = 1;
                string username = "";
                string password = "";
                int timeout = Convert.ToInt32(comboBox1.SelectedItem.ToString());
                foreach (XmlNode xn2 in xnf1)
                {
                    if (z == 1)
                    {
                        str[0] = xn2.InnerText;
                        str[1] = "";//IP
                        str[2] = "";//NR状态
                        str[3] = "";//Ping超时计数器
                        str[4] = "";//nr开始时间
                        str[5] = "";//nr结束时间
                        str[6] = nrDate();//nr日

                        Ping ping = new Ping();
                        //ping.PingCompleted += new PingCompletedEventHandler(this.PingCompletedCallBack);//设置PingCompleted事件处理程序  
                        //ping.SendAsync(xn2.InnerText,2000,null);
                        PingReply reply = ping.Send(xn2.InnerText, timeout);
                        if (reply.Status == IPStatus.Success)
                            str[1] = reply.RoundtripTime.ToString();
                    }
                    if (z == 2) username = xn2.InnerText;
                    if (z == 3) password = xn2.InnerText;
                    z++;
                    y++;
                }
                x++;
                y = 0;
                str[2] = nrCheck(xe.GetAttribute("name"), username, des.DecryptDES(password, Properties.Resources.Seckey));
                ListView1_Update(str, xe, 0);               
            }

            //载入门店            
            xmlDoc.Load("XML/store.xml");
            xn = xmlDoc.SelectSingleNode("store");
            xnl = xn.ChildNodes;
            i = xn.ChildNodes.Count;
            x = 0;
            y = 0;
            foreach (XmlNode xnf in xnl)
            {
                XmlElement xe = (XmlElement)xnf;
                string[] str = new string[7];            
                XmlNodeList xnf1 = xe.ChildNodes;
                int z = 1;
                string username = "";
                string password = "";
                int timeout = Convert.ToInt32(comboBox1.SelectedItem.ToString());
                foreach (XmlNode xn2 in xnf1)
                {
                    if (z == 1)
                    {
                        str[0] = xn2.InnerText;
                        str[1] = "";//IP
                        str[2] = "";//NR状态
                        str[3] = "";//Ping超时计数器
                        str[4] = "";//nr开始时间
                        str[5] = "";//nr结束时间
                        str[6] = nrDate();//nr日

                        Ping ping = new Ping();
                        PingReply reply = ping.Send(xn2.InnerText, timeout);
                        if (reply.Status == IPStatus.Success)
                            str[1] = reply.RoundtripTime.ToString();                        
                        
                        //ping.PingCompleted += new PingCompletedEventHandler(this.PingCompletedCallBack);//设置PingCompleted事件处理程序  
                        //ping.SendAsync(xn2.InnerText, 2000, null);
                    }
                    if (z == 2) username = xn2.InnerText;
                    if (z == 3) password = xn2.InnerText;

                    z++;
                    y++;
                }
                x++;
                y = 0;
                //nrCheck(xe.GetAttribute("name"), username, des.DecryptDES(password, Properties.Resources.Seckey));
                ListView1_Update(str, xe, 1);
               
            }
        }

        /// <summary>
        /// 更新listview
        /// </summary>
        /// <param name="str">数据组</param>
        /// <param name="xe">Xml元素</param>
        /// <param name="grpIdx">listview组index</param>
        private void ListView1_Update(string[] str,XmlElement xe ,int grpIdx)
        {
            ListViewItem li = new ListViewItem();
            li.SubItems[0].Text = xe.GetAttribute("name");
            //li.Name = Text = xe.GetAttribute("name");
            //更新listView
            bool ckUp = true;
            for (int ii = 0; ii < listView1.Items.Count; ii++)
            {
                if (listView1.Items[ii].SubItems[1].Text == str[0])
                {
                    listView1.Items[ii].SubItems[2].Text = str[1]; //ping
                    listView1.Items[ii].SubItems[3].Text = str[2]; //nr状态
                    ckUp = false;
                }
            }
            if (ckUp)
            {
                li.SubItems.AddRange(str);
                listView1.Items.Add(li);
                li.Group = listView1.Groups[grpIdx];
                //li.Group = lig;
            }
        }

        /// <summary>
        /// 用于异步交互的ping完毕
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">PingCompletedEventArgs</param>
        private void PingCompletedCallBack(object sender, PingCompletedEventArgs e)
        {
            //Msg(e.Reply.Status.ToString());
            if (e.Cancelled)
            {
                Msg("Ping Canncel");
                return;
            }
            if (e.Error != null)
            {
                Msg(e.Error.Message);
                return;
            }
            StringBuilder sbuilder;
            PingReply reply = e.Reply;
            if (reply.Status == IPStatus.Success)
            {
                sbuilder = new StringBuilder();
                sbuilder.Append(string.Format("Address: {0} ", reply.Address.ToString()));
                sbuilder.Append(string.Format("RoundTrip time: {0} ", reply.RoundtripTime));
                sbuilder.Append(string.Format("Time to live: {0} ", reply.Options.Ttl));
                sbuilder.Append(string.Format("Don't fragment: {0} ", reply.Options.DontFragment));
                sbuilder.Append(string.Format("Buffer size: {0} ", reply.Buffer.Length));
                //listBox1.Items.Add(sbuilder.ToString()); 
                Msg(reply.Status.ToString());
                listView_Update_Async(reply.Address.ToString(), reply.RoundtripTime.ToString());
            }
            else if (reply.Status == IPStatus.TimedOut)
            {
                Msg(IPStatus.TimedOut.ToString());
                //超时
                return;

            }

        }

        /// <summary>
        /// 查找更新listview 用于异步交互
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="time"></param>
        private void listView_Update_Async(string ip, string time)
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].SubItems[1].Text == ip)
                {
                    listView1.Items[i].SubItems[2].Text = time;
                    /*
                    int a = Convert.ToInt32(time);
                    if (a > 100)
                    {
                        listView1.Items[i].SubItems[2].ForeColor = Color.Red;
                    }
                    else
                    {
                        listView1.Items[i].SubItems[2].ForeColor = Color.Green;
                    }
                    */
                    //listView1.Items.Remove(listView1.Items[i]); //删除该条记录                   
                }
            }
        }

        /// <summary>
        /// ping超时
        /// </summary>
        private void listViewTimeOut()
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].SubItems[2].Text == "")
                {
                    listView1.Items[i].SubItems[2].Text = "超时";
                    if (listView1.Items[i].SubItems[4].Text == "")//超时计数器
                    {
                        listView1.Items[i].SubItems[4].Text = "0";
                    }
                    else
                    {
                        listView1.Items[i].SubItems[4].Text = Convert.ToString(Convert.ToInt32(listView1.Items[i].SubItems[4].Text) + 1);
                    }

                }
                else
                {
                    listView1.Items[i].SubItems[4].Text = "0"; //超时重置
                }

            }

        }

        private void button3_Click(object sender, EventArgs e)
        {

            if (this.backgroundWorker1.IsBusy)
            {
                return;
            }
            else
            {
                this.backgroundWorker1.RunWorkerAsync();
            }
     
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            ListView_Load();
            //Ping检测超时
            listViewTimeOut();
            //nr开始结束时间检查
            nrTimeStartAndFinish();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.backgroundWorker1.CancelAsync();
            //Msg("bw ok!");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //Msg(timer1.Enabled.ToString());
            if (timer1.Enabled)
            {
                timer1.Stop();
                Msg("NR 检测结束");
                button4.Text = "START";
                return;
            }
            timer1.Interval = 10000;
            timer1.Start();
            Msg("NR 检查开始");
            button4.Text = "STOP";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 获取NR建议日期
        /// </summary>
        /// <returns>yyyymmdd</returns>
        private string nrDate() 
        {
            string nrDate;
               nrDate = DateTime.Now.ToString("HH");
            if (Convert.ToInt32(nrDate) < 20)
            {
                nrDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                label1.Text = "NR检查日期：";
                label1.Text += DateTime.Now.AddDays(-1).ToString("yyyy/M/d");
                
            }
            else
            {
                nrDate = DateTime.Now.ToString("yyyyMMdd");
                label1.Text = "NR检查日期：";
                label1.Text += DateTime.Now.ToString("yyyy/M/d");
            }
            return nrDate;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Msg(nrDate());
        }

        private void nr_Finish_check(string hosts,string status)
        {
            string nrDate;
            nrDate = DateTime.Now.ToString("HH");
            if (Convert.ToInt32(nrDate) > 7 && (status == "未完成" || status == "未启动"))
            {
                Msg("NR未按时完成");
                
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 检查数据库状态
        /// </summary>
        /// <param name="oraId"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private string nrCheck(string oraId, string username, string password)
        {
            string nrStatus;
            string dateString;
            dateString = nrDate();
            //Msg(dateString);
            OraCtl _OraCtl = new OraCtl();
            nrStatus = _OraCtl.NrStatusCheck(dateString, oraId, username, password);
            //Msg(nrStatus);
            if (nrStatus == "no row selected") nrStatus = "未启动";
            if (nrStatus == "0") nrStatus = "已启动";
            if (nrStatus == "1") nrStatus = "已完成";
            return nrStatus;
        }
        
        /// <summary>
        /// 记录开始结束时间
        /// </summary>
        private void nrTimeStartAndFinish()
        {
             for (int i = 0; i < listView1.Items.Count; i++)
             {
                if(listView1.Items[i].SubItems[3].Text=="未启动")
                {
                    listView1.Items[i].SubItems[5].Text="";
                    listView1.Items[i].SubItems[6].Text="";
                }
                if(listView1.Items[i].SubItems[3].Text=="已启动" && listView1.Items[i].SubItems[5].Text=="")
                    listView1.Items[i].SubItems[5].Text=DateTime.Now.ToString();
                if(listView1.Items[i].SubItems[3].Text=="已完成" && listView1.Items[i].SubItems[6].Text=="")
                    listView1.Items[i].SubItems[6].Text=DateTime.Now.ToString();             
             }            
        }

        /// <summary>
        /// 读取联系人
        /// </summary>
        /// <returns></returns>
        private string[,] readContact()
        { 
            
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("XML/contact.xml");
            XmlNode xn = xmlDoc.SelectSingleNode("phone");
            XmlNodeList xnl = xn.ChildNodes;
            int i = xn.ChildNodes.Count;
            string[,] contact=new string[i,4];
            i = 0;
            foreach(XmlNode xnf in xnl)
            {
                XmlElement usr = (XmlElement)xnf;
                contact[i, 0] = usr.GetAttribute("name");
                contact[i, 1] = usr.GetAttribute("mobile");
                contact[i, 2] = usr.GetAttribute("email");

                string now=DateTime.Now.DayOfWeek.ToString();
                XmlNodeList xnf1 = usr.ChildNodes;
                foreach (XmlNode xn2 in xnf1)
                {
                    XmlElement week = (XmlElement)xn2;
                    if (week.Name == now) contact[i, 3] = week.InnerText;
                }

                i++;
            }
            return contact;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string[,] users = readContact();
            for (int i = 0; i < users.GetLength(0); i++)
            {
                Msg(users[i, 0] + " 手机：" + users[i, 1] + "邮箱：" + users[i, 2] + "状态：" + users[i, 3]);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {

            DES des = new DES();
            Msg(des.EncryptDES(textBox2.Text, Properties.Resources.Seckey));


            if (this.backgroundWorker2.IsBusy)
            {
                return;
            }
            else
            {
                this.backgroundWorker2.RunWorkerAsync();
            }
        }

        private void call()
        {
            if (this.backgroundWorker2.IsBusy)
            {
                return;
            }
            else
            {
                this.backgroundWorker2.RunWorkerAsync();                
            }            
        }

         /// <summary>
        /// 发送AT命令
        /// </summary>
        /// <param name="ATCom">AT命令</param>
        /// <returns></returns>
        private string sendAT(string ATCom)
        {
            if (!openFlag)
            {
                //Msg("未选择端口");
                return "未选择端口";
            }

            string str = string.Empty;
            //忽略接收缓冲区内容，准备发送
            sPort.DiscardInBuffer();
            sPort.DiscardOutBuffer();
            try
            {                
                sPort.WriteLine(ATCom + "\r");                
            }
            catch (Exception ex)                
            {                
                Msg(ex.ToString());
                throw ex;
            }
            Thread.Sleep(2000);
            try
            {
                string temp = string.Empty;
                while ((temp.Trim() != "OK") && (temp.Trim() != "ERROR") && (temp.Trim() != "BUSY") && (temp.Trim() != "NO CARRIER"))
                {
                    sPort.ReadTimeout = -1;
                    temp = sPort.ReadLine();
                    str += temp + "|";
                }
            }
            catch (Exception ex)
            {
                
                Msg(ex.ToString());
                throw ex;
            }
            
            return str;
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            string[,] users = readContact();
            for (int i = 0; i < users.GetLength(0); i++)
            {
                Msg(users[i, 0] + "手机：" + users[i, 1] + "邮箱：" + users[i, 2] + "状态：" + users[i, 3]);
                if (users[i, 3] == "1")
                {
                    //Msg(sendAT("atd10086;"));
                    Mail(WarningTXT, users[i, 2], "NR 状态警报" + DateTime.Now.ToString());
                    Msg(sendAT("ath"));//挂断
                    Msg("开始拨打电话:" + users[i, 1]);                    
                    Msg(sendAT("atd" + users[i, 1] + ";"));
                }
            } 
            //Msg("开始拨打电话:" + e.Argument.ToString());
            //Msg(sendAT(e.Argument.ToString()));
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.backgroundWorker2.CancelAsync();
        }

        private void clockTimer_Tick(object sender, EventArgs e)
        {
            label3.Text = "Clock:" + DateTime.Now.ToString();

            //开启预警
            int hh = Convert.ToInt32(DateTime.Now.ToString("HH"));
            int mm = Convert.ToInt32(DateTime.Now.ToString("mm"));
            int ss = Convert.ToInt32(DateTime.Now.ToString("ss"));
            //Msg(hh.ToString() + mm.ToString() + ss.ToString());
            if (finWarnFlag && hh == Convert.ToInt32(finHourBox.Text) && mm == Convert.ToInt32(finMinuteBox.Text) && ss == 0)
            {
                Msg("NR结束预警start!");
                nrWarnig("fin");
            }
            if (starWarnFlag && hh == Convert.ToInt32(starHourBox.Text) && mm == Convert.ToInt32(starMinuteBox.Text) && ss == 0)
            {
                Msg("NR开始预警start!");
                nrWarnig("star");
            }
        }

        private void finWarningInit()
        {
            for (int i = 5; i < 9; i++)
            {
                finHourBox.Items.Add(i.ToString());
            }
            finHourBox.SelectedItem = "7";
            for (int i = 0; i < 60; i++)
            {
                finMinuteBox.Items.Add(i.ToString());
            }
            finMinuteBox.SelectedItem = "0";
        }

        private void starWarningInit()
        {
            for (int i = 0; i < 5; i++)
            {
                starHourBox.Items.Add(i.ToString());
            }
            starHourBox.SelectedItem = "1";
            for (int i = 0; i < 60; i++)
            {
                starMinuteBox.Items.Add(i.ToString());
            }
            starMinuteBox.SelectedItem = "0";
        }

        private void finWarningSwitch_Click(object sender, EventArgs e)
        {
            if (!openFlag) Msg("GSM Modem未连接！");
            if (finWarningSwitch.Text == "开启")
            {
                finWarningSwitch.Text = "关闭";
                nrFinGrp.Text = "NR结束预警[开启]";
                finHourBox.Enabled = false;
                finMinuteBox.Enabled = false;
                finWarnFlag = true;
            }
            else
            {
                finWarningSwitch.Text = "开启";
                nrFinGrp.Text = "NR结束预警[关闭]";
                finWarnFlag = false;
                finHourBox.Enabled = true;
                finMinuteBox.Enabled = true;
            }
        }

        private void starWarningSwitch_Click(object sender, EventArgs e)
        {
            if (!openFlag) Msg("GSM Modem未连接！");
            if (starWarningSwitch.Text == "开启")
            {
                starWarningSwitch.Text = "关闭";
                nrStarGrp.Text = "NR启动预警[开启]";
                starWarnFlag = true;
                starHourBox.Enabled = false;
                starMinuteBox.Enabled = false;
            }
            else
            {
                starWarningSwitch.Text = "开启";
                nrStarGrp.Text = "NR启动预警[关闭]";
                starWarnFlag = false;
                starHourBox.Enabled = true;
                starMinuteBox.Enabled = true;       
            }
        }

        /// <summary>
        /// 警告触发事件
        /// </summary>
        /// <param name="flag">触发类型[fin]|[star]</param>
        private void nrWarnig(string flag)
        {
            string descr="";
            string finDescr="";
            if(flag=="fin") 
            {
                descr = "已完成";
                finDescr="未完成";
            }
            if (flag == "star") 
            {
                descr = "已启动";
                finDescr="未启动";
            }
            sendWarnFlag = false;
            if (listView1.Groups[0].Items.Count == 0)
            {
                Msg("未找到服务器状态记录！");
                return;
            }

            //判断是否完成
            WarningTXT = "";
            for (int i = 0; i < listView1.Groups[0].Items.Count; i++)
            {
                if (listView1.Groups[0].Items[i].SubItems[3].Text != descr)
                {
                    sendWarnFlag = true;
                    WarningTXT += listView1.Groups[0].Items[i].SubItems[0].Text + " " + listView1.Groups[0].Items[i].SubItems[1].Text + " " + "NR" + finDescr + "<br />";
                    Msg(listView1.Groups[0].Items[i].SubItems[0].Text + "NR" + finDescr);                    
                }
            }
            if (sendWarnFlag)
            {
                if (openFlag)
                {

                    if (backgroundWorker2.IsBusy)
                    {
                        Msg("呼叫等待中");
                    }
                    else
                    {
                        call();
                    }
                }
                else
                {
                    Msg("GSM Modem未连接！");
                }
            }
                
                
        }

        /// <summary>
        /// 发送警报邮件
        /// </summary>
        /// <param name="mailBody">正文</param>
        /// <param name="mailTo">收件人</param>
        /// <param name="subject">标题</param>
        private void Mail(string mailBody,string mailTo,string subject)
        {

            DES des = new DES();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("XML/smtp.xml");
            XmlNode xn = xmlDoc.SelectSingleNode("set");
            XmlElement xl = (XmlElement)xn.ChildNodes[0];
            
            //Msg(xl.GetAttribute("host") + xl.GetAttribute("user") + xl.GetAttribute("passwd"));

            EmailParms mailstr = new EmailParms();
            mailstr.EmailBody = mailBody;
            mailstr.EmailHostName = xl.GetAttribute("host");
            mailstr.EmailPersonName = "NR STATUS警报";
            mailstr.EmailPort = Convert.ToInt32(xl.GetAttribute("port"));
            mailstr.EmailPriority = "high";
            mailstr.EmailSubject = subject;
            mailstr.EncodingType = "UTF8";
            mailstr.FromEmailAddress = xl.GetAttribute("user");
            mailstr.FromEmailPassword = des.DecryptDES(xl.GetAttribute("passwd"), Properties.Resources.Seckey);
            mailstr.isBodyHtml = true;
            mailstr.isEnableSsl = false;
            mailstr.ToEmailAddress = mailTo;
            if (sendmail.SendingEmail(mailstr))
            {
                Msg("邮件成功发送给：" + mailTo);
            }
            else
            {
                Msg(mailTo + "发送失败");
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

    }
}
