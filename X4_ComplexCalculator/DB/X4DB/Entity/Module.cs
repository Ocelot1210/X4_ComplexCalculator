using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity;

/// <summary>
/// モジュール情報用クラス
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="ware">ウェア情報</param>
/// <param name="macro">マクロ名</param>
/// <param name="moduleType">モジュール種別</param>
/// <param name="maxWorkers">従業員数</param>
/// <param name="workersCapacity">最大収容人数</param>
/// <param name="noBluePrint">設計図が無いか</param>
/// <param name="products">モジュールの製品</param>
/// <param name="storage">保管庫情報</param>
/// <param name="equipments">装備一覧</param>
public sealed partial class Module(
    IWare ware,
    string macro,
    IModuleType moduleType,
    long maxWorkers,
    long workersCapacity,
    bool noBluePrint,
    IReadOnlyList<IModuleProduct> products,
    IModuleStorage storage,
    IReadOnlyDictionary<string, IWareEquipment> equipments
    ) : IX4Module
{
    /// <inheritdoc/>
    public int CompareTo(object? obj)
    {
        if (obj is not IX4Module other)
        {
            return 1;
        }

        return ID.CompareTo(other.ID);
    }


    /// <inheritdoc/>
    public override int GetHashCode() => ID.GetHashCode();


    /// <summary>
    /// 指定したコネクション名に装備可能なEquipmentを取得する
    /// </summary>
    /// <returns></returns>
    public IEnumerable<T> GetEquippableEquipment<T>(string connectionName) where T : IEquipment
    {
        if (Equipments.TryGetValue(connectionName, out var wareEquipment))
        {
            return X4Database.Instance.Ware.GetAll<T>()
                .Where(x => !x.EquipmentTags.Except(wareEquipment.Tags).Any());
        }

        return [];
    }
}
