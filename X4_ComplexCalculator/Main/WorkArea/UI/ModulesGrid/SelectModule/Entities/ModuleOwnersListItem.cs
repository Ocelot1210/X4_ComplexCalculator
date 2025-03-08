using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.SelectModule.Entities;


/// <summary>
/// コンストラクタ
/// </summary>
/// <param name="id">ID</param>
/// <param name="name">名称</param>
/// <param name="isChecked">チェック状態</param>
public sealed partial class ModuleOwnersListItem(IMessenger messenger, IFaction faction, bool isChecked) : ObservableRecipientEx(messenger, true)
{
    /// <summary>
    /// 派閥情報
    /// </summary>
    private readonly IFaction _faction = faction;


    /// <summary>
    /// 種族ID
    /// </summary>
    public string RaceID => _faction.Race.RaceID;


    /// <summary>
    /// 種族名
    /// </summary>
    public string RaceName => _faction.Race.Name;


    /// <summary>
    /// 派閥ID
    /// </summary>
    public string FactionID => _faction.FactionID;


    /// <summary>
    /// 表示名称
    /// </summary>
    public string FactionName => _faction.Name;


    /// <summary>
    /// チェック状態
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    public partial bool IsChecked { get; set; } = isChecked;
}
