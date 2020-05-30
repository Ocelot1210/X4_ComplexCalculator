using Prism.Commands;
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
                               (99999 < value) ? 99999L : value;

                SetPropertyEx(ref _ModuleCount, setValue);
            }
        }

        /// <summary>
        /// 装備中のタレットの個数
        /// </summary>
        public int TurretsCount => Module.Equipment.Turret.AllEquipmentsCount;

        /// <summary>
        /// タレットのツールチップ文字列
        /// </summary>
        public string TurretsToolTip => MakeEquipmentToolTipString(Module.Equipment.Turret);


        /// <summary>
        /// 装備中のシールドの個数
        /// </summary>
        public int ShieldsCount => Module.Equipment.Shield.AllEquipmentsCount;

        /// <summary>
        /// シールドのツールチップ文字列
        /// </summary>
        public string ShieldsToolTip => MakeEquipmentToolTipString(Module.Equipment.Shield);


        /// <summary>
        /// 編集ボタンを表示すべきか
        /// </summary>
        public Visibility EditEquipmentButtonVisiblity => (Module.Equipment.CanEquipped) ? Visibility.Visible : Visibility.Hidden;


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
        public ModulesGridItem(string moduleID, long moduleCount = 1) : this(new Module(moduleID), null, moduleCount)
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

            _SelectedMethod = selectedMethod ?? Module.ModuleProductions[0];
        }


        /// <summary>
        /// コンストラクタ(xmlより作成)
        /// </summary>
        /// <param name="element">モジュール情報が記載されたxml</param>
        public ModulesGridItem(XElement element)
        {
            Module = new Module(element.Attribute("id").Value);
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
                    Module.AddEquipment(new Equipment(turret.Attribute("id").Value));
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
                    Module.AddEquipment(new Equipment(shield.Attribute("id").Value));
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

                foreach (var turret in Module.Equipment.Turret.AllEquipments)
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

                foreach (var shield in Module.Equipment.Shield.AllEquipments)
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
        /// ツールチップ文字列を更新する
        /// </summary>
        public void UpdateTooltip()
        {
            if (Module.Equipment.CanEquipped)
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
            var turretsOld = Module.Equipment.Turret.AllEquipments.Select(x => x.EquipmentID).OrderBy(x => x).ToArray();
            var shieldsOld = Module.Equipment.Shield.AllEquipments.Select(x => x.EquipmentID).OrderBy(x => x).ToArray();

            var window = new EditEquipmentWindow(Module);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();

            bool equipmentChanged = false;

            // 変更があった場合のみ通知
            if (!turretsOld.SequenceEqual(Module.Equipment.Turret.AllEquipments.Select(x => x.EquipmentID).OrderBy(x => x)))
            {
                RaisePropertyChanged(nameof(TurretsCount));
                RaisePropertyChanged(nameof(TurretsToolTip));
                equipmentChanged = true;
            }

            if (!shieldsOld.SequenceEqual(Module.Equipment.Shield.AllEquipments.Select(x => x.EquipmentID).OrderBy(x => x)))
            {
                RaisePropertyChanged(nameof(ShieldsCount));
                RaisePropertyChanged(nameof(ShieldsToolTip));
                equipmentChanged = true;
            }

            if (equipmentChanged)
            {
                RaisePropertyChangedEx(turretsOld.Concat(shieldsOld), Module.Equipment.GetAllEquipment().Select(x =>x.EquipmentID).ToArray(), nameof(Module.Equipment));
                RaisePropertyChanged(nameof(Module));
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
    }
}
