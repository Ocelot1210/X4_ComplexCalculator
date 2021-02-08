using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Builder
{
    /// <summary>
    /// Engineクラスのインスタンスを作成するBuilderクラス
    /// </summary>
    class EngineBuilder
    {
        #region メンバ
        /// <summary>
        /// エンジン情報一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, X4_DataExporterWPF.Entity.Engine> _Engines;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        public EngineBuilder(IDbConnection conn)
        {
            _Engines = conn.Query<X4_DataExporterWPF.Entity.Engine>("SELECT * FROM Engine")
                .ToDictionary(x => x.EquipmentID);
        }


        /// <summary>
        /// エンジン情報作成
        /// </summary>
        /// <param name="equipment">ベースとなる装備情報</param>
        /// <returns>エンジン情報または装備情報</returns>
        public IEquipment Build(IEquipment equipment)
        {
            if (_Engines.TryGetValue(equipment.ID, out var item))
            {
                return new Engine(
                    equipment,
                    new EngineThrust(item.ForwardThrust, item.ReverseThrust, item.BoostThrust, item.TravelThrust),
                    item.BoostDuration,
                    item.BoostReleaseTime,
                    item.TravelReleaseTime
                );
            }

            return equipment;
        }
    }
}
