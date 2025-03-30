using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.BuildResources;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StationSummary.BuildingCost;

/// <summary>
/// 建造コスト用
/// </summary>
partial class BuildingCostModel : ObservableRecipientEx
{
    #region メンバ
    /// <summary>
    /// 建造リソース情報
    /// </summary>
    private readonly BuildResourcesInfo _buildResources;
    #endregion


    #region プロパティ
    /// <summary>
    /// 建造リソース一覧
    /// </summary>
    public ObservableCollection<BuildResourcesGridItem> BuildResources => _buildResources.BuildResources;


    /// <summary>
    /// 建造コスト
    /// </summary>
    [ObservableProperty]
    public partial long BuildingCost { get; private set; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="messanger">メッセージ交換用</param>
    /// <param name="resources">建造リソース一覧</param>
    public BuildingCostModel(IMessenger messanger, BuildResourcesInfo resources) : base(messanger, true)
    {
        _buildResources = resources;
        _buildResources.BuildResources.CollectionChanged += Resources_OnCollectionChanged;

        Messenger.RegisterPropertyChangedMessage(this, static (BuildResourcesGridItem x) => x.Price, static (r, m) => r.BuildingCost -= (m.OldValue - m.NewValue));
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        _buildResources.BuildResources.CollectionChanged -= Resources_OnCollectionChanged;
        BuildResources.Clear();
        Messenger.UnregisterAll(this);
    }


    /// <summary>
    /// 建造に必要なウェア一覧に変更があった場合
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Resources_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            BuildingCost += e.NewItems.Cast<BuildResourcesGridItem>().Sum(x => x.Price);
        }

        if (e.OldItems is not null)
        {
            BuildingCost -= e.OldItems.Cast<BuildResourcesGridItem>().Sum(x => x.Price);
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            BuildingCost = BuildResources.Sum(x => x.Price);
        }
    }
}
