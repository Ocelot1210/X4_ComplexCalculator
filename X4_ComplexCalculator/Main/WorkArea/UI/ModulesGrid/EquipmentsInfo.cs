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
    private readonly EquippableWareEquipmentManager _manager;


    /// <summary>
    /// 表示対象の装備ID
    /// </summary>
    private readonly string _equipmentTypeID;


    /// <summary>
    /// 更新が必要か
    /// </summary>
    private bool _updateNeeded;


    /// <summary>
    /// 詳細表示文字列
    /// </summary>
    private string _detailsText = "";


    /// <summary>
    /// 表示対象の装備の個数
    /// </summary>
    private int _count;
    #endregion


    #region プロパティ
    /// <summary>
    /// 詳細表示文字列
    /// </summary>
    public string DetailsText
    {
        get
        {
            if (_updateNeeded)
            {
                Update();
            }
            return _detailsText;
        }
        set
        {
            SetProperty(ref _detailsText, value);
        }
    }


    /// <summary>
    /// 表示対象の装備の個数
    /// </summary>
    public int Count
    {
        get
        {
            if (_updateNeeded)
            {
                Update();
            }
            return _count;
        }
        set
        {
            SetProperty(ref _count, value);
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
        _manager = manager;
        _equipmentTypeID = equipmentTypeID;
        _updateNeeded = true;
    }



    /// <summary>
    /// ツールチップ文字列の更新を要求する
    /// </summary>
    public void RequireUpdate() => _updateNeeded = true;



    /// <summary>
    /// 表示内容を更新
    /// </summary>
    /// <returns></returns>
    public void Update()
    {
        var equipments = _manager.AllEquipments
            .Where(x => x.EquipmentType.EquipmentTypeID == _equipmentTypeID);

        if (!equipments.Any())
        {
            _count = 0;
            DetailsText = (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("Lang:Common_NotEquippedToolTipText", null, null);
            _updateNeeded = false;
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
        _updateNeeded = false;
    }
}
