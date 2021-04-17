using System.Collections.Generic;

namespace X4_ComplexCalculator.DB.X4DB.Interfaces
{
    /// <summary>
    /// モジュールの保管庫情報用インターフェイス
    /// </summary>
    public interface IModuleStorage
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
        public HashSet<ITransportType> Types { get; }
        #endregion
    }
}
