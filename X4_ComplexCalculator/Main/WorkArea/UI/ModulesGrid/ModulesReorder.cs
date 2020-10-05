using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Modules;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid
{
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
        /// 選択数
        /// </summary>
        private int _Selection;


        /// <summary>
        /// 入れ替え可能か
        /// </summary>
        private bool _CanReorder;
        #endregion


        #region プロパティ
        /// <summary>
        /// モジュール選択コマンド
        /// </summary>
        public ICommand SelectModulesCommand { get; }


        /// <summary>
        /// モジュール選択解除コマンド
        /// </summary>
        public ICommand ClearSelectionCommand { get; }


        /// <summary>
        /// 選択項目を上に移動するコマンド
        /// </summary>
        public DelegateCommand MoveUpTheSelectionCommand { get; }


        /// <summary>
        /// 選択項目を下に移動するコマンド
        /// </summary>
        public DelegateCommand MoveDownTheSelectionCommand { get; }


        /// <summary>
        /// 入れ替え可能か
        /// </summary>
        private bool CanReorder
        {
            get => _CanReorder;
            set
            {
                if (SetProperty(ref _CanReorder, value))
                {
                    MoveUpTheSelectionCommand.RaiseCanExecuteChanged();
                    MoveDownTheSelectionCommand.RaiseCanExecuteChanged();
                }
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="modulesInfo">モジュール一覧情報</param>
        public ModulesReorder(IModulesInfo modulesInfo)
        {
            _ModulesInfo = modulesInfo;
            SelectModulesCommand        = new DelegateCommand(SelectModules);
            ClearSelectionCommand       = new DelegateCommand(ClearSelection);
            MoveUpTheSelectionCommand   = new DelegateCommand(MoveUpTheSelection, () => CanReorder);
            MoveDownTheSelectionCommand = new DelegateCommand(MoveDownTheSelection, () => CanReorder);
        }



        /// <summary>
        /// 選択された項目を選択状態にする
        /// </summary>
        private void SelectModules()
        {
            _Selection = 0;

            foreach (var item in _ModulesInfo.Modules)
            {
                item.IsReorderTarget = item.IsSelected;
                if (item.IsReorderTarget)
                {
                    _Selection++;
                }
            }

            CanReorder = 0 < _Selection;
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
            CanReorder = false;
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
            if (!CanReorder)
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
            CanReorder = false;
        }
    }
}
