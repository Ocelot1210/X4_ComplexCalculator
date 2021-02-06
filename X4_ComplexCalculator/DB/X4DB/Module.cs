using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// モジュール情報管理用クラス
    /// </summary>
    public partial class Module : IX4Module
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ware">ウェア情報</param>
        /// <param name="macro">マクロ名</param>
        /// <param name="moduleType">モジュール種別</param>
        /// <param name="maxWorkers">従業員数</param>
        /// <param name="workersCapacity">最大収容人数</param>
        /// <param name="noBluePrint">設計図が無いか</param>
        /// <param name="products">モジュールの製品</param>
        /// <param name="storage">保管庫情報</param>
        /// <param name="equipments">装備一覧</param>
        public Module(
            IWare ware,
            string macro,
            ModuleType moduleType,
            long maxWorkers,
            long workersCapacity,
            bool noBluePrint,
            IReadOnlyList<ModuleProduct> products,
            ModuleStorage storage,
            IReadOnlyDictionary<string, WareEquipment> equipments
        )
        {
            ID = ware.ID;
            Name = ware.Name;
            WareGroup = ware.WareGroup;
            TransportType = ware.TransportType;
            Description = ware.Description;
            Volume = ware.Volume;
            MinPrice = ware.MinPrice;
            AvgPrice = ware.AvgPrice;
            MaxPrice = ware.MaxPrice;
            Owners = ware.Owners;
            Productions = ware.Productions;
            Resources = ware.Resources;
            Tags = ware.Tags;
            WareEffects = ware.WareEffects;

            Macro = macro;
            ModuleType = moduleType;
            MaxWorkers = maxWorkers;
            WorkersCapacity = workersCapacity;
            NoBluePrint = noBluePrint;
            Products = products;
            Storage = storage;
            Equipments = equipments;
        }


        /// <summary>
        /// オブジェクト比較
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object? obj)
        {
            if (obj is not IX4Module other)
            {
                return 1;
            }

            return ID.CompareTo(other.ID);
        }


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => ID.GetHashCode();



        /// <summary>
        /// 指定したコネクション名に装備可能なEquipmentを取得する
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> GetEquippableEquipment<T>(string connectionName) where T : IEquipment
        {
            if (Equipments.TryGetValue(connectionName, out var wareEquipment))
            {
                return X4Database.Instance.Ware.GetAll<T>()
                    .Where(x => !x.EquipmentTags.Except(wareEquipment.Tags).Any());
            }

            return Enumerable.Empty<T>();
        }
    }
}
