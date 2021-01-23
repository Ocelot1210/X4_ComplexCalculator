using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Entity
{
    /// <summary>
    /// ウェアの装備品管理クラス
    /// </summary>
    public class WareEquipmentManager : INotifyCollectionChanged
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


        #region イベント
        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;
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
        /// コンストラクタ
        /// </summary>
        /// <param name="ware">管理対象のウェア</param>
        /// <param name="element">シリアライズされたXElement</param>
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
        /// <returns>追加された個数</returns>
        public void AddEquipment(Equipment equipment, long count = 1)
        {
            var added = CollectionChanged is null ? null : new List<Equipment>((int)count);
            var ret = 0L;
            while (ret < count && AddEquipmentInternal(equipment))
            {
                ret++;
                added?.Add(equipment);
            }

            if (CollectionChanged is not null && 0 < added!.Count)
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added));
            }
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
