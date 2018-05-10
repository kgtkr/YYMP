using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using NAudio.Wave;
using System.IO;
using NAudio.Wave.SampleProviders;
using System.Windows.Forms;
using NAudio;

namespace YYMP
{
    /// <summary>
    /// フリースタイルを表すクラス
    /// </summary>
    public class FreeStyle : IPlay
    {
        #region フィールド
        /// <summary>
        /// トリック名一覧
        /// </summary>
        private readonly List<Name> trickNameList = new List<Name>();

        /// <summary>
        /// コンボ名一覧
        /// </summary>
        private readonly List<Name> comboNameList = new List<Name>();

        /// <summary>
        /// フリースタイルの曲
        /// </summary>
        private Player music;

        /// <summary>
        /// コンボ一覧
        /// </summary>
        private readonly List<Combo> comboList = new List<Combo>();

        /// <summary>
        /// 長さ
        /// </summary>
        private int freeStyleLength;

        /// <summary>
        /// 待ち時間
        /// </summary>
        private int wait;

        /// <summary>
        /// ループするか
        /// </summary>
        private bool loop;

        /// <summary>
        /// 現在の状況
        /// </summary>
        private PlaybackState state;

        /// <summary>
        /// ボリューム
        /// </summary>
        private float volume = 1;

        /// <summary>
        /// タイマー
        /// </summary>
        private Timer timer;

        /// <summary>
        /// 現在位置
        /// </summary>
        private int position;

        /// <summary>
        /// 状況
        /// </summary>
        private PlaybackState playState;

        /// <summary>
        /// timerEvent
        /// </summary>
        public event FreeStyleEvent timerEvent;

        /// <summary>
        /// イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void FreeStyleEvent(FreeStyle fs);
        #endregion
        #region プロパティ
        /// <summary>
        /// フリースタイルの長さ
        /// </summary>
        public int FreeStyleLength
        {
            get
            {
                return this.freeStyleLength;
            }

            set
            {
                if (this.playState != PlaybackState.Stopped) throw new NotChangeException();
                if (this.music == null) throw new NotMusicException();
                if (value < 1 || value > this.music.Length) throw new ArgumentOutOfRangeException();
                this.freeStyleLength = value;
            }
        }

        /// <summary>
        /// 現在位置
        /// </summary>
        public double Position
        {
            get
            {
                return this.position;
            }
            set
            {
                if (this.music == null) throw new NotMusicException();
                if (value < 0 || value > this.Length) throw new ArgumentOutOfRangeException();
                this.position = (int)value;
                if (this.music != null)
                {
                    if (value < wait)
                    {
                        try {
                            this.music.Position = 0;
                            this.music.Stop();
                        }
                        catch (MmException)
                        {
                            throw;
                        }
                    }
                    else
                    {
                        this.music.Position = value - this.wait;
                        if (this.playState == PlaybackState.Playing) this.music.Play();
                    }
                }

                if (this.NowCombo != null)
                {
                    this.NowCombo.Position = (int)this.Position - this.wait - this.NowCombo.Start;
                }
            }
        }

        /// <summary>
        /// 現在のプレイ状況
        /// </summary>
        public PlaybackState Playing
        {
            get { return this.playState; }
        }

        /// <summary>
        /// ボリューム
        /// </summary>
        public float Volume
        {
            get { return this.volume; }
            set
            {
                if (this.music != null) this.music.Volume = value;
                if (value < 0 || value > 1) throw new ArgumentOutOfRangeException();
                this.volume = value;
            }
        }

        /// <summary>
        /// 待ち時間
        /// </summary>
        public int Wait
        {
            get { return this.wait; }
            set
            {
                if (this.playState != PlaybackState.Stopped) throw new NotChangeException();
                if (value < 0) throw new ArgumentOutOfRangeException();
                this.wait = value;

            }
        }

        /// <summary>
        /// 長さ
        /// </summary>
        public double Length
        {
            get
            {
                return this.FreeStyleLength + this.wait;
            }
        }

        /// <summary>
        /// ループするか？
        /// </summary>
        public bool Loop
        {
            get { return this.loop; }
            set { this.loop = value; }
        }

        /// <summary>
        /// フリースタイルの音楽
        /// </summary>
        public Player Music
        {
            get { return this.music; }
        }

        /// <summary>
        /// コンボ名リスト
        /// </summary>
        public List<string> ComboNameList
        {
            get { return Name.ToStringArray(this.comboNameList); }
        }

        /// <summary>
        /// 技名リスト
        /// </summary>
        public List<string> TrickNameList
        {
            get { return Name.ToStringArray(this.trickNameList); }

        }

        /// <summary>
        /// コンボリスト
        /// </summary>
        public List<Combo> ComboList
        {
            get { return this.comboList; }
        }

        #region 表示関係
        /// <summary>
        /// 現在のコンボ
        /// </summary>
        public Combo NowCombo
        {
            get
            {
                //現在位置
                int p = this.position - this.wait;
                foreach (Combo c in this.comboList)
                {
                    if (p >= c.Start && p < c.End)
                    {
                        return c;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// 次のコンボ
        /// </summary>
        public Combo NextCombo
        {
            get
            {
                Combo c;
                if ((c = this.NowCombo) != null)
                {//現在のコンボがあるなら
                    int index = this.comboList.IndexOf(c) + 1;
                    if (index != this.comboList.Count)//最後のコンボでなければ
                    {
                        return this.comboList[index];
                    }
                    else//次のコンボはない
                    {
                        return null;
                    }
                }
                else//現在のコンボがない
                {
                    Combo afterCombo = null;
                    foreach (Combo co in this.comboList)
                    {
                        int p = this.position - this.wait;
                        if ((afterCombo != null ? afterCombo.End : 0) <= p && p < co.Start)
                        {
                            return co;
                        }
                        afterCombo = co;
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// 現在の技
        /// </summary>
        public Trick NowTrick
        {
            get
            {
                Combo c = this.NowCombo;
                if (c != null)
                {
                    return c.NowTrick;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 次の技
        /// </summary>
        public Trick NextTrick
        {
            get
            {
                Combo c = this.NowCombo;
                if (c != null)
                {
                    return c.NextTrick;
                }
                else
                {
                    c = this.NextCombo;
                    if (c != null)
                    {
                        return c.FastTrick;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// 次のコンボの最初の技
        /// </summary>
        public Trick NextComboFastTrick
        {
            get
            {
                Combo c = this.NextCombo;
                if (c != null)
                {
                    return c.FastTrick;
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion
        #endregion
        #region コンストラクタ
        /// <summary>
        /// 新しいフリースタイルを作成します
        /// </summary>
        public FreeStyle()
        {
            this.timer = new Timer();
            this.timer.Interval = 1000;
            this.timer.Stop();
            timer.Tick += new EventHandler(this.Clock);
        }

        /// <summary>
        /// ファイルを読み込んで新しく作る
        /// </summary>
        /// <param name="fileName"></param>
        public FreeStyle(string fileName) : this()
        {
            /*ファイルを読み込む*/
            string[] data = null;
            using (StreamReader sr = new StreamReader(
                fileName, Encoding.GetEncoding("UTF-8")))
            {
                data = sr.ReadToEnd().Split(new string[] { "\n" }, StringSplitOptions.None);
            }

            string[] header = data[0].Split(new string[] { "," }, StringSplitOptions.None);
            if (header[0] != "yymp") throw new TypeFormatException();
            if (header[1] != "0") throw new VerFormatException();

            this.freeStyleLength = int.Parse(data[1]);
            if (data[2].Length != 0)
            {
                this.comboNameList = Name.ToNameArray(data[2].Split(new string[] { "," }, StringSplitOptions.None).ToList());
            }
            else
            {
                this.comboNameList = new List<Name>();
            }

            if (data[3].Length != 0)
            {
                this.trickNameList = Name.ToNameArray(data[3].Split(new string[] { "," }, StringSplitOptions.None).ToList());
            }
            else
            {
                this.trickNameList = new List<Name>();
            }

            if (data[4].Length != 0)
            {
                {
                    string[] comboStrings = data[4].Split(new string[] { "," }, StringSplitOptions.None);
                    foreach (string comboString in comboStrings)
                    {
                        string[] s = comboString.Split(new string[] { ":" }, StringSplitOptions.None);
                        this.comboList.Add(new Combo(int.Parse(s[1]),
                            int.Parse(s[2]),
                            this.comboNameList[int.Parse(s[0])]
                            ));
                    }
                }
                {
                    if (data[5].Length != 0)
                    {
                        string[] trickStrings = data[5].Split(new string[] { "," }, StringSplitOptions.None);
                        foreach (string trickString in trickStrings)
                        {
                            string[] s = trickString.Split(new string[] { ":" }, StringSplitOptions.None);
                            this.comboList[int.Parse(s[0])].AddTrick(new Trick(
                                int.Parse(s[2]),
                                int.Parse(s[3]),
                                this.trickNameList[int.Parse(s[1])]
                                ));
                        }
                    }
                }
            }
            else
            {
                this.comboList = new List<Combo>();
            }

            try {
                this.music = new Player(Convert.FromBase64String(data[6]));
            }
            catch (MmException)
            {
                throw;
            }
        }
        #endregion
        #region メソッド
        /// <summary>
        /// タイマーイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Clock(object sender, EventArgs e)
        {
            this.position++;

            if (this.music != null)
            {
                //再生
                if (this.position > this.wait)
                {
                    try {
                        this.music.Play();
                    }
                    catch (MmException)
                    {
                        throw;
                    }
                }
                //停止
                if ((int)this.music.Position >= this.freeStyleLength)
                {
                    this.Stop();
                    if (this.loop)
                    {
                        this.Play();
                    }
                }
            }

            if (this.NowCombo != null)
            {
                this.NowCombo.Position = (int)this.Position - this.wait - this.NowCombo.Start;
            }

            if (this.timerEvent != null) this.timerEvent(this);
        }

        /// <summary>
        /// 音楽を読み込みます
        /// その後長さも変更します
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadMusic(string fileName)
        {
            if (this.playState != PlaybackState.Stopped) throw new NotChangeException();
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                byte[] bs = new byte[fs.Length];
                fs.Read(bs, 0, bs.Length);
                fs.Close();
                try {
                    this.music = new Player(bs);
                }
                catch (MmException)
                {
                    throw;
                }
            }
            this.FreeStyleLength = (int)this.music.Length;
        }

        /// <summary>
        /// コンボ名を追加します。
        /// </summary>
        /// <param name="Name">名前</param>
        public void AddComboName(string Name)
        {
            if (this.playState != PlaybackState.Stopped) throw new NotChangeException();
            this.comboNameList.Add(new Name(Name));
        }

        /// <summary>
        /// Trick名を追加します。
        /// </summary>
        /// <param name="comboName">名前</param>
        public void AddTrickName(string Name)
        {
            if (this.playState != PlaybackState.Stopped) throw new NotChangeException();
            this.trickNameList.Add(new Name(Name));
        }


        /// <summary>
        /// 名前を変更
        /// </summary>
        /// <param name="before"></param>
        /// <param name="after"></param>
        public void ChangeComboName(string before, string after)
        {
            if (this.playState != PlaybackState.Stopped) throw new NotChangeException();
            GetComboName(before).Name_ = after;
        }

        /// <summary>
        /// 名前を変更
        /// </summary>
        /// <param name="before"></param>
        /// <param name="after"></param>
        public void ChangeTrickName(string before, string after)
        {
            if (this.playState != PlaybackState.Stopped) throw new NotChangeException();
            GetTrickName(before).Name_ = after;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private Name GetComboName(string name)
        {
            int index = this.comboNameList.IndexOf(new Name(name));
            if (index == -1)
            {
                this.comboNameList.Add(new Name(name));
                return GetComboName(name);
            }
            else
            {
                return this.comboNameList[index];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private Name GetTrickName(string name)
        {
            int index = this.trickNameList.IndexOf(new Name(name));
            if (index == -1)
            {
                this.trickNameList.Add(new Name(name));
                return GetTrickName(name);
            }
            else
            {
                return this.trickNameList[index];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void AddCombo(string name, int start, int end)
        {
            if (this.playState != PlaybackState.Stopped) throw new NotChangeException();
            Name nameO;
            try
            {
                nameO = GetComboName(name);
            }
            catch (ArgumentException)
            {
                throw;
            }
            if (end > this.freeStyleLength)
            {
                end = this.freeStyleLength;
            }
            foreach (Combo c in this.comboList.ToArray())
            {
                //新規が覆う
                if (c.Start >= start && c.End <= end)
                {
                    this.comboList.Remove(c);
                }
                //新規が分割
                else if (c.Start < start && c.End > end)
                {
                    c.End = start;
                }
                //新規が右
                else if (c.End > start && c.End <= end && c.Start < start)
                {
                    c.End = start;
                }
                //新規が左
                else if (end > c.Start && end <= c.End && start < c.Start)
                {
                    c.Start = end;
                }
            }
            this.comboList.Add(new Combo(start, end, nameO));
            this.comboList.Sort();
        }


        public void AddTrick(string name, Combo combo, int start, int end)
        {
            if (this.playState != PlaybackState.Stopped) throw new NotChangeException();
            combo.AddTrick(new Trick(start, end, GetTrickName(name)));
        }

        public void DeleteCombo(Combo combo)
        {
            if (this.playState != PlaybackState.Stopped) throw new NotChangeException();
            this.comboList.Remove(combo);
        }

        public void DeleteTrick(Combo combo, Trick trick)
        {
            if (this.playState != PlaybackState.Stopped) throw new NotChangeException();
            combo.DeleteTrick(trick);
        }

        public void Save(string fileName)
        {
            if (this.playState != PlaybackState.Stopped) throw new NotChangeException();

            //データ作成
            string text = "yymp,0\n";
            {
                text += this.freeStyleLength + "\n";
                text += FreeStyle.ToCSV(Name.ToStringArray(this.comboNameList)) + "\n";
                text += FreeStyle.ToCSV(Name.ToStringArray(this.trickNameList)) + "\n";

                List<string> comboString = new List<string>();
                List<string> trickString = new List<string>();
                foreach (Combo c in this.comboList)
                {
                    string cText = this.comboNameList.IndexOf(new Name(c.Name)) + ":";
                    cText += c.Start + ":";
                    cText += c.End;
                    comboString.Add(cText);

                    foreach (Trick t in c.TrickList)
                    {
                        string tText = this.comboList.IndexOf(c) + ":";
                        tText += this.trickNameList.IndexOf(new Name(t.Name)) + ":";
                        tText += t.Start + ":";
                        tText += t.End;
                        trickString.Add(tText);
                    }
                }
                text += FreeStyle.ToCSV(comboString) + "\n";
                text += FreeStyle.ToCSV(trickString) + "\n";
                if (this.music != null) text += Convert.ToBase64String(this.music.ByteData);
            }

            using (StreamWriter writer =
              new StreamWriter(fileName, false, Encoding.GetEncoding("UTF-8")))
            {
                writer.WriteLine(text);
            }
        }

        public void SaveComboFile(string fileName)
        {
            if (this.playState != PlaybackState.Stopped) throw new NotChangeException();
            FreeStyle.WriteCsv(Name.ToStringArray(this.comboNameList), fileName, "ympc,0");
        }
        public void LoadComboFile(string fileName)
        {
            if (this.playState != PlaybackState.Stopped) throw new NotChangeException();
            string[] strs = FreeStyle.ReadCSV(fileName, "ympc", "0");
            foreach (string s in strs)
            {
                this.AddComboName(s);
            }
        }

        public void SaveTrickFile(string fileName)
        {
            if (this.playState != PlaybackState.Stopped) throw new NotChangeException();
            FreeStyle.WriteCsv(Name.ToStringArray(this.trickNameList), fileName, "ympt,0");
        }

        public void LoadTrickFile(string fileName)
        {
            if (this.playState != PlaybackState.Stopped) throw new NotChangeException();
            string[] strs = FreeStyle.ReadCSV(fileName, "ympt", "0");
            foreach (string s in strs)
            {
                this.AddTrickName(s);
            }
        }

        /// <summary>
        /// 再生します
        /// </summary>
        public void Play()
        {
            if (this.music == null) throw new NotMusicException();
            this.timer.Start();
            this.playState = PlaybackState.Playing;
        }

        /// <summary>
        /// 停止します
        /// </summary>
        public void Stop()
        {
            if (this.music == null) throw new NotMusicException();
            this.timer.Stop();
            try {
                if (this.music != null) this.music.Stop();
            }
            catch (MmException)
            {
                throw;
            }
            this.position = 0;
            this.playState = PlaybackState.Stopped;
        }

        /// <summary>
        /// 一時停止します
        /// </summary>
        public void Pause()
        {
            if (this.music == null) throw new NotMusicException();
            this.timer.Stop();
            try {
                if (this.music != null) this.music.Pause();
                if (this.music != null) this.music.Position = (int)this.music.Position;
            }
            catch (MmException)
            {
                throw;
            }
            this.playState = PlaybackState.Paused;
        }

        /// <summary>
        /// CSVを読み込んで配列を返します
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="header">ファイルタイプ,バージョン</param>
        /// <returns></returns>
        private static string[] ReadCSV(string fileName, string type, string ver)
        {
            /*ファイルを読み込む*/
            string text = "";
            using (StreamReader sr = new StreamReader(
                fileName, Encoding.GetEncoding("UTF-8")))
            {
                text = sr.ReadToEnd();
            }

            //コンマで分割
            string[] data = text.Split(new string[] { "\n" }, StringSplitOptions.None);

            string[] header = data[0].Split(new string[] { "," }, StringSplitOptions.None);
            if (header[0] != type) throw new TypeFormatException();
            if (header[1] != ver) throw new VerFormatException();

            if (data[1].Length != 0)
            {
                return data[1].Split(new string[] { "," }, StringSplitOptions.None);
            }
            else
            {
                return new string[0];
            }
        }


        /// <summary>
        /// csv形式の文字列に変換します
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private static string ToCSV(List<string> list)
        {
            string data = "";
            if (list.Count != 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    data += list[i];

                    //最後のデータでなければコンマ出力
                    if (i != list.Count - 1) data += ",";
                }
            }
            return data;
        }

        /// <summary>
        /// 指定されたリストをCSVとして保存します
        /// </summary>
        /// <param name="list">リスト</param>
        /// <param name="fileName">ファイル名</param>
        /// <param name="header"></param>
        private static void WriteCsv(List<string> list, string fileName, string header)
        {
            /*保存*/
            using (StreamWriter writer =
              new StreamWriter(fileName, false, Encoding.GetEncoding("UTF-8")))
            {
                writer.WriteLine(header + "\n" + FreeStyle.ToCSV(list));
            }
        }
        #endregion
    }
}