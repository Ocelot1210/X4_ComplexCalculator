﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using X4_ComplexCalculator.Common.Dialog.SelectStringDialog;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main.Menu.File.Export
{
    /// <summary>
    /// StationCalclator向けにステーションの情報をエクスポートする
    /// </summary>
    class StationCalclatorExport : IExport
    {
        /// <summary>
        /// タイトル文字列
        /// </summary>
        public string Title => "Station Calclator";

        /// <summary>
        /// Viewより呼ばれるコマンド
        /// </summary>
        public ICommand Command { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="command">Viewより呼ばれるCommand</param>
        public StationCalclatorExport(ICommand command)
        {
            Command = command;
        }


        /// <summary>
        /// エクスポート処理
        /// </summary>
        /// <param name="workArea"></param>
        /// <returns></returns>
        public bool Export(IWorkArea workArea)
        {
            var sb = new StringBuilder();

            sb.Append(@"http://www.x4-game.com/#/station-calculator?");

            // モジュール情報を追加
            sb.Append("l=@");
            foreach (var module in workArea.Modules)
            {
                sb.Append($"$module-{module.Module.ModuleID},count:{module.ModuleCount};,");
            }
            sb.Length -= 2;

            SelectStringDialog.ShowDialog("Station Calclatorへエクスポート", "URLをコピーして下さい", sb.ToString(), hideCancelButton:true);

            return true;
        }
    }
}