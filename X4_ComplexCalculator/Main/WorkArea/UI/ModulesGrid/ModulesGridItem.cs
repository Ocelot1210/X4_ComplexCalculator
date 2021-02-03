using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using Prism.Commands;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Entity;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment;
using System.Collections.Specialized;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid
{
    /// <summary>
    /// ModuleクラスをDataGrid表示用クラス
    /// </summary>
    public class ModulesGridItem : BindableBaseEx, IEditable, ISelectable, IReorderble
    {
        #region スタティックメンバ
        /// <summary>
        /// モジュール数最大値
        /// </summary>
        public const long MAX_MODULE_COUNT = 99999;
        #endregion


        #region メンバ
        /// <summary>
        /// モジュール数
        /// </summary>
        private long _ModuleCount = 1;


        /// <summary>
        /// タレット用ツールチップ
        /// </summary>
        private readonly EquipmentsInfo _TurretToolTip;


        /// <summary>
        /// シールド用ツールチップ
        /// </summary>
        private readonly EquipmentsInfo _ShieldToolTip;


        /// <summary>
        /// 選択された建造方式
        /// </summary>
        private WareProduction _SelectedMethod;


        /// <summary>
        /// 選択されているか
        /// </summary>
        private bool _IsSelected;


        /// <summary>
        /// 編集状態
        /// </summary>
        private EditStatus _EditStatus = EditStatus.Unedited;


        /// <summary>
        /// 順番入れ替え対象か
        /// </summary>
        private bool _IsReorderTarget;
        #endregion


        #region プロパティ
        /// <summary>
        /// モジュール
        /// </summary>
        public Module Module { get; }


        /// <summary>
        /// 装備情報
        /// </summary>
        public WareEquipmentManager Equipments { get; }


        /// <summary>
        /// モジュールの装備編集
        /// </summary>
        public ICommand EditEquipmentCommand { get; }


        /// <summary>
        /// 選択されているか
        /// </summary>
        public bool IsSelected
        {
            get => _IsSelected;
            set => SetProperty(ref _IsSelected, value);
        }


        /// <summary>
        /// モジュール数
        /// </summary>
        public long ModuleCount
        {
            get => _ModuleCount;
            set
            {
                var setValue = (value < 0) ? 0L :
                               (MAX_MODULE_COUNT < value) ? MAX_MODULE_COUNT : value;

                if (SetPropertyEx(ref _ModuleCount, setValue))
                {
                    EditStatus = EditStatus.Edited;
                }
            }
        }

        /// <summary>
        /// 装備中のタレットの個数
        /// </summary>
        public int TurretsCount => _TurretToolTip.Count;


        /// <summary>
        /// タレットのツールチップ文字列
        /// </summary>
        public string TurretsToolTip => _TurretToolTip.DetailsText;


        /// <summary>
        /// 装備中のシールドの個数
        /// </summary>
        public int ShieldsCount => _ShieldToolTip.Count;


        /// <summary>
        /// シールドのツールチップ文字列
        /// </summary>
        public string ShieldsToolTip => _ShieldToolTip.DetailsText;


        /// <summary>
        /// 編集ボタンを表示すべきか
        /// </summary>
        public Visibility EditEquipmentButtonVisiblity => (Equipments.CanEquipped) ? Visibility.Visible : Visibility.Hidden;


        /// <summary>
        /// 建造方式を表示すべきか
        /// </summary>
        public Visibility SelectedMethodVisiblity => (2 <= Module.Productions.Count) ? Visibility.Visible : Visibility.Hidden;


        /// <summary>
        /// 選択中の建造方式
        /// </summary>
        public WareProduction SelectedMethod
        {
            get => _SelectedMethod;
            set
            {
                if (SetPropertyEx(ref _SelectedMethod, value))
                {
                    EditStatus = EditStatus.Edited;
                }
            }
        }


        /// <summary>
        /// 編集状態
        /// </summary>
        public EditStatus EditStatus
        {
            get => _EditStatus;
            set => SetProperty(ref _EditStatus, value);
        }


        /// <summary>
        /// 順番入れ替え対象か
        /// </summary>
        public bool IsReorderTarget
        {
            get => _IsReorderTarget;
            set => SetProperty(ref _IsReorderTarget, value);
        }
        #endregion




        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="module">モジュール</param>
        /// <param name="selectedMethod">選択中の建造方式</param>
        /// <param name="moduleCount">モジュール数</param>
        public ModulesGridItem(Module module, WareProduction? selectedMethod = null, long moduleCount = 1)
        {
            Module = module;
            ModuleCount = moduleCount;
            EditEquipmentCommand = new DelegateCommand(EditEquipment);
            Equipments = new WareEquipmentManager(module);
            Equipments.CollectionChanged += Equipments_CollectionChanged;

            _TurretToolTip = new EquipmentsInfo(Equipments, "turrets");
            _ShieldToolTip = new EquipmentsInfo(Equipments, "shields");

            _SelectedMethod = selectedMethod ?? Module.Productions[0];
        }


        /// <summary>
        /// コンストラクタ(xmlより作成)
        /// </summary>
        /// <param name="element">モジュール情報が記載されたxml</param>
        public ModulesGridItem(XElement element)
        {
            Module = Ware.TryGet<Module>(element.Attribute("id").Value) ?? throw new ArgumentException("Invalid XElement.", nameof(element));
            Equipments = new WareEquipmentManager(Module, element.Element("equipments"));

            ModuleCount = long.Parse(element.Attribute("count").Value);
            SelectedMethod = Module.Productions.FirstOrDefault(x => x.Method == element.Attribute("method")?.Value)
                ?? Module.Productions.First();
            _SelectedMethod = SelectedMethod;

            _TurretToolTip = new EquipmentsInfo(Equipments, "turrets");
            _ShieldToolTip = new EquipmentsInfo(Equipments, "shields");
            UpdateEquipmentInfo();

            EditEquipmentCommand = new DelegateCommand(EditEquipment);
        }


        /// <summary>
        /// xml化する
        /// </summary>
        /// <returns>xml化した情報</returns>
        public XElement ToXml()
        {
            // それぞれのモジュールの情報を設定
            var ret = new XElement("module");
            ret.Add(new XAttribute("id", Module.ID));
            ret.Add(new XAttribute("count", ModuleCount));
            ret.Add(new XAttribute("method", SelectedMethod.Method));

            // 装備をXML化
            ret.Add(Equipments.Serialize());

            return ret;
        }


        /// <summary>
        /// 装備を追加
        /// </summary>
        /// <param name="equipment">追加したい装備</param>
        public void AddEquipment(Equipment equipment) => Equipments.Add(equipment);


        /// <summary>
        /// 装備情報を更新する
        /// </summary>
        public void UpdateEquipmentInfo()
        {
            if (Equipments.CanEquipped)
            {
                _TurretToolTip.RequireUpdate();
                _ShieldToolTip.RequireUpdate();
                RaisePropertyChanged(nameof(TurretsCount));
                RaisePropertyChanged(nameof(ShieldsCount));
                RaisePropertyChanged(nameof(TurretsToolTip));
                RaisePropertyChanged(nameof(ShieldsToolTip));
            }
        }


        /// <summary>
        /// 装備を編集
        /// </summary>
        private void EditEquipment()
        {
            // 変更前
            var turretsOld = Equipments.AllEquipments
                .Where(x => x.EquipmentType.EquipmentTypeID == "turrets")
                .Select(x => x.ID)
                .OrderBy(x => x).ToArray();

            var shieldsOld = Equipments.AllEquipments
                .Where(x => x.EquipmentType.EquipmentTypeID == "shields")
                .Select(x => x.ID)
                .OrderBy(x => x).ToArray();


            var window = new EditEquipmentWindow(Equipments);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();

            bool equipmentChanged = false;

            // 変更があった場合のみ通知
            if (!turretsOld.SequenceEqual(Equipments.AllEquipments.Where(x => x.EquipmentType.EquipmentTypeID == "turrets").Select(x => x.ID).OrderBy(x => x)))
            {
                RaisePropertyChanged(nameof(TurretsCount));
                RaisePropertyChanged(nameof(TurretsToolTip));
                equipmentChanged = true;
            }

            if (!shieldsOld.SequenceEqual(Equipments.AllEquipments.Where(x => x.EquipmentType.EquipmentTypeID == "shields").Select(x => x.ID).OrderBy(x => x)))
            {
                RaisePropertyChanged(nameof(ShieldsCount));
                RaisePropertyChanged(nameof(ShieldsToolTip));
                equipmentChanged = true;
            }

            if (equipmentChanged)
            {
                var newItems = Equipments.AllEquipments
                    .Where(x => x.EquipmentType.EquipmentTypeID == "shields")
                    .Select(x => x.ID)
                    .ToArray();
                RaisePropertyChangedEx(turretsOld.Concat(shieldsOld), newItems, nameof(Equipments));
                EditStatus = EditStatus.Edited;
            }
        }



        /// <summary>
        /// 装備に変更があった場合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Equipments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var addedTurretCount = 0;
            var addedShieldCount = 0;

            if (e.NewItems is not null)
            {
                var groups = e.NewItems
                    .Cast<Equipment>()
                    .Where(x => x.EquipmentType.EquipmentTypeID == "shields" || x.EquipmentType.EquipmentTypeID == "turrets")
                    .GroupBy(x => x.EquipmentType);
                
                foreach (var grp in groups)
                {
                    if (grp.Key.EquipmentTypeID == "shields")
                    {
                        addedTurretCount += grp.Count();
                        continue;
                    }

                    if (grp.Key.EquipmentTypeID == "turrets")
                    {
                        addedShieldCount += grp.Count();
                        continue;
                    }
                }
            }

            if (e.OldItems is not null)
            {
                var groups = e.OldItems
                    .Cast<Equipment>()
                    .Where(x => x.EquipmentType.EquipmentTypeID == "shields" || x.EquipmentType.EquipmentTypeID == "turrets")
                    .GroupBy(x => x.EquipmentType);

                foreach (var grp in groups)
                {
                    if (grp.Key.EquipmentTypeID == "shields")
                    {
                        addedTurretCount -= grp.Count();
                        continue;
                    }

                    if (grp.Key.EquipmentTypeID == "turrets")
                    {
                        addedShieldCount -= grp.Count();
                        continue;
                    }
                }
            }

            UpdateEquipmentInfo();
        }


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Module, ModuleCount, Equipments, SelectedMethod);
        }
    }
}
