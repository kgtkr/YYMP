using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using System.Globalization;

namespace YYMP
{
    public class FreeStyleViewModel : ViewModelBase
    {
        /// <summary>
        /// フリースタイル
        /// </summary>
        private FreeStyle freeStyle;

        private ComboBox trickDeleteComboComboBox;

        public FreeStyleViewModel(ComboBox trickDeleteComboComboBox)
        {
            this.trickDeleteComboComboBox = trickDeleteComboComboBox;
            this.FreeStyle = new FreeStyle();
        }

        private FreeStyle FreeStyle
        {
            set
            {
                this.freeStyle = value;
                this.freeStyle.timerEvent += new FreeStyle.FreeStyleEvent(TimerEvent);
            }
        }

        /// <summary>
        /// プロパティ変更イベントを発生させます
        /// </summary>
        public void Update()
        {
            RaisePropertyChanged(null);
        }

        /// <summary>
        /// タイマーイベント
        /// </summary>
        /// <param name="fs"></param>
        private void TimerEvent(FreeStyle fs)
        {
            RaisePropertyChanged(nameof(NowComboName));
            RaisePropertyChanged(nameof(NowTrickName));
            RaisePropertyChanged(nameof(NextTrickName));
            RaisePropertyChanged(nameof(NextComboName));
            RaisePropertyChanged(nameof(NextComboFastTrickName));
            RaisePropertyChanged(nameof(NowComboLength));
            RaisePropertyChanged(nameof(NowComboPosition));
            RaisePropertyChanged(nameof(NowTrickLength));
            RaisePropertyChanged(nameof(NowTrickPosition));

            RaisePropertyChanged(nameof(Position));
            RaisePropertyChanged(nameof(FreeStylePosition));

            RaisePropertyChanged(nameof(StopButton));

            RaisePropertyChanged(nameof(FreeLenSetIsEnabled));
            RaisePropertyChanged(nameof(StopIsEnabled));
        }

        #region プロパティのラッパー
        #region 有効/無効
        /// <summary>
        /// 止まっているか
        /// </summary>
        public bool StopIsEnabled
        {
            get { return this.freeStyle.Playing == PlaybackState.Stopped; }
        }

        /// <summary>
        /// プレイ関係が可能か
        /// </summary>
        public bool PlayIsEnabled
        {
            get { return this.freeStyle.Music != null; }
        }

        /// <summary>
        /// フリースタイルの長さ変更が可能か
        /// </summary>
        public bool FreeLenSetIsEnabled
        {
            get
            {
                return this.StopIsEnabled && this.freeStyle.Music != null;
            }
        }
        #endregion
        #region 編集
        /// <summary>
        /// フリースタイルの長さ
        /// </summary>
        public string FreeStyleLength
        {
            get {
                return this.freeStyle.FreeStyleLength.ToString();
            }
            set
            {
                try {
                    this.freeStyle.FreeStyleLength = int.Parse(value);
                }
                catch (Exception e) when (e is FormatException || e is OverflowException||e is ArgumentOutOfRangeException)
                {
                    this.freeStyle.FreeStyleLength = 0;
                }

                RaisePropertyChanged(nameof(FreeStyleLength));
                RaisePropertyChanged(nameof(Length));
            }
        }

        /// <summary>
        /// コンボ名リスト
        /// </summary>
        public List<object> ComboNameList
        {
            get
            {
                var list = this.freeStyle.ComboNameList;
                List<object> list2 = list.ConvertAll(x => (object)x);
                return list2;
            }
        }

        /// <summary>
        /// トリック名リスト
        /// </summary>
        public List<object> TrickNameList
        {
            get
            {
                var list = this.freeStyle.TrickNameList;
                List<object> list2 = list.ConvertAll(x => (object)x);
                return list2;
            }
        }

        /// <summary>
        /// コンボリスト
        /// </summary>
        public List<Combo> ComboList
        {
            get { return this.freeStyle.ComboList; }
        }

        /// <summary>
        /// トリックリスト
        /// </summary>
        public List<Trick> TrickList
        {
            get
            {
                Combo c = (Combo)this.trickDeleteComboComboBox.SelectedItem;
                return c == null ? new List<Trick>() : c.TrickList;
            }
        }
        #endregion
        #region 再生文字
        public string NowComboName
        {
            get
            {
                Combo c = this.freeStyle.NowCombo;
                return c == null ? "(現在のコンボ)" : c.Name;
            }
        }

        public string NowTrickName
        {
            get
            {
                Trick t = this.freeStyle.NowTrick;
                return t == null ? "(現在の技)" : t.Name;
            }
        }

        public string NextTrickName
        {
            get
            {
                Trick t = this.freeStyle.NextTrick;
                return t == null ? "(次の技)" : t.Name;
            }
        }

        public string NextComboName
        {
            get
            {
                Combo c = this.freeStyle.NextCombo;
                return c == null ? "(次のコンボ)" : c.Name;
            }
        }

        public string NextComboFastTrickName
        {
            get
            {
                Trick t = this.freeStyle.NextComboFastTrick;
                return t == null ? "(次のコンボの最初の技)" : t.Name;
            }
        }

        public int NowComboLength
        {
            get
            {
                Combo c = this.freeStyle.NowCombo;
                return c == null ? 0 : c.Length;
            }
        }

        public int NowComboPosition
        {
            get
            {
                Combo c = this.freeStyle.NowCombo;
                return c == null ? 0 : c.Position;
            }
        }

        public int NowTrickLength
        {
            get
            {
                Trick t = this.freeStyle.NowTrick;
                return t == null ? 0 : t.Length;
            }
        }

        public int NowTrickPosition
        {
            get
            {
                Trick t = this.freeStyle.NowTrick;
                return t == null ? 0 : t.Position;
            }
        }
        #endregion
        #region 再生
        public int Length
        {
            get { return (int)this.freeStyle.Length; }
        }

        public int Position
        {
            get { return (int)this.freeStyle.Position; }
            set {
                this.freeStyle.Position = value;
                RaisePropertyChanged(nameof(Position));
                RaisePropertyChanged(nameof(FreeStylePosition));
            }
        }

        public int FreeStylePosition
        {
            get { return (int)this.freeStyle.Position - this.freeStyle.Wait; }
        }

        public string StopButton
        {
            get { return this.freeStyle.Playing == PlaybackState.Playing ? "一時停止" : "再生"; }
        }

        /// <summary>
        /// loopするか？
        /// </summary>
        public bool Loop
        {
            get { return this.freeStyle.Loop; }
            set {
                this.freeStyle.Loop = value;
                RaisePropertyChanged(nameof(Loop));
            }
        }

        /// <summary>
        /// 待ち時間
        /// </summary>
        public string Wait
        {
            get { return this.freeStyle.Wait.ToString(); }
            set
            {
                try
                {
                    this.freeStyle.Wait = int.Parse(value);
                }
                catch (Exception e) when (e is FormatException || e is OverflowException||e is ArgumentOutOfRangeException)
                {
                    this.freeStyle.Wait = 0;
                }
                RaisePropertyChanged(nameof(Wait));
                RaisePropertyChanged(nameof(Length));
            }
        }

        /// <summary>
        /// 音量
        /// 0～1
        /// </summary>
        public int Volume
        {
            get { return (int)(this.freeStyle.Volume * 100); }
            set {
                this.freeStyle.Volume = value / 100.0f;
                RaisePropertyChanged(nameof(Volume));
            }
        }
        #endregion
        #endregion
        #region 操作
        #region メニュー
        /// <summary>
        /// 開く
        /// </summary>
        /// <param name="fileName"></param>
        public void Load(string fileName)
        {
            this.FreeStyle = new FreeStyle(fileName);
            RaisePropertyChanged(null);
        }

        /// <summary>
        /// 新規
        /// </summary>
        public void New()
        {
            this.FreeStyle = new FreeStyle();
            RaisePropertyChanged(null);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(string fileName)
        {
            this.freeStyle.Save(fileName);
        }
        #endregion
        #region 編集
        /// <summary>
        /// 音楽をロード
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadMusic(string fileName)
        {
            this.freeStyle.LoadMusic(fileName);
            RaisePropertyChanged(nameof(FreeStyleLength));
            RaisePropertyChanged(nameof(Length));
            RaisePropertyChanged(nameof(FreeLenSetIsEnabled));
            RaisePropertyChanged(nameof(PlayIsEnabled));
            RaisePropertyChanged(nameof(StopIsEnabled));
        }

        /// <summary>
        /// コンボファイルロード
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadComboFile(string fileName)
        {
            this.freeStyle.LoadComboFile(fileName);
            RaisePropertyChanged(nameof(ComboNameList));
        }

        /// <summary>
        /// コンボファイルセーブ
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveComboFile(string fileName)
        {
            this.freeStyle.SaveComboFile(fileName);
        }

        /// <summary>
        /// コンボ名変更
        /// </summary>
        /// <param name="before"></param>
        /// <param name="after"></param>
        public void ChangeComboName(string before, string after)
        {
            this.freeStyle.ChangeComboName(before, after);
            RaisePropertyChanged(nameof(ComboNameList));
            RaisePropertyChanged(nameof(ComboList));
        }

        /// <summary>
        /// 技ファイルロード
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadTrickFile(string fileName)
        {
            this.freeStyle.LoadTrickFile(fileName);
            RaisePropertyChanged(nameof(TrickNameList));
        }

        /// <summary>
        /// 技ファイルセーブ
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveTrickFile(string fileName)
        {
            this.freeStyle.SaveTrickFile(fileName);
        }

        /// <summary>
        /// 技名変更
        /// </summary>
        /// <param name="before"></param>
        /// <param name="after"></param>
        public void ChangeTrickName(string before, string after)
        {
            this.freeStyle.ChangeTrickName(before, after);
            RaisePropertyChanged(nameof(TrickNameList));
            RaisePropertyChanged(nameof(TrickList));
        }

        /// <summary>
        /// コンボ追加
        /// </summary>
        /// <param name="name"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void AddCombo(string name,int start,int end)
        {
            this.freeStyle.AddCombo(name, start, end);
            RaisePropertyChanged(nameof(ComboList));
        }

        public void AddTrick(string name, Combo combo, int start, int end)
        {
            this.freeStyle.AddTrick(name, combo, start, end);
            RaisePropertyChanged(nameof(TrickList));
        }

        public void DeleteCombo(Combo combo)
        {
            this.freeStyle.DeleteCombo(combo);
            RaisePropertyChanged(nameof(ComboList));
        }

        public void DeleteTrick(Combo combo, Trick trick)
        {
            this.DeleteTrick(combo, trick);
            RaisePropertyChanged(nameof(TrickList));
        }
        #endregion
        #region 再生
        public void PlayPause()
        {
            if (freeStyle.Playing == PlaybackState.Playing)
            {
                this.freeStyle.Pause();
            }
            else
            {
                this.freeStyle.Play();
            }
            RaisePropertyChanged(nameof(StopButton));
            RaisePropertyChanged(nameof(FreeLenSetIsEnabled));
            RaisePropertyChanged(nameof(PlayIsEnabled));
            RaisePropertyChanged(nameof(StopIsEnabled));
        }

        public void Stop()
        {
            this.freeStyle.Stop();
            RaisePropertyChanged(nameof(StopButton));
            RaisePropertyChanged(nameof(FreeLenSetIsEnabled));
            RaisePropertyChanged(nameof(PlayIsEnabled));
            RaisePropertyChanged(nameof(StopIsEnabled));
        }

        public void Back()
        {
            this.Position = 0;
        }
        #endregion
        #endregion
    }

    /// <summary>
    /// リストに「新規...」を追加するコンバーター
    /// </summary>
    public class NewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<object> list = new List<object>(((List<object>)value));
            list.Insert(0, "新規..."); ;
            return list;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
