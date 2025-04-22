using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;
using System.Text;
using ZLinq;

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
    #endregion


    #region プロパティ
    /// <summary>
    /// 詳細表示文字列
    /// </summary>
    public string DetailsText => GetDetailText();


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
        UpdateCount();
    }


    /// <summary>
    /// 個数を更新
    /// </summary>
    public void UpdateCount()
    {
        Count = _manager.AllEquipments.Where(x => x.EquipmentType.EquipmentTypeID == _equipmentTypeID).Count();
    }


    /// <summary>
    /// 表示内容を更新
    /// </summary>
    /// <returns></returns>
    private string GetDetailText()
    {
        var equipments = _manager.AllEquipments
            .AsValueEnumerable()
            .Where(x => x.EquipmentType.EquipmentTypeID == _equipmentTypeID);

        // 装備が無い場合は専用のテキストを表示
        if (!equipments.Any())
        {
            return (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("Lang:Common_NotEquippedToolTipText", null, null);
        }

        var sb = new StringBuilder(256);

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
                    // サイズが切り替わった直後なら改行する
                    if (sb.Length != 0)
                    {
                        sb.AppendLine();
                    }
                    sb.AppendLine($"【{group.Key?.Name ?? ""}】");
                }
                sb.AppendLine($"{cnt++:D2} : {ware.Name}");
            }
        }

        // 最後の改行を消す
        sb.Length -= 2;

        return sb.ToString();
    }


    //public static EquipmentsInfo Craete(IEquippableWare ware, string equipmentTypeID)
    //{
    //    ware.Equipments
    //}
}
