﻿using Prism.Commands;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid
{
    /// <summary>
    /// ModuleクラスをDataGrid表示用クラス
    /// </summary>
    public class ModulesGridItem : BindableBaseEx
    {
        #region スタティックメンバ
        /// <summary>
        /// モジュール数最大値
        /// </summary>
        public const long MAX_MODULE_COUNT = 99999;
        #endregion

        #region "メンバ変数"
        /// <summary>
        /// モジュール数
        /// </summary>
        private long _ModuleCount = 1;

        /// <summary>
        /// 選択された建造方式
        /// </summary>
        private ModuleProduction _SelectedMethod;

        /// <summary>
        /// 選択されているか
        /// </summary>
        private bool _IsSelected;
        #endregion


        #region "プロパティ
        /// <summary>
        /// モジュール
        /// </summary>
        public Module Module { get; private set; }


        /// <summary>
        /// 装備情報
        /// </summary>
        public ModuleEquipment ModuleEquipment { get; private set; }


        /// <summary>
        /// モジュールの装備編集
        /// </summary>
        public ICommand EditEquipmentCommand { get; }


        /// <summary>
        /// 選択されているか
        /// </summary>
        public bool IsSelected
        {
            get => _IsSelected;
            set => SetProperty(ref _IsSelected, value);
        }


        /// <summary>
        /// モジュール数
        /// </summary>
        public long ModuleCount
        {
            get => _ModuleCount;
            set
            {
                var setValue = (value < 0) ? 0L :
                               (MAX_MODULE_COUNT < value) ? MAX_MODULE_COUNT : value;

                SetPropertyEx(ref _ModuleCount, setValue);
            }
        }

        /// <summary>
        /// 装備中のタレットの個数
        /// </summary>
        public int TurretsCount => ModuleEquipment.Turret.AllEquipmentsCount;

        /// <summary>
        /// タレットのツールチップ文字列
        /// </summary>
        public string TurretsToolTip => MakeEquipmentToolTipString(ModuleEquipment.Turret);


        /// <summary>
        /// 装備中のシールドの個数
        /// </summary>
        public int ShieldsCount => ModuleEquipment.Shield.AllEquipmentsCount;

        /// <summary>
        /// シールドのツールチップ文字列
        /// </summary>
        public string ShieldsToolTip => MakeEquipmentToolTipString(ModuleEquipment.Shield);


        /// <summary>
        /// 編集ボタンを表示すべきか
        /// </summary>
        public Visibility EditEquipmentButtonVisiblity => (ModuleEquipment.CanEquipped) ? Visibility.Visible : Visibility.Hidden;


        /// <summary>
        /// 建造方式を表示すべきか
        /// </summary>
        public Visibility SelectedMethodVisiblity => (2 <= Module.ModuleProductions.Count()) ? Visibility.Visible : Visibility.Hidden;


        /// <summary>
        /// 選択中の建造方式
        /// </summary>
        public ModuleProduction SelectedMethod
        {
            get => _SelectedMethod;
            set => SetPropertyEx(ref _SelectedMethod, value);
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="moduleCount">モジュール数</param>
        public ModulesGridItem(string moduleID, long moduleCount = 1) : this(Module.Get(moduleID), null, moduleCount)
        {

        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュール</param>
        /// <param name="moduleCount">モジュール数</param>
        public ModulesGridItem(Module module, ModuleProduction? selectedMethod = null, long moduleCount = 1)
        {
            Module = module;
            ModuleCount = moduleCount;
            EditEquipmentCommand = new DelegateCommand(EditEquipment);
            ModuleEquipment = ModuleEquipment.Get(Module.ModuleID);

            _SelectedMethod = selectedMethod ?? Module.ModuleProductions[0];
        }


        /// <summary>
        /// コンストラクタ(xmlより作成)
        /// </summary>
        /// <param name="element">モジュール情報が記載されたxml</param>
        public ModulesGridItem(XElement element)
        {
            Module = Module.Get(element.Attribute("id").Value);
            ModuleEquipment = ModuleEquipment.Get(Module.ModuleID);

            ModuleCount = long.Parse(element.Attribute("count").Value);
            SelectedMethod = Module.ModuleProductions.Where(x => x.Method == element.Attribute("method").Value).FirstOrDefault();
            if (SelectedMethod == null)
            {
                SelectedMethod = Module.ModuleProductions.First();
            }
            _SelectedMethod = SelectedMethod;
            EditEquipmentCommand = new DelegateCommand(EditEquipment);

            // タレット追加
            foreach (var turret in element.XPathSelectElement("turrets").Elements())
            {
                try
                {
                    AddEquipment(Equipment.Get(turret.Attribute("id").Value));
                }
                catch
                {
                    
                }
            }

            // シールド追加
            foreach (var shield in element.XPathSelectElement("shields").Elements())
            {
                try
                {
                    AddEquipment(Equipment.Get(shield.Attribute("id").Value));
                }
                catch
                {

                }
            }
        }


        /// <summary>
        /// xml化する
        /// </summary>
        /// <returns>xml化した情報</returns>
        public XElement ToXml()
        {
            // それぞれのモジュールの情報を設定
            var ret = new XElement("module");
            ret.Add(new XAttribute("id", Module.ModuleID));
            ret.Add(new XAttribute("count", ModuleCount));
            ret.Add(new XAttribute("method", SelectedMethod.Method));

            // タレット追加
            {
                var turretsXml = new XElement("turrets");

                foreach (var turret in ModuleEquipment.Turret.AllEquipments)
                {
                    var turXml = new XElement("turret");
                    turXml.Add(new XAttribute("id", turret.EquipmentID));

                    turretsXml.Add(turXml);
                }

                ret.Add(turretsXml);
            }


            // シールド追加
            {
                var shieldsXml = new XElement("shields");

                foreach (var shield in ModuleEquipment.Shield.AllEquipments)
                {
                    var sldXml = new XElement("shield");
                    sldXml.Add(new XAttribute("id", shield.EquipmentID));

                    shieldsXml.Add(sldXml);
                }

                ret.Add(shieldsXml);
            }

            return ret;
        }


        /// <summary>
        /// 装備を追加
        /// </summary>
        /// <param name="equipment">追加したい装備</param>
        public void AddEquipment(Equipment equipment)
        {
            // 装備できないモジュールの場合、何もしない
            if (!ModuleEquipment.CanEquipped)
            {
                return;
            }

            switch (equipment.EquipmentType.EquipmentTypeID)
            {
                case "turrets":
                    ModuleEquipment.Turret.AddEquipment(equipment);
                    break;

                case "shields":
                    ModuleEquipment.Shield.AddEquipment(equipment);
                    break;

                default:
                    throw new InvalidOperationException("追加できるのはタレットかシールドのみです。");
            }
        }



        /// <summary>
        /// ツールチップ文字列を更新する
        /// </summary>
        public void UpdateTooltip()
        {
            if (ModuleEquipment.CanEquipped)
            {
                RaisePropertyChanged(nameof(TurretsToolTip));
                RaisePropertyChanged(nameof(ShieldsToolTip));
            }
        }


        /// <summary>
        /// 装備を編集
        /// </summary>
        private void EditEquipment()
        {
            // 変更前
            var turretsOld = ModuleEquipment.Turret.AllEquipments.Select(x => x.EquipmentID).OrderBy(x => x).ToArray();
            var shieldsOld = ModuleEquipment.Shield.AllEquipments.Select(x => x.EquipmentID).OrderBy(x => x).ToArray();

            var window = new EditEquipmentWindow(this);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();

            bool equipmentChanged = false;

            // 変更があった場合のみ通知
            if (!turretsOld.SequenceEqual(ModuleEquipment.Turret.AllEquipments.Select(x => x.EquipmentID).OrderBy(x => x)))
            {
                RaisePropertyChanged(nameof(TurretsCount));
                RaisePropertyChanged(nameof(TurretsToolTip));
                equipmentChanged = true;
            }

            if (!shieldsOld.SequenceEqual(ModuleEquipment.Shield.AllEquipments.Select(x => x.EquipmentID).OrderBy(x => x)))
            {
                RaisePropertyChanged(nameof(ShieldsCount));
                RaisePropertyChanged(nameof(ShieldsToolTip));
                equipmentChanged = true;
            }

            if (equipmentChanged)
            {
                RaisePropertyChangedEx(turretsOld.Concat(shieldsOld), ModuleEquipment.GetAllEquipment().Select(x =>x.EquipmentID).ToArray(), nameof(ModuleEquipment));
            }
        }

        /// <summary>
        /// 装備のツールチップ文字列を作成
        /// </summary>
        /// <returns></returns>
        private string MakeEquipmentToolTipString(ModuleEquipmentManager equipmentManager)
        {
            var sb = new StringBuilder();

            foreach (var size in equipmentManager.Sizes)
            {
                var cnt = 1;

                foreach (var eq in equipmentManager.GetEquipment(size))
                {
                    if (cnt == 1)
                    {
                        if (sb.Length != 0)
                        {
                            sb.AppendLine();
                        }
                        sb.AppendLine($"【{size.Name}】");
                    }
                    sb.AppendLine($"{cnt++:D2} ： {eq.Name}");
                }
            }

            if (sb.Length == 0)
            {
                sb.Append((string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("Lang:NotEquippedToolTipText", null, null));
            }

            return sb.ToString();
        }


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Module, ModuleCount, ModuleEquipment, SelectedMethod);
        }
    }
}