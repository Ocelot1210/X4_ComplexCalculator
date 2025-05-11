using Collections.Pooled;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Modules;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

/// <summary>
/// モジュール一覧の入れ替えを行うクラス
/// </summary>
public sealed partial class ModulesReorder : ObservableObject
{
    #region メンバ
    /// <summary>
    /// モジュール一覧情報
    /// </summary>
    private readonly ModulesInfo _modulesInfo;


    /// <summary>
    /// モジュール一覧(表示用)
    /// </summary>
    private readonly ListCollectionView _collectionView;


    /// <summary>
    /// 選択数
    /// </summary>
    private int _selection;
    #endregion


    #region プロパティ
    /// <summary>
    /// 選択されたか
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanMove))]
    [NotifyCanExecuteChangedFor(nameof(MoveUpTheSelectionCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveDownTheSelectionCommand))]
    public partial bool HasSelected { get; private set; }


    /// <summary>
    /// ソート済み列数
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanMove))]
    [NotifyCanExecuteChangedFor(nameof(MoveUpTheSelectionCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveDownTheSelectionCommand))]
    public partial int SortedColumnCount { get; set; }


    /// <summary>
    /// 移動可能か(選択済みかつ、ソートされていない)
    /// </summary>
    private bool CanMove => HasSelected && 0 == SortedColumnCount;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="modulesInfo">モジュール一覧情報</param>
    public ModulesReorder(ModulesInfo modulesInfo, ListCollectionView listCollectionView)
    {
        _modulesInfo = modulesInfo;
        _collectionView = listCollectionView;
    }


    /// <summary>
    /// 選択された項目を選択状態にする
    /// </summary>
    [RelayCommand]
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
            modules = _collectionView.OfType<ModulesGridItem>().Where(x => !x.IsReorderTarget);
        }
        else
        {
            // 選択追加モードでない場合、現在選択されているもののみソート対象にする
            modules = _collectionView.OfType<ModulesGridItem>();
        }

        foreach (var item in modules)
        {
            item.IsReorderTarget = item.IsSelected;
            if (item.IsReorderTarget)
            {
                added++;
            }
        }

        _selection = isAddMode ? (_selection + added) : added;
        HasSelected = 0 < _selection;
    }


    /// <summary>
    /// 選択解除
    /// </summary>
    [RelayCommand]
    private void ClearSelection()
    {
        foreach (var item in _modulesInfo.Modules.Where(x => x.IsReorderTarget))
        {
            item.IsReorderTarget = false;
        }

        _selection = 0;
        HasSelected = false;
    }


    /// <summary>
    /// 選択項目を上に移動
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanMove))]
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
    [RelayCommand(CanExecute = nameof(CanMove))]
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

        var collection = _modulesInfo.Modules
            .Select((x, idx) => (Module: x, Index: idx))
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
        if (_modulesInfo.Modules.Count - _selection < insertIdx)
        {
            insertIdx = _modulesInfo.Modules.Count - _selection;
        }

        // 移動対象を退避
        using var list = new PooledList<ModulesGridItem>(_selection);
        list.AddRange(_modulesInfo.Modules.Where(x => x.IsReorderTarget));

        // 移動対象を削除
        _modulesInfo.Modules.RemoveAll(x => x.IsReorderTarget);

        // 挿入位置に挿入
        _modulesInfo.Modules.InsertRange(insertIdx, list);

        // 移動対象から除外＆編集した事にする
        foreach (var item in list)
        {
            item.EditStatus |= EditStatus.Edited;
            item.IsReorderTarget = false;
        }

        _selection = 0;
        HasSelected = false;
    }
}
