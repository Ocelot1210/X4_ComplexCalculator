using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Entity
{
    /// <summary>
    /// ウェアの装備品管理クラス
    /// </summary>
    public class WareEquipmentManager
    {
        #region メンバ
        /// <summary>
        /// 装備可能な情報
        /// </summary>
        private readonly IReadOnlyList<WareEquipment> _WareEquipments;


        /// <summary>
        /// 接続名をキーにした装備中の項目一覧
        /// </summary>
        private readonly Dictionary<string, Equipment> _Equipped = new();
        #endregion


        #region プロパティ
        /// <summary>
        /// 編集対象のウェア
        /// </summary>
        public Ware Ware { get; }


        /// <summary>
        /// 装備中の装備一覧
        /// </summary>
        public IEnumerable<Equipment> AllEquipments => _Equipped.Values;


        /// <summary>
        /// 装備を持てるか
        /// </summary>
        public bool CanEquipped => _WareEquipments.Any();
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ware">ウェア</param>
        public WareEquipmentManager(Ware ware)
        {
            Ware = ware;
            _WareEquipments = WareEquipment.Get(ware.ID);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ware"></param>
        /// <param name="element"></param>
        public WareEquipmentManager(Ware ware, XElement element) : this(ware)
        {
            try
            {
                foreach (var elm in element.Elements())
                {
                    var connection = elm.Name.LocalName;
                    var id = elm.Attribute("id")?.Value;
                    if (string.IsNullOrEmpty(id)) continue;

                    _Equipped.Add(connection, Ware.Get<Equipment>(id));
                }
            }
            catch
            {
                throw new ArgumentException("Invalid xml data.", nameof(element));
            }
        }



        /// <summary>
        /// 装備を追加する
        /// </summary>
        /// <param name="equipment">追加対象</param>
        /// <param name="count">追加個数</param>
        public void AddEquipment(Equipment equipment, long count = 1)
        {
            var i = 0L;
            while (i++ < count && AddEquipmentInternal(equipment)) { }
        }


        /// <summary>
        /// 装備を追加する(内部用)
        /// </summary>
        /// <param name="equipment">追加対象</param>
        /// <returns>追加に成功したか</returns>
        private bool AddEquipmentInternal(Equipment equipment)
        {
            // 装備可能な接続情報を取得する
            var equippableInfo = _WareEquipments
                .FirstOrDefault(x => !_Equipped.ContainsKey(x.ConnectionName) && x.CanEquipped(equipment));

            // 装備可能な接続がある場合、装備する
            if (equippableInfo is not null)
            {
                _Equipped.Add(equippableInfo.ConnectionName, equipment);
                return true;
            }

            return false;
        }



        /// <summary>
        /// XML にシリアライズする
        /// </summary>
        /// <returns>インスタンスの現在の状態を表す XElement</returns>
        public XElement Serialize()
        {
            var equipments = _Equipped.Select(x => new XElement(x.Key, new XAttribute("id", x.Value.ID)));

            return new XElement("equipments", equipments);
        }


        /// <inheritdoc />
        public bool Equals(WareEquipmentManager? other)
            => other is not null && Ware.Equals(other.Ware) && other._Equipped.SequenceEqual(_Equipped);


        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is WareEquipmentManager other && Equals(other);


        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(Ware);
            foreach (var equipment in _Equipped)
            {
                hash.Add(equipment);
            }

            return hash.ToHashCode();
        }
    }
}
