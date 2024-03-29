﻿using Prism.Mvvm;
using System;

namespace X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;

/// <summary>
/// ステーション設定用クラス
/// </summary>
public class StationSettings : BindableBase, IStationSettings
{
    #region メンバ
    /// <summary>
    /// 本部か
    /// </summary>
    private bool _isHeadquarters;


    /// <summary>
    /// 日光[%]
    /// </summary>
    private double _sunlight = 100;
    #endregion


    #region プロパティ
    /// <summary>
    /// 本部か
    /// </summary>
    public bool IsHeadquarters
    {
        get => _isHeadquarters;
        set => SetProperty(ref _isHeadquarters, value);
    }


    /// <summary>
    /// 本部の必要労働者数
    /// </summary>
    public int HQWorkers { get; } = 200;


    /// <summary>
    /// 労働者
    /// </summary>
    public WorkforceManager Workforce { get; } = new();


    /// <summary>
    /// 日光[%]
    /// </summary>
    public double Sunlight
    {
        get => _sunlight;
        set => SetProperty(ref _sunlight, value);
    }
    #endregion
}
