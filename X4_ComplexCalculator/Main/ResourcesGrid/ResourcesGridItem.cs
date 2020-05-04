using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.ResourcesGrid
{
    /// <summary>
    /// 建造に必要なウェアを表示するDataGridViewの1レコード分のクラス
    /// </summary>
    public class ResourcesGridItem : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// 単価
        /// </summary>
        private long _UnitPrice;

        /// <summary>
        /// 詳細情報
        /// </summary>
        private List<ResourcesGridDetailsItem> _Details;
        #endregion


        #region プロパティ
        /// <summary>
        /// 建造に必要なウェア
        /// </summary>
        public Ware Ware { get; private set; }


        /// <summary>
        /// 建造に必要なウェア数量
        /// </summary>
        public long Amount
        {
            get => _Details.Sum(x => x.TotalAmount);
        }


        /// <summary>
        /// 金額
        /// </summary>
        public long Price => Amount * UnitPrice;


        /// <summary>
        /// 単価
        /// </summary>
        public long UnitPrice
        {
            get => _UnitPrice;
            set
            {
                // 最低価格≦ 入力価格 ≦ 最高価格かつ価格が変更された場合のみ更新
                if (_UnitPrice == value)
                {
                    // 変更無しの場合は何もしない
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
                OnPropertyChanged(nameof(Price));
            }
        }

        /// <summary>
        /// 選択されたか
        /// </summary>
        public bool IsSelected { get; set; }


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
        /// <param name="wareID">建造に必要なウェアID</param>
        /// <param name="details">詳細情報</param>
        public ResourcesGridItem(string wareID, IEnumerable<ResourcesGridDetailsItem> details)
        {
            Ware = new Ware(wareID);
            _Details = new List<ResourcesGridDetailsItem>(details);
            UnitPrice = (Ware.MaxPrice + Ware.MinPrice) / 2;
        }


        /// <summary>
        /// 詳細情報を追加
        /// </summary>
        /// <param name="details">追加対象</param>
        public void AddDetails(IEnumerable<ResourcesGridDetailsItem> details)
        {
            var addItems = new List<ResourcesGridDetailsItem>();

            foreach (var item in details)
            {
                var tmp = _Details.Where(x => x.ID == item.ID).FirstOrDefault();
                if (tmp != null)
                {
                    // 既にレコードがある場合
                    tmp.Count += item.Count;
                }
                else
                {
                    // 新規追加
                    addItems.Add(item);
                }
            }

            _Details.AddRange(addItems);

            OnPropertyChanged(nameof(Amount));
            OnPropertyChanged(nameof(Price));
        }


        /// <summary>
        /// 詳細情報を削除
        /// </summary>
        /// <param name="details">削除対象</param>
        public void RemoveDetails(IEnumerable<ResourcesGridDetailsItem> details)
        {
            foreach (var item in details)
            {
                var tmp = _Details.Where(x => x.ID == item.ID).FirstOrDefault();
                if (tmp != null)
                {
                    // 既にレコードがある場合
                    tmp.Count -= item.Count;
                }
            }

            // 空のレコードを削除
            _Details.RemoveAll(x => x.Count == 0);

            OnPropertyChanged(nameof(Amount));
            OnPropertyChanged(nameof(Price));
        }


        /// <summary>
        /// 詳細情報を設定
        /// </summary>
        /// <param name="details">設定対象</param>
        public void SetDetails(IEnumerable<ResourcesGridDetailsItem> details)
        {
            foreach (var item in details)
            {
                var tmp = _Details.Where(x => x.ID == item.ID).FirstOrDefault();
                if (tmp != null)
                {
                    tmp.Count = item.Count;
                }
            }

            OnPropertyChanged(nameof(Amount));
            OnPropertyChanged(nameof(Price));
        }
    }
}
