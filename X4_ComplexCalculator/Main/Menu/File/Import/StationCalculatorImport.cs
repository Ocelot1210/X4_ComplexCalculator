using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Prism.Mvvm;
using X4_ComplexCalculator.Common.Dialog.SelectStringDialog;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.WorkArea;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.Menu.File.Import
{
    /// <summary>
    /// Station Calculatorからインポートする
    /// </summary>
    class StationCalculatorImport : BindableBase, IImport
    {
        #region メンバ
        /// <summary>
        /// 入力されたURL
        /// </summary>
        private string _InputUrl = "";
        #endregion


        #region プロパティ
        /// <summary>
        /// メニュー表示用タイトル
        /// </summary>
        public string Title => "Station Calculator";


        /// <summary>
        /// Viewより呼ばれるCommand
        /// </summary>
        public ICommand Command { get; }


        /// <summary>
        /// インポート数
        /// </summary>
        public int Count { get; private set; } = 0;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="command">Viewより呼ばれるCommand</param>
        public StationCalculatorImport(ICommand command)
        {
            Command = command;
        }


        /// <summary>
        /// インポート対象を選択
        /// </summary>
        /// <returns>インポート対象数</returns>
        public int Select()
        {
            var ret = 0;
            var onOK = false;

            (onOK, _InputUrl) = SelectStringDialog.ShowDialog("Lang:StationCalculatorImportTitle", "Lang:StationCalculatorImportDescription");
            if (onOK)
            {
                ret = 1;
            }

            return ret;
        }


        /// <summary>
        /// インポート実行
        /// </summary>
        /// <param name="WorkArea">作業エリア</param>
        /// <returns>インポートに成功したか</returns>
        public bool Import(IWorkArea WorkArea)
        {
            var ret = false;

            try
            {
                var query = _InputUrl.Split('?').Last();

                var paramDict = new Dictionary<string, string>();


                var paramParser = new Regex(@"(\w+)=(.*)");
                foreach (var param in query.Split('&'))
                {
                    var m = paramParser.Match(param);
                    paramDict.Add(m.Groups[1].Value, m.Groups[2].Value);
                }


                var moduleParser = new Regex(@"\$module-(.*?),count:(.*)");
                var modules = paramDict["l"].Split(";,")
                                            .Select(x => moduleParser.Match(x))
                                            .Select(x => (Module: Module.Get(x.Groups[1].Value), Count: long.Parse(x.Groups[2].Value)))
                                            .Where(x => x.Module != null)
                                            .Select(x => (Module: x.Module!, x.Count))
                                            .Select(x => new ModulesGridItem(x.Module, null, x.Count) { EditStatus = EditStatus.Unedited });

                WorkArea.StationData.ModulesInfo.Modules.AddRange(modules);
                // 編集状態を全て未編集にする
                IEnumerable<IEditable>[] editables = { WorkArea.StationData.ProductsInfo.Products, WorkArea.StationData.BuildResourcesInfo.BuildResources, WorkArea.StationData.StorageAssignInfo.StorageAssign };
                foreach (var editable in editables.SelectMany(x => x))
                {
                    editable.EditStatus = EditStatus.Unedited;
                }

                ret = true;
            }
            catch (Exception e)
            {
                LocalizedMessageBox.Show("Lang:ImportFailureMessage", "Lang:Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, e.Message);
            }

            return ret;
        }
    }
}
