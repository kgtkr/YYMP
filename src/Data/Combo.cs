using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YYMP
{
    /// <summary>
    /// コンボを表すクラス
    /// </summary>
    public class Combo : INext, IComparable<Combo>
    {
        #region フィールド
        /// <summary>
        /// コンボ名
        /// </summary>
        private readonly Name name;

        /// <summary>
        /// 技一覧
        /// </summary>
        private readonly List<Trick> trickList = new List<Trick>();

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
        /// トリックリスト
        /// </summary>
        public List<Trick> TrickList
        {
            get { return this.trickList; }
        }

        /// <summary>
        /// 長さ
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
                if (this.NowTrick != null)
                {
                    this.NowTrick.Position = this.Position - this.NowTrick.Start;
                }
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

        /// <summary>
        /// 現在の技
        /// </summary>
        public Trick NowTrick
        {
            get
            {
                //現在位置
                foreach (Trick t in this.trickList)
                {
                    if (this.position >= t.Start && this.position < t.End)
                    {
                        return t;
                    }
                }
                return null;
            }
        }

        //最初のトリック
        public Trick FastTrick
        {
            get
            {
                if (this.trickList.Count != 0)
                {
                    return this.trickList[0];
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
                Trick t;
                if ((t = this.NowTrick) != null)
                {//現在のトリックがあるなら
                    int index = this.trickList.IndexOf(t) + 1;
                    if (index != this.trickList.Count)//最後のトリックでなければ
                    {
                        return this.trickList[index];
                    }
                    else//次のトリックはない
                    {
                        return null;
                    }
                }
                else//現在のトリックがない
                {
                    Trick afterTrick = null;
                    foreach (Trick tr in this.trickList)
                    {
                        if ((afterTrick != null ? afterTrick.End : 0)<= this.position && this.position < tr.Start)
                        {
                            return tr;
                        }
                        afterTrick = tr;
                    }
                    return null;
                }
            }
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
        /// <param name="name">コンボ名</param>
        public Combo(int start, int end, Name name)
        {
            this.start = start;
            this.end = end;
            this.name = name;
        }
        #endregion
        #region メソッド

        /// <summary>
        /// 技を追加します
        /// </summary>
        /// <param name="trick">技</param>
        public void AddTrick(Trick trick)
        {
            if (trick.End > this.Length)
            {
                trick.End = this.Length;
            }
            foreach (Trick t in this.trickList.ToArray())
            {
                //新規が覆う
                if (t.Start >= trick.Start && t.End <= trick.End)
                {
                    this.trickList.Remove(t);
                }
                //新規が分割
                else if (t.Start < trick.Start && t.End > trick.End)
                {
                    t.End = trick.Start;
                }
                //新規が右
                else if (t.End > start && t.End <= end && t.Start < start)
                {
                    t.End = trick.Start;
                }
                //新規が左
                else if (end > t.Start && end <= t.End && start < t.Start)
                {
                    t.Start = trick.End;
                }
            }
            this.trickList.Add(trick);
            this.trickList.Sort();
        }

        /// <summary>
        /// 技を削除します
        /// </summary>
        /// <param name="trick">削除する技</param>
        public void DeleteTrick(Trick trick)
        {
            this.trickList.Remove(trick);
        }

        public int CompareTo(Combo other)
        {
            return this.start.CompareTo(other.start);
        }

        public override string ToString()
        {
            return this.name + " " + this.start + "～" + this.end+" "+this.Length+"秒";
        }
        #endregion
    }
}
