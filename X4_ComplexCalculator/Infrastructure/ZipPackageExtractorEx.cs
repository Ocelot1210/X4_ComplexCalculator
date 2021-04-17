using Onova.Services;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace X4_ComplexCalculator.Infrastructure
{
    /// <summary>
    /// <see cref="ZipPackageExtractor">ZipPackageExtractor</see>
    /// をベースに特定のフォルダ内のみを解凍する PackageExtractor
    /// </summary>
    internal class ZipExcerptPackageExtractor : IPackageExtractor
    {
        #region スタティックメンバ
        /// <summary>
        /// 解凍するフォルダのパス
        /// </summary>
        private const string _RequiredDirPath = "X4_ComplexCalculator/bin/";
        #endregion


        /// <inheritdoc />
        public async Task ExtractPackageAsync(string sourceFilePath, string destDirPath,
                                              IProgress<double>? progress = null,
                                              CancellationToken cancellationToken = default)
        {
            using var archive = ZipFile.OpenRead(sourceFilePath);
            var entries = archive.Entries
                    .Where(entry => entry.FullName.StartsWith(_RequiredDirPath))
                    .ToArray();

            var totalBytes = entries.Sum(e => e.Length);
            var totalBytesCopied = 0L;

            foreach (var entry in entries)
            {
                var fullName = entry.FullName.Replace(_RequiredDirPath, "");
                var entryDestFilePath = Path.Combine(destDirPath, fullName);
                var entryDestDirPath = Path.GetDirectoryName(entryDestFilePath);

                if (!string.IsNullOrWhiteSpace(entryDestDirPath))
                {
                    Directory.CreateDirectory(entryDestDirPath);
                }

                if (entry.FullName.EndsWith(Path.DirectorySeparatorChar)
                    || entry.FullName.EndsWith(Path.AltDirectorySeparatorChar))
                {
                    continue;
                }

                using var input = entry.Open();
                using var output = File.Create(entryDestFilePath);

                var buffer = new byte[81920];
                int bytesCopied;
                do
                {
                    bytesCopied = await input.ReadAsync(buffer, cancellationToken);
                    await output.WriteAsync(buffer, 0, bytesCopied, cancellationToken);

                    totalBytesCopied += bytesCopied;
                    progress?.Report(1.0 * totalBytesCopied / totalBytes);
                } while (bytesCopied > 0);
            }
        }
    }
}
