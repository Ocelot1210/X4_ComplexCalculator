using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StationSummary.WorkForce.NeedWareInfo
{
    /// <summary>
    /// 必要ウェア情報
    /// </summary>
    class NeedWareInfoModel : BindableBase, IDisposable
    {
        #region メンバ
        /// <summary>
        /// 製品情報
        /// </summary>
        private readonly ObservablePropertyChangedCollection<ProductsGridItem> Products;


        /// <summary>
        /// 労働者数
        /// </summary>
        private long _WorkersCount;


        /// <summary>
        /// 集計対象ウェア数量ディクショナリ
        /// </summary>
        /// <remarks>
        /// Key   = ウェアID
        /// Value = 生産数
        /// 
        /// イベントの発火順番が前後する事を考慮したいため、集計対象のウェアの情報をここに格納しておく
        /// </remarks>
        private readonly Dictionary<string, long> AggregateTargetProducts = new Dictionary<string, long>();
        #endregion


        #region プロパティ
        /// <summary>
        /// 必要ウェア情報詳細
        /// </summary>
        public ObservableRangeCollection<NeedWareInfoDetailsItem> NeedWareInfoDetails { get; } = new ObservableRangeCollection<NeedWareInfoDetailsItem>();


        /// <summary>
        /// 必要労働者数
        /// </summary>
        public long WorkersCount
        {
            get => _WorkersCount;
            set
            {
                if (SetProperty(ref _WorkersCount, value))
                {
                    OnWorkersChanged();
                }
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="products">製品一覧</param>
        public NeedWareInfoModel(ObservablePropertyChangedCollection<ProductsGridItem> products)
        {
            Products = products;
            Products.CollectionChanged += Products_CollectionChanged;
            Products.CollectionPropertyChanged += Products_CollectionPropertyChanged;

            var query = @"
SELECT
	DISTINCT WareID
	
FROM
	WorkUnitResource
	
WHERE
	WorkUnitID = 'workunit_busy'";

            // 集計対象ウェアを取得
            DBConnection.X4DB.ExecQuery(query, (dr, _) =>
            {
                AggregateTargetProducts.Add((string)dr["WareID"], 0);
            });
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            Products.CollectionChanged -= Products_CollectionChanged;
            Products.CollectionPropertyChanged -= Products_CollectionPropertyChanged;
        }

        
        /// <summary>
        /// 労働者数に変更があった場合
        /// </summary>
        private void OnWorkersChanged()
        {
            var query = @$"
SELECT
	ifnull(Race.Name, (SELECT Name FROM Race WHERE RaceID = 'argon')) AS Method,
	Ware.WareID,
	Ware.Name,
	CAST(((CAST({WorkersCount} AS REAL) / WorkUnitProduction.Amount) * WorkUnitResource.Amount + 0.5) AS INTEGER) AS Amount
	
FROM
	Ware,
	WorkUnitProduction,
	WorkUnitResource
	
	LEFT JOIN Race
		ON Race.RaceID = WorkUnitResource.Method
	
WHERE
	WorkUnitProduction.WorkUnitID = WorkUnitResource.WorkUnitID AND
	WorkUnitProduction.WorkUnitID = 'workunit_busy' AND
	WorkUnitProduction.Method     = WorkUnitResource.Method AND
	WorkUnitResource.WareID       = Ware.WareID";

            var addItems = new List<NeedWareInfoDetailsItem>();

            DBConnection.X4DB.ExecQuery(query, (dr, args) =>
            {
                var method = (string)dr["Method"];
                var wareID = (string)dr["WareID"];

                var item = NeedWareInfoDetails.Where(x => x.Method == method && x.WareID == wareID).FirstOrDefault();
                if (item != null)
                {
                    // 既にウェアがある場合
                    item.NeedAmount = (long)dr["Amount"];
                    item.ProductionAmount = AggregateTargetProducts[wareID];
                }
                else
                {
                    // ウェアが無い場合
                    addItems.Add(new NeedWareInfoDetailsItem(method, wareID, (string)dr["Name"], (long)dr["Amount"], AggregateTargetProducts[wareID]));
                }
            });

            NeedWareInfoDetails.AddRange(addItems);
        }



        /// <summary>
        /// 製品のプロパティ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Products_CollectionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // ウェア数量以外の変更なら何もしない
            if (e.PropertyName != nameof(ProductsGridItem.Count))
            {
                return;
            }

            // キャストに失敗したら何もしない
            if (!(sender is ProductsGridItem product))
            {
                return;
            }


            // 同じウェアの数量更新
            var keys = AggregateTargetProducts.Keys.ToArray();
            foreach (var wareID in keys.Where(x => x == product.Ware.WareID))
            {
                var amount = product.Details.Where(x => 0 < x.Amount).Sum(x => x.Amount);
                AggregateTargetProducts[wareID] = amount;

                // 現在の列にあればそちらを使用
                foreach (var itm in NeedWareInfoDetails.Where(x => x.WareID == wareID))
                {
                    itm.ProductionAmount = amount;
                }
            }
        }


        /// <summary>
        /// 製品一覧変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Products_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            System.Collections.IList[] items = { e.NewItems, e.OldItems };

            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }


                foreach (var prod in item.Cast<ProductsGridItem>().Where(x => AggregateTargetProducts.ContainsKey(x.Ware.WareID)))
                {
                    var amount = prod.Details.Where(x => 0 < x.Amount).Sum(x => x.Amount);
                    AggregateTargetProducts[prod.Ware.WareID] = amount;

                    var ware = NeedWareInfoDetails.Where(x => x.WareID == prod.Ware.WareID).FirstOrDefault();
                    if (ware != null)
                    {
                        ware.ProductionAmount = amount;
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                NeedWareInfoDetails.Clear();

                foreach (var key in AggregateTargetProducts.Keys.ToArray())
                {
                    AggregateTargetProducts[key] = 0;
                }
            }
        }
    }
}
