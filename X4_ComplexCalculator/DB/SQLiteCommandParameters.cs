using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace X4_ComplexCalculator.DB
{
    /// <summary>
    /// バインド変数用クラス
    /// </summary>
    class SQLiteCommandParameters
    {
        #region メンバ変数
        /// <summary>
        /// パラメータ一覧
        /// </summary>
        private readonly List<SQLiteParameter> _Parameters;

        /// <summary>
        /// 
        /// </summary>
        private readonly int ValueCnt;
        #endregion

        #region プロパティ
        /// <summary>
        /// パラメータ一覧
        /// </summary>
        public IEnumerable<IEnumerable<SQLiteParameter>> Parameters
        {
            get
            {
                return _Parameters
                                .Select((v, i) => new { v, i })
                                .GroupBy(x => x.i / ValueCnt)
                                .Select(g => g.Select(x => x.v));
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="valueCnt">1レコードの数</param>
        public SQLiteCommandParameters(int valueCnt)
        {
            _Parameters = new List<SQLiteParameter>();
            ValueCnt = valueCnt;
        }


        /// <summary>
        /// レコード追加
        /// </summary>
        /// <param name="name">バインド変数名</param>
        /// <param name="dbType">型</param>
        /// <param name="value">値</param>
        public void Add(string name, DbType dbType, object value)
        {
            var param = new SQLiteParameter(name, dbType);
            param.Value = value;
            _Parameters.Add(param);
        }

        /// <summary>
        /// レコードを追加(複数バージョン)
        /// </summary>
        /// <param name="name">バインド変数名</param>
        /// <param name="dbType">型</param>
        /// <param name="values">値のコレクション</param>
        public void AddRange(string name, DbType dbType, IEnumerable<object> values)
        {
            foreach(var value in values)
            {
                Add(name, dbType, value);
            }
        }
    }
}
