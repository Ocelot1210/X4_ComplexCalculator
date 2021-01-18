using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Entity;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment.EquipmentList
{
    /// <summary>
    /// 装備リストのModel
    /// </summary>
    class EquipmentListModel : IDisposable
    {
        #region メンバ
        /// <summary>
        /// 装備管理用インスタンス
        /// </summary>
        private readonly WareEquipmentManager _Manager;


        /// <summary>
        /// 種族一覧
        /// </summary>
        private readonly ObservablePropertyChangedCollection<FactionsListItem> _Factions;


        /// <summary>
        /// 装備ID
        /// </summary>
        private readonly string _EquipmentTypeID;
        #endregion


        #region プロパティ
        /// <summary>
        /// 装備可能な装備一覧
        /// </summary>
        public ObservablePropertyChangedCollection<EquipmentListItem> Equippable { get; } = new();


        /// <summary>
        /// 装備済みの装備一覧
        /// </summary>
        public ObservablePropertyChangedCollection<EquipmentListItem> Equipped { get; } = new();
        #endregion



        public EquipmentListModel(
            WareEquipmentManager manager,
            string equipmentTypeID,
            ObservablePropertyChangedCollection<FactionsListItem> factions
        )
        {
            _Manager = manager;
            _Factions = factions;
            _EquipmentTypeID = equipmentTypeID;
        }


        public void Dispose()
        {

        }


        public void SaveEquipment()
        {

        }


        public bool AddSelectedEquipments()
        {
            return false;
        }


        public bool RemoveSelectedEquipments()
        {
            return false;
        }



        /// <summary>
        /// プリセット一覧変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnPresetsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {

        }


        /// <summary>
        /// プリセット保存
        /// </summary>
        public void SavePreset()
        {

        }
    }
}
