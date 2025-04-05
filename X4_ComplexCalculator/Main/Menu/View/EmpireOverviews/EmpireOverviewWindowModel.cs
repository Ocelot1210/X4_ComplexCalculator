using Collections.Pooled;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collections;
using X4_ComplexCalculator.Main.WorkArea;
using X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;

namespace X4_ComplexCalculator.Main.Menu.View.EmpireOverviews;

/// <summary>
/// 帝国の概要用Model
/// </summary>
public sealed class EmpireOverviewWindowModel : ObservableRecipient, IDisposable
{
    #region メンバ
    /// <summary>
    /// 開いている計画一覧
    /// </summary>
    private readonly ObservableCollection<WorkAreaViewModel> _sourceWorkAreas;
    #endregion


    #region プロパティ
    /// <summary>
    /// 製品一覧
    /// </summary>
    public ObservableRangeCollection<EmpireOverViewProductsGridItem> Products { get; } = [];


    /// <summary>
    /// 計画一覧
    /// </summary>
    public ObservableRangeCollection<WorkAreaItem> WorkAreas { get; } = [];
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="workAreas">開いている計画一覧</param>
    public EmpireOverviewWindowModel(ObservableCollection<WorkAreaViewModel> workAreas) : base(new WeakReferenceMessenger())
    {
        _sourceWorkAreas = workAreas;
        _sourceWorkAreas.CollectionChanged += SourceWorkAreas_CollectionChanged;

        WorkAreas.CollectionChanged += WorkAreas_CollectionChanged;
        Messenger.RegisterPropertyChangedMessage(this, static (WorkAreaItem x) => x.IsChecked, static (r, m) => r.RecalculateAll());

        // 初期値として、既に表示中の計画を追加
        WorkAreas.Reset(_sourceWorkAreas.Select(x => new WorkAreaItem(Messenger, x, true)));
    }


    /// <inheritdoc/>
    public void Dispose()
    {
        _sourceWorkAreas.CollectionChanged -= SourceWorkAreas_CollectionChanged;
        WorkAreas.CollectionChanged -= WorkAreas_CollectionChanged;

        Messenger.UnregisterAll(this);

        foreach (var products in WorkAreas.Select(x => x.WorkArea.Products.ProductsInfo.Products))
        {
            products.CollectionChanged -= Products_CollectionChanged;
        }
    }


    /// <summary>
    /// 開いている計画一覧と帝国の概要画面に表示する計画一覧を同期する
    /// </summary>
    private void SourceWorkAreas_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                WorkAreas.AddRange(e.NewItems!.Cast<WorkAreaViewModel>().Select(x => new WorkAreaItem(Messenger, x, true)));
                break;

            case NotifyCollectionChangedAction.Remove:
                WorkAreas.RemoveAll(x => e.OldItems!.Contains(x.WorkArea));
                break;

            case NotifyCollectionChangedAction.Reset:
                WorkAreas.Reset(_sourceWorkAreas.Select(x => new WorkAreaItem(Messenger, x, true)));
                break;

            default:
                break;
        }
    }


    /// <summary>
    /// 計画の要素数に変更があった場合
    /// </summary>
    private void WorkAreas_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                OnWorkAreaAdded(e.NewItems?.Cast<WorkAreaItem>() ?? []);
                break;

            case NotifyCollectionChangedAction.Remove:
                OnWorkAreaRemoved(e.OldItems?.Cast<WorkAreaItem>() ?? []);
                break;

            case NotifyCollectionChangedAction.Reset:
                OnWorkAreaReseted();
                break;
        }
    }


    /// <summary>
    /// 計画が追加された場合
    /// </summary>
    private void OnWorkAreaAdded(IEnumerable<WorkAreaItem> addedWorkAreas)
    {
        foreach (var item in addedWorkAreas)
        {
            var products = item.WorkArea.Products.ProductsInfo.Products;

            products.CollectionChanged += Products_CollectionChanged;
            item.WorkArea.GetMessenger().RegisterPropertyChangedMessage(this, static (ProductsGridItem x) => x.Count, static (r, m) => r.OnProductsCountChanged(m));

            OnProductsAdded(products);
        }
    }


    /// <summary>
    /// 計画が削除された場合
    /// </summary>
    private void OnWorkAreaRemoved(IEnumerable<WorkAreaItem> removedWorkAreas)
    {
        foreach (var item in removedWorkAreas)
        {
            var products = item.WorkArea.Products.ProductsInfo.Products;

            products.CollectionChanged -= Products_CollectionChanged;
            item.WorkArea.GetMessenger().UnregisterAll(this);

            OnProductsRemoved(products);
        }
    }


    /// <summary>
    /// 計画がリセットされた場合
    /// </summary>
    private void OnWorkAreaReseted()
    {
        foreach (var item in WorkAreas)
        {
            var products = item.WorkArea.Products.ProductsInfo.Products;

            products.CollectionChanged += Products_CollectionChanged;
            item.WorkArea.GetMessenger().RegisterPropertyChangedMessage(this, static (ProductsGridItem x) => x.Count, static (r, m) => r.OnProductsCountChanged(m));

            OnProductsAdded(products);
        }
    }


    /// <summary>
    /// ある計画の製品のプロパティに変更があった場合
    /// </summary>
    private void OnProductsCountChanged(PropertyChangedMessage<long> message)
    {
        if (message.Sender is not ProductsGridItem product)
        {
            return;
        }

        Products.FirstOrDefault(x => x.Ware.ID == product.Ware.ID)?.UpdateProduct(message.OldValue, message.NewValue);
    }


    /// <summary>
    /// ある計画の製品一覧に変更があった場合
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Products_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                OnProductsAdded(e.NewItems?.OfType<ProductsGridItem>() ?? []);
                break;

            case NotifyCollectionChangedAction.Remove:
                OnProductsRemoved(e.OldItems?.OfType<ProductsGridItem>() ?? []);
                break;

            case NotifyCollectionChangedAction.Reset:
                RecalculateAll();
                break;
        }
    }


    /// <summary>
    /// 製品が追加された場合
    /// </summary>
    private void OnProductsAdded(IEnumerable<ProductsGridItem> addedItems)
    {
        using var addTarget = new PooledList<EmpireOverViewProductsGridItem>();     // 追加対象

        using var currProduct = Products.ToPooledDictionary(x => x.Ware.ID, x => x);

        foreach (var addedItem in addedItems)
        {
            // 追加された製品は、どこかのステーションで既に生産/消費しているか？
            if (currProduct.TryGetValue(addedItem.Ware.ID, out var prod))
            {
                // すでに表示している要素の生産量に加算して重複しないようにする
                prod.AddProduct(addedItem.Count);
            }
            else
            {
                // どのステーションでもまだ生産/消費していないウェアの場合、追加対象に追加する
                var item = new EmpireOverViewProductsGridItem(addedItem.Ware, addedItem.Count);
                addTarget.Add(item);
                currProduct.Add(item.Ware.ID, item);
            }
        }

        // 追加対象の要素を一気に追加する
        Products.AddRange(addTarget);
    }


    /// <summary>
    /// 製品が削除された場合
    /// </summary>
    private void OnProductsRemoved(IEnumerable<ProductsGridItem> removedItems)
    {
        using var currProduct = Products.ToPooledDictionary(x => x.Ware.ID, x => x);

        // 削除された製品の生産/消費量を減算する
        foreach (var removedItem in removedItems)
        {
            if (currProduct.TryGetValue(removedItem.Ware.ID, out var prod))
            {
                prod.DeleteProduct(removedItem.Count);
            }
        }
    }


    /// <summary>
    /// 全て再計算
    /// </summary>
    private void RecalculateAll()
    {
        Products.Clear();

        foreach (var products in WorkAreas.Where(x => x.IsChecked).Select(x => x.WorkArea.Products.ProductsInfo.Products))
        {
            OnProductsAdded(products);
        }
    }
}
