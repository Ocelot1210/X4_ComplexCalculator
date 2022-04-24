using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Modules;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

/// <summary>
/// モジュール一覧の入れ替えを行うクラス
/// </summary>
public class ModulesReorder : BindableBase
{
    #region メンバ
    /// <summary>
    /// モジュール一覧情報
    /// </summary>
    private readonly IModulesInfo _ModulesInfo;


    /// <summary>
    /// モジュール一覧(表示用)
    /// </summary>
    private readonly ListCollectionView _CollectionView;


    /// <summary>
    /// 選択数
    /// </summary>
    private int _Selection;


    /// <summary>
    /// 選択されたか
    /// </summary>
    private bool _HasSelected;


    /// <summary>
    /// ソート済み列数
    /// </summary>
    private int _SortedColumnCount;
    #endregion


    #region プロパティ
    /// <summary>
    /// モジュール選択
    /// </summary>
    public DelegateCommand SelectModulesCommand { get; }


    /// <summary>
    /// モジュール選択解除
    /// </summary>
    public DelegateCommand ClearSelectionCommand { get; }


    /// <summary>
    /// 選択項目を上に移動する
    /// </summary>
    public DelegateCommand MoveUpTheSelectionCommand { get; }


    /// <summary>
    /// 選択項目を下に移動する
    /// </summary>
    public DelegateCommand MoveDownTheSelectionCommand { get; }


    /// <summary>
    /// 選択されたか
    /// </summary>
    private bool HasSelected
    {
        get => _HasSelected;
        set
        {
            if (SetProperty(ref _HasSelected, value))
            {
                MoveUpTheSelectionCommand.RaiseCanExecuteChanged();
                MoveDownTheSelectionCommand.RaiseCanExecuteChanged();
            }
        }
    }


    /// <summary>
    /// ソート済み列数
    /// </summary>
    public int SortedColumnCount
    {
        get => _SortedColumnCount;
        set
        {
            if (SetProperty(ref _SortedColumnCount, value))
            {
                MoveUpTheSelectionCommand.RaiseCanExecuteChanged();
                MoveDownTheSelectionCommand.RaiseCanExecuteChanged();
            }
        }
    }


    /// <summary>
    /// 移動可能か(選択済みかつ、ソートされていない)
    /// </summary>
    private bool CanMode => HasSelected && 0 == SortedColumnCount;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="modulesInfo">モジュール一覧情報</param>
    public ModulesReorder(IModulesInfo modulesInfo, ListCollectionView listCollectionView)
    {
        _ModulesInfo = modulesInfo;
        _CollectionView = listCollectionView;
        SelectModulesCommand        = new DelegateCommand(SelectModules);
        ClearSelectionCommand       = new DelegateCommand(ClearSelection);
        MoveUpTheSelectionCommand   = new DelegateCommand(MoveUpTheSelection, () => CanMode);
        MoveDownTheSelectionCommand = new DelegateCommand(MoveDownTheSelection, () => CanMode);
    }



    /// <summary>
    /// 選択された項目を選択状態にする
    /// </summary>
    private void SelectModules()
    {
        var modules = Enumerable.Empty<ModulesGridItem>();
        var added = 0;

        // 選択追加モードか？(Ctrlキーが押されているか？)
        bool isAddMode = (Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down)  == KeyStates.Down ||
                         (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) == KeyStates.Down;

        // 選択追加モードか？
        if (isAddMode)
        {
            // 選択追加モードの場合、ソート対象の項目に加え、現在選択している項目を追加する
            modules = _CollectionView.OfType<ModulesGridItem>().Where(x => !x.IsReorderTarget);
        }
        else
        {
            // 選択追加モードでない場合、現在選択されているもののみソート対象にする
            modules = _CollectionView.OfType<ModulesGridItem>();
        }

        foreach (var item in modules)
        {
            item.IsReorderTarget = item.IsSelected;
            if (item.IsReorderTarget)
            {
                added++;
            }
        }

        _Selection = isAddMode ? (_Selection + added) : added;
        HasSelected = 0 < _Selection;
    }


    /// <summary>
    /// 選択解除
    /// </summary>
    private void ClearSelection()
    {
        foreach (var item in _ModulesInfo.Modules.Where(x => x.IsReorderTarget))
        {
            item.IsReorderTarget = false;
        }

        _Selection = 0;
        HasSelected = false;
    }


    /// <summary>
    /// 選択項目を上に移動
    /// </summary>
    private void MoveUpTheSelection()
    {
        // 挿入位置を取得
        var idx = GetInsertIndex();
        if (idx < 0)
        {
            return;
        }

        Move(idx);
    }


    /// <summary>
    /// 選択項目を下に移動
    /// </summary>
    private void MoveDownTheSelection()
    {
        // 挿入位置を取得
        var idx = GetInsertIndex();
        if (idx < 0)
        {
            return;
        }

        Move(idx + 1);
    }


    /// <summary>
    /// 挿入位置を取得
    /// </summary>
    /// <returns>挿入位置</returns>
    private int GetInsertIndex()
    {
        // 移動不能なら何もしない
        if (!HasSelected)
        {
            return -1;
        }

        int prev = 0;       // 挿入位置より前にある移動対象の要素数
        int ret = 0;

        var collection = _ModulesInfo.Modules.Select((x, idx) => (Module: x, Index: idx))
                                             .Where(x => x.Module.IsSelected || x.Module.IsReorderTarget);

        foreach (var (module, idx) in collection)
        {
            // 挿入位置が見つかればbreak
            if (ret == 0 && module.IsSelected)
            {
                ret = idx;
                break;
            }

            // 挿入位置より前にあればカウントアップ
            if (ret == 0 && module.IsReorderTarget)
            {
                prev++;
            }
        }

        return ret - prev;
    }


    /// <summary>
    /// 選択項目を移動
    /// </summary>
    /// <param name="insertIdx">移動先要素番号</param>
    private void Move(int insertIdx)
    {
        if (_ModulesInfo.Modules.Count - _Selection < insertIdx)
        {
            insertIdx = _ModulesInfo.Modules.Count - _Selection;
        }

        // 移動対象を退避
        var list = new List<ModulesGridItem>(_Selection);
        list.AddRange(_ModulesInfo.Modules.Where(x => x.IsReorderTarget));

        // 移動対象を削除
        _ModulesInfo.Modules.RemoveAll(x => x.IsReorderTarget);

        // 挿入位置に挿入
        _ModulesInfo.Modules.InsertRange(insertIdx, list);

        // 移動対象から除外＆編集した事にする
        foreach (var item in list)
        {
            item.EditStatus |= EditStatus.Edited;
            item.IsReorderTarget = false;
        }

        _Selection = 0;
        HasSelected = false;
    }
}
