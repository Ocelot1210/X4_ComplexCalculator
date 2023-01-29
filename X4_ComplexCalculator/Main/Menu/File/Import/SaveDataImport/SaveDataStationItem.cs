using Prism.Mvvm;
using System.Xml.Linq;

namespace X4_ComplexCalculator.Main.Menu.File.Import.SaveDataImport;

/// <summary>
/// X4 セーブデータインポート時のステーション一覧1レコード分
/// </summary>
public class SaveDataStationItem : BindableBase
{
    #region メンバ
    /// <summary>
    /// チェックされたか
    /// </summary>
    private bool _isChecked;
    #endregion


    #region プロパティ
    /// <summary>
    /// セクター名
    /// </summary>
    public string SectorName { get; }


    /// <summary>
    /// ステーション名
    /// </summary>
    public string StationName { get; }


    /// <summary>
    /// チェックされたか
    /// </summary>
    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }


    /// <summary>
    /// xml内容
    /// </summary>
    public XElement XElement { get; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="sectorName">セクター名</param>
    /// <param name="xElement">xml内容</param>
    public SaveDataStationItem(string sectorName, XElement xElement)
    {
        SectorName = sectorName;
        StationName = xElement.Attribute("name")?.Value ?? "";
        XElement = xElement;
    }
}
