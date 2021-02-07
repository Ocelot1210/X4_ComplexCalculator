namespace X4_ComplexCalculator.DB.X4DB.Interfaces
{
    /// <summary>
    /// マクロ名を持つウェアを表すインターフェイス
    /// </summary>
    public interface IMacro : IWare
    {
        #region プロパティ
        /// <summary>
        /// マクロ名
        /// </summary>
        public string MacroName { get; }
        #endregion
    }
}
