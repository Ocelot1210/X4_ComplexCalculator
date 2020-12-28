﻿using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.Menu.View.DBViewer.Wares
{
    /// <summary>
    /// ウェア閲覧用DataGridの1レコード分
    /// </summary>
    class WaresItem : BindableBase
    {
        #region メンバ
        /// <summary>
        /// 表示対象ウェア
        /// </summary>
        private readonly Ware _Ware;
        #endregion


        /// <summary>
        /// ウェア名
        /// </summary>
        public string WareName => _Ware.Name;


        /// <summary>
        /// ウェア種別名
        /// </summary>
        public string WareGroupName => _Ware.WareGroup.Name;


        /// <summary>
        /// カーゴ種別名
        /// </summary>
        public string Transport => _Ware.TransportType.Name;


        /// <summary>
        /// 最安値
        /// </summary>
        public long MinPrice => _Ware.MinPrice;


        /// <summary>
        /// 最高値
        /// </summary>
        public long MaxPrice => _Ware.MaxPrice;


        /// <summary>
        /// 利益
        /// </summary>
        public long Profit => MaxPrice - MinPrice;


        /// <summary>
        /// 容量
        /// </summary>
        public long Volume => _Ware.Volume;


        /// <summary>
        /// 容量当たりの利益
        /// </summary>
        public double ProfitPreVolume => Math.Round((double)Profit / Volume, 1);


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ware">表示対象ウェア</param>
        public WaresItem(Ware ware)
        {
            _Ware = ware;
        }
    }
}
