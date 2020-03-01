using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace X4_ComplexCalculator.Common
{
    /// <summary>
    /// データ中継用クラス
    /// </summary>
    public class BindingProxy : Freezable
    {
        #region 実装
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }
        #endregion


        /// <summary>
        /// 中継するデータ
        /// </summary>
        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }


        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(
                "Data",
                typeof(object),
                typeof(BindingProxy),
                new UIPropertyMetadata(null)
            );
    }
}
