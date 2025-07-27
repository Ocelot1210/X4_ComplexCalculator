using Collections.Pooled;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment;
using ZLinq;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

/// <summary>
/// Module一覧DataGridの1レコード分の情報を管理するクラス
/// </summary>
public sealed partial class ModulesGridItem : ObservableRecipientEx, IEditable, ISelectable, IReorderble
{
    #region プロパティ
    /// <summary>
    /// モジュール
    /// </summary>
    public IX4Module Module { get; }


    /// <summary>
    /// 装備情報
    /// </summary>
    public EquippableWareEquipmentManager Equipments { get; }


    /// <summary>
    /// 選択されているか
    /// </summary>
    [ObservableProperty]
    public partial bool IsSelected { get; set; }


    /// <summary>
    /// モジュール数
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    public partial long ModuleCount { get; set; } = 1;


    /// <summary>
    /// タレット数
    /// </summary>
    [ObservableProperty]
    public partial long TurretsCount { get; private set; }


    /// <summary>
    /// シールド数
    /// </summary>
    [ObservableProperty]
    public partial long ShieldsCount { get; private set; }


    /// <summary>
    /// 選択中の建造方式
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    public partial IWareProduction SelectedMethod { get; set; }


    /// <summary>
    /// 編集状態
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    public partial EditStatus EditStatus { get; set; } = EditStatus.Unedited;


    /// <summary>
    /// 順番入れ替え対象か
    /// </summary>
    [ObservableProperty]
    public partial bool IsReorderTarget { get; set; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="messenger">メッセージ通知用</param>
    /// <param name="module">モジュール</param>
    /// <param name="selectedMethod">選択中の建造方式</param>
    /// <param name="moduleCount">モジュール数</param>
    public ModulesGridItem(IMessenger messenger, IX4Module module, IWareProduction? selectedMethod = null, long moduleCount = 1, IEnumerable<IEquipment>? equipments = null, EditStatus editStatus = EditStatus.Unedited) : base(messenger, false, nameof(ModulesGridItem))
    {
        Module = module;
        ModuleCount = moduleCount;
        Equipments = new EquippableWareEquipmentManager(module, equipments ?? []);
        UpdateEquipmentsCount();

        SelectedMethod = selectedMethod ?? Module.Productions.First().Value;
        EditStatus = editStatus;

        IsActive = true;
    }


    /// <summary>
    /// コンストラクタ(xmlより作成)
    /// </summary>
    /// <param name="messenger">メッセージ通知用</param>
    /// <param name="element">モジュール情報が記載されたxml</param>
    public ModulesGridItem(IMessenger messenger, XElement element, EditStatus editStatus) : base(messenger, false, nameof(ModulesGridItem))
    {
        Module = X4Database.Instance.Ware.TryGet<IX4Module>(element.Attribute("id")!.Value) ?? throw new ArgumentException("Invalid XElement.", nameof(element));
        Equipments = new EquippableWareEquipmentManager(Module, element.Element("equipments"));
        UpdateEquipmentsCount();

        ModuleCount = long.Parse(element.Attribute("count")?.Value ?? "1");

        SelectedMethod = 
            Module.Productions.TryGetValue(element.Attribute("method")?.Value ?? "default", out var method) ? method : Module.Productions.Values.First();

        EditStatus = editStatus;

        IsActive = true;
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
    /// <param name="count">追加数</param>
    public void AddEquipment(IEquipment equipment, long count = 1) => Equipments.Add(equipment, count);


    /// <summary>
    /// モジュール数変更時
    /// </summary>
    partial void OnModuleCountChanged(long value)
    {
        EditStatus = EditStatus.Edited;
    }


    /// <summary>
    /// 建造方式変更時
    /// </summary>
    partial void OnSelectedMethodChanged(IWareProduction value)
    {
        EditStatus = EditStatus.Edited;
    }


    /// <summary>
    /// 装備を編集
    /// </summary>
    [RelayCommand]
    private void EditEquipment()
    {
        var turretsType = X4Database.Instance.EquipmentType.Get("turrets");
        var shieldsType = X4Database.Instance.EquipmentType.Get("shields");

        // 変更前
        using var turretsOld = Equipments.AllEquipments.Where(x => x.EquipmentType.Equals(turretsType)).ToPooledList();
        using var shieldsOld = Equipments.AllEquipments.Where(x => x.EquipmentType.Equals(shieldsType)).ToPooledList();


        var window = new EditEquipmentWindow(Messenger, Equipments)
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();


        {
            (IList<IEquipment> OldEquipement, IEquipmentType EquipmentType)[] equipemtns = 
            [
                (turretsOld, turretsType),
                (shieldsOld, shieldsType)
            ];

            // 装備の内容に変更があったか？(建造コスト計算用。順番の変更は変更と見なさない)
            if (equipemtns.Any(x => !x.OldEquipement.AsValueEnumerable().OrderBy(x => x.ID).SequenceEqual(Equipments.AllEquipments.AsValueEnumerable().Where(y => y.EquipmentType.Equals(x.EquipmentType)).OrderBy(x => x.ID))))
            {
                using var newItems = Equipments.AllEquipments
                    .Where(x => x.EquipmentType.Equals(turretsType) || x.EquipmentType.Equals(shieldsType))
                    .ToPooledList();

                Broadcast(turretsOld.Concat(shieldsOld), newItems, nameof(Equipments));
            }
            

            // 装備の内容に変更があったものについては個数を更新(モジュール一覧表示用のため、順番の変更も変更と見なす)
            if (equipemtns.AsValueEnumerable().Any(x => !x.OldEquipement.AsValueEnumerable().SequenceEqual(Equipments.AllEquipments.AsValueEnumerable().Where(y => y.EquipmentType.Equals(x.EquipmentType)))))
            {
                UpdateEquipmentsCount();
                OnPropertyChanged(nameof(Equipments));
                EditStatus = EditStatus.Edited;
            }
        }
    }


    /// <summary>
    /// タレット数とシールド数を更新
    /// </summary>
    private void UpdateEquipmentsCount()
    {
        TurretsCount = Equipments.AllEquipments.Count(x => x.EquipmentType.EquipmentTypeID == "turrets");
        ShieldsCount = Equipments.AllEquipments.Count(x => x.EquipmentType.EquipmentTypeID == "shields");
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
