using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YYMP
{
    /// <summary>
    /// 変更できないのに変更しようとした
    /// </summary>
    public class NotChangeException : Exception
    {
        public NotChangeException() : base("再生、一時停止中は変更できません。")
        {

        }
    }

    /// <summary>
    /// 音楽が設定されていないので操作できない
    /// </summary>
    public class NotMusicException : Exception
    {
        public NotMusicException() : base("音楽がセットされていません")
        {

        }
    }

    /// <summary>
    /// ファイルのフォーマットがおかしい
    /// </summary>
    public class FileFormatException : Exception
    {
        public FileFormatException() : base("ファイルのフォーマットが不正です。")
        {

        }
    }

    /// <summary>
    /// ヘッダーがおかしい
    /// </summary>
    public class HeaderFormatException : FileFormatException
    {
        public HeaderFormatException()
        {

        }
    }

    /// <summary>
    /// ファイルタイプがおかしい
    /// </summary>
    public class TypeFormatException: HeaderFormatException
    {

    }

    /// <summary>
    /// バージョンがおかしい
    /// </summary>
    public class VerFormatException : HeaderFormatException
    {
        public VerFormatException()
        {

        }
    }



    /// <summary>
    /// データがおかしい
    /// </summary>
    public class DataFormatException : FileFormatException
    {
        public DataFormatException()
        {

        }
    }
}
