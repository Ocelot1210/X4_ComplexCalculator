using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibX4;

public static class X4Path
{
    /// <summary>
    /// X4のインストール先フォルダを取得する
    /// </summary>
    /// <returns>成功した場合 X4 のインストール先フォルダパス。失敗した場合空文字列</returns>
    public static string GetX4InstallDirectory()
    {
        // アプリケーションのアンインストール情報が保存されている場所
        const string LOCATION = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

        // レジストリ情報の取得を試みる
        RegistryKey? parent = Registry.LocalMachine.OpenSubKey(LOCATION, false);
        if (parent is null)
        {
            // だめだった場合諦める
            return "";
        }

        var ret = "";

        // 子のレジストリの名前の数だけ処理をする
        // Steam以外(GOG等)からインストールされる事を考慮してレジストリのキーを決め打ちにしないで全部探す
        foreach (var subKeyName in parent.GetSubKeyNames())
        {
            // 子のレジストリの情報を取得する
            RegistryKey? child = Registry.LocalMachine.OpenSubKey(@$"{LOCATION}\{subKeyName}", false);
            if (child is null)
            {
                // 取得に失敗したら次のレジストリを見に行く
                continue;
            }

            // 表示名を保持しているオブジェクトを取得する
            var value = child.GetValue("DisplayName");
            if (value is null)
            {
                // 取得に失敗したら次のレジストリを見に行く
                continue;
            }

            if (value.ToString() == "X4: Foundations")
            {
                ret = child.GetValue("InstallLocation")?.ToString() ?? "";
                break;
            }
        }

        return ret;
    }


    /// <summary>
    /// X4 の設定フォルダパスを列挙する
    /// </summary>
    /// <returns>X4 の設定フォルダパスの列挙</returns>
    public static IEnumerable<string> EnumerateConfigFolders()
    {
        var x4Dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Egosoft", "X4");
        if (!Directory.Exists(x4Dir))
        {
            yield break;
        }

        // Steam 版の設定フォルダを検索
        foreach (var dir in Directory.GetDirectories(x4Dir))
        {
            // 名前が全て数字で構成されているフォルダの中に config.xml があればそのフォルダを設定フォルダと見なす
            var folderName = Path.GetFileName(dir);
            if (folderName.All(char.IsDigit) && File.Exists(Path.Combine(dir, "config.xml")))
            {
                yield return dir;
            }
        }

        // 非 Steam 版の設定フォルダか判定
        if (File.Exists(Path.Combine(x4Dir, "config.xml")))
        {
            yield return x4Dir;
        }
    }
}
