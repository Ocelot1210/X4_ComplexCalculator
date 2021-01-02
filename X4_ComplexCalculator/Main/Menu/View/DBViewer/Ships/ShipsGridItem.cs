using Prism.Mvvm;
using System.Windows.Data;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB.X4DB;
using System.ComponentModel;
using System.Linq;


namespace X4_ComplexCalculator.Main.Menu.View.DBViewer.Ships
{
    class ShipsGridItem : BindableBase
    {
        #region プロパティ
        /// <summary>
        /// 艦船情報
        /// </summary>
        public Ship Ship { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ship">艦船情報</param>
        public ShipsGridItem(Ship ship)
        {
            Ship = ship;
        }
    }
}
