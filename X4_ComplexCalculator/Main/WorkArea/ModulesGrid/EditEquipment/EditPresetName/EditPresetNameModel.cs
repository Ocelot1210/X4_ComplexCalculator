using System;
using System.Collections.Generic;
using System.Text;

namespace X4_ComplexCalculator.Main.WorkArea.ModulesGrid.EditEquipment.EditPresetName
{
    /// <summary>
    /// プリセット名編集用Model
    /// </summary>
    class EditPresetNameModel
    {

        /// <summary>
        /// 変更前プリセット名
        /// </summary>
        public string OrigPresetName { get; }


        /// <summary>
        /// 変更後プリセット名
        /// </summary>
        public string NewPresetName { set; private get; }


        /// <summary>
        /// プリセット名が有効か
        /// </summary>
        public bool IsValidPresetName => !string.IsNullOrWhiteSpace(NewPresetName);


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="origPresetName">変更前プリセット名</param>
        public EditPresetNameModel(string origPresetName)
        {
            OrigPresetName = origPresetName;
        }
    }
}
