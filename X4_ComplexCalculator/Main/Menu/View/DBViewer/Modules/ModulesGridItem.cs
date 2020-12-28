using Prism.Mvvm;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.Menu.View.DBViewer.Modules
{
    class ModulesGridItem : BindableBase
    {
        #region メンバ
        /// <summary>
        /// 表示対象モジュール
        /// </summary>
        private readonly Module _Module;

        /// <summary>
        /// 製品
        /// </summary>
        private readonly Ware? _Product;
        #endregion


        #region プロパティ
        /// <summary>
        /// モジュール名
        /// </summary>
        public string ModuleName => _Module.Name;


        /// <summary>
        /// モジュール種別
        /// </summary>
        public string ModuleType => _Module.ModuleType.Name;


        /// <summary>
        /// 所有派閥
        /// </summary>
        public string Race { get; }


        /// <summary>
        /// 従業員数
        /// </summary>
        public long MaxWorkers => _Module.MaxWorkers;


        /// <summary>
        /// 収容人数
        /// </summary>
        public long WorkersCapacity => _Module.WorkersCapacity;


        /// <summary>
        /// 製品
        /// </summary>
        public string Product => _Product?.Name ?? "";


        /// <summary>
        /// 最大効率
        /// </summary>
        public long MaxEfficiency { get; }
        #endregion



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="module">表示対象モジュール</param>
        public ModulesGridItem(Module module)
        {
            _Module = module;
            
            // 所有派閥を設定(ある種族固有の場合のみ表示)
            var ownerRaces = _Module.Owners.Select(x => x.Race.Name).OrderBy(x => x).Distinct().ToArray();
            Race = (ownerRaces.Length == 1) ? ownerRaces[0] : "";


            // 製品がある場合、製品の情報を設定
            if (_Module.ModuleProduct is not null)
            {
                _Product = Ware.Get(_Module.ModuleProduct.WareID);
                MaxEfficiency = (long)((WareEffect.Get(_Product.WareID, _Module.ModuleProduct.Method, "work")!.Product + 1) * 100);
            }
        }
    }
}
