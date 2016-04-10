using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YYMP
{
    /// <summary>
    /// 技を表すクラス
    /// </summary>
    public class Trick : INext, IComparable<Trick>
    {
        #region フィールド
        /// <summary>
        /// 技名
        /// </summary>
        private Name name;

        /// <summary>
        /// 始まる時間
        /// </summary>
        private int start;

        /// <summary>
        /// 終わる時間
        /// </summary>
        private int end;

        /// <summary>
        /// 現在位置
        /// </summary>
        private int position;
        #endregion
        #region プロパティ
        /// <summary>
        /// 技の長さ
        /// </summary>
        public int Length
        {
            get { return this.end-this.start; }
        }

        /// <summary>
        /// 現在位置
        /// </summary>
        public int Position
        {
            get
            {
                return this.position;
            }
            set
            {
                this.position = value;
            }
        }

        public int Start
        {
            get { return this.start; }
            set { this.start = value; }
        }

        public int End
        {
            get { return this.end; }
            set { this.end = value; }
        }
        public string Name
        {
            get { return this.name.Name_; }
        }
        #endregion
        #region コンストラクタ
        /// <summary>
        /// 新しいイントランスを作成します
        /// </summary>
        /// <param name="start">始まる時間</param>
        /// <param name="end">終わる時間</param>
        /// <param name="name">トリック名</param>
        public Trick(int start, int end, Name name)
        {
            this.start = start;
            this.end = end;
            this.name = name;
        }
        #endregion
        #region メソッド
        public int CompareTo(Trick other)
        {
            return this.start.CompareTo(other.start);
        }

        public override string ToString()
        {
            return this.name + " " + this.start + "～" + this.end;
        }
        #endregion
    }
}
