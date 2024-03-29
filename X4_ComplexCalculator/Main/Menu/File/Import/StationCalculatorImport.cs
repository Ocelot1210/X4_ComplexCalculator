﻿using Collections.Pooled;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using X4_ComplexCalculator.Common.Dialog.MessageBoxes;
using X4_ComplexCalculator.Common.Dialog.SelectStringDialog;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Main.WorkArea;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.Menu.File.Import;

/// <summary>
/// Station Calculatorからインポートする
/// </summary>
class StationCalculatorImport : BindableBase, IImport
{
    #region メンバ
    /// <summary>
    /// メッセージボックス表示用
    /// </summary>
    private readonly ILocalizedMessageBox _messageBox;


    /// <summary>
    /// 入力されたURL
    /// </summary>
    private string _inputUrl = "";
    #endregion


    #region プロパティ
    /// <summary>
    /// メニュー表示用タイトル
    /// </summary>
    public string Title => "Lang:MainWindow_Menu_File_MenuItem_Import_MenuItem_StationCalculator_Header";


    /// <summary>
    /// Viewより呼ばれるCommand
    /// </summary>
    public ICommand Command { get; }


    /// <summary>
    /// インポート数
    /// </summary>
    public int Count { get; } = 0;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="command">Viewより呼ばれる <see cref="ICommand"/></param>
    /// <param name="messageBox">メッセージボックス表示用</param>
    public StationCalculatorImport(ICommand command, ILocalizedMessageBox messageBox)
    {
        _messageBox = messageBox;
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

        (onOK, _inputUrl) = SelectStringDialog.ShowDialog("Lang:StationCalculatorImport_Title", "Lang:StationCalculatorImport_Description");
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
            var query = _inputUrl.Split('?').Last();

            using var paramDict = new PooledDictionary<string, string>();


            var paramParser = new Regex(@"(\w+)=(.*)");
            foreach (var param in query.Split('&'))
            {
                var m = paramParser.Match(param);
                paramDict.Add(m.Groups[1].Value, m.Groups[2].Value);
            }


            var moduleParser = new Regex(@"\$module-(.*?),count:(.*)");
            var modules = paramDict["l"].Split(";,")
                                        .Select(x => moduleParser.Match(x))
                                        .Select(x => (Module: X4Database.Instance.Ware.TryGet<IX4Module>(x.Groups[1].Value), Count: long.Parse(x.Groups[2].Value)))
                                        .Where(x => x.Module is not null)
                                        .Select(x => (Module: x.Module!, x.Count))
                                        .Select(x => new ModulesGridItem(x.Module, null, x.Count) { EditStatus = EditStatus.Unedited });

            WorkArea.StationData.ModulesInfo.Modules.AddRange(modules);
            // 編集状態を全て未編集にする
            IEnumerable<IEditable>[] editables =
            {
                WorkArea.StationData.ProductsInfo.Products,
                WorkArea.StationData.BuildResourcesInfo.BuildResources,
                WorkArea.StationData.StorageAssignInfo.StorageAssign,
            };
            foreach (var editable in editables.SelectMany(x => x))
            {
                editable.EditStatus = EditStatus.Unedited;
            }

            ret = true;
        }
        catch (Exception e)
        {
            _messageBox.Error("Lang:MainWindow_ImportFailureMessage", "Lang:Common_MessageBoxTitle_Error", e.Message);
        }

        return ret;
    }
}
