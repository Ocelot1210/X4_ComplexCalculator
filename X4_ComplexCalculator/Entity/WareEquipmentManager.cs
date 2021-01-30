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
        /// グループ名と接続名をキーにした装備中の項目一覧
        /// </summary>
        private readonly Dictionary<WareEquipment, Equipment> _Equipped;
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
            _Equipped = new();
        }


        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="manager">コピー元インスタンス</param>
        public WareEquipmentManager(WareEquipmentManager manager)
        {
            Ware = manager.Ware;
            _WareEquipments = WareEquipment.Get(Ware.ID);
            _Equipped = manager._Equipped.ToDictionary(x => x.Key, x => x.Value);
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
                    var conn = elm.Attribute("connection")?.Value;
                    var group = elm.Attribute("group")?.Value;
                    var id = elm.Attribute("id")?.Value;
                    if (string.IsNullOrEmpty(conn) || string.IsNullOrEmpty(group) || string.IsNullOrEmpty(id))
                    {
                        continue;
                    }

                    var wareEquipment = _WareEquipments
                        .FirstOrDefault(x => x.GroupName == group && x.ConnectionName == conn);

                    if (wareEquipment is not null)
                    {
                        _Equipped.Add(wareEquipment, Ware.Get<Equipment>(id));
                    }
                }
            }
            catch
            {
                throw new ArgumentException("Invalid xml data.", nameof(element));
            }
        }


        /// <summary>
        /// 装備をリセットする
        /// </summary>
        /// <param name="equipments"></param>
        public void ResetEquipment(IEnumerable<Equipment> equipments)
        {
            _Equipped.Clear();

            foreach (var equipment in equipments)
            {
                AddEquipmentInternal(equipment);
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }


        /// <summary>
        /// 装備を削除する
        /// </summary>
        /// <param name="equipments"></param>
        public void RemoveRange(IEnumerable<Equipment> equipments)
        {
            var removed = CollectionChanged is null ? null : new List<Equipment>();

            foreach (var equipment in equipments)
            {
                var key = _Equipped.Where(x => x.Value == equipment).Select(x => x.Key).FirstOrDefault();
                if (key is not null)
                {
                    _Equipped.Remove(key);
                    removed?.Add(equipment);
                }
            }

            if (CollectionChanged is not null && 0 < removed!.Count)
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
            }
        }



        /// <summary>
        /// 装備を追加する
        /// </summary>
        /// <param name="equipment">追加対象の列挙</param>
        public void AddRange(IEnumerable<Equipment> equipments)
        {
            var added = CollectionChanged is null ? null : new List<Equipment>();

            foreach (var equipment in equipments)
            {
                if (AddEquipmentInternal(equipment))
                {
                    added?.Add(equipment);
                }
            }

            if (CollectionChanged is not null && 0 < added!.Count)
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added));
            }
        }


        /// <summary>
        /// 装備を追加する
        /// </summary>
        /// <param name="equipment">追加対象</param>
        /// <param name="count">追加個数</param>
        public void Add(Equipment equipment, long count = 1)
        {
            var added = CollectionChanged is null ? null : new List<Equipment>((int)count);
            var cnt = 0L;
            while (cnt < count && AddEquipmentInternal(equipment))
            {
                cnt++;
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
                .FirstOrDefault(x => !_Equipped.ContainsKey(x) && x.CanEquipped(equipment));

            // 装備可能な接続がある場合、装備する
            if (equippableInfo is not null)
            {
                _Equipped.Add(equippableInfo, equipment);
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
            var equipments = _Equipped
                .Select(x =>new XElement(
                    "equipment",
                    new XAttribute("group", x.Key.GroupName),
                    new XAttribute("connection", x.Key.ConnectionName),
                    new XAttribute("id", x.Value.ID)
                    )
                );

            return new XElement("equipments", equipments);
        }


        /// <summary>
        /// 現在装備可能な個数を取得する
        /// </summary>
        /// <param name="type">装備ID</param>
        /// <param name="size">装備サイズ</param>
        /// <returns>装備IDと装備サイズに対応する装備があと何個装備できるか</returns>
        public int GetEquippableCount(EquipmentType type, X4Size size)
            => _WareEquipments.Count(x =>
            !_Equipped.ContainsKey(x) &&
            x.Tags.Contains(type.EquipmentTypeID[..^1]) &&
            x.Tags.Contains(size.SizeID)
        );


        /// <summary>
        /// 装備可能な最大個数を取得する
        /// </summary>
        /// <param name="type">装備ID</param>
        /// <param name="size">装備サイズ</param>
        /// <returns>装備IDと装備サイズに対応する装備が何個装備できるか</returns>
        public int GetMaxEquippableCount(EquipmentType type, X4Size size)
            => _WareEquipments.Count(x => x.Tags.Contains(type.EquipmentTypeID[..^1]) && x.Tags.Contains(size.SizeID));


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
