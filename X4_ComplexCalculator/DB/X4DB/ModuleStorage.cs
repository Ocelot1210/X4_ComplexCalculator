using System.Collections.Generic;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// モジュールの保管庫情報
    /// </summary>
    public class ModuleStorage
    {
        #region プロパティ
        /// <summary>
        /// モジュールID
        /// </summary>
        public string ID { get; }


        /// <summary>
        /// 容量
        /// </summary>
        public long Amount { get; }


        /// <summary>
        /// 保管庫種別一覧
        /// </summary>
        public HashSet<TransportType> Types { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">モジュールID</param>
        /// <param name="amount">容量</param>
        /// <param name="types">保管庫種別一覧</param>
        public ModuleStorage(string id, long amount, HashSet<TransportType> types)
        {
            ID = id;
            Amount = amount;
            Types = types;
        }
    }
}
