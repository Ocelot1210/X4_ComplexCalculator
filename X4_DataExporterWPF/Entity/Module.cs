using System;
using System.Collections.Generic;

namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュール
    /// </summary>
    public class Module : IEquatable<Module>, IEqualityComparer<Module>
    {
        #region プロパティ
        /// <summary>
        /// モジュールID
        /// </summary>
        public string ModuleID { get; }


        /// <summary>
        /// モジュール種別ID
        /// </summary>
        public string ModuleTypeID { get; }


        /// <summary>
        /// マクロ名
        /// </summary>
        public string Macro { get; }


        /// <summary>
        /// 最大労働者数
        /// </summary>
        public long MaxWorkers { get; }


        /// <summary>
        /// 収容可能な労働者数
        /// </summary>
        public long WorkersCapacity { get; }


        /// <summary>
        /// 設計図有無
        /// </summary>
        public bool NoBlueprint { get; }


        public byte[]? Thumbnail { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="moduleTypeID">モジュール種別ID</param>
        /// <param name="name">モジュール名</param>
        /// <param name="macro">マクロ名</param>
        /// <param name="maxWorkers">最大労働者数</param>
        /// <param name="workersCapacity">収容可能な労働者数</param>
        /// <param name="noBlueprint">設計図有無</param>
        /// <param name="thumbnail">サムネ画像</param>
        public Module(string moduleID, string moduleTypeID, string macro,
                      long maxWorkers, long workersCapacity, bool noBlueprint, byte[]? thumbnail)
        {
            ModuleID = moduleID;
            ModuleTypeID = moduleTypeID;
            Macro = macro;
            MaxWorkers = maxWorkers;
            WorkersCapacity = workersCapacity;
            NoBlueprint = noBlueprint;
            Thumbnail = thumbnail;
        }


        /// <summary>
        /// 指定のオブジェクトと等価であるかを判定する
        /// </summary>
        /// <param name="module">比較対象のオブジェクト</param>
        /// <returns>等価である場合は true、それ以外の場合は false</returns>
        public bool Equals(Module? module)
            => this.ModuleID == module?.ModuleID && this.ModuleTypeID == module.ModuleTypeID
            && this.Macro == module.Macro
            && this.MaxWorkers == module.MaxWorkers
            && this.WorkersCapacity == module.WorkersCapacity
            && this.NoBlueprint == module.NoBlueprint;


        /// <summary>
        /// 指定した 2 つのオブジェクトが等価であるかを判定する
        /// </summary>
        /// <param name="x">比較対象のオブジェクト</param>
        /// <param name="y">比較対象のオブジェクト</param>
        /// <returns>等価である場合は true、それ以外の場合は false</returns>
        public bool Equals(Module? x, Module? y) => x?.Equals(y) ?? false;


        /// <summary>
        /// 指定したオブジェクトのハッシュコードを算出する
        /// </summary>
        /// <param name="obj">算出対象のオブジェクト</param>
        /// <returns>指定したオブジェクトのハッシュコード</returns>
        public int GetHashCode(Module obj) => obj.GetHashCode();


        /// <summary>
        /// 指定のオブジェクトと等価であるかを判定する
        /// </summary>
        /// <param name="obj">比較対象のオブジェクト</param>
        /// <returns>等価である場合は true、それ以外の場合は false</returns>
        public override bool Equals(object? obj) => obj is Module module && Equals(module);


        /// <summary>
        /// 指定したオブジェクトのハッシュコードを算出する
        /// </summary>
        /// <returns>指定したオブジェクトのハッシュコード</returns>
        public override int GetHashCode()
            => HashCode.Combine(this.ModuleID, this.ModuleTypeID, this.Macro,
                                this.MaxWorkers, this.WorkersCapacity, this.NoBlueprint);
    }
}
