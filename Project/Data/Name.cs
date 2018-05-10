using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YYMP
{
    /// <summary>
    /// 名前を表すクラス
    /// </summary>
    public class Name : IComparable<Name>
    {
        #region フィールド
        /// <summary>
        /// 許可する名前の正規表現
        /// 1～10文字
        /// 漢字、ひらがな、カタカナ、数字、アルファベット、アンダーバー、ハイフン
        /// </summary>
        public const string regexName = @"[一-龠ぁ-んァ-ン0-9a-zA-z_\-]{1,10}";

        /// <summary>
        /// 名前
        /// </summary>
        private string name;
        #endregion
        #region プロパティ
        /// <summary>
        /// 名前
        /// </summary>
        public string Name_
        {
            get { return this.name; }
            set {
                if (!Name.StringCheck(value)) throw new ArgumentException();
                this.name = value;
            }
        }
        #endregion
        #region コンストラクタ
        /// <summary>
        /// 新しいイントランスを作成します。
        /// </summary>
        /// <param name="name"></param>
        public Name(string name)
        {
            if (!Name.StringCheck(name)) throw new ArgumentException();
            this.name = name;
        }
        #endregion
        #region メソッド
        /// <summary>
        /// このイントランスの文字列表現を返します。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.name;
        }

        /// <summary>
        /// 指定された名前配列を文字列配列に変換します。
        /// </summary>
        /// <param name="names">名前配列</param>
        /// <returns>文字列配列</returns>
        public static List<string> ToStringArray(List<Name> names)
        {
            List<string> list = new List<string>(names.Count);

            if (names.Count != 0)
            {
                for (int i = 0; i < names.Count; i++)
                {
                    list.Add(names[i].ToString());
                }
            }

            return list;
        }

        public static List<Name> ToNameArray(List<string> strings)
        {
            List<Name> list = new List<Name>(strings.Count);
            if (strings.Count != 0)
            {
                for (int i = 0; i < strings.Count; i++)
                {
                    list.Add(new Name(strings[i]));
                }
            }
            return list;
        }
        #region コレクション用
        /// <summary>
        /// このオブジェクトと指定されたオブジェクトが等価かを返します
        /// </summary>
        /// <param name="obj">比較対象のオブジェクト</param>
        /// <returns>等価か？</returns>
        public override bool Equals(object obj)
        {
            //objがnullか、型が違うときは、等価でない
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            return this.name == ((Name)obj).name;
        }

        /// <summary>
        /// このオブジェクトのハッシュ値を返します
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode()
        {
            return this.name.GetHashCode();
        }

        /// <summary>
        /// 順序を付けます
        /// </summary>
        /// <param name="other">比較するオブジェクト</param>
        /// <returns>結果</returns>
        public int CompareTo(Name other)
        {
            return this.name.CompareTo(other.ToString());
        }

        /// <summary>
        /// 文字列が正規表現に一致するか調べます
        /// </summary>
        /// <param name="str">調べたい文字列</param>
        /// <returns>結果</returns>
        public static bool StringCheck(string str)
        {
            return Regex.IsMatch(str, Name.regexName);
        }
        #endregion
        #endregion
    }
}
