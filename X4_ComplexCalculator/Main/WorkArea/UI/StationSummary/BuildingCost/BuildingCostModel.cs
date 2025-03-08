using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
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
partial class BuildingCostModel : ObservableRecipient
{
    #region メンバ
    /// <summary>
    /// 建造リソース情報
    /// </summary>
    private readonly IBuildResourcesInfo _buildResources;
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
    public BuildingCostModel(IMessenger messanger, IBuildResourcesInfo resources) : base(messanger)
    {
        _buildResources = resources;
        _buildResources.BuildResources.CollectionChanged += Resources_OnCollectionChanged;

        Messenger.RegisterPropertyChangedMessage(this, (BuildResourcesGridItem x) => x.Price, OnBuildResourcePriceChanged);
    }


    /// <summary>
    /// 建造に必要なウェア一覧のプロパティに変更があった場合
    /// </summary>
    private void OnBuildResourcePriceChanged(BuildingCostModel model, PropertyChangedMessage<long> message)
    {
        BuildingCost -= (message.OldValue - message.NewValue);
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        _buildResources.BuildResources.CollectionChanged -= Resources_OnCollectionChanged;
        BuildResources.Clear();
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
