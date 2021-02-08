using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.DB.X4DB.Manager;

namespace X4_ComplexCalculator.DB.X4DB.Builder
{
    /// <summary>
    /// <see cref="Equipment"/> クラスのインスタンスを作成するBuilderクラス
    /// </summary>
    class EquipmentBuilder
    {
        #region メンバ
        /// <summary>
        /// タグ情報一覧
        /// </summary>
        private readonly EquipmentTagsManager _EquipmentTagsManager;


        /// <summary>
        /// <see cref="IEngine"/> 情報ビルダ
        /// </summary>
        private readonly EngineBuilder _EngineBuilder;


        /// <summary>
        /// <see cref="IShield"/> 情報ビルダ
        /// </summary>
        private readonly ShieldBuilder _ShieldBuilder;


        /// <summary>
        /// <see cref="IThruster"/> 情報ビルダ
        /// </summary>
        private readonly ThrusterBuilder _ThrusterBuilder;


        /// <summary>
        /// 装備一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, X4_DataExporterWPF.Entity.Equipment> _Equipments;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="conn"></param>
        public EquipmentBuilder(IDbConnection conn)
        {
            _EquipmentTagsManager = new(conn);

            _EngineBuilder = new(conn);

            _ShieldBuilder = new(conn);

            _ThrusterBuilder = new(conn);

            _Equipments = conn.Query<X4_DataExporterWPF.Entity.Equipment>("SELECT * FROM Equipment")
                .ToDictionary(x => x.EquipmentID);
        }


        /// <summary>
        /// 装備情報作成
        /// </summary>
        /// <param name="ware">ベースとなるウェア情報</param>
        /// <returns>装備情報</returns>
        public IWare Build(IWare ware)
        {
            if (!ware.Tags.Contains("equipment"))
            {
                throw new ArgumentException();
            }

            if (_Equipments.TryGetValue(ware.ID, out var item))
            {
                var tags = _EquipmentTagsManager.Get(ware.ID);
                var ret = new Equipment(
                    ware,
                    item.MacroName,
                    X4Database.Instance.EquipmentType.Get(item.EquipmentTypeID),
                    item.Hull,
                    item.HullIntegrated,
                    item.Mk,
                    X4Database.Instance.Race.TryGet(item.MakerRace ?? ""),
                    tags,
                    tags.Select(x => X4Database.Instance.X4Size.TryGet(x)).FirstOrDefault(x => x is not null)
                );


                return ret.EquipmentType.EquipmentTypeID switch
                {
                    "engines" => _EngineBuilder.Build(ret),
                    "shields" => _ShieldBuilder.Build(ret),
                    "thrusters" => _ThrusterBuilder.Build(ret),
                    _ => ret,
                };
            }
            else
            {
                return ware;
            }
        }
    }
}
