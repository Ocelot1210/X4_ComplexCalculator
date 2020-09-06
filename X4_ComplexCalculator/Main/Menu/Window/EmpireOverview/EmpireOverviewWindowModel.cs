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

namespace X4_ComplexCalculator.Main.Menu.Window.EmpireOverview
{
    /// <summary>
    /// 帝国の概要用Model
    /// </summary>
    public class EmpireOverviewWindowModel : IDisposable
    {
        #region メンバ
        /// <summary>
        /// 開いている計画一覧
        /// </summary>
        private readonly ObservableCollection<WorkAreaViewModel> _WorkAreas;


        /// <summary>
        /// 製品記憶用ディクショナリ
        /// </summary>
        private readonly ListDictionary<IList<ProductsGridItem>, ProductsGridItem> _ProductsBak
            = new ListDictionary<IList<ProductsGridItem>, ProductsGridItem>();
        #endregion


        #region プロパティ
        /// <summary>
        /// 製品一覧
        /// </summary>
        public ObservableRangeCollection<EmpireOverViewProductsGridItem> Products { get; } 
            = new ObservableRangeCollection<EmpireOverViewProductsGridItem>();
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="workAreas">開いている計画一覧</param>
        public EmpireOverviewWindowModel(ObservableCollection<WorkAreaViewModel> workAreas)
        {
            _WorkAreas = workAreas;

            _WorkAreas.CollectionChanged += WorkAreas_CollectionChanged;
            foreach (var products in _WorkAreas.Select(x => x.Products.ProductsInfo.Products))
            {
                products.CollectionChanged += Products_CollectionChanged;
                products.CollectionPropertyChanged += Products_CollectionPropertyChanged;

                OnProductsAdded(products, products);
            }
        }


        /// <inheritdoc/>
        public void Dispose()
        {
            _WorkAreas.CollectionChanged -= WorkAreas_CollectionChanged;
            _ProductsBak.Clear();
            foreach (var products in _WorkAreas.Select(x => x.Products.ProductsInfo.Products))
            {
                products.CollectionChanged -= Products_CollectionChanged;
                products.CollectionPropertyChanged -= Products_CollectionPropertyChanged;
            }
        }


        /// <summary>
        /// 計画の要素数に変更があった場合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkAreas_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.OfType<WorkAreaViewModel>().Select(x => x.Products.ProductsInfo.Products))
                {
                    item.CollectionChanged -= Products_CollectionChanged;
                    item.CollectionPropertyChanged -= Products_CollectionPropertyChanged;

                    // 削除された製品の生産/消費量を減算する
                    foreach (var removedItem in _ProductsBak[item])
                    {
                        Products.First(x => x.Ware.WareID == removedItem.Ware.WareID).Count -= removedItem.Count;
                    }

                    _ProductsBak.Remove(item);

                    // どのステーションでも生産/消費されていない要素を削除する
                    Products.RemoveAll(x => x.Count == 0);
                }
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems.OfType<WorkAreaViewModel>().Select(x => x.Products.ProductsInfo.Products))
                {
                    item.CollectionChanged += Products_CollectionChanged;
                    item.CollectionPropertyChanged += Products_CollectionPropertyChanged;

                    OnProductsAdded(item, item);
                }
            }
        }


        /// <summary>
        /// ある計画の製品のプロパティに変更があった場合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Products_CollectionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is ProductsGridItem product))
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(ProductsGridItem.Count):
                    {
                        if (!(e is PropertyChangedExtendedEventArgs<long> ev))
                        {
                            return;
                        }

                        Products.First(x => x.Ware.WareID == product.Ware.WareID).Count += ev.NewValue - ev.OldValue;
                    }
                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// ある計画の製品一覧に変更があった場合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Products_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!(sender is ObservableCollection<ProductsGridItem> products))
            {
                return;
            }

            // 生産/消費ウェアが削除された場合
            if (e.OldItems != null)
            {
                OnProductsRemoved(products, e.OldItems.OfType<ProductsGridItem>());
            }

            // 生産/消費ウェアが追加された場合
            if (e.NewItems != null)
            {
                OnProductsAdded(products, e.NewItems.OfType<ProductsGridItem>());
            }

            // 生産/消費ウェアがリセットされた場合
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                var added = new List<ProductsGridItem>();
                var removed = new List<ProductsGridItem>();

                foreach (var prod in _ProductsBak[products])
                {
                    if (products.Any(x => x.Ware.WareID == prod.Ware.WareID))
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
            var prodBak = _ProductsBak[parent];

            // 削除された製品の生産/消費量を減算する
            foreach (var removedItem in removedItems)
            {
                Products.First(x => x.Ware.WareID == removedItem.Ware.WareID).Count -= removedItem.Count;
                prodBak.Remove(removedItem);
            }

            // どのステーションでも生産/消費されていない要素を削除する
            Products.RemoveAll(x => x.Count == 0);
        }


        /// <summary>
        /// 製品が追加された場合
        /// </summary>
        /// <param name="removedItems"></param>
        private void OnProductsAdded(IList<ProductsGridItem> parent, IEnumerable<ProductsGridItem> addedItems)
        {
            var addTarget = new List<EmpireOverViewProductsGridItem>();     // 追加対象
            var prodBak = _ProductsBak[parent];

            foreach (var addedItem in addedItems)
            {
                var prod = Products.FirstOrDefault(x => x.Ware.WareID == addedItem.Ware.WareID);

                if (prod == null)
                {
                    // どのステーションでもまだ生産/消費していないウェアの場合
                    // 追加対象に突っ込む
                    addTarget.Add(new EmpireOverViewProductsGridItem(addedItem.Ware, addedItem.Count));
                }
                else
                {
                    // どこかのステーションで既に生産/消費しているウェアの場合
                    // すでにある要素の生産量に加算する
                    prod.Count += addedItem.Count;
                }

                prodBak.Add(addedItem);
            }

            // 追加対象の要素を一気に追加する
            Products.AddRange(addTarget);
        }
    }
}
