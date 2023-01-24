using System.Linq;
using System.Text;
using X4_ComplexCalculator.Entity;
using Prism.Mvvm;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

/// <summary>
/// 装備情報
/// </summary>
public class EquipmentsInfo : BindableBase
{
    #region メンバ 
    /// <summary>
    /// 装備管理オブジェクト
    /// </summary>
    private readonly EquippableWareEquipmentManager _Manager;


    /// <summary>
    /// 表示対象の装備ID
    /// </summary>
    private readonly string _EquipmentTypeID;


    /// <summary>
    /// 更新が必要か
    /// </summary>
    private bool _UpdateNeeded;


    /// <summary>
    /// 詳細表示文字列
    /// </summary>
    private string _DetailsText = "";


    /// <summary>
    /// 表示対象の装備の個数
    /// </summary>
    private int _Count;
    #endregion


    #region プロパティ
    /// <summary>
    /// 詳細表示文字列
    /// </summary>
    public string DetailsText
    {
        get
        {
            if (_UpdateNeeded)
            {
                Update();
            }
            return _DetailsText;
        }
        set
        {
            SetProperty(ref _DetailsText, value);
        }
    }


    /// <summary>
    /// 表示対象の装備の個数
    /// </summary>
    public int Count
    {
        get
        {
            if (_UpdateNeeded)
            {
                Update();
            }
            return _Count;
        }
        set
        {
            SetProperty(ref _Count, value);
        }
    }
    #endregion



    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="manager">装備管理オブジェクト</param>
    /// <param name="equipmentTypeID">表示対象の装備ID</param>
    public EquipmentsInfo(EquippableWareEquipmentManager manager, string equipmentTypeID)
    {
        _Manager = manager;
        _EquipmentTypeID = equipmentTypeID;
        _UpdateNeeded = true;
    }



    /// <summary>
    /// ツールチップ文字列の更新を要求する
    /// </summary>
    public void RequireUpdate() => _UpdateNeeded = true;



    /// <summary>
    /// 表示内容を更新
    /// </summary>
    /// <returns></returns>
    public void Update()
    {
        var equipments = _Manager.AllEquipments
            .Where(x => x.EquipmentType.EquipmentTypeID == _EquipmentTypeID);

        if (!equipments.Any())
        {
            _Count = 0;
            DetailsText = (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("Lang:Common_NotEquippedToolTipText", null, null);
            _UpdateNeeded = false;
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
        _UpdateNeeded = false;
    }
}
