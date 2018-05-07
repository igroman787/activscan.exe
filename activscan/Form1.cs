using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hooks;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace activscan
{
    public partial class Form1 : Form
    {
        private string host = "schistory.space";
        private int port = 4800;

        private Thread t0, t1, t2, t3;

        private List<string> nicknameList = new List<string>();
        private string lastNicknameFromChat = "";

        private bool isChatUsing = false;
        private int chatLength = 1;
        private bool isMousUsing = false;
        private int timerSeconds = 3000;

        public Form1()
        {
            InitializeComponent();

            if (File.Exists("log.txt"))
            {
                File.Delete("log.txt");
            }

            this.FormClosed += new FormClosedEventHandler(frmMain_FormClosed);
            MouseHook.MouseDown += new MouseEventHandler(MouseHook_MouseDown);
            MouseHook.MouseMove += new MouseEventHandler(MouseHook_MouseMove);
            MouseHook.MouseUp += new MouseEventHandler(MouseHook_MouseUp);
            MouseHook.LocalHook = false;
            MouseHook.InstallHook();

            // Start the asynchronous operation.
            BackgroundWorker backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork); // Read game logs
            backgroundWorker1.RunWorkerAsync();

            // Start the asynchronous operation.
            BackgroundWorker backgroundWorker2 = new BackgroundWorker();
            backgroundWorker2.DoWork += new DoWorkEventHandler(backgroundWorker2_DoWork); // Check is mous move with user
            backgroundWorker2.RunWorkerAsync();

            // Start the asynchronous operation.
            BackgroundWorker backgroundWorker3 = new BackgroundWorker();
            backgroundWorker3.DoWork += new DoWorkEventHandler(backgroundWorker3_DoWork); // Mous move per 30s
            backgroundWorker3.RunWorkerAsync();

            

            
        }

        private void button1_Click(object sender, EventArgs e) // Start
        {
            button1.Text = "Working...";
            button1.Enabled = false;
            textBox1.ReadOnly = true;

            // Start the asynchronous operation.
            t1 = new Thread(new ThreadStart(General));
            t1.SetApartmentState(ApartmentState.STA);
            t1.IsBackground = true;
            if (checkBox1.Checked == true)
            {
                t1.Start();
            }

            // Start the asynchronous operation.
            t2 = new Thread(new ThreadStart(Census));
            t2.SetApartmentState(ApartmentState.STA);
            t2.IsBackground = true;
            if (checkBox2.Checked == true)
            {
                t2.Start();
            }

            // Start the asynchronous operation.
            t3 = new Thread(new ThreadStart(AdvertisingFromFile));
            t3.SetApartmentState(ApartmentState.STA);
            t3.IsBackground = true;
            if (checkBox3.Checked == true)
            {
                t3.Start();
            }
        }
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            e.Handled = ((c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я') || c == 'Ё' || c == 'ё');
        }
        private void General()
        {
            AddLog("Start General mod");

            Thread.Sleep(5000);
            while (true)
            {
                CheckLostUids();
                Thread.Sleep(21600000);
            }
        }
        private void Census()
        {
            AddLog("Start Census mod");

            Thread.Sleep(5000);
            while (true)
            {
                // Получить пачку uid's
                string buffer;
                buffer = GetDataFromBrowser("http://ts2.scorpclub.ru/api/v1/getuids.php");
                Uids uids = JsonConvert.DeserializeObject<Uids>(buffer);

                // Пропинговать каждый uid
                foreach (Int64 uid in uids.data.uid.Distinct())
                {
                    AddLog("ping uid: " + uid, "debug");

                    ChatBlock();
                    EmulSendText("/w #" + uid + " ping");
                    Thread.Sleep(1000);
                    EmulSendText("/w #" + uid + " pong");
                    Thread.Sleep(2000);
                    ChatUnlock();
                }

                // Полученные ники отправить на ts2.scorpclub.ru
                string[] bufferArr = nicknameList.ToArray();
                nicknameList.Clear();
                foreach (string nickname in bufferArr)
                {
                    AddLog("Send nickname: " + nickname + " --> [ts2.scorpclub.ru]");
                    string url = "http://ts2.scorpclub.ru/api/v1/addnickname.php?nickname=" + nickname + "&searcher=" + textBox1.Text;
                    buffer = GetDataFromBrowser(url);
                }
            }
        }
        private void AdvertisingFromFile()
        {
            AddLog("Start AdvertisingFromFile mod");

            Thread.Sleep(5000);
            while (true)
            {
                AdFromFile();
                Thread.Sleep(500000);
            }
        }

        private void Ad()
        {
            AddLog("Start Ad", "debug");

            if (lastNicknameFromChat.Length < 1)
            {
                return;
            }
            string nickanme = lastNicknameFromChat;
            string text = nickanme + ", Хочешь посмотреть свою историю? заходи на http://schistory.space/userinfo.php?nickname=" + nickanme;

            ChatBlock();
            EmulSendText(text);
            ChatUnlock();
        }
        private void AdFromFile()
        {
            AddLog("Start AdFromFile", "debug");

            var arr = new object[5];
            var buffer = new string[3];
            buffer[0] = "===================";
            buffer[1] = "Играешь в Star Conflict и хочешь посмотреть свой ежедневный результат? Заходи на http://schistory.space/";
            buffer[2] = "===================";
            arr[0] = buffer.ToArray();

            buffer[0] = "===================";
            buffer[1] = "Играешь в Star Conflict и хочешь найти достойных игроков в свою корпорацию? Заходи на http://schistory.space/";
            buffer[2] = "===================";
            arr[1] = buffer.ToArray();

            buffer[0] = "===================";
            buffer[1] = "Играешь в Star Conflict и хочешь посмотреть статистику своих противников? Заходи на http://schistory.space/";
            buffer[2] = "===================";
            arr[2] = buffer.ToArray();

            buffer[0] = "===================";
            buffer[1] = "Просто http://schistory.space/ - Просто о сложном";
            buffer[2] = "===================";
            arr[3] = buffer.ToArray();

            buffer[0] = "===================";
            buffer[1] = "Заходи на http://schistory.space/ и добавляй в закладки ;)";
            buffer[2] = "===================";
            arr[4] = buffer.ToArray();

            foreach (string[] item in arr)
            {
                while (chatLength < 100)
                {
                    Thread.Sleep(1000);
                }
                chatLength = 1;

                ChatBlock();
                foreach (string text in item)
                {
                    EmulSendText(text);
                }
                ChatUnlock();
            }
        }
        private void CheckLostUids()
        {
            AddLog("Start CheckLostUids", "debug");

            string buffer;
            buffer = GetDataFromBrowser("http://schistory.space/api/v1/getlostuids.php");
            Uids uids = JsonConvert.DeserializeObject<Uids>(buffer);

            foreach (Int64 uid in uids.data.uid.Distinct())
            {
                buffer = GetDataFromBrowser("http://schistory.space/api/v1/userinfo.php?uid=" + uid + "&limit=1");
                Space space = JsonConvert.DeserializeObject<Space>(buffer);
                if (space.bigdata.Count == 1 && GetDataFromSC(space.bigdata[0].nickname).code == 0 && GetDataFromSC(space.bigdata[0].nickname).data.uid == uid)
                {
                    AddLog("uid: " + uid + " is correct");
                    continue;
                }
                AddLog("ping uid: " + uid, "debug");

                ChatBlock();
                EmulSendText("/w #" + uid + " ping");
                Thread.Sleep(1000);
                EmulSendText("/w #" + uid + " pong");
                Thread.Sleep(2000);
                ChatUnlock();
            }

            TcpClient tcpClient = new TcpClient(host, port);
            NetworkStream stream = tcpClient.GetStream();
            string[] bufferArr = nicknameList.ToArray();
            nicknameList.Clear();
            foreach (string item in bufferArr)
            {
                string message = "<nickname>" + item + "</nickname>";
                TcpSend(message, stream);
            }

            stream.Close();
            tcpClient.Close();
            AddLog("Find nicknames: " + String.Join(", ", bufferArr));
        }
        private void EmulSendText(string text)
        {
            AddLog("Start EmulSendText: " + text, "debug");
            
            try
            {
                Process[] processes = Process.GetProcessesByName("game");
                Process game1 = processes[0];
                IntPtr p = game1.MainWindowHandle;
                SetForegroundWindow(p);
            }
            catch
            {
                AddLog("The game is not running. Continue the default output.", "warning");
            }

            Clipboard.SetText(text);
            Thread.Sleep(100);
            Pause();
            //SendKeys.SendWait("^{v}");
            Keyboard.SendKeyDown(Keyboard.KeyCode.CONTROL);
            Keyboard.SendKeyPress(Keyboard.KeyCode.KEY_V);
            Keyboard.SendKeyUp(Keyboard.KeyCode.CONTROL);
            //SendKeys.SendWait(Clipboard.GetText());
            Thread.Sleep(200);
            Pause();
            //SendKeys.SendWait("{ENTER}");
            Keyboard.SendKeyPress(Keyboard.KeyCode.ENTER);
            Thread.Sleep(500);
            Pause();
        }
        private string GetDataFromBrowser(string url)
        {
            WebClient client = new WebClient();
            string buffer;

            for (int i = 1; i < 5; i++)
            {
                try
                {
                    buffer = client.DownloadString(url);
                    return buffer;
                }
                catch (Exception err)
                {
                    AddLog("GetDataFromBrowser: Attempt: " + i + " " + err, "warning");
                    Thread.Sleep(i*1000);
                }
            }
            return null;
        }
        private SC GetDataFromSC(string nickname)
        {
            WebClient client = new WebClient();
            string buffer = client.DownloadString("http://gmt.star-conflict.com/pubapi/v1/userinfo.php?nickname=" + nickname);
            SC sc = JsonConvert.DeserializeObject<SC>(buffer);

            if (sc.code == 1)
            {
                return sc;
            }

            if (sc.data.clan == null)
            {
                sc.data.clan = new SCdataClan();
                sc.data.clan.name = "-------------------";
                sc.data.clan.tag = "-----";
            }
            if (sc.data.clan.tag == null)
            {
                sc.data.clan.tag = "-----";
            }
            if (sc.data.pvp == null || sc.data.pvp.gamePlayed == null || sc.data.pvp.gameWin == null || sc.data.pvp.totalAssists == null
                || sc.data.pvp.totalBattleTime == null || sc.data.pvp.totalDeath == null || sc.data.pvp.totalDmgDone == null
                || sc.data.pvp.totalHealingDone == null || sc.data.pvp.totalKill == null || sc.data.pvp.totalVpDmgDone == null)
            {
                sc.data.pvp = new SCdataPvp();
                sc.data.pvp.gamePlayed = 0;
                sc.data.pvp.gameWin = 0;
                sc.data.pvp.totalAssists = 0;
                sc.data.pvp.totalBattleTime = 0;
                sc.data.pvp.totalDmgDone = 0;
                sc.data.pvp.totalHealingDone = 0;
                sc.data.pvp.totalKill = 0;
                sc.data.pvp.totalVpDmgDone = 0;
            }

            return sc;
        }
        private void Pause()
        {
            if (isMousUsing)
            {
                Console.Beep(1000, 1000);
            }
            while (isMousUsing)
            {
                Thread.Sleep(100);
            }
        }
        private void ChatBlock()
        {
            while (isChatUsing)
            {
                Thread.Sleep(1000);
            }
            isChatUsing = true;
        }
        private void ChatUnlock()
        {
            isChatUsing = false;
        }

        private string TcpSend(string sendText, NetworkStream stream)
        {
            byte[] buffer = new byte[2048];
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(sendText);
            stream.Write(data, 0, data.Length);
            //int bytes = stream.Read(buffer, 0, buffer.Length);
            //Decoder decoder = Encoding.UTF8.GetDecoder();
            //char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];

            StringBuilder messageData = new StringBuilder();
            int bytes = -1;
            do
            {
                bytes = stream.Read(buffer, 0, buffer.Length);

                // Use Decoder class to convert from bytes to UTF8
                // in case a character spans two buffers.
                Decoder decoder = Encoding.UTF8.GetDecoder();
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);
                // Check for EOF.
                if (messageData.ToString().IndexOf("<EOF>") != -1)
                {
                    break;
                }
            } while (bytes != 0);
            string outputText = messageData.ToString();
            if (outputText.IndexOf("<EOF>") > -1) { outputText = outputText.Remove(outputText.IndexOf("<EOF>")); }
            return outputText;
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            AddLog("Start read game logs", "debug");

            string gamelogs_route = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\StarConflict\logs";
            List<string> dirs = new List<string>(Directory.EnumerateDirectories(gamelogs_route));
            string good_log = dirs[dirs.Count - 1] + @"\";

            FileStream gamelog_file = new FileStream(good_log + "chat.log", FileMode.Open, FileAccess.Read, FileShare.ReadWrite); //создаем файловый поток
            StreamReader gamelog_reader = new StreamReader(gamelog_file);


            while (true)
            {
                string item = gamelog_reader.ReadLine();

                if (item == null || item.Length == 0) // Only text
                {
                    Thread.Sleep(1000);
                    continue;
                }
                if (item.IndexOf("CHAT") < 0 || item.IndexOf('[') < 0) // Only chat and only with nickname
                {
                    continue;
                }

                string nickname = item.Remove(0, item.IndexOf('[') + 1);
                nickname = nickname.Remove(nickname.IndexOf(']'));
                nickname = nickname.Trim();
                lastNicknameFromChat = nickname;

                if (item.IndexOf("#general") > 0)
                {
                    chatLength += 1;
                    AddLog("chatLength: " + chatLength, "debug");
                }

                if (item.IndexOf("PRIVATE") < 0) // Only privat chat
                {
                    continue;
                }

                if (nickname.Length > 1 && nicknameList.IndexOf(nickname) < 0 &&
                    nickname.IndexOf("UID") < 0 && nickname.IndexOf("[") < 0 && nickname.IndexOf("(") < 0)
                {
                    AddLog("Find new nickname: " + nickname);
                    nicknameList.Add(nickname);
                }
            }
        }
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            AddLog("Start check is mous move with user", "debug");

            while (true)
            {
                if (isMousUsing == true && timerSeconds == 0)
                {
                    isMousUsing = false;
                    Console.Beep(1200, 500);
                }
                if (isMousUsing)
                {
                    while (timerSeconds > 0)
                    {
                        timerSeconds -= 100;
                        Thread.Sleep(100);
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }
        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            AddLog("Start move mouse per 30s", "debug");

            while (true)
            {
                Thread.Sleep(30000);
                Jiggler.Jiggle(3, 3);
                Thread.Sleep(30000);
                Jiggler.Jiggle(-3, -3);
            }
        }

        private void AddLog(string inputText, string mode = "info")
        {
            DateTime localDate = DateTime.Now;
            string timeNow = localDate.ToString();
            string logText =  timeNow + " [" + mode + "] " + inputText;
            //File.AppendAllText("log.txt", logText + "\r\n");

            try
            {
                BeginInvoke(new MethodInvoker(delegate { listBox1.Items.Add(logText); }));
                BeginInvoke(new MethodInvoker(delegate { listBox1.SelectedIndex = listBox1.Items.Count - 1; }));
            }
            catch
            {
                listBox1.Items.Add(logText);
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
            }
        }

        void MouseHook_MouseMove(object sender, MouseEventArgs e)
        {
            isMousUsing = true;
            timerSeconds = 3000;
        }
        void MouseHook_MouseUp(object sender, MouseEventArgs e)
        {
            
        }
        void MouseHook_MouseDown(object sender, MouseEventArgs e)
        {
            
        }
        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            MouseHook.UnInstallHook(); // Обязательно !!!
            if (t1 != null)
            {
                t1.Abort();
            }
            if (t2 != null)
            {
                t2.Abort();
            }
            if (t3 != null)
            {
                t3.Abort();
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        

        
    }



    public class Uids
    {
        public int result { get; set; }
        public string text { get; set; }
        public UidsData data { get; set; }
    }
    public class UidsData
    {
        public List<Int64> uid { get; set; }
    }

    public class Space
    {
        public int result { get; set; }
        public string text { get; set; }
        public List<SpaceBigdata> bigdata { get; set; }
    }
    public class SpaceBigdata : IComparable<SpaceBigdata>
    {
        public DateTime date { get; set; }
        public Int64 uid { get; set; }
        public string nickname { get; set; }
        public Int64 effRating { get; set; }
        public Int64 karma { get; set; }
        public double prestigeBonus { get; set; }
        public Int64 gamePlayed { get; set; }
        public Int64 gameWin { get; set; }
        public Int64 totalAssists { get; set; }
        public Int64 totalBattleTime { get; set; }
        public Int64 totalDeath { get; set; }
        public Int64 totalDmgDone { get; set; }
        public Int64 totalHealingDone { get; set; }
        public Int64 totalKill { get; set; }
        public Int64 totalVpDmgDone { get; set; }
        public string clanName { get; set; }
        public string clanTag { get; set; }

        public int CompareTo(SpaceBigdata p)
        {
            return this.date.CompareTo(p.date);
        }
    }

    public class SC : IEquatable<SC>, IComparable<SC>
    {
        public string result { get; set; }
        public int code { get; set; }
        public string text { get; set; }
        public SCdata data { get; set; }
        public DateTime date { get; set; }

        public bool Equals(SC sc)
        {
            if (sc.code == 2 && this.data.nickname == sc.data.nickname)
            {
                return true;
            }
            else if (this.data.nickname == sc.data.nickname && this.date.Year == sc.date.Year && this.date.Month == sc.date.Month && this.date.Day == sc.date.Day)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public int CompareTo(SC p)
        {
            return this.date.CompareTo(p.date);
        }
    }
    public class SCdata
    {
        public Int64 effRating { get; set; }
        public Int64 karma { get; set; }
        public string nickname { get; set; }
        public double prestigeBonus { get; set; }
        public Int64 uid { get; set; }
        public SCdataPvp pvp { get; set; }
        public SCdataClan clan { get; set; }
    }
    public class SCdataPvp
    {
        public Int64 gamePlayed { get; set; }
        public Int64 gameWin { get; set; }
        public Int64 totalAssists { get; set; }
        public Int64 totalBattleTime { get; set; }
        public Int64 totalDeath { get; set; }
        public Int64 totalDmgDone { get; set; }
        public Int64 totalHealingDone { get; set; }
        public Int64 totalKill { get; set; }
        public Int64 totalVpDmgDone { get; set; }
    }
    public class SCdataClan
    {
        public string name { get; set; }
        public string tag { get; set; }
    }

    public class Jiggler
    {
        internal const int INPUT_MOUSE = 0;
        internal const int MOUSEEVENTF_MOVE = 0x0001;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

        public static void Jiggle(int dx, int dy)
        {
            var inp = new INPUT();
            inp.TYPE = Jiggler.INPUT_MOUSE;
            inp.dx = dx;
            inp.dy = dy;
            inp.mouseData = 0;
            inp.dwFlags = Jiggler.MOUSEEVENTF_MOVE;
            inp.time = 0;
            inp.dwExtraInfo = (IntPtr)0;

            if (SendInput(1, ref inp, 28) != 1)
                throw new Win32Exception();
        }
    }
    internal struct INPUT
    {
        public int TYPE;
        public int dx;
        public int dy;
        public int mouseData;
        public int dwFlags;
        public int time;
        public IntPtr dwExtraInfo;
    }

    public class Keyboard
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint numberOfInputs, INPUT[] inputs, int sizeOfInputStructure);

        public static void SendKeyPress(KeyCode keyCode)
        {
            INPUT input = new INPUT
            {
                Type = 1
            };
            input.Data.Keyboard = new KEYBDINPUT()
            {
                Vk = (ushort)keyCode,
                Scan = 0,
                Flags = 0,
                Time = 0,
                ExtraInfo = IntPtr.Zero,
            };

            INPUT input2 = new INPUT
            {
                Type = 1
            };
            input2.Data.Keyboard = new KEYBDINPUT()
            {
                Vk = (ushort)keyCode,
                Scan = 0,
                Flags = 2,
                Time = 0,
                ExtraInfo = IntPtr.Zero
            };
            INPUT[] inputs = new INPUT[] { input, input2 };
            if (SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT))) == 0)
                throw new Exception();
        }

        /// <summary>
        /// Send a key down and hold it down until sendkeyup method is called
        /// </summary>
        /// <param name="keyCode"></param>
        public static void SendKeyDown(KeyCode keyCode)
        {
            INPUT input = new INPUT
            {
                Type = 1
            };
            input.Data.Keyboard = new KEYBDINPUT();
            input.Data.Keyboard.Vk = (ushort)keyCode;
            input.Data.Keyboard.Scan = 0;
            input.Data.Keyboard.Flags = 0;
            input.Data.Keyboard.Time = 0;
            input.Data.Keyboard.ExtraInfo = IntPtr.Zero;
            INPUT[] inputs = new INPUT[] { input };
            if (SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT))) == 0)
            {
                throw new Exception();
            }
        }

        /// <summary>
        /// Release a key that is being hold down
        /// </summary>
        /// <param name="keyCode"></param>
        public static void SendKeyUp(KeyCode keyCode)
        {
            INPUT input = new INPUT
            {
                Type = 1
            };
            input.Data.Keyboard = new KEYBDINPUT();
            input.Data.Keyboard.Vk = (ushort)keyCode;
            input.Data.Keyboard.Scan = 0;
            input.Data.Keyboard.Flags = 2;
            input.Data.Keyboard.Time = 0;
            input.Data.Keyboard.ExtraInfo = IntPtr.Zero;
            INPUT[] inputs = new INPUT[] { input };
            if (SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT))) == 0)
                throw new Exception();

        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646270(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct INPUT
        {
            public uint Type;
            public MOUSEKEYBDHARDWAREINPUT Data;
        }

        /// <summary>
        /// http://social.msdn.microsoft.com/Forums/en/csharplanguage/thread/f0e82d6e-4999-4d22-b3d3-32b25f61fb2a
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        internal struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public HARDWAREINPUT Hardware;
            [FieldOffset(0)]
            public KEYBDINPUT Keyboard;
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646310(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct HARDWAREINPUT
        {
            public uint Msg;
            public ushort ParamL;
            public ushort ParamH;
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646310(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct KEYBDINPUT
        {
            public ushort Vk;
            public ushort Scan;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        /// <summary>
        /// http://social.msdn.microsoft.com/forums/en-US/netfxbcl/thread/2abc6be8-c593-4686-93d2-89785232dacd
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            public int X;
            public int Y;
            public uint MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        public enum KeyCode : ushort
        {
            #region Media

            /// <summary>
            /// Next track if a song is playing
            /// </summary>
            MEDIA_NEXT_TRACK = 0xb0,

            /// <summary>
            /// Play pause
            /// </summary>
            MEDIA_PLAY_PAUSE = 0xb3,

            /// <summary>
            /// Previous track
            /// </summary>
            MEDIA_PREV_TRACK = 0xb1,

            /// <summary>
            /// Stop
            /// </summary>
            MEDIA_STOP = 0xb2,

            #endregion

            #region math

            /// <summary>Key "+"</summary>
            ADD = 0x6b,
            /// <summary>
            /// "*" key
            /// </summary>
            MULTIPLY = 0x6a,

            /// <summary>
            /// "/" key
            /// </summary>
            DIVIDE = 0x6f,

            /// <summary>
            /// Subtract key "-"
            /// </summary>
            SUBTRACT = 0x6d,

            #endregion

            #region Browser
            /// <summary>
            /// Go Back
            /// </summary>
            BROWSER_BACK = 0xa6,
            /// <summary>
            /// Favorites
            /// </summary>
            BROWSER_FAVORITES = 0xab,
            /// <summary>
            /// Forward
            /// </summary>
            BROWSER_FORWARD = 0xa7,
            /// <summary>
            /// Home
            /// </summary>
            BROWSER_HOME = 0xac,
            /// <summary>
            /// Refresh
            /// </summary>
            BROWSER_REFRESH = 0xa8,
            /// <summary>
            /// browser search
            /// </summary>
            BROWSER_SEARCH = 170,
            /// <summary>
            /// Stop
            /// </summary>
            BROWSER_STOP = 0xa9,
            #endregion

            #region Numpad numbers
            /// <summary>
            /// 
            /// </summary>
            NUMPAD0 = 0x60,
            /// <summary>
            /// 
            /// </summary>
            NUMPAD1 = 0x61,
            /// <summary>
            /// 
            /// </summary>
            NUMPAD2 = 0x62,
            /// <summary>
            /// 
            /// </summary>
            NUMPAD3 = 0x63,
            /// <summary>
            /// 
            /// </summary>
            NUMPAD4 = 100,
            /// <summary>
            /// 
            /// </summary>
            NUMPAD5 = 0x65,
            /// <summary>
            /// 
            /// </summary>
            NUMPAD6 = 0x66,
            /// <summary>
            /// 
            /// </summary>
            NUMPAD7 = 0x67,
            /// <summary>
            /// 
            /// </summary>
            NUMPAD8 = 0x68,
            /// <summary>
            /// 
            /// </summary>
            NUMPAD9 = 0x69,

            #endregion

            #region Fkeys
            /// <summary>
            /// F1
            /// </summary>
            F1 = 0x70,
            /// <summary>
            /// F10
            /// </summary>
            F10 = 0x79,
            /// <summary>
            /// 
            /// </summary>
            F11 = 0x7a,
            /// <summary>
            /// 
            /// </summary>
            F12 = 0x7b,
            /// <summary>
            /// 
            /// </summary>
            F13 = 0x7c,
            /// <summary>
            /// 
            /// </summary>
            F14 = 0x7d,
            /// <summary>
            /// 
            /// </summary>
            F15 = 0x7e,
            /// <summary>
            /// 
            /// </summary>
            F16 = 0x7f,
            /// <summary>
            /// 
            /// </summary>
            F17 = 0x80,
            /// <summary>
            /// 
            /// </summary>
            F18 = 0x81,
            /// <summary>
            /// 
            /// </summary>
            F19 = 130,
            /// <summary>
            /// 
            /// </summary>
            F2 = 0x71,
            /// <summary>
            /// 
            /// </summary>
            F20 = 0x83,
            /// <summary>
            /// 
            /// </summary>
            F21 = 0x84,
            /// <summary>
            /// 
            /// </summary>
            F22 = 0x85,
            /// <summary>
            /// 
            /// </summary>
            F23 = 0x86,
            /// <summary>
            /// 
            /// </summary>
            F24 = 0x87,
            /// <summary>
            /// 
            /// </summary>
            F3 = 0x72,
            /// <summary>
            /// 
            /// </summary>
            F4 = 0x73,
            /// <summary>
            /// 
            /// </summary>
            F5 = 0x74,
            /// <summary>
            /// 
            /// </summary>
            F6 = 0x75,
            /// <summary>
            /// 
            /// </summary>
            F7 = 0x76,
            /// <summary>
            /// 
            /// </summary>
            F8 = 0x77,
            /// <summary>
            /// 
            /// </summary>
            F9 = 120,

            #endregion

            #region Other
            /// <summary>
            /// 
            /// </summary>
            OEM_1 = 0xba,
            /// <summary>
            /// 
            /// </summary>
            OEM_102 = 0xe2,
            /// <summary>
            /// 
            /// </summary>
            OEM_2 = 0xbf,
            /// <summary>
            /// 
            /// </summary>
            OEM_3 = 0xc0,
            /// <summary>
            /// 
            /// </summary>
            OEM_4 = 0xdb,
            /// <summary>
            /// 
            /// </summary>
            OEM_5 = 220,
            /// <summary>
            /// 
            /// </summary>
            OEM_6 = 0xdd,
            /// <summary>
            /// 
            /// </summary>
            OEM_7 = 0xde,
            /// <summary>
            /// 
            /// </summary>
            OEM_8 = 0xdf,
            /// <summary>
            /// 
            /// </summary>
            OEM_CLEAR = 0xfe,
            /// <summary>
            /// 
            /// </summary>
            OEM_COMMA = 0xbc,
            /// <summary>
            /// 
            /// </summary>
            OEM_MINUS = 0xbd,
            /// <summary>
            /// 
            /// </summary>
            OEM_PERIOD = 190,
            /// <summary>
            /// 
            /// </summary>
            OEM_PLUS = 0xbb,

            #endregion

            #region KEYS

            /// <summary>
            /// 
            /// </summary>
            KEY_0 = 0x30,
            /// <summary>
            /// 
            /// </summary>
            KEY_1 = 0x31,
            /// <summary>
            /// 
            /// </summary>
            KEY_2 = 50,
            /// <summary>
            /// 
            /// </summary>
            KEY_3 = 0x33,
            /// <summary>
            /// 
            /// </summary>
            KEY_4 = 0x34,
            /// <summary>
            /// 
            /// </summary>
            KEY_5 = 0x35,
            /// <summary>
            /// 
            /// </summary>
            KEY_6 = 0x36,
            /// <summary>
            /// 
            /// </summary>
            KEY_7 = 0x37,
            /// <summary>
            /// 
            /// </summary>
            KEY_8 = 0x38,
            /// <summary>
            /// 
            /// </summary>
            KEY_9 = 0x39,
            /// <summary>
            /// 
            /// </summary>
            KEY_A = 0x41,
            /// <summary>
            /// 
            /// </summary>
            KEY_B = 0x42,
            /// <summary>
            /// 
            /// </summary>
            KEY_C = 0x43,
            /// <summary>
            /// 
            /// </summary>
            KEY_D = 0x44,
            /// <summary>
            /// 
            /// </summary>
            KEY_E = 0x45,
            /// <summary>
            /// 
            /// </summary>
            KEY_F = 70,
            /// <summary>
            /// 
            /// </summary>
            KEY_G = 0x47,
            /// <summary>
            /// 
            /// </summary>
            KEY_H = 0x48,
            /// <summary>
            /// 
            /// </summary>
            KEY_I = 0x49,
            /// <summary>
            /// 
            /// </summary>
            KEY_J = 0x4a,
            /// <summary>
            /// 
            /// </summary>
            KEY_K = 0x4b,
            /// <summary>
            /// 
            /// </summary>
            KEY_L = 0x4c,
            /// <summary>
            /// 
            /// </summary>
            KEY_M = 0x4d,
            /// <summary>
            /// 
            /// </summary>
            KEY_N = 0x4e,
            /// <summary>
            /// 
            /// </summary>
            KEY_O = 0x4f,
            /// <summary>
            /// 
            /// </summary>
            KEY_P = 80,
            /// <summary>
            /// 
            /// </summary>
            KEY_Q = 0x51,
            /// <summary>
            /// 
            /// </summary>
            KEY_R = 0x52,
            /// <summary>
            /// 
            /// </summary>
            KEY_S = 0x53,
            /// <summary>
            /// 
            /// </summary>
            KEY_T = 0x54,
            /// <summary>
            /// 
            /// </summary>
            KEY_U = 0x55,
            /// <summary>
            /// 
            /// </summary>
            KEY_V = 0x56,
            /// <summary>
            /// 
            /// </summary>
            KEY_W = 0x57,
            /// <summary>
            /// 
            /// </summary>
            KEY_X = 0x58,
            /// <summary>
            /// 
            /// </summary>
            KEY_Y = 0x59,
            /// <summary>
            /// 
            /// </summary>
            KEY_Z = 90,

            #endregion

            #region volume
            /// <summary>
            /// Decrese volume
            /// </summary>
            VOLUME_DOWN = 0xae,

            /// <summary>
            /// Mute volume
            /// </summary>
            VOLUME_MUTE = 0xad,

            /// <summary>
            /// Increase volue
            /// </summary>
            VOLUME_UP = 0xaf,

            #endregion


            /// <summary>
            /// Take snapshot of the screen and place it on the clipboard
            /// </summary>
            SNAPSHOT = 0x2c,

            /// <summary>Send right click from keyboard "key that is 2 keys to the right of space bar"</summary>
            RightClick = 0x5d,

            /// <summary>
            /// Go Back or delete
            /// </summary>
            BACKSPACE = 8,

            /// <summary>
            /// Control + Break "When debuging if you step into an infinite loop this will stop debug"
            /// </summary>
            CANCEL = 3,
            /// <summary>
            /// Caps lock key to send cappital letters
            /// </summary>
            CAPS_LOCK = 20,
            /// <summary>
            /// Ctlr key
            /// </summary>
            CONTROL = 0x11,

            /// <summary>
            /// Alt key
            /// </summary>
            ALT = 18,

            /// <summary>
            /// "." key
            /// </summary>
            DECIMAL = 110,

            /// <summary>
            /// Delete Key
            /// </summary>
            DELETE = 0x2e,


            /// <summary>
            /// Arrow down key
            /// </summary>
            DOWN = 40,

            /// <summary>
            /// End key
            /// </summary>
            END = 0x23,

            /// <summary>
            /// Escape key
            /// </summary>
            ESC = 0x1b,

            /// <summary>
            /// Home key
            /// </summary>
            HOME = 0x24,

            /// <summary>
            /// Insert key
            /// </summary>
            INSERT = 0x2d,

            /// <summary>
            /// Open my computer
            /// </summary>
            LAUNCH_APP1 = 0xb6,
            /// <summary>
            /// Open calculator
            /// </summary>
            LAUNCH_APP2 = 0xb7,

            /// <summary>
            /// Open default email in my case outlook
            /// </summary>
            LAUNCH_MAIL = 180,

            /// <summary>
            /// Opend default media player (itunes, winmediaplayer, etc)
            /// </summary>
            LAUNCH_MEDIA_SELECT = 0xb5,

            /// <summary>
            /// Left control
            /// </summary>
            LCONTROL = 0xa2,

            /// <summary>
            /// Left arrow
            /// </summary>
            LEFT = 0x25,

            /// <summary>
            /// Left shift
            /// </summary>
            LSHIFT = 160,

            /// <summary>
            /// left windows key
            /// </summary>
            LWIN = 0x5b,


            /// <summary>
            /// Next "page down"
            /// </summary>
            PAGEDOWN = 0x22,

            /// <summary>
            /// Num lock to enable typing numbers
            /// </summary>
            NUMLOCK = 0x90,

            /// <summary>
            /// Page up key
            /// </summary>
            PAGE_UP = 0x21,

            /// <summary>
            /// Right control
            /// </summary>
            RCONTROL = 0xa3,

            /// <summary>
            /// Return key
            /// </summary>
            ENTER = 13,

            /// <summary>
            /// Right arrow key
            /// </summary>
            RIGHT = 0x27,

            /// <summary>
            /// Right shift
            /// </summary>
            RSHIFT = 0xa1,

            /// <summary>
            /// Right windows key
            /// </summary>
            RWIN = 0x5c,

            /// <summary>
            /// Shift key
            /// </summary>
            SHIFT = 0x10,

            /// <summary>
            /// Space back key
            /// </summary>
            SPACE_BAR = 0x20,

            /// <summary>
            /// Tab key
            /// </summary>
            TAB = 9,

            /// <summary>
            /// Up arrow key
            /// </summary>
            UP = 0x26,

        }
    }
}
