using System;
using System.IO;

namespace X4_DataExporterWPF.Export;


/// <summary>
/// DB をバックアップするクラス
/// </summary>
internal sealed class DbBackupper : IDisposable
{
    /// <summary>
    /// 破棄されたか
    /// </summary>
    private bool _disposed;


    /// <summary>
    /// 変更が確定されたか
    /// </summary>
    private bool _comitted;


    /// <summary>
    /// ファイルロック用の <see cref="FileStream"/>
    /// </summary>
    private readonly FileStream? _file;


    /// <summary>
    /// バックアップされたファイルパス
    /// </summary>
    private readonly string _bakFilePath = "";


    /// <summary>
    /// 元のファイルパス
    /// </summary>
    private readonly string _orgFilePath;


    /// <summary>
    /// バックアップ対象のファイルパス
    /// </summary>
    /// <param name="filePath"></param>
    internal DbBackupper(string filePath)
    {
        _orgFilePath = filePath;

        try
        {
            if (File.Exists(_orgFilePath))
            {
                _bakFilePath = Path.Combine(Path.GetDirectoryName(filePath) ?? throw new ArgumentException($"Invalid path : {filePath}", nameof(filePath)), Path.GetFileName(filePath) + $"_{DateTime.Now:yyyy_MM_dd-HHmmss-fff}_bak");
                File.Move(_orgFilePath, _bakFilePath);
                _file = new FileStream(_bakFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
            }
        }
        catch (Exception ex)
        {
            throw new DbBackupException(ex);
        }
    }


    /// <summary>
    /// 変更を確定する
    /// </summary>
    public void Commit()
    {
        try
        {
            _file?.Dispose();

            // 不要になったバックアップファイルを削除する
            if (!string.IsNullOrEmpty(_bakFilePath) && File.Exists(_bakFilePath))
            {
                File.Delete(_bakFilePath);
            }

            _comitted = true;
        }
        catch (Exception)
        {
        }
    }


    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _file?.Dispose();

            // 未コミット == ロールバックする必要があるか？
            if (!_comitted && !string.IsNullOrEmpty(_bakFilePath))
            {
                if (File.Exists(_orgFilePath))
                {
                    File.Delete(_orgFilePath);
                }

                File.Move(_bakFilePath, _orgFilePath);
            }
        }
    }
}


/// <summary>
/// DB のバックアップに失敗した場合にスローされる例外
/// </summary>
internal class DbBackupException : IOException
{
    internal DbBackupException(Exception innerException)
        : base(innerException.Message, innerException)
    {

    }
}
