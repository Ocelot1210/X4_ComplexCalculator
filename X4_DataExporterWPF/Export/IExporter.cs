using System;
using System.Data;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// Export処理用インターフェイス
    /// </summary>
    interface IExporter
    {
        /// <summary>
        /// エクスポート処理
        /// </summary>
        /// <param name="connection"></param>
        void Export(IDbConnection connection, IProgress<(int currentStep, int maxSteps)> progless);
    }
}
