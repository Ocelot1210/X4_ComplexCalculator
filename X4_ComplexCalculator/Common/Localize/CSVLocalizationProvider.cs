﻿#region Copyright information
// <copyright file="CSVLocalizationProvider.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Providers;

namespace X4_ComplexCalculator.Common.Localize;

/// <summary>
/// A localization provider for comma separated files
/// </summary>
public class CSVLocalizationProvider : FrameworkElement, ILocalizationProvider
{
    private string _fileName = "";
    /// <summary>
    /// The name of the file without an extension.
    /// </summary>
    public string FileName
    {
        get => _fileName;
        set
        {
            if (_fileName != value)
            {
                _fileName = value;

                AvailableCultures.Clear();

                var appPath = GetWorkingDirectory();
                var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

                foreach (var c in cultures)
                {
                    var csv = Path.Combine(appPath, "Localization", FileName + (string.IsNullOrEmpty(c.Name) ? "" : $".{c.Name}") + ".csv");
                    if (File.Exists(csv))
                        AvailableCultures.Add(c);
                }

                OnProviderChanged();
            }
        }
    }

    private bool _hasHeader = false;
    /// <summary>
    /// A flag indicating, if it has a header row.
    /// </summary>
    public bool HasHeader
    {
        get => _hasHeader;
        set
        {
            _hasHeader = value;
            OnProviderChanged();
        }
    }

    /// <summary>
    /// Raise a <see cref="ILocalizationProvider.ProviderChanged"/> event.
    /// </summary>
    private void OnProviderChanged()
    {
        ProviderChanged?.Invoke(this, new ProviderChangedEventArgs(null));
    }

    /// <summary>
    /// Calls the <see cref="ILocalizationProvider.ProviderError"/> event.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="key">The key.</param>
    /// <param name="message">The error message.</param>
    private void OnProviderError(DependencyObject target, string key, string message)
    {
        ProviderError?.Invoke(this, new ProviderErrorEventArgs(target, key, message));
    }

    /// <summary>
    /// Get the working directory, depending on the design mode or runtime.
    /// </summary>
    /// <returns>The working directory.</returns>
    private static string GetWorkingDirectory()
    {
        if (AppDomain.CurrentDomain.FriendlyName.Contains("XDesProc"))
        {
            foreach (var process in Process.GetProcesses())
            {
                if (!process.ProcessName.Contains(".vshost"))
                    continue;

                // Get the executable path (all paths are cached now in order to reduce WMI load.
                var dir = Path.GetDirectoryName(GetExecutablePath(process.Id));

                if (string.IsNullOrEmpty(dir))
                    continue;

                return dir;
            }

            throw new InvalidOperationException();
        }
        else
        {
            return Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) ?? throw new InvalidOperationException();
        }
    }

    private static readonly Dictionary<int, string> _ExecutablePaths = new();

    /// <summary>
    /// Get the executable path for both x86 and x64 processes.
    /// </summary>
    /// <param name="processId">The process id.</param>
    /// <returns>The path if found; otherwise, null.</returns>
    private static string GetExecutablePath(int processId)
    {
        if (_ExecutablePaths.ContainsKey(processId))
            return _ExecutablePaths[processId];

        const string WMI_QUERY_STRING = "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process";
        using (var searcher = new ManagementObjectSearcher(WMI_QUERY_STRING))
        using (var results = searcher.Get())
        {
            var query = from p in Process.GetProcesses()
                        join mo in results.Cast<ManagementObject>()
                        on p.Id equals (int)(uint)mo["ProcessId"]
                        where p.Id == processId
                        select new
                        {
                            Process = p,
                            Path = (string)mo["ExecutablePath"],
                            CommandLine = (string)mo["CommandLine"],
                        };
            foreach (var item in query)
            {
                _ExecutablePaths.Add(processId, item.Path);
                return item.Path;
            }
        }

        throw new InvalidOperationException();
    }

    /// <summary>
    /// Parses a key ([[Assembly:]Dict:]Key and return the parts of it.
    /// </summary>
    /// <param name="inKey">The key to parse.</param>
    /// <param name="outAssembly">The found or default assembly.</param>
    /// <param name="outDict">The found or default dictionary.</param>
    /// <param name="outKey">The found or default key.</param>
    public static void ParseKey(string inKey, out string outAssembly, out string outDict, out string outKey)
    {
        // Reset everything to null.
        outAssembly = "";
        outDict = "";
        outKey = "";

        if (!string.IsNullOrEmpty(inKey))
        {
            string[] split = inKey.Trim().Split(":".ToCharArray());

            // assembly:dict:key
            if (split.Length == 3)
            {
                outAssembly = !string.IsNullOrEmpty(split[0]) ? split[0] : "";
                outDict = !string.IsNullOrEmpty(split[1]) ? split[1] : "";
                outKey = split[2];
            }

            // dict:key
            if (split.Length == 2)
            {
                outDict = !string.IsNullOrEmpty(split[0]) ? split[0] : "";
                outKey = split[1];
            }

            // key
            if (split.Length == 1)
            {
                outKey = split[0];
            }
        }
    }

    public FullyQualifiedResourceKeyBase GetFullyQualifiedResourceKey(string key, DependencyObject target)
    {
        ParseKey(key, out string assembly, out string dictionary, out key);

        if (target is null)
            return new FQAssemblyDictionaryKey(key, assembly, dictionary);

        if (string.IsNullOrEmpty(assembly))
            assembly = GetAssembly(target);

        if (string.IsNullOrEmpty(dictionary))
            dictionary = GetDictionary(target);

        return new FQAssemblyDictionaryKey(key, assembly, dictionary);
    }

    #region Variables
    /// <summary>
    /// A dictionary for notification classes for changes of the individual target Parent changes.
    /// </summary>
    private readonly ParentNotifiers _parentNotifiers = new();
    #endregion

    /// <summary>
    /// An action that will be called when a parent of one of the observed target objects changed.
    /// </summary>
    /// <param name="obj">The target <see cref="DependencyObject"/>.</param>
    private void ParentChangedAction(DependencyObject obj)
    {
        OnProviderChanged(obj);
    }

    /// <summary>
    /// Calls the <see cref="ILocalizationProvider.ProviderChanged"/> event.
    /// </summary>
    /// <param name="target">The target object.</param>
    protected virtual void OnProviderChanged(DependencyObject target)
    {
        try
        {
            var assembly = GetAssembly(target);
            var dictionary = GetDictionary(target);

            //if (!String.IsNullOrEmpty(assembly) && !String.IsNullOrEmpty(dictionary))
            //    GetResourceManager(assembly, dictionary);
        }
        catch
        {
        }

        ProviderChanged?.Invoke(this, new ProviderChangedEventArgs(target));
    }

    /// <summary>
    /// Get the assembly from the context, if possible.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <returns>The assembly name, if available.</returns>
    protected string GetAssembly(DependencyObject? target)
    {
        if (target is null)
        {
            return "";
        }

        return target.GetValueOrRegisterParentNotifier<string>(CSVEmbeddedLocalizationProvider.DefaultAssemblyProperty, ParentChangedAction, _parentNotifiers);
    }

    /// <summary>
    /// Get the dictionary from the context, if possible.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <returns>The dictionary name, if available.</returns>
    protected string GetDictionary(DependencyObject? target)
    {
        if (target is null)
        {
            return "";
        }

        return target.GetValueOrRegisterParentNotifier<string>(CSVEmbeddedLocalizationProvider.DefaultDictionaryProperty, ParentChangedAction, _parentNotifiers);
    }

    private readonly Dictionary<CultureInfo, Dictionary<string, string>> _languageDict = new();


    /// <summary>
    /// Get the localized object.
    /// </summary>
    /// <param name="key">The key to the value.</param>
    /// <param name="target">The target object.</param>
    /// <param name="culture">The culture to use.</param>
    /// <returns>The value corresponding to the source/dictionary/key path for the given culture (otherwise NULL).</returns>
    public object GetLocalizedObject(string key, DependencyObject target, CultureInfo culture)
    {
        // Call this function to provide backward compatibility.
        ParseKey(key, out _, out var dictionary, out key);

        // Now try to read out the default assembly and/or dictionary.
        if (string.IsNullOrEmpty(dictionary))
            dictionary = GetDictionary(target);

        // Try to get the culture specific file.
        const string CSV_DIRECTORY = "Localization";
        var csvPath = "";

        if (culture is null)
        {
            culture = LocalizeDictionary.CurrentCulture;
        }

        while (culture != CultureInfo.InvariantCulture)
        {
            csvPath = Path.Combine(GetWorkingDirectory(), CSV_DIRECTORY, dictionary + (string.IsNullOrEmpty(culture.Name) ? "" : "." + culture.Name) + ".csv");

            if (File.Exists(csvPath))
                break;

            culture = culture.Parent;
        }

        if (!File.Exists(csvPath))
        {
            // Take the invariant culture.
            csvPath = Path.Combine(GetWorkingDirectory(), CSV_DIRECTORY, dictionary + ".csv");

            if (!File.Exists(csvPath))
            {
                OnProviderError(target, key, "A file for the provided culture " + culture.EnglishName + " does not exist at " + csvPath + ".");
                return "";
            }
        }

        ReadCsv(culture, csvPath);
        _languageDict[culture].TryGetValue(key, out string? ret);

        // 見つからなかったらデフォルトの言語ファイルから探す
        if (culture != CultureInfo.InvariantCulture && string.IsNullOrEmpty(ret))
        {
            _languageDict[CultureInfo.InvariantCulture].TryGetValue(key, out ret);
        }

        // Nothing found -> Raise the error message.
        if (ret is null)
            OnProviderError(target, key, "The key does not exist in " + csvPath + ".");

        return ret ?? key;
    }

    /// <summary>
    /// Read the csv file
    /// </summary>
    /// <param name="culture">The culture to use.</param>
    /// <param name="csvPath">The csv file path.</param>
    private void ReadCsv(CultureInfo culture, string csvPath)
    {
        if (_languageDict.ContainsKey(culture))
        {
            return;
        }

        _languageDict.Add(culture, new Dictionary<string, string>());

        // Open the file.
        using var reader = new StreamReader(csvPath, Encoding.Default);

        // Skip the header if needed.
        if (HasHeader && !reader.EndOfStream)
            reader.ReadLine();

        // Read each line and split it.
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (!string.IsNullOrEmpty(line) && line[0] != '#')
            {
                var parts = line.Split(';');

                if (parts.Length < 2)
                    continue;

                _languageDict[culture].Add(parts[0], EscapeString(parts[1]));
            }
        }
    }

    private static string EscapeString(string str)
    {
        var sb = new StringBuilder(str);

        sb.Replace(@"\a", "\a");
        sb.Replace(@"\b", "\b");
        sb.Replace(@"\f", "\f");
        sb.Replace(@"\n", "\n");
        sb.Replace(@"\r", "\r");
        sb.Replace(@"\t", "\t");
        sb.Replace(@"\v", "\v");
        sb.Replace(@"\\", "\\");

        return sb.ToString();
    }


    /// <summary>
    /// An observable list of available cultures.
    /// </summary>
    public ObservableCollection<CultureInfo> AvailableCultures { get; } = new ObservableCollection<CultureInfo>();

    /// <summary>
    /// Gets fired when the provider changed.
    /// </summary>
    public event ProviderChangedEventHandler? ProviderChanged;

    /// <summary>
    /// An event that is fired when an error occurred.
    /// </summary>
    public event ProviderErrorEventHandler? ProviderError;

#pragma warning disable CS0067
    /// <summary>
    /// An event that is fired when a value changed.
    /// </summary>
    public event ValueChangedEventHandler? ValueChanged;
#pragma warning restore CS0067
}
