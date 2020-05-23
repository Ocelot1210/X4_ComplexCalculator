using Prism.Mvvm;
using System;
using System.ComponentModel;
using System.Windows.Data;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Providers;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StorageAssign
{
    /// <summary>
    /// 保管庫割当用ViewModel
    /// </summary>
    class StorageAssignViewModel : BindableBase, IDisposable
    {
        #region メンバ
        /// <summary>
        /// 保管庫割当用Model
        /// </summary>
        readonly StorageAssignModel _Model;

        /// <summary>
        /// n時間後の保管庫状態書式文字列
        /// </summary>
        private string _StorageConditionAfterHourFormat;
        #endregion

        #region プロパティ
        /// <summary>
        /// 保管庫割当情報
        /// </summary>
        public ListCollectionView StorageAssignInfo { get; }


        /// <summary>
        /// n時間後の保管庫状態文字列
        /// </summary>
        public string StorageConditionAfterHour => string.Format(_StorageConditionAfterHourFormat, Hour);


        /// <summary>
        /// 指定時間
        /// </summary>
        public long Hour
        {
            get => _Model.Hour;
            set
            {
                _Model.Hour = value;
                RaisePropertyChanged(nameof(StorageConditionAfterHour));
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="model">保管庫割当Model</param>
        public StorageAssignViewModel(StorageAssignModel model)
        {
            _Model = model;

            StorageAssignInfo = (ListCollectionView)CollectionViewSource.GetDefaultView(_Model.StorageAssignGridItems);
            StorageAssignInfo.SortDescriptions.Clear();
            StorageAssignInfo.SortDescriptions.Add(new SortDescription(nameof(StorageAssignGridItem.Tier), ListSortDirection.Ascending));
            StorageAssignInfo.SortDescriptions.Add(new SortDescription(nameof(StorageAssignGridItem.WareName), ListSortDirection.Ascending));

            StorageAssignInfo.GroupDescriptions.Clear();
            StorageAssignInfo.GroupDescriptions.Add(new PropertyGroupDescription(nameof(StorageAssignGridItem.TransportTypeName)));

            LocalizeDictionary.Instance.PropertyChanged += Instance_PropertyChanged;
            _StorageConditionAfterHourFormat = (string)LocalizeDictionary.Instance.GetLocalizedObject("Lang:StorageConditionAfterHour", null, null);
        }


        /// <summary>
        /// 言語変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LocalizeDictionary.Instance.Culture))
            {
                _StorageConditionAfterHourFormat = (string)LocalizeDictionary.Instance.GetLocalizedObject("Lang:StorageConditionAfterHour", null, null);
                RaisePropertyChanged(nameof(StorageConditionAfterHour));
            }
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            _Model.Dispose();
            LocalizeDictionary.Instance.PropertyChanged -= Instance_PropertyChanged;
        }
    }
}
