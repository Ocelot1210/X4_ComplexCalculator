using System;
using System.Collections.Generic;

namespace X4_DataExporterWPF.Entity;

/// <summary>
/// ウェア種別
/// </summary>
/// <param name="WareGroupID">ウェア種別ID</param>
/// <param name="Name">ウェア種別名</param>
/// <param name="Tier">階級</param>
public sealed record WareGroup(string WareGroupID, string Name, long Tier);
