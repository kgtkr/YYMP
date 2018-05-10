using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YYMP
{
    /// <summary>
    /// 再生、停止等を持つインターフェイスです
    /// </summary>
    public interface IPlay
    {
        /// <summary>
        /// 再生中か
        /// </summary>
        PlaybackState Playing
        {
            get;
        }

        /// <summary>
        /// 長さ
        /// </summary>
        double Length
        {
            get;
        }

        /// <summary>
        /// 現在位置
        /// </summary>
        double Position
        {
            get;
            set;
        }

        /// <summary>
        /// 音量
        /// </summary>
        float Volume
        {
            get;
            set;
        }

        /// <summary>
        /// 再生を行います
        /// </summary>
        void Play();

        /// <summary>
        /// 停止します
        /// </summary>
        void Stop();

        /// <summary>
        /// 一時停止します
        /// </summary>
        void Pause();
    }

    /// <summary>
    /// 簡単な再生インターフェイスです
    /// </summary>
    public interface INext
    {
        /// <summary>
        /// 長さ
        /// </summary>
        int Length
        {
            get;
        }

        /// <summary>
        /// 現在位置
        /// </summary>
        int Position
        {
            get;
            set;
        }
    }
}
