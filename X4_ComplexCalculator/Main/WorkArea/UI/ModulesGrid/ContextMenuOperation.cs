﻿using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Linq;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Modules;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

/// <summary>
/// モジュール一覧タブのコンテキストメニューの処理用クラス
/// </summary>
public sealed class ContextMenuOperation : BindableBase, IDisposable
{
    #region メンバ
    /// <summary>
    /// モジュール一覧
    /// </summary>
    private readonly IModulesInfo _modulesInfo;


    /// <summary>
    /// モジュール一覧(表示用)
    /// </summary>
    private readonly ListCollectionView _collectionView;


    /// <summary>
    /// モジュール並び替え用
    /// </summary>
    private readonly ModulesReorder _moduleReorder;
    #endregion


    #region プロパティ
    /// <summary>
    /// モジュールをコピー
    /// </summary>
    public ICommand CopyModulesCommand { get; }


    /// <summary>
    /// モジュールを貼り付け
    /// </summary>
    public ICommand PasteModulesCommand { get; }


    /// <summary>
    /// モジュールを削除
    /// </summary>
    public ICommand DeleteModulesCommand { get; }

    // ---------------------------------------------------------------------- //

    /// <summary>
    /// モジュール選択
    /// </summary>
    public ICommand SelectModulesCommand => _moduleReorder.SelectModulesCommand;


    /// <summary>
    /// モジュール選択解除
    /// </summary>
    public ICommand ClearSelectionCommand => _moduleReorder.ClearSelectionCommand;


    /// <summary>
    /// 選択項目を上に移動する
    /// </summary>
    public ICommand MoveUpTheSelectionCommand => _moduleReorder.MoveUpTheSelectionCommand;


    /// <summary>
    /// 選択項目を下に移動する
    /// </summary>
    public ICommand MoveDownTheSelectionCommand => _moduleReorder.MoveDownTheSelectionCommand;


    // ---------------------------------------------------------------------- //

    /// <summary>
    /// ソート順を初期化
    /// </summary>
    public DelegateCommand ResetSortOrderCommand { get; }

    // ---------------------------------------------------------------------- //

    /// <summary>
    /// セルフォーカス用のコマンド
    /// </summary>
    public ICommand? CellFocusCommand { private get; set; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="modulesInfo">モジュール一覧情報</param>
    /// <param name="listCollectionView">モジュール一覧のCollectionView</param>
    public ContextMenuOperation(IModulesInfo modulesInfo, ListCollectionView listCollectionView)
    {
        _modulesInfo    = modulesInfo;
        _collectionView = listCollectionView;
        _moduleReorder  = new ModulesReorder(modulesInfo, listCollectionView);

        ((INotifyCollectionChanged)_collectionView.SortDescriptions).CollectionChanged += ContextMenuOperation_CollectionChanged;

        CopyModulesCommand    = new DelegateCommand(CopyModules);
        PasteModulesCommand   = new DelegateCommand<DataGrid>(PasteModules);
        DeleteModulesCommand  = new DelegateCommand<DataGrid>(DeleteModules);
        ResetSortOrderCommand = new DelegateCommand(
            () => _collectionView.SortDescriptions.Clear(),
            () => 0 < _collectionView.SortDescriptions.Count
        );
    }


    /// <inheritdoc/>
    public void Dispose()
    {
        ((INotifyCollectionChanged)_collectionView.SortDescriptions).CollectionChanged -= ContextMenuOperation_CollectionChanged;
    }


    /// <summary>
    /// ソート内容が変更された場合
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ContextMenuOperation_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // 未ソートの場合のみ入れ替え可能にする
        _moduleReorder.SortedColumnCount = _collectionView.SortDescriptions.Count;

        // ソート順初期化可不可更新
        ResetSortOrderCommand.RaiseCanExecuteChanged();
    }


    /// <summary>
    /// 選択中のモジュールをコピー
    /// </summary>
    private void CopyModules()
    {
        var xml = new XElement("modules");
        var selectedModules = CollectionViewSource.GetDefaultView(_collectionView)
                                                  .Cast<ModulesGridItem>()
                                                  .Where(x => x.IsSelected);

        foreach (var module in selectedModules)
        {
            xml.Add(module.ToXml());
        }

        Clipboard.SetText(xml.ToString());
    }


    /// <summary>
    /// コピーしたモジュールを貼り付け
    /// </summary>
    /// <param name="dataGrid"></param>
    private void PasteModules(DataGrid dataGrid)
    {
        try
        {
            var clipboardXml = XDocument.Parse(Clipboard.GetText());
            if (clipboardXml.Root is null) return;

            // xmlの内容に問題がないか確認するため、ここでToArray()する
            var modules = clipboardXml.Root.Elements().Select(x => new ModulesGridItem(x) { EditStatus = EditStatus.Edited }).ToArray();

            _modulesInfo.Modules.AddRange(modules);

            dataGrid.Focus();
        }
        catch
        {

        }
    }


    /// <summary>
    /// 選択中のモジュールを削除
    /// </summary>
    /// <param name="dataGrid"></param>
    private void DeleteModules(DataGrid dataGrid)
    {
        var currPos = _collectionView.CurrentPosition;

        // モジュール数を編集した後に削除するとcurrPosが-1になる場合があるため、
        // ここで最初に選択されている表示上のモジュールの要素番号を取得する
        if (currPos == -1)
        {
            var cnt = 0;
            foreach (var module in _collectionView.OfType<ModulesGridItem>())
            {
                if (module.IsSelected)
                {
                    currPos = cnt;
                    break;
                }
                cnt++;
            }
        }

        var items = CollectionViewSource.GetDefaultView(_collectionView)
                                        .Cast<ModulesGridItem>()
                                        .Where(x => x.IsSelected);

        _modulesInfo.Modules.RemoveRange(items);

        // 削除後に全部の選択状態を外さないと余計なものまで選択される
        foreach (var module in _modulesInfo.Modules)
        {
            module.IsSelected = false;
        }

        // 選択行を設定
        if (currPos < 0)
        {
            // 先頭行を削除した場合
            _collectionView.MoveCurrentToFirst();
        }
        else if (_collectionView.Count <= currPos)
        {
            // 最後の行を消した場合、選択行を最後にする
            _collectionView.MoveCurrentToLast();
        }
        else
        {
            // 中間行の場合
            _collectionView.MoveCurrentToPosition(currPos);
        }

        // 再度選択
        if (_collectionView.CurrentItem is ModulesGridItem item)
        {
            item.IsSelected = true;
        }

        // セルフォーカス
        if (dataGrid.CurrentCell.Column is not null)
        {
            CellFocusCommand?.Execute(new Tuple<DataGrid, int, int>(dataGrid, _collectionView.CurrentPosition, dataGrid.CurrentCell.Column.DisplayIndex));
        }
    }
}
