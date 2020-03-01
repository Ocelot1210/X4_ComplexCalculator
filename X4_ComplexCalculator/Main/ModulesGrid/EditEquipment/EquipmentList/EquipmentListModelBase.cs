using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.ModulesGrid.EditEquipment.EquipmentList
{
    abstract class EquipmentListModelBase
    {
        #region メンバ
        /// <summary>
        /// 種族一覧
        /// </summary>
        private readonly MemberChangeDetectCollection<FactionsListItem> Factions;


        /// <summary>
        /// 選択中のサイズ
        /// </summary>
        protected Size _SelectedSize;


        /// <summary>
        /// 装備編集対象モジュール
        /// </summary>
        protected Module Module;


        /// <summary>
        /// 装備一覧
        /// </summary>
        protected Dictionary<Size, SmartCollection<Equipment>> _Equipments = new Dictionary<Size, SmartCollection<Equipment>>();


        /// <summary>
        /// 装備中の装備
        /// </summary>
        protected Dictionary<Size, SmartCollection<Equipment>> _Equipped = new Dictionary<Size, SmartCollection<Equipment>>();


        /// <summary>
        /// 装備可能な個数
        /// </summary>
        protected Dictionary<Size, int> _MaxAmount = new Dictionary<Size, int>();
        #endregion


        #region プロパティ
        /// <summary>
        /// 装備一覧
        /// </summary>
        public IReadOnlyDictionary<Size, SmartCollection<Equipment>> Equipments => _Equipments;


        /// <summary>
        /// 装備中
        /// </summary>
        public IReadOnlyDictionary<Size, SmartCollection<Equipment>> Equipped => _Equipped;


        /// <summary>
        /// 装備可能な個数
        /// </summary>
        public IReadOnlyDictionary<Size, int> MaxAmount => _MaxAmount;


        /// <summary>
        /// 現在のサイズ
        /// </summary>
        public Size SelectedSize
        {
            get { return _SelectedSize; }
            set
            {
                _SelectedSize = value;
                UpdateEquipments();
            }
        }


        /// <summary>
        /// 選択中の種族
        /// </summary>
        protected IReadOnlyList<FactionsListItem> SelectedFactions => Factions.Where(x => x.Checked).ToList();
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="module">編集対象モジュール</param>
        /// <param name="factions">種族一覧</param>
        public EquipmentListModelBase(Module module, MemberChangeDetectCollection<FactionsListItem> factions)
        {
            Module = module;
            Factions = factions;
            Factions.OnCollectionChangedMain += (object sender, NotifyCollectionChangedEventArgs e) => { if (SelectedSize != null) { UpdateEquipments(); } };
        }

        /// <summary>
        /// 装備一覧を更新
        /// </summary>
        protected abstract void UpdateEquipments();


        /// <summary>
        /// 装備を追加
        /// </summary>
        /// <param name="targets">追加対象</param>
        public void AddEquipments(IEnumerable<Equipment> targets)
        {
            int addRange = MaxAmount[SelectedSize] - Equipped[SelectedSize].Count;  // 追加可能な個数

            // 1つ以上空きスロットがあるか？(追加可能か？)
            if (0 < addRange)
            {
                // 追加可能な分だけ追加する
                Equipped[SelectedSize].AddRange(targets.Take(addRange));
            }
        }


        /// <summary>
        /// 装備を削除
        /// </summary>
        /// <param name="targets">削除対象</param>
        public void RemoveEquipments(IEnumerable<Equipment> targets)
        {
            if (0 < Equipped[SelectedSize].Count)
            {
                var tgt = targets.Cast<Equipment>().ToList();
                foreach (var equipment in tgt)
                {
                    Equipped[SelectedSize].Remove(equipment);
                }
            }
            else
            {
                throw new IndexOutOfRangeException("これ以上装備を削除できません");
            }
        }


        /// <summary>
        /// 装備を保存
        /// </summary>
        public abstract void SaveEquipment();
    }
}
