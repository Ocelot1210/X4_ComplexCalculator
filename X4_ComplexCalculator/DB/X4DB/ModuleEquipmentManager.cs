using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// モジュールの装備品管理用クラス
    /// </summary>
    public class ModuleEquipmentManager
    {
        #region メンバ
        /// <summary>
        /// 装備品
        /// </summary>
        private readonly Dictionary<X4Size, List<Equipment>> _Equipments = new Dictionary<X4Size, List<Equipment>>();
        #endregion


        #region プロパティ
        /// <summary>
        /// 装備可能な数
        /// </summary>
        public IReadOnlyDictionary<X4Size, int> MaxAmount
            => _Equipments.ToDictionary(p => p.Key, p => p.Value.Capacity);


        /// <summary>
        /// サイズ一覧
        /// </summary>
        public IEnumerable<X4Size> Sizes => _Equipments.Keys;


        /// <summary>
        /// 装備を持てるか
        /// </summary>
        public bool CanEquipped => 0 < _Equipments.Count;


        /// <summary>
        /// 全装備を取得
        /// </summary>
        public IEnumerable<Equipment> AllEquipments => _Equipments.Values.SelectMany(x => x);


        /// <summary>
        /// 全装備の個数
        /// </summary>
        public int AllEquipmentsCount => (CanEquipped) ? _Equipments.Sum(x => x.Value.Count) : 0;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="capacity">装備可能な装備の数</param>
        public ModuleEquipmentManager(IReadOnlyDictionary<X4Size, int> capacity)
            => _Equipments = capacity.ToDictionary(p => p.Key, p => new List<Equipment>(p.Value));


        /// <summary>
        /// 装備一覧を取得
        /// </summary>
        /// <param name="size">サイズ</param>
        /// <returns>装備一覧</returns>
        public IReadOnlyList<Equipment> GetEquipment(X4Size size) => _Equipments[size];


        /// <summary>
        /// 装備一覧をリセット
        /// </summary>
        /// <param name="size">サイズ</param>
        /// <param name="equipments">装備一覧</param>
        public void ResetEquipment(X4Size size, ICollection<Equipment> equipments)
        {
            if (_Equipments[size].Capacity < equipments.Count)
            {
                throw new IndexOutOfRangeException("これ以上装備できません。");
            }

            _Equipments[size].Clear();
            _Equipments[size].AddRange(equipments);
        }

        /// <summary>
        /// 装備を追加
        /// </summary>
        /// <param name="equipment">追加対象</param>
        public void AddEquipment(Equipment equipment)
        {
            if (_Equipments[equipment.Size].Count < _Equipments[equipment.Size].Capacity)
            {
                _Equipments[equipment.Size].Add(equipment);
            }
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj) => obj is ModuleEquipmentManager tgt && _Equipments.Equals(tgt._Equipments);


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode()
        {
            var sb = new StringBuilder();

            foreach (var equipmentID in _Equipments.SelectMany(x => x.Value.Select(y => y.EquipmentID).OrderBy(x => x)))
            {
                sb.Append(equipmentID);
            }

            var ret = sb.ToString().GetHashCode();

            return ret;
        }
    }
}
