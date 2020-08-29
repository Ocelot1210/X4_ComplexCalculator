using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using Dapper;

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
        public IEnumerable<IEnumerable<SQLiteParameter>> Parameters =>
            _Parameters.Select((v, i) => (v, i)).GroupBy(x => x.i / ValueCnt).Select(g => g.Select(x => x.v));
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
            var param = new SQLiteParameter(name, dbType) { Value = value };
            _Parameters.Add(param);
        }


        /// <summary>
        /// Dapper のパラメータに指定できる、DynamicParameters クラスに変換する
        /// </summary>
        /// <returns>DynamicParameters に変換されたパラメータ</returns>
        public DynamicParameters AsDynamicParameters()
        {
            var param = new DynamicParameters();
            foreach (var sqlParams in Parameters)
            {
                foreach (var sqlParam in sqlParams)
                {
                    param.Add(sqlParam.ParameterName, sqlParam.Value, sqlParam.DbType);
                }
            }
            return param;
        }
    }
}
