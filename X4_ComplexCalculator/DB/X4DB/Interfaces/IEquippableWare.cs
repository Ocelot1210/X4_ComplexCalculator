using System.Collections.Generic;

namespace X4_ComplexCalculator.DB.X4DB.Interfaces
{

    public interface IEquippableWare : IWare
    {
        #region プロパティ
        /// <summary>
        /// 装備一覧
        /// </summary>
        public IReadOnlyDictionary<string, WareEquipment> Equipments { get; }
        #endregion


        #region メソッド
        /// <summary>
        /// 指定したコネクション名に装備可能な装備情報を取得する
        /// </summary>
        /// <returns>指定したコネクション名に装備可能な装備情報</returns>
        public IEnumerable<T> GetEquippableEquipment<T>(string connectionName) where T : IEquipment;
        #endregion
    }
}
