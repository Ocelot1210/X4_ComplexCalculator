using LibX4.FileSystem;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using WPFLocalizeExtension.Engine;

namespace X4_DataExporterWPF.ExportWindow.DependencyResolutionFailedWindows
{
    internal class DependencyResolutionFailedModel : BindableBase
    {
        /// <summary>
        /// 依存関係情報
        /// </summary>
        public ObservableCollection<DependencyResolutionFailedInfo> Info { get; }


        /// <summary>
        /// 新しいインスタンスを作成する
        /// </summary>
        /// <param name="mods">依存関係の解決に失敗した Mod の一覧</param>
        public DependencyResolutionFailedModel(IReadOnlyList<ModInfo> mods)
        {
            Info = new ObservableCollection<DependencyResolutionFailedInfo>();

            foreach (var mod in mods)
            {
                foreach (var item in EnumerateInfo(mod))
                {
                    Info.Add(item);
                }
            }
        }


        /// <summary>
        /// Mod の情報から依存関係を再帰的に列挙する
        /// </summary>
        /// <param name="modInfo">列挙する Mod の情報</param>
        /// <param name="level">依存関係の深さ</param>
        /// <returns>依存関係の情報を表す <see cref="DependencyResolutionFailedInfo"/> の列挙</returns>
        private static IEnumerable<DependencyResolutionFailedInfo> EnumerateInfo(ModInfo modInfo, int level = 0)
        {
            yield return new DependencyResolutionFailedInfo(modInfo.Name, modInfo.ID, Path.GetFileName(modInfo.Directory) ?? "", "", level);

            foreach (var dependency in modInfo.Dependencies)
            {
                // dependency.ModInfo が null 以外の場合も出力する。
                // → 依存関係が循環していると Mod の詳細情報 (dependency.ModInfo) が 非 null になるため。
                
                // 依存する Mod が無いか？
                if (dependency.ModInfo is null)
                {
                    if (dependency.Optional)
                    {
                        var remarks = (string)LocalizeDictionary.Instance.GetLocalizedObject("Lang:DependencyResolutionFailedWindow_OptionalModNotFound", null, null);
                        yield return new DependencyResolutionFailedInfo(dependency.Name, dependency.ID, "", remarks, level + 1);
                    }
                    else
                    {
                        var remarks = (string)LocalizeDictionary.Instance.GetLocalizedObject("Lang:DependencyResolutionFailedWindow_RequiredModNotFound", null, null);
                        yield return new DependencyResolutionFailedInfo(dependency.Name, dependency.ID, "", remarks, level + 1);
                    }
                }
                else
                {
                    // 依存する Mod がある場合、再帰的に出力
                    foreach (var info in EnumerateInfo(dependency.ModInfo, level + 1))
                    {
                        yield return info;
                    }
                }
            }
        }
    }
}
