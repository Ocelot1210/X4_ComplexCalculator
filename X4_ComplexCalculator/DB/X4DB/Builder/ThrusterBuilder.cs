using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Builder
{
    /// <summary>
    /// Thrusterクラスのインスタンスを作成するBuilderクラス
    /// </summary>
    class ThrusterBuilder
    {
        #region メンバ
        /// <summary>
        /// スラスター情報一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, X4_DataExporterWPF.Entity.Thruster> _Thrusters;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        public ThrusterBuilder(IDbConnection conn)
        {
            _Thrusters = conn.Query<X4_DataExporterWPF.Entity.Thruster>("SELECT * FROM Thruster")
                .ToDictionary(x => x.EquipmentID);
        }


        /// <summary>
        /// スラスター情報作成
        /// </summary>
        /// <param name="equipment">ベースとなる装備情報</param>
        /// <returns>スラスター情報または装備情報</returns>
        public IEquipment Build(IEquipment equipment)
        {
            if (_Thrusters.TryGetValue(equipment.ID, out var item))
            {
                return new Thruster(
                    equipment,
                    item.ThrustStrafe,
                    item.ThrustPitch,
                    item.ThrustYaw,
                    item.ThrustRoll,
                    item.AngularRoll,
                    item.AngularPitch
                );
            }

            return equipment;
        }
    }
}
