﻿using X4_ComplexCalculator.Main.Menu.File.Export;
using X4_ComplexCalculator.Main.Menu.File.Import;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main;

/// <summary>
/// インポート/エクスポート処理用
/// </summary>
class ImportExporter
{
    #region メンバ
    /// <summary>
    /// 作業エリア管理用
    /// </summary>
    private readonly WorkAreaManager _workAreaManager;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="workAreaManager">作業エリア管理用</param>
    public ImportExporter(WorkAreaManager workAreaManager)
    {
        _workAreaManager = workAreaManager;
    }


    /// <summary>
    /// インポート実行
    /// </summary>
    /// <param name="import"></param>
    public void Import(IImport import)
    {
        // インポート対象数を取得
        var importCnt = import.Select();

        for (var cnt = 0; cnt < importCnt; cnt++)
        {
            var vm = new WorkAreaViewModel(_workAreaManager.ActiveLayoutID);

            if (vm.Import(import))
            {
                _workAreaManager.Documents.Add(vm);
            }
            else
            {
                vm.Dispose();
            }
        }
    }


    /// <summary>
    /// エクスポート実行
    /// </summary>
    /// <param name="export"></param>
    public void Export(IExport export)
    {
        _workAreaManager.ActiveContent?.Export(export);
    }
}
