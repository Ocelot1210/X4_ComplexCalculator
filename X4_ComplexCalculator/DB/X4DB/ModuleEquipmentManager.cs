using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
        private readonly Dictionary<Size, List<Equipment>> _Equipments = new Dictionary<Size, List<Equipment>>();


        /// <summary>
        /// 装備可能な数
        /// </summary>
        private readonly Dictionary<Size, int> _MaxAmount = new Dictionary<Size, int>();


        /// <summary>
        /// サイズ一覧
        /// </summary>
        private readonly List<Size> _Sizes = new List<Size>();
        #endregion


        #region プロパティ
        /// <summary>
        /// 装備可能な数
        /// </summary>
        public IReadOnlyDictionary<Size, int> MaxAmount => _MaxAmount;


        /// <summary>
        /// サイズ一覧
        /// </summary>
        public IReadOnlyList<Size> Sizes => _Sizes;


        /// <summary>
        /// 装備を持てるか
        /// </summary>
        public bool CanEquipped { private set; get; }


        /// <summary>
        /// 全装備を取得
        /// </summary>
        public List<Equipment> AllEquipments
        {
            get
            {
                var ret = new List<Equipment>();

                foreach(var itm in _Equipments.Values)
                {
                    ret.AddRange(itm);
                }

                return ret;
            }
        }


        /// <summary>
        /// 全装備の個数
        /// </summary>
        public int AllEquipmentsCount => (CanEquipped) ? _Equipments.Sum(x => x.Value.Count) : 0;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="equipmentType">装備種別("Turret"か"Shield"のどちらか")</param>
        public ModuleEquipmentManager(string moduleID, string equipmentType)
        {
            CanEquipped = false;

            if (string.IsNullOrEmpty(equipmentType))
            {
                throw new ArgumentException("Invalid equipment type.", nameof(equipmentType));
            }

            var query = $@"SELECT SizeID, Amount FROM Module{equipmentType} WHERE ModuleID = '{moduleID}'";

            DBConnection.X4DB.ExecQuery(query,
                (SQLiteDataReader dr, object[] args) =>
                {
                    var size = new Size((string)dr["SizeID"]);
                    _Sizes.Add(size);
                    var maxAmount = (int)(long)dr["Amount"];
                    _MaxAmount.Add(size, maxAmount);
                    _Equipments.Add(size, new List<Equipment>(maxAmount));
                    CanEquipped = true;
                });
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="manager"></param>
        public ModuleEquipmentManager(ModuleEquipmentManager manager)
        {
            CanEquipped = manager.CanEquipped;
            _Sizes      = manager._Sizes.ToList();
            _MaxAmount  = new Dictionary<Size, int>();
            foreach(var amount in manager._MaxAmount)
            {
                _MaxAmount.Add(amount.Key, amount.Value);
            }
            
            _Equipments = new Dictionary<Size, List<Equipment>>();
            foreach(var equipment in manager._Equipments)
            {
                _Equipments.Add(equipment.Key, equipment.Value.ToList());
            }
        }

        /// <summary>
        /// 装備一覧を取得
        /// </summary>
        /// <param name="size">サイズ</param>
        /// <returns>装備一覧</returns>
        public IReadOnlyList<Equipment> GetEquipment(Size size)
        {
            return _Equipments[size];
        }


        /// <summary>
        /// 装備一覧をリセット
        /// </summary>
        /// <param name="size">サイズ</param>
        /// <param name="equipments">装備一覧</param>
        public void ResetEquipment(Size size, ICollection<Equipment> equipments)
        {
            if(MaxAmount[size] < equipments.Count)
            {
                throw new System.IndexOutOfRangeException("これ以上装備できません。");
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
            if (_Equipments[equipment.Size].Count < MaxAmount[equipment.Size])
            {
                _Equipments[equipment.Size].Add(equipment);
            }
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (!(obj is ModuleEquipmentManager tgt)) return false;

            return _Equipments.Equals(tgt._Equipments);
        }


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
