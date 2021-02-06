using System;
using System.Collections.Generic;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// ウェア情報管理用クラス
    /// </summary>
    public partial class Ware : IWare
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">ウェアID</param>
        /// <param name="name">ウェア名</param>
        /// <param name="wareGroup">ウェア種別</param>
        /// <param name="transportType">カーゴ種別</param>
        /// <param name="description">説明文</param>
        /// <param name="volume">コンテナサイズ</param>
        /// <param name="minPrice">最低価格</param>
        /// <param name="avgPrice">平均価格</param>
        /// <param name="maxPrice">最高価格</param>
        /// <param name="owners">所有派閥</param>
        /// <param name="productions">生産方式</param>
        /// <param name="resources">生産に必要なウェア情報</param>
        /// <param name="tags">タグ一覧</param>
        /// <param name="wareEffects">ウェア生産時の追加効果情報</param>
        public Ware(
            string id,
            string name,
            WareGroup wareGroup,
            TransportType transportType,
            string description,
            long volume,
            long minPrice,
            long avgPrice,
            long maxPrice,
            IReadOnlyList<Faction> owners,
            IReadOnlyDictionary<string, WareProduction> productions,
            IReadOnlyDictionary<string, IReadOnlyList<WareResource>> resources,
            HashSet<string> tags,
            WareEffects wareEffects
        )
        {
            ID = id;
            Name = name;
            WareGroup = wareGroup;
            TransportType = transportType;
            Description = description;
            Volume = volume;
            MinPrice = minPrice;
            AvgPrice = avgPrice;
            MaxPrice = maxPrice;
            Owners = owners;
            Productions = productions;
            Resources = resources;
            Tags = tags;
            WareEffects = wareEffects;
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj) => obj is IWare tgt && tgt.ID == ID;


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual bool Equals(IWare other) => ID == other.ID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(ID);
    }
}
