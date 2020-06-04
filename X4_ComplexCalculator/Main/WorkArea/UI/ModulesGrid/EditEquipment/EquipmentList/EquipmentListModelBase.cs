using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment.EquipmentList
{
    abstract class EquipmentListModelBase
    {
        #region メンバ
        /// <summary>
        /// 種族一覧
        /// </summary>
        private readonly ObservablePropertyChangedCollection<FactionsListItem> Factions;


        /// <summary>
        /// 選択中のサイズ
        /// </summary>
        protected DB.X4DB.Size? _SelectedSize;


        /// <summary>
        /// 装備編集対象モジュール
        /// </summary>
        protected ModulesGridItem Module;


        /// <summary>
        /// 装備一覧
        /// </summary>
        protected Dictionary<DB.X4DB.Size, ObservableRangeCollection<Equipment>> _Equipments = new Dictionary<DB.X4DB.Size, ObservableRangeCollection<Equipment>>();


        /// <summary>
        /// 装備中の装備
        /// </summary>
        protected Dictionary<DB.X4DB.Size, ObservableRangeCollection<Equipment>> _Equipped = new Dictionary<DB.X4DB.Size, ObservableRangeCollection<Equipment>>();


        /// <summary>
        /// 装備可能な個数
        /// </summary>
        protected Dictionary<DB.X4DB.Size, int> _MaxAmount = new Dictionary<DB.X4DB.Size, int>();
        #endregion


        #region プロパティ
        /// <summary>
        /// 装備一覧
        /// </summary>
        public IReadOnlyDictionary<DB.X4DB.Size, ObservableRangeCollection<Equipment>> Equipments => _Equipments;


        /// <summary>
        /// 装備中
        /// </summary>
        public IReadOnlyDictionary<DB.X4DB.Size, ObservableRangeCollection<Equipment>> Equipped => _Equipped;


        /// <summary>
        /// 装備可能な個数
        /// </summary>
        public IReadOnlyDictionary<DB.X4DB.Size, int> MaxAmount => _MaxAmount;


        /// <summary>
        /// 現在のサイズ
        /// </summary>
        public DB.X4DB.Size? SelectedSize
        {
            get { return _SelectedSize; }
            set
            {
                _SelectedSize = value;
                UpdateEquipmentsMain();
            }
        }


        /// <summary>
        /// 選択中の種族
        /// </summary>
        protected IReadOnlyList<FactionsListItem> SelectedFactions => Factions.Where(x => x.IsChecked).ToArray();


        /// <summary>
        /// 選択中のプリセット
        /// </summary>
        public abstract PresetComboboxItem? SelectedPreset { protected get; set; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="module">編集対象モジュール</param>
        /// <param name="factions">種族一覧</param>
        /// <param name="presetsCollectionChanged">プリセット変更時のイベントハンドラー</param>
        public EquipmentListModelBase(ModulesGridItem module, ObservablePropertyChangedCollection<FactionsListItem> factions)
        {
            Module = module;
            Factions = factions;
            Factions.CollectionPropertyChanged += UpdateEquipments;
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public virtual void Dispose()
        {
            Factions.CollectionPropertyChanged -= UpdateEquipments;
        }


        /// <summary>
        /// 装備一覧を更新
        /// </summary>
        private void UpdateEquipments(object sender, PropertyChangedEventArgs e)
        {
            UpdateEquipmentsMain();
        }


        /// <summary>
        /// 装備一覧更新メイン
        /// </summary>
        protected abstract void UpdateEquipmentsMain();



        /// <summary>
        /// プリセット変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public abstract void OnPresetsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e);


        /// <summary>
        /// 装備を追加
        /// </summary>
        /// <param name="targets">追加対象</param>
        public void AddEquipments(IEnumerable<Equipment> targets)
        {
            if (SelectedSize == null)
            {
                throw new InvalidOperationException();
            }

            int addRange = MaxAmount[SelectedSize] - Equipped[SelectedSize].Count;  // 追加可能な個数

            // 追加対象が無ければ何もしない
            if (!targets.Any())
            {
                return;
            }

            // 左Shiftキー押下時なら選択アイテムを全追加
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                while (0 < addRange)
                {
                    // 追加可能な分だけ追加する
                    Equipped[SelectedSize].AddRange(targets.Take(addRange));

                    // 再計算
                    addRange = MaxAmount[SelectedSize] - Equipped[SelectedSize].Count;
                }
            }
            else
            {
                if (0 < addRange)
                {
                    // 追加可能な分だけ追加する
                    Equipped[SelectedSize].AddRange(targets.Take(addRange));
                }
            }
        }


        /// <summary>
        /// 装備を削除
        /// </summary>
        /// <param name="targets">削除対象</param>
        public void RemoveEquipments(IEnumerable<Equipment> targets)
        {
            if (SelectedSize == null)
            {
                throw new InvalidOperationException();
            }

            // 削除対象が無ければ何もしない
            if (!targets.Any())
            {
                return;
            }

            if (0 < Equipped[SelectedSize].Count)
            {
                var tgt = targets.Cast<Equipment>().ToArray();
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


        /// <summary>
        /// プリセット保存
        /// </summary>
        public virtual void SavePreset()
        {
            // 選択中のプリセットがあるか？
            if (SelectedPreset != null)
            {
                var id = Equipped.Values.SelectMany((x) => x).FirstOrDefault()?.EquipmentType.EquipmentTypeID;

                if (!string.IsNullOrEmpty(id))
                {
                    // 前回値削除
                    DBConnection.CommonDB.ExecQuery($"DELETE FROM ModulePresetsEquipment WHERE ModuleID = '{Module.Module.ModuleID}' AND PresetID = {SelectedPreset.ID} AND EquipmentType = '{id}'");

                    // 装備中のプリセットを追加
                    foreach (var equipment in Equipped.Values.SelectMany((x) => x))
                    {
                        var query = $"INSERT INTO ModulePresetsEquipment(ModuleID, PresetID, EquipmentID, EquipmentType) VALUES('{Module.Module.ModuleID}', {SelectedPreset.ID}, '{equipment.EquipmentID}', '{equipment.EquipmentType.EquipmentTypeID}')";
                        DBConnection.CommonDB.ExecQuery(query);
                    }
                }
            }
        }
    }
}
