using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio.Wave;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualBasic;

namespace YYMP
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// ビューモデル
        /// </summary>
        private FreeStyleViewModel fss;

        /// <summary>
        /// コンボファイルのパス
        /// </summary>
        private string comboFileName = "";

        /// <summary>
        /// 技ファイルへのパス
        /// </summary>
        private string trickFileName = "";

        /// <summary>
        /// YYMPファイルへのパス
        /// </summary>
        private string yympFileName = "";

        public MainWindow()
        {
            InitializeComponent();
            this.fss = new FreeStyleViewModel(this.TrickDeleteComboComboBox);
            this.DataContext = this.fss;
            if (Environment.GetCommandLineArgs().Length!=1)
            {
                OpenYYMP(Environment.GetCommandLineArgs()[1]);
            }
        }

        #region YYMPを開く
        /// <summary>
        /// YYMPファイルを開きます
        /// </summary>
        /// <param name="file"></param>
        private void OpenYYMP(string file)
        {
            try
            {
                try
                {
                    this.yympFileName = file;
                    this.comboFileName = null;
                    this.trickFileName = null;
                    this.fss.Load(file);
                }
                catch (NAudio.MmException)
                {
                    System.Windows.Forms.MessageBox.Show("デバイスエラー。終了します。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                    System.Windows.Forms.Application.Exit();
                }
            }
            catch (TypeFormatException)
            {
                System.Windows.Forms.MessageBox.Show("ファイルフォーマットが不正です。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (VerFormatException)
            {
                System.Windows.Forms.MessageBox.Show("ファイルバージョンが不正です。最新版のYYMPをご利用下さい。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                System.Windows.Forms.MessageBox.Show("入出力エラーが発生しました。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        #endregion

        #region メニュー
        private void NewMenu_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show(
                "現在の内容を破棄しますか？", "確認",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question
            ) == System.Windows.Forms.DialogResult.Yes)
            {
                this.yympFileName = null;
                this.comboFileName = null;
                this.trickFileName = null;

                this.fss.New();
            }
        }

        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show(
                "現在の内容を破棄しますか？", "確認",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question
            ) == System.Windows.Forms.DialogResult.Yes)
            {
                string file = OpenFileDialog("YYMPファイル(*.yymp) | *.yymp");
                if (!string.IsNullOrEmpty(file))
                {
                    OpenYYMP(file);
                }
            }
        }

        private void SaveMenu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.yympFileName))
            {
                SaveAsMenu_Click(null, null);
            }
            else
            {
                try
                {
                    this.fss.Save(this.yympFileName);
                }
                catch (IOException ex)
                {
                    System.Windows.Forms.MessageBox.Show("入出力エラーが発生しました。",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void SaveAsMenu_Click(object sender, RoutedEventArgs e)
        {
            string fileName = FileSaveDialog("YYMPファイル(*.yymp)|*.yymp", "*.yymp");
            if (!string.IsNullOrEmpty(fileName))
            {
                this.yympFileName = fileName;
                SaveMenu_Click(null, null);
            }
        }
        #endregion
        #region Open、Save
        /// <summary>
        /// ファイルを開くダイアログを表示し、もしOKが押されたなら
        /// ファイルパスを指定されたテキストボックスに表示します。
        /// </summary>
        /// <param name="filter">フィルター</param>
        /// <param name="textBox">表示するテキストボックス</param>
        private static string OpenFileDialog(string filter, System.Windows.Controls.TextBox textBox = null)
        {
            //OpenFileDialogクラスのインスタンスを作成
            OpenFileDialog ofd = new OpenFileDialog();

            //[ファイルの種類]に表示される選択肢を指定する
            //指定しないとすべてのファイルが表示される
            ofd.Filter = filter;
            //[ファイルの種類]ではじめに
            //「対応ファイル」が選択されているようにする
            ofd.FilterIndex = 1;
            //タイトルを設定する
            ofd.Title = "ファイルを選択してください";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd.RestoreDirectory = true;
            //存在しないファイルの名前が指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            ofd.CheckFileExists = true;
            //存在しないパスが指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            ofd.CheckPathExists = true;

            //ダイアログを表示する
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (textBox != null)
                {
                    //OKボタンがクリックされたとき
                    //選択されたファイル名をテキストBOXに表示する
                    textBox.Text = ofd.FileName;
                }
                return ofd.FileName;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 保存ダイアログを表示します
        /// </summary>
        /// <param name="filter">フィルター</param>
        /// <param name="default_">デフォルトのファイル名</param>
        /// <returns>選択されたパス。もし選択されなければ空の文字列</returns>
        private static string FileSaveDialog(string filter, string default_)
        {
            //SaveFileDialogクラスのインスタンスを作成
            SaveFileDialog sfd = new SaveFileDialog();

            //はじめのファイル名を指定する
            sfd.FileName = default_;
            //[ファイルの種類]に表示される選択肢を指定する
            sfd.Filter = filter;
            //[ファイルの種類]ではじめに
            //「すべてのファイル」が選択されているようにする
            sfd.FilterIndex = 2;
            //タイトルを設定する
            sfd.Title = "保存先のファイルを選択してください";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            sfd.RestoreDirectory = true;
            //既に存在するファイル名を指定したとき警告する
            //デフォルトでTrueなので指定する必要はない
            sfd.OverwritePrompt = true;
            //存在しないパスが指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            sfd.CheckPathExists = true;

            //ダイアログを表示する
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return sfd.FileName;
            }
            else
            {
                return "";
            }
        }
        #endregion
        #region 編集
        private void MusicOpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog
               ("対応ファイル(*.mp3;*.wave;*.wav)|*.mp3;*.wave;*.wav|mp3ファイル(*.mp3)|*.mp3|waveファイル(*.wave;*.wav)|*.wave;*.wav",
               this.MusicPathTextBox);
        }

        private void MusicSetButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.MusicPathTextBox.Text))
            {
                try
                {
                    this.fss.LoadMusic(this.MusicPathTextBox.Text);
                    this.MusicPathTextBox.Text = "";
                }
                catch (IOException)
                {
                    System.Windows.Forms.MessageBox.Show("入出力エラーが発生しました。",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                catch (HeaderFormatException)
                {
                    System.Windows.Forms.MessageBox.Show("対応していないファイル形式です。",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("ファイルが選択されていません。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void YMPCOpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog
                ("YYMPコンボファイル(*.ympc)|*.ympc",
                this.YMPCPathText);
        }

        private void YMPCSetButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.YMPCPathText.Text))
            {
                try
                {
                    this.fss.LoadComboFile(this.YMPCPathText.Text);
                    this.comboFileName = this.YMPCPathText.Text;
                    this.YMPCPathText.Text = "";
                }
                catch (TypeFormatException)
                {
                    System.Windows.Forms.MessageBox.Show("ファイルフォーマットが不正です。",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                catch (VerFormatException)
                {
                    System.Windows.Forms.MessageBox.Show("ファイルバージョンが不正です。最新版のYYMPをご利用下さい。",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                catch (IOException)
                {
                    System.Windows.Forms.MessageBox.Show("入出力エラーが発生しました。",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("ファイルが選択されていません。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void YMPCSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.comboFileName))
            {
                YMPCSaveAsButton_Click(null, null);
            }
            else
            {
                try
                {
                    this.fss.SaveComboFile(this.comboFileName);
                }
                catch (IOException)
                {
                    System.Windows.Forms.MessageBox.Show("入出力エラーが発生しました。",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void YMPCSaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            string fileName = FileSaveDialog("YYMPコンボファイル(*.ympc)|*.ympc", "*.ympc");
            if (!string.IsNullOrEmpty(this.comboFileName))
            {
                this.comboFileName = fileName;
                YMPCSaveButton_Click(null, null);
            }
        }

        private void ComboNameChangeButton_Click(object sender, RoutedEventArgs e)
        {
           if (!(this.ComboNameChangeBeforeComboBox.SelectedIndex == -1 || this.ComboNaneChangeAfterTextBox.Text == ""))
            {
                try
                {
                    this.fss.ChangeComboName(this.ComboNameChangeBeforeComboBox.Text, this.ComboNaneChangeAfterTextBox.Text);

                    this.ComboNameChangeBeforeComboBox.SelectedIndex = -1;
                    this.ComboNaneChangeAfterTextBox.Text = "";
                }
                catch (ArgumentException)
                {
                    System.Windows.Forms.MessageBox.Show(YYMP.Name.regexName + "に一致しません。",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("コンボボックス、テキストボックスの両方に値を設定して下さい。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void YMPTOpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog
                ("YYMPトリックファイル(*.ympt)|*.ympt",
                this.YMPTPathTextBox);
        }

        private void YMPTSetButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.YMPTPathTextBox.Text))
            {
                try
                {
                    this.fss.LoadTrickFile(this.YMPTPathTextBox.Text);
                    this.trickFileName = this.YMPTPathTextBox.Text;

                    this.YMPTPathTextBox.Text = "";
                }
                catch (TypeFormatException)
                {
                    System.Windows.Forms.MessageBox.Show("ファイルフォーマットが不正です。",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                catch (VerFormatException)
                {
                    System.Windows.Forms.MessageBox.Show("ファイルバージョンが不正です。最新版のYYMPをご利用下さい。",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                catch (IOException)
                {
                    System.Windows.Forms.MessageBox.Show("入出力エラーが発生しました。",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("ファイルが選択されていません。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void YMPTSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.trickFileName))
            {
                YMPTSaveAsButton_Click(null, null);
            }
            else
            {
                try
                {
                    this.fss.SaveTrickFile(this.trickFileName);
                }
                catch (IOException)
                {
                    System.Windows.Forms.MessageBox.Show("入出力エラーが発生しました。",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void YMPTSaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            string fileName = FileSaveDialog("YYMPトリックファイル(*.ympt)|*.ympt", "*.ympt");
            if (!string.IsNullOrEmpty(this.trickFileName))
            {
                this.trickFileName = fileName;
                YMPTSaveButton_Click(null, null);
            }
        }

        private void TrickNameChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(this.TrickNameChangeBeforeComboBox.SelectedIndex == -1 || this.TrickNameChangeAfterTextBox.Text == ""))
            {
                try
                {
                    this.fss.ChangeTrickName(this.TrickNameChangeBeforeComboBox.Text, this.TrickNameChangeAfterTextBox.Text);

                    this.TrickNameChangeBeforeComboBox.SelectedIndex = -1;
                    this.TrickNameChangeAfterTextBox.Text = "";
                }
                catch (ArgumentException)
                {
                    System.Windows.Forms.MessageBox.Show(YYMP.Name.regexName + "に一致しません。",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("コンボボックス、テキストボックスの両方に値を設定して下さい。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ComboAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ComboAddComboNameComboBox.SelectedIndex != -1 &&
                !string.IsNullOrEmpty(this.ComboAddStartTextBox.Text) &&
                !string.IsNullOrEmpty(this.ComboAddEndTextBox.Text))
            {
                string name = this.ComboAddComboNameComboBox.Text == "新規..." ? Interaction.InputBox("コンボ名は？", "コンボ名", "", 200, 100) : this.ComboAddComboNameComboBox.Text;
                if (!string.IsNullOrEmpty(name))
                {
                    try
                    {
                        this.fss.AddCombo(name,
                            int.Parse(this.ComboAddStartTextBox.Text),
                            int.Parse(ComboAddEndTextBox.Text));

                        this.ComboAddStartTextBox.Text = "";
                        this.ComboAddEndTextBox.Text = "";
                        this.ComboAddComboNameComboBox.SelectedIndex = -1;
                    }
                    catch (ArgumentException)
                    {
                        System.Windows.Forms.MessageBox.Show(YYMP.Name.regexName + "に一致しません。",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("全ての値を入力or選択して下さい。",
                "エラー",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            }
        }

        private void ComboDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ComboDeleteComboBox.SelectedIndex != -1)
            {
                this.fss.DeleteCombo((Combo)this.ComboDeleteComboBox.SelectedItem);

                this.ComboDeleteComboBox.SelectedIndex = -1;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("コンボを選択して下さい。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void TrickAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.TrickAddComboComboBox.SelectedIndex != -1 &&
                !string.IsNullOrEmpty(this.TrickAddStartTextBox.Text) &&
                !string.IsNullOrEmpty(this.TrickAddEndTextBox.Text) &&
                this.TrickAddTrickNameComboBox.SelectedIndex != -1)
            {
                string name = this.TrickAddTrickNameComboBox.Text == "新規..." ? Interaction.InputBox("トリック名は？", "トリック名", "", 200, 100) : this.TrickAddTrickNameComboBox.Text;
                try
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        this.fss.AddTrick(name,
                            (Combo)this.TrickAddComboComboBox.SelectedItem,
                            int.Parse(this.TrickAddStartTextBox.Text),
                            int.Parse(this.TrickAddEndTextBox.Text)
                            );

                        this.TrickAddStartTextBox.Text = "";
                        this.TrickAddEndTextBox.Text = "";
                        this.TrickAddTrickNameComboBox.SelectedIndex = -1;
                        this.TrickAddComboComboBox.SelectedIndex = -1;
                    }
                }
                catch (ArgumentException)
                {
                    System.Windows.Forms.MessageBox.Show(YYMP.Name.regexName + "に一致しません。",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("全ての値を入力or選択して下さい。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void TrickDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (TrickDeleteComboComboBox.SelectedIndex != -1 &&
                TrickDeleteTrickComboBox.SelectedIndex != -1)
            {
                ((Combo)this.TrickDeleteComboComboBox.SelectedItem).DeleteTrick((Trick)this.TrickDeleteTrickComboBox.SelectedItem);

                this.TrickDeleteComboComboBox.SelectedIndex = -1;
                this.TrickDeleteTrickComboBox.SelectedIndex = -1;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("コンボとトリックを選択して下さい。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        #endregion
        #region 再生
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            this.fss.Stop();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {

            this.fss.PlayPause();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {

            this.fss.Back();
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show(
                "現在の内容を破棄しますか？", "確認",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question
            ) == System.Windows.Forms.DialogResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}
