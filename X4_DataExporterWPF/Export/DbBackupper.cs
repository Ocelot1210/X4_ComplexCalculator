using System;
using System.IO;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// DB をバックアップするクラス
    /// </summary>
    internal sealed class DbBackupper : IDisposable
    {
        /// <summary>
        /// 破棄されたか
        /// </summary>
        private bool _Disposed;


        /// <summary>
        /// 変更が確定されたか
        /// </summary>
        private bool _Comitted;


        /// <summary>
        /// ファイルロック用の <see cref="FileStream"/>
        /// </summary>
        private readonly FileStream? _File;


        /// <summary>
        /// バックアップされたファイルパス
        /// </summary>
        private readonly string _BakFilePath = "";


        /// <summary>
        /// 元のファイルパス
        /// </summary>
        private readonly string _OrgFilePath;


        /// <summary>
        /// バックアップ対象のファイルパス
        /// </summary>
        /// <param name="filePath"></param>
        internal DbBackupper(string filePath)
        {
            _OrgFilePath = filePath;

            try
            {
                if (File.Exists(_OrgFilePath))
                {
                    _BakFilePath = Path.Combine(Path.GetDirectoryName(filePath) ?? throw new ArgumentException(), Path.GetFileName(filePath) + $"_{DateTime.Now:yyyy_MM_dd-HHmmss-fff}_bak");
                    File.Move(_OrgFilePath, _BakFilePath);
                    _File = new FileStream(_BakFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
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
                _File?.Dispose();

                // 不要になったバックアップファイルを削除する
                if (!string.IsNullOrEmpty(_BakFilePath) && File.Exists(_BakFilePath))
                {
                    File.Delete(_BakFilePath);
                }

                _Comitted = true;
            }
            catch (Exception)
            {
            }
        }


        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_Disposed)
            {
                _Disposed = true;
                _File?.Dispose();

                // 未コミット == ロールバックする必要があるか？
                if (!_Comitted && !string.IsNullOrEmpty(_BakFilePath))
                {
                    if (File.Exists(_OrgFilePath))
                    {
                        File.Delete(_OrgFilePath);
                    }

                    File.Move(_BakFilePath, _OrgFilePath);
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
}
