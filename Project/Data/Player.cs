using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NAudio;
using NAudio.Wave;
using NAudioDemo;
using NAudio.Wave.SampleProviders;


namespace YYMP
{
    /// <summary>
    /// 曲データを管理する
    /// 対応形式…MP3、WAVE
    /// 参考…http://akabeko.me/blog/tag/naudio/
    /// </summary>
    public class Player : IDisposable, IPlay
    {
        private WaveStream ws;
        private WaveChannel32 wc32;
        private IWavePlayer iwp;
        private readonly double totalTime;//時間
        private readonly byte[] byteData;//バイトデータ

        /// <summary>
        /// イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void PlayerEvent(Player player);

        /// <summary>
        /// 再生位置変更
        /// </summary>
        public event PlayerEvent positionChange;

        public double Length
        {
            get { return this.totalTime; }
        }

        public double Position
        {
            get { return this.ws.CurrentTime.TotalSeconds; }
            set {
                this.ws.CurrentTime = TimeSpan.FromSeconds(value);
                if (this.positionChange != null) this.positionChange(this);
            }
        }

        public PlaybackState Playing
        {
            get { return this.iwp.PlaybackState; }
        }


        public byte[] ByteData
        {
            get { return this.byteData; }
        }
        /// <summary>
        /// イントランスを初期化します
        /// </summary>
        /// <param name="data">音楽ファイル形式のバイナリ配列</param>
        public Player(byte[] data)
        {
            Format format = Check(data);
            WaveChannel32 wc32;
            if (Format.WAV == format)
            {
                WaveStream reader = new WaveFileReader(new MemoryStream(data));
                if (reader.WaveFormat.Encoding != WaveFormatEncoding.Pcm)
                {
                    reader = WaveFormatConversionStream.CreatePcmStream(reader);
                    reader = new BlockAlignReductionStream(reader);
                }

                if (reader.WaveFormat.BitsPerSample != 16)
                {
                    var wf = new WaveFormat(reader.WaveFormat.SampleRate, 16, reader.WaveFormat.Channels);
                    reader = new WaveFormatConversionStream(wf, reader);
                }
                wc32 = new WaveChannel32(reader);
            }
            else if (Format.MP3 == format)
            {
                var mfr = new Mp3FileReader(new MemoryStream(data));
                var wfcs = WaveFormatConversionStream.CreatePcmStream(mfr);
                var bars = new BlockAlignReductionStream(wfcs);

                wc32 = new WaveChannel32(bars);
            }
            else
            {
                //例外
                throw new HeaderFormatException();
            }
            this.wc32 = wc32;
            this.ws = new MeteringStream(wc32, wc32.WaveFormat.SampleRate / 10);

            this.iwp = new WaveOut() { DesiredLatency = 200 };
            this.iwp.Init(this.ws);
            this.totalTime = this.ws.TotalTime.TotalSeconds;//時間オブジェクト取得→秒数取得
            this.byteData = data;
        }
        /// <summary>
        /// フォーマットを確認します
        /// </summary>
        /// <param name="data">音楽ファイル形式のバイナリ配列</param>
        /// <returns>フォーマット。未対応ならERROR</returns>
        public static Format Check(byte[] data)
        {
            if (data.Length < 3) return Format.ERROR;//もしデータの長さが3未満ならどれでもない
            if ((data[0] == 0x49 && data[1] == 0x44 && data[2] == 0x33)||
                (data[0]==0xFF&&data[1]==0xFB))//ID3ヘッダorもう一つのヘッダ確認
            {
                return Format.MP3;
            }



            /*wav確認*/
            if (data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46)//RIFFヘッダ確認
            {
                return Format.WAV;
            }
            return Format.ERROR;//エラー

        }

        /// <summary>
        /// 再生を開始します。
        /// </summary>
        public void Play()
        {
            switch (this.iwp.PlaybackState)
            {
                case PlaybackState.Playing:
                    break;

                case PlaybackState.Paused:
                case PlaybackState.Stopped:
                    
                    this.iwp.Play();
                    
                    break;
            }
        }

        /// <summary>
        /// 再生を停止します。
        /// </summary>
        public void Stop()
        {
            this.iwp.Stop();
            this.ws.Position = 0;
        }


        /// <summary>
        /// 一時停止します
        /// </summary>
        public void Pause()
        {
            this.iwp.Pause();
        }

        /// <summary>
        /// ボリュームを取得または設定します。
        /// </summary>
        public float Volume
        {
            get
            {
                return this.wc32.Volume;
            }
            set
            {
                this.wc32.Volume = value;
            }
        }

        /// <summary>
        /// リソースの解放を行います。
        /// </summary>
        public void Dispose()
        {
            if (this.iwp != null)
            {
                this.iwp.Stop();
            }

            if (this.ws != null)
            {
                this.wc32.Close();
                this.wc32 = null;

                this.ws.Close();
                this.ws = null;
            }

            if (this.iwp != null)
            {
                this.iwp.Dispose();
                this.iwp = null;
            }
        }
    }

    //フォーマット
    public enum Format { MP3, WAV, ERROR }
}
