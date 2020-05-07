using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using X4_ComplexCalculator.Common.Dialog.SelectStringDialog;
using X4_ComplexCalculator.Main.WorkArea;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.Menu.File.Import
{
    /// <summary>
    /// Station Calclatorからインポートする
    /// </summary>
    class StationCalclatorImport : IImport
    {
        /// <summary>
        /// メニュー表示用タイトル
        /// </summary>
        public string Title => "Station Calclator";


        /// <summary>
        /// Viewより呼ばれるCommand
        /// </summary>
        public ICommand Command { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="command">Viewより呼ばれるCommand</param>
        public StationCalclatorImport(ICommand command)
        {
            Command = command;
        }


        /// <summary>
        /// インポート実行
        /// </summary>
        /// <returns>インポートに成功したか</returns>
        public bool Import(IWorkArea workArea)
        {
            var ret = false;

            var (onOK, url) = SelectStringDialog.ShowDialog("Station Calclatorからインポート", "URLを入力して下さい");
            if (onOK)
            {
                try
                {
                    var query = url.Split('?').Last();

                    var paramDict = new Dictionary<string, string>();


                    var paramParser = new Regex(@"(\w+)=(.*)");
                    foreach (var param in query.Split('&'))
                    {
                        var m = paramParser.Match(param);
                        paramDict.Add(m.Groups[1].Value, m.Groups[2].Value);
                    }


                    var moduleParser = new Regex(@"\$module-(.*?),count:(.*)");
                    var modules = paramDict["l"].Split(";,").Select(x =>
                    {
                        var m = moduleParser.Match(x);

                        return new ModulesGridItem(m.Groups[1].Value, long.Parse(m.Groups[2].Value));
                    });
                    workArea.Modules.AddRange(modules);

                    ret = true;
                }
                catch(Exception e)
                {
                    MessageBox.Show($"インポートに失敗しました。\r\n■理由：\r\n{e.Message}\r\n", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return ret;
        }
    }
}
