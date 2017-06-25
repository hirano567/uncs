//============================================================================
// Classes.cs (uncs\Utilities\)
//
// 2015/04/20 (hirano567@hotmail.co.jp)
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace Uncs
{
    //======================================================================
    // class BCDictionaryEx
    //
    /// <summary>
    /// <para>
    /// Dictionary の拡張クラス。(PCSClasses.cs)
    /// </para>
    /// </summary>
    //======================================================================
    internal class BCDictionaryEx<KEY, VALUE>
    {
        private Dictionary<KEY, VALUE> dictionary = new Dictionary<KEY, VALUE>();

        //------------------------------------------------------------
        // BCDictionaryEx   Constructor
        //
        /// <summary>コンストラクタ。Initialize() を呼び出す。</summary>
        //------------------------------------------------------------
        internal BCDictionaryEx()
        {
            Initialize();
        }

        //------------------------------------------------------------
        // BCDictionaryEx.Add
        //
        /// <summary>キーと値の組を追加する。</summary>
        //------------------------------------------------------------
        internal bool Add(KEY key, VALUE value)
        {
            DebugUtil.Assert(this.dictionary != null);

            try
            {
                dictionary.Add(key, value);
                return true;
            }
            catch (ArgumentException)
            {
            }
            return false;
        }

        //------------------------------------------------------------
        // BCDictionaryEx.GetValue
        //
        /// <summary></summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool GetValue(KEY key, out VALUE value)
        {
            DebugUtil.Assert(this.dictionary != null);

            try
            {
                if (this.dictionary.TryGetValue(key, out value))
                {
                    return true;
                }
            }
            catch (ArgumentException)
            {
            }
            value = default(VALUE);
            return false;
        }

        //------------------------------------------------------------
        // BCDictionaryEx.Indexer
        //
        /// <summary>インデクサ。</summary>
        //------------------------------------------------------------
        internal VALUE this[KEY key]
        {
            get
            {
                DebugUtil.Assert(this.dictionary != null);

                VALUE value;
                try
                {
                    if (this.dictionary.TryGetValue(key, out value))
                    {
                        return value;
                    }
                }
                catch (ArgumentException)
                {
                }
                return default(VALUE);
            }
            set
            {
                DebugUtil.Assert(this.dictionary != null);

                try
                {
                    if (this.dictionary.ContainsKey(key))
                    {
                        this.dictionary[key] = value;
                    }
                    else
                    {
                        this.dictionary.Add(key, value);
                    }
                }
                catch (ArgumentException)
                {
                }
            }
        }

        //------------------------------------------------------------
        // BCDictionaryEx.Initialize
        //
        /// <summary>初期化。特にすることはない。</summary>
        //------------------------------------------------------------
        virtual internal void Initialize() { }
    }

    //======================================================================
    // CTextReader
    //
    /// <summary>
    /// <para>This class holds a read-only string, a index of a position to read next,
    /// and methods to get characters of it.</para>
    /// <para>Defined in Utilities\Classes.cs</para>
    /// </summary>
    //======================================================================
    internal class CTextReader
    {
        //------------------------------------------------------------
        // CTextReader   Fields and Properties
        //------------------------------------------------------------
        private string text = null;

        /// <summary>
        /// Current index to read.
        /// </summary>
        internal int Index = 0;

        internal string Text
        {
            get { return text; }
        }

        /// <summary>
        /// <para>Return a character at Index.</para>
        /// <para>If text or Index is invalid, return null character.</para>
        /// </summary>
        internal char Char
        {
            get
            {
                DebugUtil.Assert(text != null);
#if false
                if (this.Index >= 0 && this.Index < this.text.Length)
                {
                    return text[this.Index];
                }
                return '\0';
#else
                try
                {
                   return text[this.Index];
                }
                catch (IndexOutOfRangeException)
                {
                }
                return '\0';
#endif
            }
        }

        /// <summary>
        /// <para>Return the length of text.</para>
        /// <para>If text is null, return 0.</para>
        /// </summary>
        internal int Length
        {
            get
            {
                DebugUtil.Assert(this.text != null);
                return text.Length;
            }
        }

        //------------------------------------------------------------
        // CTextReader.Indexer
        //
        /// <summary>
        /// <para>Return the character of the specified index.</para>
        /// <para>If the specified index is invalid, reutrn null character.</para>
        /// </summary>
        //------------------------------------------------------------
        internal char this[int idx]
        {
            get
            {
                DebugUtil.Assert(text != null);
#if true
                if (idx >= 0 && idx < text.Length)
                {
                    return text[idx];
                }
                return '\0';
#else
                try
                {
                    return text[idx];
                }
                catch (IndexOutOfRangeException)
                {
                }
                return '\0';
#endif
            }
        }

        //------------------------------------------------------------
        // CTextReader   Constructor (1)
        //
        /// <summary></summary>
        //------------------------------------------------------------
        //internal CTextReader() { }

        //------------------------------------------------------------
        // CTextReader   Constructor (2)
        //
        /// <summary></summary>
        /// <param name="str"></param>
        //------------------------------------------------------------
        internal CTextReader(string str)
        {
            text = str;
        }

        //------------------------------------------------------------
        // CTextReader   Constructor (3)
        //
        /// <summary></summary>
        /// <param name="str"></param>
        /// <param name="idx"></param>
        //------------------------------------------------------------
        internal CTextReader(string str, int idx)
        {
            text = str;
            this.Index = idx;
        }

        //------------------------------------------------------------
        // CTextReader   Constructor (4)
        //
        /// <summary></summary>
        /// <param name="src"></param>
        //------------------------------------------------------------
        internal CTextReader(CTextReader src)
        {
            Assign(src);
        }

        //------------------------------------------------------------
        // CTextReader.SetText
        //
        /// <summary></summary>
        /// <param name="tx"></param>
        //------------------------------------------------------------
        internal void SetText(string tx)
        {
            DebugUtil.Assert(tx != null);

            this.text = tx;
            this.Index = (this.text != null ? 0 : -1);
        }

        //------------------------------------------------------------
        // CTextReader.Clone
        //------------------------------------------------------------
        internal CTextReader Clone()
        {
            return new CTextReader(this);
        }

        //------------------------------------------------------------
        // NextChar (1)
        //
        /// <summary>
        /// Index is not changed.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal char NextChar()
        {
            int nidx = this.Index + 1;
            if (nidx < this.text.Length)
            {
                return this.text[nidx];
            }
            return '\0';
        }

        //------------------------------------------------------------
        // NextChar (2)
        //
        /// <summary>
        /// Index is not changed.
        /// </summary>
        /// <param name="dif"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal char NextChar(int dif)
        {
            int nidx = this.Index + dif;
            if (nidx >= 0 && nidx < this.text.Length)
            {
                return this.text[nidx];
            }
            return '\0';
        }

        //------------------------------------------------------------
        // PreviousChar (1)
        //
        /// <summary>
        /// Index is not changed.
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal char PreviousChar()
        {
            int pidx = this.Index - 1;
            if (pidx >= 0)
            {
                return this.text[pidx];
            }
            return '\0';
        }

        //------------------------------------------------------------
        // PreviousChar (2)
        //
        /// <summary>
        /// Index is not changed.
        /// </summary>
        /// <param name="dif"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal char PreviousChar(int dif)
        {
            int pidx = this.Index - dif;
            if (pidx >= 0 && pidx < this.text.Length)
            {
                return this.text[pidx];
            }
            return '\0';
        }

        //------------------------------------------------------------
        // CTextReader.ValidIndex
        //------------------------------------------------------------
        internal bool ValidIndex()
        {
            DebugUtil.Assert(this.text != null);
            return (0 <= this.Index && this.Index < this.text.Length);
        }

        //------------------------------------------------------------
        // CTextReader.Init (1)
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void Init()
        {
            DebugUtil.Assert(this.text != null);
            this.Index = 0;
        }

        //------------------------------------------------------------
        // CTextReader.Init (2)
        //
        /// <summary></summary>
        /// <param name="tx"></param>
        //------------------------------------------------------------
        internal void Init(string tx)
        {
            DebugUtil.Assert(tx != null);
            text = tx;
            this.Index = 0;
        }

        //------------------------------------------------------------
        // CTextReader.Begin
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void Begin()
        {
            DebugUtil.Assert(this.text != null);
            this.Index = 0;
        }

        //------------------------------------------------------------
        // CTextReader.End
        //
        /// <summary></summary>
        /// <returns></returns>
        //------------------------------------------------------------
        internal bool End()
        {
            DebugUtil.Assert(this.text != null);
            return this.Index >= text.Length;
        }

        //------------------------------------------------------------
        // CTextReader.Next
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void Next()
        {
            DebugUtil.Assert(this.text != null);

            if (this.Index < 0)
            {
                this.Index = 0;
            }

            if (this.Index < this.text.Length)
            {
                this.Index++;
            }
            else
            {
                this.Index = this.text.Length;
            }
        }

        //------------------------------------------------------------
        // CTextReader.Back
        //
        /// <summary></summary>
        //------------------------------------------------------------
        internal void Back()
        {
            DebugUtil.Assert(this.text != null);

            if (this.Index > 0)
            {
                if (this.Index > this.text.Length)
                {
                    this.Index = this.text.Length;
                }
                this.Index--;
            }
            else
            {
                this.Index = 0;
            }
        }

        //------------------------------------------------------------
        // CTextReader.Assign
        //
        /// <summary></summary>
        /// <param name="src"></param>
        //------------------------------------------------------------
        internal void Assign(CTextReader src)
        {
            DebugUtil.Assert(src != null);

            this.text = src.text;
            this.Index = src.Index;
        }

        //------------------------------------------------------------
        // CTextReader.Substring
        //
        /// <summary>
        /// Return the substring of this.text [this.Index, end.Index)
        /// </summary>
        /// <param name="end"></param>
        /// <returns></returns>
        //------------------------------------------------------------
        internal string Substring(CTextReader end)
        {
            DebugUtil.Assert(this.text != null && end != null && end.text != null);

            if (this.text != end.text)
            {
                return null;
            }
            if (!this.ValidIndex() || !end.ValidIndex())
            {
                return null;
            }

            if (this.Index >= end.Index)
            {
                return null;
            }
            return this.text.Substring(this.Index, end.Index - this.Index);
        }
    }

    //======================================================================
    // class StringEqualityComparerIgnoreCase
    //
    /// <summary>
    /// <para>
    /// Dictonary&lt;&gt; の一致判定用クラス。IEqualityComparer&lt;&gt; を実装する。
    /// 文字列を大文字小文字を区別せずに比較する。(PCSClasses.cs)
    /// </para>
    /// <para>
    /// このクラスのメソッドは引数をチェックしない。
    /// null を引数にしないよう呼び出し前にチェックすること。
    /// (uncs\Utilities\Classes.cs)
    /// </para>
    /// </summary>
    //======================================================================
    internal class StringEqualityComparerIgnoreCase : IEqualityComparer<string>
    {
        /// <summary>
        /// このクラスは IEqualityComparer&lt;&gt; を実装する。
        /// 文字列を大文字小文字を区別せずに比較するためのクラスである。(PCSClasses.cs)
        /// </summary>
        internal StringEqualityComparerIgnoreCase() { }

        /// <summary>
        /// 文字列を比較する。大文字小文字を区別しない。
        /// このメソッドは引数をチェックしない。null を引数にしないよう呼び出し前にチェックすること。
        /// </summary>
        /// <param name="s1">比較対象の文字列 1</param>
        /// <param name="s2">比較対象の文字列 2</param>
        /// <returns>大文字小文字の区別しないときに一致するなら true を返す。</returns>
        public bool Equals(string s1, string s2)
        {
            return (String.Compare(s1, s2, true) == 0);
        }

        /// <summary>
        /// 文字列を小文字に変換してからハッシュ値を計算する。
        /// このメソッドは引数をチェックしない。null を引数にしないよう呼び出し前にチェックすること。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public int GetHashCode(string str)
        {
            return (str.ToLower()).GetHashCode();
        }
    }

    //======================================================================
    // class MultiDimensionalCounter
    //======================================================================
    internal class MultiDimensionalCounter
    {
        //------------------------------------------------------------
        // MultiDimensionalCounter Fields
        //------------------------------------------------------------
        private List<int> ceilings = null;
        private List<int> counts = null;

        private bool hasError = false;

        public int Size
        {
            get { return counts.Count; }
        }

        //------------------------------------------------------------
        // MultiDimensionalCounter Properties
        //------------------------------------------------------------
        public int this[int i]
        {
            get
            {
                if (0 <= i && i < counts.Count)
                {
                    return counts[i];
                }
                return -1;
            }
        }

        public bool HasError
        {
            get { return hasError; }
        }

        //------------------------------------------------------------
        // MultiDimensionalCounter Constructor
        //------------------------------------------------------------
        public MultiDimensionalCounter(List<int> newCeilings)
        {
            this.ceilings = new List<int>(newCeilings);

            this.counts = new List<int>();
            for (int i = 0; i < this.ceilings.Count; ++i)
            {
                this.counts.Add(0);
            }
        }

        //------------------------------------------------------------
        // MultiDimensionalCounter.Inc
        //------------------------------------------------------------
        public bool Inc()
        {
            hasError = true;
            for (int i = counts.Count - 1; i >= 0; --i)
            {
                ++counts[i];
                if (counts[i] < ceilings[i])
                {
                    hasError = false;
                    break;
                }
                counts[i] = 0;
            }
            return hasError;
        }

        //------------------------------------------------------------
        // MultiDimensionalCounter.Reset
        //------------------------------------------------------------
        public void Reset()
        {
            for (int i = 0; i < counts.Count; ++i)
            {
                counts[i] = 0;
            }
        }
    }
}
