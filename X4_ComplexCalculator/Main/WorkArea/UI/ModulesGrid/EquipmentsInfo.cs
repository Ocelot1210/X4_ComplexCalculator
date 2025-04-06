using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

/// <summary>
/// 装備情報
/// </summary>
public sealed partial class EquipmentsInfo : ObservableObject
{
    #region メンバ 
    /// <summary>
    /// 装備管理オブジェクト
    /// </summary>
    private readonly EquippableWareEquipmentManager _manager;


    /// <summary>
    /// 表示対象の装備ID
    /// </summary>
    private readonly string _equipmentTypeID;


    /// <summary>
    /// 詳細表示文字列
    /// </summary>
    private string _detailsText = "";
    #endregion


    #region プロパティ
    /// <summary>
    /// 詳細表示文字列
    /// </summary>
    public string DetailsText
    {
        get
        {
            Update();
            return _detailsText;
        }
        set
        {
            SetProperty(ref _detailsText, value);
        }
    }


    public IEnumerable<IWareEquipment> Equipments
    {
        get
        {
            yield break; 
        }
    }


    /// <summary>
    /// 表示対象の装備の個数
    /// </summary>
    [ObservableProperty]
    public partial int Count { get; private set; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="manager">装備管理オブジェクト</param>
    /// <param name="equipmentTypeID">表示対象の装備ID</param>
    public EquipmentsInfo(EquippableWareEquipmentManager manager, string equipmentTypeID)
    {
        _manager = manager;
        _equipmentTypeID = equipmentTypeID;

        Count = _manager.AllEquipments.Where(x => x.EquipmentType.EquipmentTypeID == _equipmentTypeID).Count();
    }


    /// <summary>
    /// 表示内容を更新
    /// </summary>
    /// <returns></returns>
    private void Update()
    {
        var equipments = _manager.AllEquipments
            .Where(x => x.EquipmentType.EquipmentTypeID == _equipmentTypeID);

        // 装備が無い場合は
        if (!equipments.Any())
        {
            Count = 0;
            DetailsText = (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("Lang:Common_NotEquippedToolTipText", null, null);
            return;
        }

        var sb = new StringBuilder();
        var total = 0;

        var groups = equipments.GroupBy(x => x.Size)
            .OrderByDescending(x => x.Key is not null)
            .ThenBy(x => x.Key);

        foreach (var group in groups)
        {
            var cnt = 1;

            foreach (var ware in group)
            {
                if (cnt == 1)
                {
                    if (sb.Length != 0)
                    {
                        sb.AppendLine();
                    }
                    sb.AppendLine($"【{group.Key?.Name ?? ""}】");
                }
                sb.AppendLine($"{cnt++:D2} : {ware.Name}");
                total++;
            }
        }

        // 最後の改行を消す
        sb.Length -= 2;

        DetailsText = sb.ToString();
        Count = total;
    }


    //public static EquipmentsInfo Craete(IEquippableWare ware, string equipmentTypeID)
    //{
    //    ware.Equipments
    //}
}
