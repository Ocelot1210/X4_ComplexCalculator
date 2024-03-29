﻿using Collections.Pooled;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Entity;

/// <summary>
/// ウェアの装備品管理クラス
/// </summary>
public class EquippableWareEquipmentManager : INotifyCollectionChanged
{
    #region メンバ
    /// <summary>
    /// グループ名と接続名をキーにした装備中の項目一覧
    /// </summary>
    private readonly Dictionary<IWareEquipment, IEquipment> _equipped;
    #endregion


    #region プロパティ
    /// <summary>
    /// 編集対象のウェア
    /// </summary>
    public IEquippableWare Ware { get; }


    /// <summary>
    /// 装備中の装備一覧
    /// </summary>
    public IEnumerable<IEquipment> AllEquipments => _equipped.Values;


    /// <summary>
    /// 装備を持てるか
    /// </summary>
    public bool CanEquipped => Ware.Equipments.Any();
    #endregion


    #region イベント
    /// <inheritdoc/>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="ware">ウェア</param>
    public EquippableWareEquipmentManager(IEquippableWare ware)
    {
        Ware = ware;
        _equipped = new();
    }


    /// <summary>
    /// コピーコンストラクタ
    /// </summary>
    /// <param name="manager">コピー元インスタンス</param>
    public EquippableWareEquipmentManager(EquippableWareEquipmentManager manager)
    {
        Ware = manager.Ware;
        _equipped = manager._equipped.ToDictionary(x => x.Key, x => x.Value);
    }


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="ware">管理対象のウェア</param>
    /// <param name="element">シリアライズされたXElement</param>
    public EquippableWareEquipmentManager(IEquippableWare ware, XElement? element) : this(ware)
    {
        if (element is null) return;

        try
        {
            foreach (var elm in element.Elements())
            {
                var conn = elm.Attribute("connection")?.Value;
                var group = elm.Attribute("group")?.Value;
                var id = elm.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(conn) || string.IsNullOrEmpty(group) || string.IsNullOrEmpty(id))
                {
                    continue;
                }

                var wareEquipment = Ware.Equipments.Values
                    .FirstOrDefault(x => x.GroupName == group && x.ConnectionName == conn);

                if (wareEquipment is not null)
                {
                    _equipped.Add(wareEquipment, X4Database.Instance.Ware.Get<IEquipment>(id));
                }
            }
        }
        catch
        {
            throw new ArgumentException("Invalid xml data.", nameof(element));
        }
    }


    /// <summary>
    /// 装備をリセットする
    /// </summary>
    /// <param name="equipments"></param>
    public void ResetEquipment(IEnumerable<IEquipment> equipments)
    {
        _equipped.Clear();

        foreach (var equipment in equipments)
        {
            AddEquipmentInternal(equipment);
        }

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }


    /// <summary>
    /// 装備を削除する
    /// </summary>
    /// <param name="equipments"></param>
    public void RemoveRange(IEnumerable<IEquipment> equipments)
    {
        using var removed = CollectionChanged is null ? null : new PooledList<IEquipment>();

        foreach (var equipment in equipments)
        {
            var key = _equipped.Where(x => x.Value == equipment).Select(x => x.Key).FirstOrDefault();
            if (key is not null)
            {
                _equipped.Remove(key);
                removed?.Add(equipment);
            }
        }

        if (CollectionChanged is not null && 0 < removed!.Count)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
        }
    }



    /// <summary>
    /// 装備を追加する
    /// </summary>
    /// <param name="equipment">追加対象の列挙</param>
    public void AddRange(IEnumerable<IEquipment> equipments)
    {
        using var added = CollectionChanged is null ? null : new PooledList<IEquipment>();

        foreach (var equipment in equipments)
        {
            if (AddEquipmentInternal(equipment))
            {
                added?.Add(equipment);
            }
        }

        if (CollectionChanged is not null && 0 < added!.Count)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added));
        }
    }


    /// <summary>
    /// 装備を追加する
    /// </summary>
    /// <param name="equipment">追加対象</param>
    /// <param name="count">追加個数</param>
    public void Add(IEquipment equipment, long count = 1)
    {
        using var added = CollectionChanged is null ? null : new PooledList <IEquipment>((int) count);
        var cnt = 0L;
        while (cnt < count && AddEquipmentInternal(equipment))
        {
            cnt++;
            added?.Add(equipment);
        }

        if (CollectionChanged is not null && 0 < added!.Count)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added));
        }
    }


    /// <summary>
    /// 装備を追加する(内部用)
    /// </summary>
    /// <param name="equipment">追加対象</param>
    /// <returns>追加に成功したか</returns>
    private bool AddEquipmentInternal(IEquipment equipment)
    {
        // 装備可能な接続情報を取得する
        var equippableInfo = Ware.Equipments.Values
            .FirstOrDefault(x => !_equipped.ContainsKey(x) && x.CanEquipped(equipment));

        // 装備可能な接続がある場合、装備する
        if (equippableInfo is not null)
        {
            _equipped.Add(equippableInfo, equipment);
            return true;
        }

        return false;
    }



    /// <summary>
    /// XML にシリアライズする
    /// </summary>
    /// <returns>インスタンスの現在の状態を表す XElement</returns>
    public XElement Serialize()
    {
        var equipments = _equipped
            .Select(x =>new XElement(
                "equipment",
                new XAttribute("group", x.Key.GroupName),
                new XAttribute("connection", x.Key.ConnectionName),
                new XAttribute("id", x.Value.ID)
                )
            );

        return new XElement("equipments", equipments);
    }


    /// <summary>
    /// 現在装備可能な個数を取得する
    /// </summary>
    /// <param name="type">装備ID</param>
    /// <param name="size">装備サイズ</param>
    /// <returns>装備IDと装備サイズに対応する装備があと何個装備できるか</returns>
    public int GetEquippableCount(IEquipmentType type, IX4Size size)
        => Ware.Equipments.Values.Count(x =>
        !_equipped.ContainsKey(x) &&
        x.Tags.Contains(type.EquipmentTypeID[..^1]) &&
        x.Tags.Contains(size.SizeID)
    );


    /// <summary>
    /// 装備可能な最大個数を取得する
    /// </summary>
    /// <param name="type">装備ID</param>
    /// <param name="size">装備サイズ</param>
    /// <returns>装備IDと装備サイズに対応する装備が何個装備できるか</returns>
    public int GetMaxEquippableCount(IEquipmentType type, IX4Size size)
        => Ware.Equipments.Values.Count(x => x.Tags.Contains(type.EquipmentTypeID[..^1]) && x.Tags.Contains(size.SizeID));


    /// <inheritdoc />
    public bool Equals(EquippableWareEquipmentManager? other)
        => other is not null && Ware.Equals(other.Ware) && other._equipped.SequenceEqual(_equipped);


    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is EquippableWareEquipmentManager other && Equals(other);


    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(Ware);
        foreach (var equipment in _equipped)
        {
            hash.Add(equipment);
        }

        return hash.ToHashCode();
    }
}
