﻿using System;
using System.Collections.Generic;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB.X4DB;
using System.Linq;  

namespace X4_ComplexCalculator.Main.ProductsGrid
{
    /// <summary>
    /// 製品一覧を表示するDataGridViewの1レコード分用クラス
    /// </summary>
    public class ProductsGridItem : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// 金額
        /// </summary>
        private long _Price;

        /// <summary>
        /// 単価
        /// </summary>
        private long _UnitPrice;

        /// <summary>
        /// Expanderが展開されているか
        /// </summary>
        private bool _IsExpanded;
        #endregion


        #region プロパティ
        /// <summary>
        /// 製品
        /// </summary>
        public Ware Ware { get; }


        /// <summary>
        /// ウェアの個数
        /// </summary>
        public long Count { get; }


        /// <summary>
        /// 損しているか
        /// </summary>
        public bool IsLosing => Count < 0;


        /// <summary>
        /// 金額
        /// </summary>
        public long Price
        {
            get
            {
                return _Price;
            }
            set
            {
                UnitPrice = (long)Math.Round((double)value / Count);

                var price = UnitPrice * Count;
                if (_Price != price)
                {
                    _Price = price;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 単価
        /// </summary>
        public long UnitPrice
        {
            get
            {
                return _UnitPrice;
            }
            set
            {
                // 最低価格≦ 入力価格 ≦ 最高価格かつ価格が変更された場合のみ更新

                // 変更無しの場合は何もしない
                if (_UnitPrice == value)
                {
                    return;
                }

                
                if (value < Ware.MinPrice)
                {
                    // 入力された値が最低価格未満の場合、最低価格を設定する
                    _UnitPrice = Ware.MinPrice;
                }
                else if (Ware.MaxPrice < value)
                {
                    // 入力された値が最高価格を超える場合、最高価格を設定する
                    _UnitPrice = Ware.MaxPrice;
                }
                else
                {
                    // 入力された値が最低価格以上、最高価格以下の場合、入力された値を設定する
                    _UnitPrice = value;
                }
                OnPropertyChanged();

                Price = _UnitPrice * Count;
            }
        }

        /// <summary>
        /// ウェア詳細(関連モジュール等)
        /// </summary>
        public IReadOnlyCollection<ProductDetailsListItem> Details { get; }


        /// <summary>
        /// 選択されているか
        /// </summary>
        public bool IsSelected { get; set; }


        /// <summary>
        /// Expanderが展開されているか
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return _IsExpanded;
            }
            set
            {
                _IsExpanded = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// 百分率ベースで価格を設定する
        /// </summary>
        /// <param name="percent">百分率の値</param>
        public void SetUnitPricePercent(long percent)
        {
            UnitPrice = (long)(Ware.MinPrice + (Ware.MaxPrice - Ware.MinPrice) * 0.01 * percent);
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="count">ウェア個数</param>
        /// <param name="datails">ウェア詳細(関連モジュール等)</param>
        /// <param name="isExpanded">関連モジュールが展開されているか</param>
        /// <param name="price">価格</param>
        public ProductsGridItem(string wareID, long count, IEnumerable<ProductDetailsListItem> datails, bool isExpanded = false, long price = 0)
        {
            Ware = new Ware(wareID);
            Count = count;
            _IsExpanded = isExpanded;
            UnitPrice = (price != 0)? price : (Ware.MinPrice + Ware.MaxPrice) / 2;
            Price = Count * UnitPrice;
            Details = datails.ToArray();
        }
    }
}
