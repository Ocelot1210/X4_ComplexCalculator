using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Builder;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Manager
{
    /// <summary>
    /// <see cref="IWare"/> の一覧を管理するクラス
    /// </summary>
    public class WareManager
    {
        #region メンバ
        /// <summary>
        /// ウェア一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, IWare> _Wares;


        /// <summary>
        /// マクロ名を持つウェア一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, IMacro> _MacroWares;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        /// <param name="transportTypeManager">カーゴ種別一覧</param>
        public WareManager(IDbConnection conn, TransportTypeManager transportTypeManager)
        {
            var builder = new WareBuilder(conn, transportTypeManager);
            _Wares = builder.BuildAll()
                .ToDictionary(x => x.ID);

            _MacroWares = _Wares.Values.OfType<IMacro>()
                .GroupBy(x => x.MacroName)
                .Select(x => x.FirstOrDefault())
                .ToDictionary(x => x.MacroName);
        }


        /// <summary>
        /// <paramref name="id"/> に対応する <see cref="IWare"/> を取得する
        /// </summary>
        /// <param name="id">ウェアID</param>
        /// <returns><paramref name="id"/> に対応する <see cref="IWare"/></returns>
        /// <exception cref="KeyNotFoundException"><paramref name="id"/> に対応する <see cref="IWare"/>が無い場合</exception>
        public IWare Get(string id) => _Wares[id];


        /// <summary>
        /// <paramref name="id"/> に対応する <see cref="IWare"/> を取得する(型指定版)
        /// </summary>
        /// <typeparam name="T"><see cref="IWare"/> を継承した任意の型</typeparam>
        /// <param name="id">ウェアID</param>
        /// <returns><paramref name="id"/> に対応する <see cref="IWare"/></returns>
        /// <exception cref="KeyNotFoundException"><paramref name="id"/> に対応する <see cref="IWare"/>が無い場合</exception>
        /// <exception cref="InvalidCastException"><paramref name="id"/> に対応する <see cref="IWare"/> と <typeparamref name="T"/> に互換性が無い場合</exception>
        public T Get<T>(string id) where T : IWare => (T)_Wares[id];


        /// <summary>
        /// <paramref name="id"/> に対応する <see cref="IWare"/> の取得を試みる(型指定版)
        /// </summary>
        /// <typeparam name="T"><see cref="IWare"/> を継承した任意の型</typeparam>
        /// <param name="id">ウェアID</param>
        /// <returns>指定した型のウェア又はnull</returns>
        public T? TryGet<T>(string id) where T : class, IWare => _Wares.TryGetValue(id, out var ret) ? ret as T : null;


        /// <summary>
        /// <paramref name="macro"/> のマクロ名に対応する <see cref="IMacro"/> の取得を試みる(型指定版)
        /// </summary>
        /// <typeparam name="T"><see cref="IMacro"/> を継承した任意の型</typeparam>
        /// <param name="macro">マクロ名</param>
        /// <returns>指定した型のウェア又はnull</returns>
        public T? TryGetMacro<T>(string macro) where T : class, IMacro =>
            _MacroWares.TryGetValue(macro, out var ret) ? ret as T : null;


        /// <summary>
        /// 全ウェアを取得
        /// </summary>
        public IEnumerable<IWare> GetAll() => _Wares.Values;


        /// <summary>
        /// 全ウェアを取得(型指定版)
        /// </summary>
        /// <typeparam name="T"><see cref="IWare"/> を継承した任意の型</typeparam>
        /// <returns>全ウェアの中で <see cref="IWare"/> と一致した全てのウェア</returns>
        public IEnumerable<T> GetAll<T>() where T : IWare => _Wares.Values.OfType<T>();
    }
}
