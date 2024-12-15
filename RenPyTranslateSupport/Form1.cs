using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;

namespace RenPyTranslateSupport
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            if (System.IO.File.Exists("setting.txt"))
            {
                Encoding enc = Encoding.GetEncoding("utf-8");
                StreamReader sr = new StreamReader("setting.txt", enc);

                string str = sr.ReadToEnd();

                sr.Close();
                string[] settingdata = str.Replace("\r", "").Split('\n');
                if (settingdata[0] == "EN")
                {
                    Process.Start("RenPyTranslateSupportEng.exe");
                    Close();
                    return;
                }
                else
                {
                    comboBox1.SelectedIndex = Convert.ToInt32(settingdata[1]);
                    comboBox2.SelectedIndex = Convert.ToInt32(settingdata[2]);
                    textBox3.Text = settingdata[3];
                    textBox4.Text = settingdata[4];
                    defaultprocess = settingdata[5];
                    replaceplayer = settingdata[6];
                    kirisute = settingdata[7];
                    renpymodmode = Convert.ToBoolean(settingdata[8]);
                    if (renpymodmode)
                    {
                        numericUpDown1.Enabled = false;
                    }
                }
            }
            else
            {
                Encoding enc = Encoding.GetEncoding("utf-8");
                StreamWriter writer = new StreamWriter("setting.txt", false, enc);

                // テキストを書き込む
                writer.WriteLine("JP");
                writer.WriteLine(comboBox1.SelectedIndex);
                writer.WriteLine(comboBox2.SelectedIndex);
                writer.WriteLine(textBox3.Text);
                writer.WriteLine(textBox4.Text);
                writer.WriteLine(defaultprocess);
                writer.WriteLine(replaceplayer);
                writer.WriteLine(kirisute);
                writer.WriteLine(renpymodmode);
                // ファイルを閉じる
                writer.Close();
            }
            if (System.IO.File.Exists("Memo.txt"))
            {
                Encoding enc = Encoding.GetEncoding("utf-8");
                StreamReader sr = new StreamReader("Memo.txt", enc);

                textBox5.Text = sr.ReadToEnd();

                sr.Close();
            }
            keyboardHook.KeyDownEvent += KeyboardHook_KeyDownEvent;
            keyboardHook.KeyUpEvent += KeyboardHook_KeyUpEvent;
            comboBox1.SelectedIndex = 5;
            comboBox2.SelectedIndex = 5;
        }
        public static string defaultprocess = "Angels with Scaly Wings";
        public static string replaceplayer = "[player_name]";
        public static string kirisute = ": ";
        public static string memo = "";
        public static bool renpymodmode = false;
        public static string language = "JP";

        KeyboardHook keyboardHook = new KeyboardHook();


        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void SetCursorPos(int X, int Y);

        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x2;
        private const int MOUSEEVENTF_LEFTUP = 0x4;

        // A-Zキーが押されているときは非0が入る
        int AtoZ = 0;
        string p = "";

        public int Tabcount(string text)
        {
            int tab = 0;
            for (int r = 0; r <= text.Length; r++)
            {
                char c = text[r];
                if (c == '\t') 
                {
                    tab++;
                }
            }
            return tab;
        }

        private void KeyboardHook_KeyDownEvent(object sender, KeyEventArg e)
        {
            KeysConverter kc = new KeysConverter();
            p = kc.ConvertToString(e.KeyCode);
        }

        private void KeyboardHook_KeyUpEvent(object sender, KeyEventArg e)
        {
            p = "";
        }

        public static void ActiveWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            //ウィンドウが最小化されている場合は元に戻す
            if (IsIconic(hWnd))
            {
                ShowWindowAsync(hWnd, SW_RESTORE);
            }

            //AttachThreadInputの準備
            //フォアグラウンドウィンドウのハンドルを取得
            IntPtr forehWnd = GetForegroundWindow();
            if (forehWnd == hWnd)
            {
                return;
            }
            //フォアグラウンドのスレッドIDを取得
            uint foreThread = GetWindowThreadProcessId(forehWnd, IntPtr.Zero);
            //自分のスレッドIDを収得
            uint thisThread = GetCurrentThreadId();

            uint timeout = 200000;
            if (foreThread != thisThread)
            {
                //ForegroundLockTimeoutの現在の設定を取得
                //Visual Studio 2010, 2012起動後は、レジストリと違う値を返す
                SystemParametersInfoGet(SPI_GETFOREGROUNDLOCKTIMEOUT, 0, ref timeout, 0);
                //レジストリから取得する場合
                //timeout = (uint)Microsoft.Win32.Registry.GetValue(
                //    @"HKEY_CURRENT_USER\Control Panel\Desktop",
                //    "ForegroundLockTimeout", 200000);

                //ForegroundLockTimeoutの値を0にする
                //(SPIF_UPDATEINIFILE | SPIF_SENDCHANGE)を使いたいが、
                //  timeoutがレジストリと違う値だと戻せなくなるので使わない
                SystemParametersInfoSet(SPI_SETFOREGROUNDLOCKTIMEOUT, 0, 0, 0);

                //入力処理機構にアタッチする
                AttachThreadInput(thisThread, foreThread, true);
            }

            //ウィンドウをフォアグラウンドにする処理
            SetForegroundWindow(hWnd);
            SetWindowPos(hWnd, HWND_TOP, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW | SWP_ASYNCWINDOWPOS);
            BringWindowToTop(hWnd);
            ShowWindowAsync(hWnd, SW_SHOW);
            SetFocus(hWnd);

            if (foreThread != thisThread)
            {
                //ForegroundLockTimeoutの値を元に戻す
                //ここでも(SPIF_UPDATEINIFILE | SPIF_SENDCHANGE)は使わない
                SystemParametersInfoSet(SPI_SETFOREGROUNDLOCKTIMEOUT, 0, timeout, 0);

                //デタッチ
                AttachThreadInput(thisThread, foreThread, false);
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd,
            int hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_SHOWWINDOW = 0x0040;
        private const int SWP_ASYNCWINDOWPOS = 0x4000;
        private const int HWND_TOP = 0;
        private const int HWND_BOTTOM = 1;
        private const int HWND_TOPMOST = -1;
        private const int HWND_NOTOPMOST = -2;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOW = 5;
        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(
            IntPtr hWnd, IntPtr ProcessId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AttachThreadInput(
            uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo",
            SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SystemParametersInfoGet(
            uint action, uint param, ref uint vparam, uint init);

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo",
            SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SystemParametersInfoSet(
            uint action, uint param, uint vparam, uint init);

        private const uint SPI_GETFOREGROUNDLOCKTIMEOUT = 0x2000;
        private const uint SPI_SETFOREGROUNDLOCKTIMEOUT = 0x2001;
        private const uint SPIF_UPDATEINIFILE = 0x01;
        private const uint SPIF_SENDCHANGE = 0x02;

        System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcesses();
        public List<IntPtr> psid = new List<IntPtr>();
        public static List<string> untranslatepath = new List<string>();
        public static List<string> translatedpath = new List<string>();
        public List<string> untranslate = new List<string>();
        public List<string> translated = new List<string>();
        public static bool linkfilesuccess = false;
        string matchfile = "";
        bool keypressing = false;
        List<int> filenumbers = new List<int>();
        List<int> linenumbers = new List<int>();
        List<string> originaltexts = new List<string>();
        int skip = 0;
        public static bool resetsetting = false;
        public List<string> identifier = new List<string>();


        private void button1_Click(object sender, EventArgs e)
        {
            //OpenFileDialogクラスのインスタンスを作成
            OpenFileDialog ofd = new OpenFileDialog();

            //はじめのファイル名を指定する
            //はじめに「ファイル名」で表示される文字列を指定する
            ofd.FileName = "";
            //[ファイルの種類]に表示される選択肢を指定する
            //指定しないとすべてのファイルが表示される
            ofd.Filter = "すべてのファイル(*.*)|*.*";
            //タイトルを設定する
            ofd.Title = "開くファイルを選択してください";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd.RestoreDirectory = true;
            //存在しないファイルの名前が指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            ofd.CheckFileExists = true;
            //存在しないパスが指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            ofd.CheckPathExists = true;
            ofd.Multiselect = true;

            //ダイアログを表示する
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                untranslatepath = new List<string>(ofd.FileNames);
                // 選択されたファイルをテキストボックスに表示する
                label1.ForeColor = Color.Green;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //OpenFileDialogクラスのインスタンスを作成
            OpenFileDialog ofd = new OpenFileDialog();

            //はじめのファイル名を指定する
            //はじめに「ファイル名」で表示される文字列を指定する
            ofd.FileName = "";
            //[ファイルの種類]に表示される選択肢を指定する
            //指定しないとすべてのファイルが表示される
            ofd.Filter = "すべてのファイル(*.*)|*.*";
            //タイトルを設定する
            ofd.Title = "開くファイルを選択してください";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd.RestoreDirectory = true;
            //存在しないファイルの名前が指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            ofd.CheckFileExists = true;
            //存在しないパスが指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            ofd.CheckPathExists = true;
            ofd.Multiselect = true;

            //ダイアログを表示する
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                translatedpath = new List<string>(ofd.FileNames);
                // 選択されたファイルをテキストボックスに表示する
                label2.ForeColor = Color.Green;
            }
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button5.Enabled = false;
                button6.Enabled = false;
                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
                listBox1.Enabled = false;
                textBox4.Enabled = false;
                timer1.Enabled = true;
                keyboardHook.Hook();
            }
            else
            {
                button7.Enabled = false;
                button4.Enabled = false;
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button5.Enabled = true;
                button6.Enabled = true;
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                listBox1.Enabled = true;
                textBox4.Enabled = true;
                timer1.Enabled = false;
                keyboardHook.UnHook();
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (p == textBox4.Text && !keypressing)
            {
                textBox1.Text = "";
                textBox2.Text = "";
                keypressing = true;
                identifier.Clear();
                originaltexts.Clear();
                filenumbers.Clear();
                linenumbers.Clear();
                skip = 0;
                string getclip = "";
                bool success = false;
                int filenumber = 0;
                int linenumber = 0;
                if (Clipboard.ContainsText())
                {
                    //文字列データがあるときはこれを取得する
                    //取得できないときは空の文字列（String.Empty）を返す
                    getclip = Clipboard.GetText();
                }
                if (getclip == "" || getclip == String.Empty)
                {
                    getclip = "取得に失敗しました。";
                }
                if (getclip.Contains(kirisute))
                {
                    getclip = getclip.Substring(getclip.IndexOf(": ") + 2);
                }
                if (textBox3.Text != "")
                {
                    if (getclip.Contains(textBox3.Text))
                    {
                        getclip = getclip.Replace(textBox3.Text, replaceplayer);
                    }
                }
                Console.WriteLine(getclip);

                foreach (string strFilePath in translatedpath)
                {
                    Encoding enc = Encoding.GetEncoding(comboBox1.Text);
                    StreamReader sr = new StreamReader(strFilePath, enc);

                    string str = sr.ReadToEnd();

                    sr.Close();
                    if (str.Contains(getclip))
                    {
                        translated = new List<string>(str.Replace("\r", "").Split('\n'));
                        matchfile = strFilePath;
                        success = true;
                        break;
                    }
                }
                if (!success)
                {
                    filenumber = 0;
                    foreach (string strFilePath in translatedpath)
                    {
                        Encoding enc = Encoding.GetEncoding(comboBox1.Text);
                        StreamReader sr = new StreamReader(strFilePath, enc);

                        string str = sr.ReadToEnd();

                        sr.Close();
                        string oldgetclip = "";
                        for (int r = 1; r <= getclip.Length; r++)
                        {
                            string checkclip = getclip.Substring(0, r);
                            if (!str.Contains(checkclip))
                            {
                                translated = new List<string>(str.Replace("\r", "").Split('\n'));
                                matchfile = strFilePath;
                                if (oldgetclip != "" && oldgetclip.Length > 1)
                                {
                                    success = true;
                                    getclip = oldgetclip;
                                }
                                break;
                            }
                            else
                            {
                                oldgetclip = checkclip;
                            }
                        }
                        if (success)
                        {
                            break;
                        }
                    }
                }
                Console.WriteLine(getclip);
                if (success)
                {
                    foreach (string strFilePath in translatedpath)
                    {
                        Encoding enc = Encoding.GetEncoding(comboBox1.Text);
                        StreamReader sr = new StreamReader(strFilePath, enc);

                        string str = sr.ReadToEnd();

                        sr.Close();
                        if (str.Contains(getclip))
                        {
                            translated = new List<string>(str.Replace("\r", "").Split('\n'));
                            matchfile = strFilePath;
                            success = true;
                            foreach (string linetext in translated)
                            {
                                System.Windows.Forms.Application.DoEvents();
                                if (linetext.Contains(getclip))
                                {
                                    if (renpymodmode)
                                    {
                                        if (translated[0].Contains("filename\tlinenumber\tidentifier\tcontents"))
                                        {
                                            int repeater = 0;
                                            int tab = 0;
                                            for (int r = 0; r < linetext.Length; r++)
                                            {
                                                char c = linetext[r];
                                                if (c == '\t')
                                                {
                                                    tab++;
                                                    if (tab == 2)
                                                    {
                                                        repeater = r;
                                                    }
                                                    else if (tab == 3)
                                                    {
                                                        identifier.Add(linetext.Substring(repeater + 1, r - (repeater + 1)));
                                                        Console.WriteLine("#" + linetext.Substring(repeater + 1, r - (repeater + 1)) + "#");
                                                        if (linetext.Contains("\""))
                                                        {
                                                            textBox1.Text = linetext.Substring(linetext.IndexOf("\"") + 1, linetext.Substring(linetext.IndexOf("\"") + 1).IndexOf("\""));
                                                            originaltexts.Add(linetext.Substring(linetext.IndexOf("\"") + 1, linetext.Substring(linetext.IndexOf("\"") + 1).IndexOf("\"")));
                                                        }
                                                        else
                                                        {
                                                            textBox1.Text = linetext.Substring(r + 1);
                                                            originaltexts.Add(linetext.Substring(r + 1));
                                                        }
                                                        Console.WriteLine("#" + textBox1.Text + "#");
                                                        break;
                                                    }
                                                }
                                            }
                                            linenumbers.Add(linenumber);
                                            filenumbers.Add(filenumber);
                                        }
                                        else if (translated[0].Contains("filename\tlinenumber\tchoice\ttranslated"))
                                        {
                                            int tab = 0;
                                            for (int r = 0; r < linetext.Length; r++)
                                            {
                                                char c = linetext[r];
                                                if (c == '\t')
                                                {
                                                    tab++;
                                                    if (tab == 3)
                                                    {
                                                        identifier.Add(linetext.Substring(0, r));
                                                        Console.WriteLine("#" + linetext.Substring(0, r) + "#");
                                                        textBox1.Text = linetext.Substring(r + 1);
                                                        originaltexts.Add(linetext.Substring(r + 1));
                                                        Console.WriteLine("#" + textBox1.Text + "#");
                                                        break;
                                                    }
                                                }
                                            }
                                            linenumbers.Add(linenumber);
                                            filenumbers.Add(filenumber);
                                        }
                                    }
                                    else
                                    {
                                        textBox1.Text = linetext;
                                        originaltexts.Add(linetext);
                                        linenumbers.Add(linenumber);
                                        filenumbers.Add(filenumber);
                                    }
                                }
                                linenumber++;
                            }
                        }
                        filenumber++;
                    }
                    

                    textBox1.Text = originaltexts[skip];
                    Encoding enc2 = Encoding.GetEncoding(comboBox2.Text);
                    StreamReader sr2 = new StreamReader(untranslatepath[filenumbers[skip]], enc2);

                    string str2 = sr2.ReadToEnd();

                    sr2.Close();
                    untranslate = new List<string>(str2.Replace("\r", "").Split('\n'));
                    if (renpymodmode)
                    {
                        if (translated[0].Contains("filename\tlinenumber\tidentifier\tcontents"))
                        {
                            foreach (string linetext in untranslate)
                            {
                                if (linetext.Contains(identifier[skip]))
                                {
                                    if (linetext.Contains("\""))
                                    {
                                        textBox2.Text = linetext.Substring(linetext.IndexOf("\"") + 1, linetext.Substring(linetext.IndexOf("\"") + 1).IndexOf("\""));
                                    }
                                    else
                                    {
                                        int repeater = 0;
                                        int tab = 0;
                                        for (int r = 0; r < linetext.Length; r++)
                                        {
                                            char c = linetext[r];
                                            if (c == '\t')
                                            {
                                                tab++;
                                                if (tab == 2)
                                                {
                                                    repeater = r;
                                                }
                                                else if (tab == 3)
                                                {
                                                    textBox2.Text = linetext.Substring(repeater + 1, r - (repeater + 1));
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (translated[0].Contains("filename\tlinenumber\tchoice\ttranslated"))
                        {
                            foreach (string linetext in untranslate)
                            {
                                if (linetext.Contains(identifier[skip]))
                                {
                                    int repeater = 0;
                                    int tab = 0;
                                    for (int r = 0; r < linetext.Length; r++)
                                    {
                                        char c = linetext[r];
                                        if (c == '\t')
                                        {
                                            tab++;
                                            if (tab == 2)
                                            {
                                                repeater = r;
                                            }
                                            else if (tab == 3)
                                            {
                                                textBox2.Text = linetext.Substring(repeater + 1, r - (repeater + 1));
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (linenumbers[skip] + Decimal.ToInt32(numericUpDown1.Value) >= untranslate.Count)
                            {
                                textBox2.Text = untranslate[untranslate.Count - 1] + "原文の行数の最大数を超えています。オフセットの数値を下げてください。";
                            }
                            else
                            {
                                textBox2.Text = untranslate[linenumbers[skip] + Decimal.ToInt32(numericUpDown1.Value)];
                            }
                            textBox2.Text += "これはRenPyTranslateModのtsvではありません。もし何度も表示されるようであれば、詳細設定→RenPyTranslateModのtsvに最適化をオフにしてください。";
                        }
                    }
                    else
                    {
                        if (linenumbers[skip] + Decimal.ToInt32(numericUpDown1.Value) >= untranslate.Count)
                        {
                            textBox2.Text = untranslate[untranslate.Count - 1] + "原文の行数の最大数を超えています。オフセットの数値を下げてください。";
                        }
                        else
                        {
                            textBox2.Text = untranslate[linenumbers[skip] + Decimal.ToInt32(numericUpDown1.Value)];
                        }
                    }
                    button7.Enabled = true;
                    button4.Enabled = true;
                }
                else
                {
                    textBox1.Text = "見つかりませんでした。";
                }
                ActiveWindow(Process.GetCurrentProcess().MainWindowHandle);
                ActiveControl = textBox1;
            }
            if (keypressing)
            {
                if (p != textBox4.Text)
                {
                    keypressing = false;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string temp = button3.Text;
            button3.Text = "取得中...";
            Refresh();
            listBox1.Items.Clear();
            psid.Clear();
            ps = System.Diagnostics.Process.GetProcesses();
            //"machinename"という名前のコンピュータで実行されている
            //すべてのプロセスを取得するには次のようにする。
            //System.Diagnostics.Process[] ps =
            //    System.Diagnostics.Process.GetProcesses("machinename");

            //配列から1つずつ取り出す
            int i = 0;
            foreach (System.Diagnostics.Process ps in ps)
            {
                try
                {
                    string processname = ps.ProcessName;
                    listBox1.Items.Add(processname);
                    psid.Add(ps.MainWindowHandle);
                    if (processname.Contains(defaultprocess))
                    {
                        listBox1.SelectedIndex = i;
                    }
                    i++;

                }
                catch (Exception ex)
                {
                    Console.WriteLine("エラー: {0}", ex.Message);
                }
            }
            button3.Text = temp;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine(psid[listBox1.SelectedIndex]);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Encoding enc = Encoding.GetEncoding(comboBox1.Text);
            StreamReader sr = new StreamReader(translatedpath[filenumbers[skip]], enc);

            string str = sr.ReadToEnd();

            sr.Close();

            str = str.Replace(originaltexts[skip], textBox1.Text);
            StreamWriter writer = new StreamWriter(translatedpath[filenumbers[skip]], false, enc);

            // テキストを書き込む
            writer.Write(str);

            // ファイルを閉じる
            writer.Close();
            ActiveWindow(psid[listBox1.SelectedIndex]);
            button7.Enabled = false;
            button4.Enabled = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            linkfilesuccess = false;
            Form2 form2 = new Form2();
            form2.ShowDialog();
            if (linkfilesuccess)
            {
                label3.ForeColor = Color.Green;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            button6.Enabled = false;
            checkBox1.Enabled = false;
            textBox4.Enabled = false;
            keyboardHook.Hook();
            p = "";
            string temp = textBox4.Text;
            textBox4.Text = "取得中...";
            var sw = new System.Diagnostics.Stopwatch();

            //-----------------
            // 計測開始
            sw.Start();
            while (sw.Elapsed.Seconds < 5)
            {
                textBox4.Text = "取得中...(" + (5 - sw.Elapsed.Seconds) + ")";
                System.Windows.Forms.Application.DoEvents();
                //Console.WriteLine(p);
                if (p != "")
                {
                    textBox4.Text = p;
                    break;
                }
            }
            if (sw.Elapsed.Seconds >= 5)
            {
                textBox4.Text = temp;
            }
            keyboardHook.UnHook();
            button6.Enabled = true;
            checkBox1.Enabled = true;
            textBox4.Enabled = true;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            skip++;
            if (originaltexts.Count > skip)
            {
                textBox1.Text = originaltexts[skip];
                Encoding enc2 = Encoding.GetEncoding(comboBox2.Text);
                StreamReader sr2 = new StreamReader(untranslatepath[filenumbers[skip]], enc2);

                string str2 = sr2.ReadToEnd();

                sr2.Close();
                untranslate = new List<string>(str2.Replace("\r", "").Split('\n'));
                if (renpymodmode)
                {
                    if (translated[0].Contains("filename\tlinenumber\tidentifier\tcontents"))
                    {
                        foreach (string linetext in untranslate)
                        {
                            if (linetext.Contains(identifier[skip]))
                            {
                                if (linetext.Contains("\""))
                                {
                                    textBox2.Text = linetext.Substring(linetext.IndexOf("\"") + 1, linetext.Substring(linetext.IndexOf("\"") + 1).IndexOf("\""));
                                }
                                else
                                {
                                    int tab = 0;
                                    for (int r = 0; r <= linetext.Length; r++)
                                    {
                                        char c = linetext[r];
                                        if (c == '\t')
                                        {
                                            tab++;
                                            if (tab == 3)
                                            {
                                                textBox2.Text = linetext.Substring(r + 1);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (translated[0].Contains("filename\tlinenumber\tchoice\ttranslated"))
                    {
                        foreach (string linetext in untranslate)
                        {
                            if (linetext.Contains(identifier[skip]))
                            {
                                int repeater = 0;
                                int tab = 0;
                                for (int r = 0; r <= linetext.Length; r++)
                                {
                                    char c = linetext[r];
                                    if (c == '\t')
                                    {
                                        tab++;
                                        if (tab == 2)
                                        {
                                            repeater = r;
                                        }
                                        else if (tab == 3)
                                        {
                                            textBox2.Text = linetext.Substring(r + 1, repeater - (r + 1));
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (linenumbers[skip] + Decimal.ToInt32(numericUpDown1.Value) >= untranslate.Count)
                        {
                            textBox2.Text = untranslate[untranslate.Count - 1] + "原文の行数の最大数を超えています。オフセットの数値を下げてください。";
                        }
                        else
                        {
                            textBox2.Text = untranslate[linenumbers[skip] + Decimal.ToInt32(numericUpDown1.Value)];
                        }
                        textBox2.Text += "これはRenPyTranslateModのtsvではありません。もし何度も表示されるようであれば、詳細設定→RenPyTranslateModのtsvに最適化をオフにしてください。";
                    }
                }
                else
                {
                    if (linenumbers[skip] + Decimal.ToInt32(numericUpDown1.Value) >= untranslate.Count)
                    {
                        textBox2.Text = untranslate[untranslate.Count - 1] + "原文の行数の最大数を超えています。オフセットの数値を下げてください。";
                    }
                    else
                    {
                        textBox2.Text = untranslate[linenumbers[skip] + Decimal.ToInt32(numericUpDown1.Value)];
                    }
                }
                button7.Enabled = true;
                button4.Enabled = true;
            }
            else
            {
                button7.Enabled = false;
                button4.Enabled = false;
                textBox1.Text = "見つかりませんでした。";
                textBox2.Text = "";
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                Encoding enc2 = Encoding.GetEncoding(comboBox2.Text);
                StreamReader sr2 = new StreamReader(untranslatepath[filenumbers[skip]], enc2);

                string str2 = sr2.ReadToEnd();

                sr2.Close();
                untranslate = new List<string>(str2.Replace("\r", "").Split('\n'));
                if (linenumbers[skip] + Decimal.ToInt32(numericUpDown1.Value) >= untranslate.Count)
                {
                    textBox2.Text = untranslate[untranslate.Count - 1] + "原文の行数の最大数を超えています。オフセットの数値を下げてください。";
                }
                else
                {
                    textBox2.Text = untranslate[linenumbers[skip] + Decimal.ToInt32(numericUpDown1.Value)];
                }
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) 
            {
                button4.PerformClick();
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3();
            form3.ShowDialog();
            if (resetsetting)
            {
                Encoding enc = Encoding.GetEncoding("utf-8");
                StreamWriter writer = new StreamWriter("setting.txt", false, enc);

                // テキストを書き込む
                writer.WriteLine("JP");
                writer.WriteLine(5);
                writer.WriteLine(5);
                writer.WriteLine("かにみそ");
                writer.WriteLine("LMenu");
                writer.WriteLine("Angels with Scaly Wings");
                writer.WriteLine("[player_name]");
                writer.WriteLine(": ");
                writer.WriteLine(false);
                // ファイルを閉じる
                writer.Close();
                Process.Start("RenPyTranslateSupport.exe");
                Close();
            }
            else
            {
                Encoding enc = Encoding.GetEncoding("utf-8");
                StreamWriter writer = new StreamWriter("setting.txt", false, enc);

                // テキストを書き込む
                writer.WriteLine(language);
                writer.WriteLine(comboBox1.SelectedIndex);
                writer.WriteLine(comboBox2.SelectedIndex);
                writer.WriteLine(textBox3.Text);
                writer.WriteLine(textBox4.Text);
                writer.WriteLine(defaultprocess);
                writer.WriteLine(replaceplayer);
                writer.WriteLine(kirisute);
                writer.WriteLine(renpymodmode);
                // ファイルを閉じる
                writer.Close();
                if (language == "EN")
                {
                    DialogResult result = MessageBox.Show("言語を変更するにはソフトの再起動が必要です。\n再起動しますか？", "", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);

                    //何が選択されたか調べる
                    if (result == DialogResult.Yes)
                    {
                        Process.Start("RenPyTranslateSupportEng.exe");
                        Close();
                    }
                }
            }
            if (renpymodmode)
            {
                numericUpDown1.Enabled = false;
            }
            else
            {
                numericUpDown1.Enabled = true;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Encoding enc = Encoding.GetEncoding("utf-8");
            StreamWriter writer = new StreamWriter("setting.txt", false, enc);

            // テキストを書き込む
            writer.WriteLine(language);
            writer.WriteLine(comboBox1.SelectedIndex);
            writer.WriteLine(comboBox2.SelectedIndex);
            writer.WriteLine(textBox3.Text);
            writer.WriteLine(textBox4.Text);
            writer.WriteLine(defaultprocess);
            writer.WriteLine(replaceplayer);
            writer.WriteLine(kirisute);
            writer.WriteLine(renpymodmode);
            // ファイルを閉じる
            writer.Close();
            writer = new StreamWriter("Memo.txt", false, enc);

            // テキストを書き込む
            writer.Write(textBox5.Text);
            // ファイルを閉じる
            writer.Close();
        }
    }
    class KeyboardHook
    {
        protected const int WH_KEYBOARD_LL = 0x000D;
        protected const int WM_KEYDOWN = 0x0100;
        protected const int WM_KEYUP = 0x0101;
        protected const int WM_SYSKEYDOWN = 0x0104;
        protected const int WM_SYSKEYUP = 0x0105;

        [StructLayout(LayoutKind.Sequential)]
        public class KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public KBDLLHOOKSTRUCTFlags flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [Flags]
        public enum KBDLLHOOKSTRUCTFlags : uint
        {
            KEYEVENTF_EXTENDEDKEY = 0x0001,
            KEYEVENTF_KEYUP = 0x0002,
            KEYEVENTF_SCANCODE = 0x0008,
            KEYEVENTF_UNICODE = 0x0004,
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, KeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private KeyboardProc proc;
        private IntPtr hookId = IntPtr.Zero;

        public void Hook()
        {
            if (hookId == IntPtr.Zero)
            {
                proc = HookProcedure;
                using (var curProcess = Process.GetCurrentProcess())
                {
                    using (ProcessModule curModule = curProcess.MainModule)
                    {
                        hookId = SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
                    }
                }
            }
        }

        public void UnHook()
        {
            UnhookWindowsHookEx(hookId);
            hookId = IntPtr.Zero;
        }

        public IntPtr HookProcedure(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                var kb = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                var vkCode = (int)kb.vkCode;
                OnKeyDownEvent(vkCode);
            }
            else if (nCode >= 0 && (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP))
            {
                var kb = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                var vkCode = (int)kb.vkCode;
                OnKeyUpEvent(vkCode);
            }
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        public delegate void KeyEventHandler(object sender, KeyEventArg e);
        public event KeyEventHandler KeyDownEvent;
        public event KeyEventHandler KeyUpEvent;

        protected void OnKeyDownEvent(int keyCode)
        {
            KeyDownEvent?.Invoke(this, new KeyEventArg(keyCode));
        }
        protected void OnKeyUpEvent(int keyCode)
        {
            KeyUpEvent?.Invoke(this, new KeyEventArg(keyCode));
        }

    }

    public class KeyEventArg : EventArgs
    {
        public int KeyCode { get; }

        public KeyEventArg(int keyCode)
        {
            KeyCode = keyCode;
        }
    }
}
