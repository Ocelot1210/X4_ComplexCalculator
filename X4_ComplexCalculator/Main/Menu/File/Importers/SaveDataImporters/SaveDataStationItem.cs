using CommunityToolkit.Mvvm.ComponentModel;
using System.Xml.Linq;

namespace X4_ComplexCalculator.Main.Menu.File.Importers.SaveDataImporters;

/// <summary>
/// X4 セーブデータインポート時のステーション一覧1レコード分
/// </summary>
/// <param name="sectorName">セクター名</param>
/// <param name="xElement">xml内容</param>
public sealed partial class SaveDataStationItem(string sectorName, XElement xElement) : ObservableObject
{
    #region プロパティ
    /// <summary>
    /// セクター名
    /// </summary>
    public string SectorName { get; } = sectorName;


    /// <summary>
    /// ステーション名
    /// </summary>
    public string StationName { get; } = xElement.Attribute("name")?.Value ?? "";


    /// <summary>
    /// チェックされたか
    /// </summary>
    [ObservableProperty]
    public partial bool IsChecked { get; set; }


    /// <summary>
    /// xml内容
    /// </summary>
    public XElement XElement { get; } = xElement;
    #endregion
}
