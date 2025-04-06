using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment;

/// <summary>
/// 派閥リストの1レコード分
/// </summary>
sealed partial class FactionsListItem : ObservableRecipientEx
{
    #region プロパティ
    /// <summary>
    /// 派閥
    /// </summary>
    public IFaction Faction { get; }


    /// <summary>
    /// 種族ID
    /// </summary>
    public string RaceID => Faction.Race.RaceID;


    /// <summary>
    /// 種族名
    /// </summary>
    public string RaceName => Faction.Race.Name;


    /// <summary>
    /// 派閥名
    /// </summary>
    public string FactionName => Faction.Name;


    /// <summary>
    /// チェック状態
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    public partial bool IsChecked { get; set; }
    #endregion


    /// <remarks>
    /// コンストラクタ
    /// </remarks>
    /// <param name="messenger">メッセージ通知用</param>
    /// <param name="faction">派閥</param>
    /// <param name="isChecked">チェック状態</param>
    public FactionsListItem(IMessenger messenger, IFaction faction, bool isChecked) : base(messenger)
    {
        Faction = faction;
        IsChecked = isChecked;
        IsActive = true;
    }
}
