using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace X4_ComplexCalculator_CustomControlLibrary.MouseUpSlider
{
    /// <summary>
    /// マウスが離された場合のみ更新するスライダー
    /// </summary>
    public class MouseUpSlider : Slider
    {
        /// <summary>
        /// ドラッグ中のバインディング退避用
        /// </summary>
        private Binding EvacuatedBinding { get; set; }

        /// <summary>
        /// ドラッグ開始時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnThumbDragStarted(DragStartedEventArgs e)
        {
            base.OnThumbDragStarted(e);

            var expr = BindingOperations.GetBindingExpression(this, ValueProperty);
            if (expr != null)
            {
                // 現在のバインド先を退避
                EvacuatedBinding = expr.ParentBinding;

                // バインド先をクリアする
                var val = Value;
                BindingOperations.ClearBinding(this, ValueProperty);
                SetValue(ValueProperty, val);
            }
        }

        /// <summary>
        /// ドラッグ終了時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnThumbDragCompleted(DragCompletedEventArgs e)
        {
            if (EvacuatedBinding != null)
            {
                var val = Value;

                // 退避したバインディングを戻す
                BindingOperations.SetBinding(this, ValueProperty, EvacuatedBinding);
                SetCurrentValue(ValueProperty, val);
                EvacuatedBinding = null;
            }

            base.OnThumbDragCompleted(e);
        }
    }
}
