using System.Collections.Generic;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity;

/// <summary>
/// モジュールの保管庫情報用クラス
/// </summary>
/// <param name="ID">モジュールID</param>
/// <param name="Amount">容量</param>
/// <param name="Types">保管庫種別一覧</param>
public sealed record ModuleStorage(
    string ID,
    long Amount,
    HashSet<ITransportType> Types
) : IModuleStorage;
