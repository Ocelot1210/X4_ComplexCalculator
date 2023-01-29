using Prism.Commands;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Entity;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment;
using System.Collections.Generic;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

/// <summary>
/// Module一覧DataGridの1レコード分の情報を管理するクラス
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
    private long _moduleCount = 1;


    /// <summary>
    /// 選択された建造方式
    /// </summary>
    private IWareProduction _selectedMethod;


    /// <summary>
    /// 選択されているか
    /// </summary>
    private bool _isSelected;


    /// <summary>
    /// 編集状態
    /// </summary>
    private EditStatus _editStatus = EditStatus.Unedited;


    /// <summary>
    /// 順番入れ替え対象か
    /// </summary>
    private bool _isReorderTarget;
    #endregion


    #region プロパティ
    /// <summary>
    /// モジュール
    /// </summary>
    public IX4Module Module { get; }


    /// <summary>
    /// モジュール名
    /// </summary>
    public string ModuleName => Module.Name;


    /// <summary>
    /// 装備情報
    /// </summary>
    public EquippableWareEquipmentManager Equipments { get; }


    /// <summary>
    /// モジュールの装備編集
    /// </summary>
    public ICommand EditEquipmentCommand { get; }


    /// <summary>
    /// 選択されているか
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }


    /// <summary>
    /// モジュール数
    /// </summary>
    public long ModuleCount
    {
        get => _moduleCount;
        set
        {
            var setValue = (value < 0) ? 0L :
                           (MAX_MODULE_COUNT < value) ? MAX_MODULE_COUNT : value;

            if (SetPropertyEx(ref _moduleCount, setValue))
            {
                EditStatus = EditStatus.Edited;
            }
        }
    }


    /// <summary>
    /// タレット情報
    /// </summary>
    public EquipmentsInfo Turrets { get; }


    /// <summary>
    /// シールド情報
    /// </summary>
    public EquipmentsInfo Shields { get; }


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
    public IWareProduction SelectedMethod
    {
        get => _selectedMethod;
        set
        {
            if (SetPropertyEx(ref _selectedMethod, value))
            {
                RaisePropertyChanged(nameof(SelectedMethodName));
                EditStatus = EditStatus.Edited;
            }
        }
    }


    /// <summary>
    /// 選択中の建造方式名称
    /// </summary>
    public string SelectedMethodName => SelectedMethod.Name;


    /// <summary>
    /// 建造方式一覧
    /// </summary>
    public IEnumerable<IWareProduction> Productions => Module.Productions.Values;
    


    /// <summary>
    /// 編集状態
    /// </summary>
    public EditStatus EditStatus
    {
        get => _editStatus;
        set => SetProperty(ref _editStatus, value);
    }


    /// <summary>
    /// 順番入れ替え対象か
    /// </summary>
    public bool IsReorderTarget
    {
        get => _isReorderTarget;
        set => SetProperty(ref _isReorderTarget, value);
    }
    #endregion




    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="module">モジュール</param>
    /// <param name="selectedMethod">選択中の建造方式</param>
    /// <param name="moduleCount">モジュール数</param>
    public ModulesGridItem(IX4Module module, IWareProduction? selectedMethod = null, long moduleCount = 1)
    {
        Module = module;
        ModuleCount = moduleCount;
        EditEquipmentCommand = new DelegateCommand(EditEquipment);
        Equipments = new EquippableWareEquipmentManager(module);
        Equipments.CollectionChanged += Equipments_CollectionChanged;
        
        Turrets = new EquipmentsInfo(Equipments, "turrets");
        Shields = new EquipmentsInfo(Equipments, "shields");

        _selectedMethod = selectedMethod ?? Module.Productions.First().Value;
    }


    /// <summary>
    /// コンストラクタ(xmlより作成)
    /// </summary>
    /// <param name="element">モジュール情報が記載されたxml</param>
    public ModulesGridItem(XElement element)
    {
        Module = X4Database.Instance.Ware.TryGet<IX4Module>(element.Attribute("id")!.Value) ?? throw new ArgumentException("Invalid XElement.", nameof(element));
        Equipments = new EquippableWareEquipmentManager(Module, element.Element("equipments"));

        ModuleCount = long.Parse(element.Attribute("count")?.Value ?? "1");

        SelectedMethod = 
            Module.Productions.TryGetValue(element.Attribute("method")?.Value ?? "default", out var method) ? method : Module.Productions.Values.First();

        _selectedMethod = SelectedMethod;

        Turrets = new EquipmentsInfo(Equipments, "turrets");
        Shields = new EquipmentsInfo(Equipments, "shields");
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
    public void AddEquipment(IEquipment equipment) => Equipments.Add(equipment);


    /// <summary>
    /// 装備情報を更新する
    /// </summary>
    public void UpdateEquipmentInfo()
    {
        if (Equipments.CanEquipped)
        {
            Turrets.RequireUpdate();
            Shields.RequireUpdate();
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


        var window = new EditEquipmentWindow(Equipments)
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();

        bool equipmentChanged = false;

        // 変更があった場合のみ通知
        if (!turretsOld.SequenceEqual(Equipments.AllEquipments.Where(x => x.EquipmentType.EquipmentTypeID == "turrets").Select(x => x.ID).OrderBy(x => x)))
        {
            Turrets.Update();
            equipmentChanged = true;
        }

        if (!shieldsOld.SequenceEqual(Equipments.AllEquipments.Where(x => x.EquipmentType.EquipmentTypeID == "shields").Select(x => x.ID).OrderBy(x => x)))
        {
            Shields.Update();
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
    private void Equipments_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var addedTurretCount = 0;
        var addedShieldCount = 0;

        if (e.NewItems is not null)
        {
            var groups = e.NewItems
                .Cast<IEquipment>()
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
                .Cast<IEquipment>()
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
