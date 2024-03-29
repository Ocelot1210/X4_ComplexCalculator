﻿using Collections.Pooled;
using Prism.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.WorkArea;
using X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;

namespace X4_ComplexCalculator.Main.Menu.View.EmpireOverview;

/// <summary>
/// 帝国の概要用Model
/// </summary>
public sealed class EmpireOverviewWindowModel : IDisposable
{
    #region メンバ
    /// <summary>
    /// 開いている計画一覧
    /// </summary>
    private readonly ObservableCollection<WorkAreaViewModel> _sourceWorkAreas;


    /// <summary>
    /// 製品記憶用ディクショナリ
    /// </summary>
    private readonly ListDictionary<IList<ProductsGridItem>, ProductsGridItem> _productsBak = new();
    #endregion


    #region プロパティ
    /// <summary>
    /// 製品一覧
    /// </summary>
    public ObservableRangeCollection<EmpireOverViewProductsGridItem> Products { get; }
        = new ObservableRangeCollection<EmpireOverViewProductsGridItem>();


    /// <summary>
    /// 計画一覧
    /// </summary>
    public ObservablePropertyChangedCollection<WorkAreaItem> WorkAreas { get; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="workAreas">開いている計画一覧</param>
    public EmpireOverviewWindowModel(ObservableCollection<WorkAreaViewModel> workAreas)
    {
        _sourceWorkAreas = workAreas;
        _sourceWorkAreas.CollectionChanged += SourceWorkAreas_CollectionChanged;

        WorkAreas = new ObservablePropertyChangedCollection<WorkAreaItem>();
        WorkAreas.CollectionChanged += WorkAreas_CollectionChanged;
        WorkAreas.CollectionPropertyChanged += WorkAreas_CollectionPropertyChanged;

        WorkAreas.AddRange(workAreas.Select(x => new WorkAreaItem(x, true)));

        foreach (var products in WorkAreas.Select(x => x.WorkArea.Products.ProductsInfo.Products))
        {
            products.CollectionChanged += Products_CollectionChanged;
            products.CollectionPropertyChanged += Products_CollectionPropertyChanged;

            OnProductsAdded(products, products);
        }
    }


    /// <inheritdoc/>
    public void Dispose()
    {
        _sourceWorkAreas.CollectionChanged -= SourceWorkAreas_CollectionChanged;
        WorkAreas.CollectionChanged -= WorkAreas_CollectionChanged;
        WorkAreas.CollectionPropertyChanged -= WorkAreas_CollectionPropertyChanged;

        _productsBak.Clear();
        foreach (var products in WorkAreas.Select(x => x.WorkArea.Products.ProductsInfo.Products))
        {
            products.CollectionChanged -= Products_CollectionChanged;
            products.CollectionPropertyChanged -= Products_CollectionPropertyChanged;
        }
    }


    /// <summary>
    /// 計画一覧の要素数に変更があった場合
    /// </summary>
    private void SourceWorkAreas_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                WorkAreas.AddRange(e.NewItems!.Cast<WorkAreaViewModel>().Select(x => new WorkAreaItem(x, true)));
                break;

            case NotifyCollectionChangedAction.Remove:
                {
                    using var oldItems = e.OldItems!.Cast<WorkAreaViewModel>().ToPooledList();
                    WorkAreas.RemoveAll(x => oldItems!.Contains(x.WorkArea));
                }
                break;

            case NotifyCollectionChangedAction.Replace:
                break;

            case NotifyCollectionChangedAction.Move:
                break;

            case NotifyCollectionChangedAction.Reset:
                WorkAreas.Reset(_sourceWorkAreas.Select(x => new WorkAreaItem(x, true)));
                break;
        }
    }


    /// <summary>
    /// コレクション内部のプロパティに変更があった場合
    /// </summary>
    private void WorkAreas_CollectionPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        _productsBak.Clear();
        Products.Clear();

        foreach (var products in WorkAreas.Where(x => x.IsChecked).Select(x => x.WorkArea.Products.ProductsInfo.Products))
        {
            OnProductsAdded(products, products);
        }
    }


    /// <summary>
    /// 計画の要素数に変更があった場合
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void WorkAreas_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var getProducts = (IList? list) => 
            list?.OfType<WorkAreaViewModel>().Select(x => x.Products.ProductsInfo.Products) ?? 
            Enumerable.Empty<ObservablePropertyChangedCollection<ProductsGridItem>>(); ;

        foreach (var items in getProducts(e.OldItems))
        {
            items.CollectionChanged -= Products_CollectionChanged;
            items.CollectionPropertyChanged -= Products_CollectionPropertyChanged;

            OnProductsRemoved(items, items);
        }

        foreach (var items in getProducts(e.NewItems))
        {
            items.CollectionChanged += Products_CollectionChanged;
            items.CollectionPropertyChanged += Products_CollectionPropertyChanged;

            OnProductsAdded(items, items);
        }
    }


    /// <summary>
    /// ある計画の製品のプロパティに変更があった場合
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Products_CollectionPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (sender is not ProductsGridItem product)
        {
            return;
        }

        if (e.PropertyName == nameof(ProductsGridItem.Count))
        {
            if (e is not PropertyChangedExtendedEventArgs<long> ev)
            {
                return;
            }

            Products.FirstOrDefault(x => x.Ware.ID == product.Ware.ID)?.UpdateProduct(ev.OldValue, ev.NewValue);
        }
    }


    /// <summary>
    /// ある計画の製品一覧に変更があった場合
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Products_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is not ObservableCollection<ProductsGridItem> products)
        {
            return;
        }

        // 生産/消費ウェアが削除された場合
        if (e.OldItems is not null)
        {
            OnProductsRemoved(products, e.OldItems.OfType<ProductsGridItem>());
        }

        // 生産/消費ウェアが追加された場合
        if (e.NewItems is not null)
        {
            OnProductsAdded(products, e.NewItems.OfType<ProductsGridItem>());
        }

        // 生産/消費ウェアがリセットされた場合
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            using var added = new PooledList<ProductsGridItem>();
            using var removed = new PooledList<ProductsGridItem>();
            
            foreach (var prod in _productsBak[products])
            {
                if (products.Any(x => x.Ware.ID == prod.Ware.ID))
                {
                    added.Add(prod);
                }
                else
                {
                    removed.Add(prod);
                }
            }

            OnProductsRemoved(products, removed);
            OnProductsAdded(products, added);
        }
    }


    /// <summary>
    /// 製品が削除された場合
    /// </summary>
    /// <param name="removedItems"></param>
    private void OnProductsRemoved(IList<ProductsGridItem> parent, IEnumerable<ProductsGridItem> removedItems)
    {
        var prodBak = _productsBak[parent];

        // 削除された製品の生産/消費量を減算する
        foreach (var removedItem in removedItems)
        {
            var prod = Products.FirstOrDefault(x => x.Ware.ID == removedItem.Ware.ID);
            if (prod is not null)
            {
                prod.DeleteProduct(removedItem.Count);
            }

            prodBak.Remove(removedItem);
        }
    }


    /// <summary>
    /// 製品が追加された場合
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="addedItems"></param>
    private void OnProductsAdded(IList<ProductsGridItem> parent, IEnumerable<ProductsGridItem> addedItems)
    {
        using var addTarget = new PooledList<EmpireOverViewProductsGridItem>();     // 追加対象
        var prodBak = _productsBak[parent];

        foreach (var addedItem in addedItems)
        {
            var prod = Products.FirstOrDefault(x => x.Ware.ID == addedItem.Ware.ID);

            if (prod is null)
            {
                // どのステーションでもまだ生産/消費していないウェアの場合
                // 追加対象に突っ込む
                addTarget.Add(new EmpireOverViewProductsGridItem(addedItem.Ware, addedItem.Count));
            }
            else
            {
                // どこかのステーションで既に生産/消費しているウェアの場合
                // すでにある要素の生産量に加算する
                prod.AddProduct(addedItem.Count);
            }

            prodBak.Add(addedItem);
        }

        // 追加対象の要素を一気に追加する
        Products.AddRange(addTarget);
    }
}
