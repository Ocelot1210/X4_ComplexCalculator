using LibX4.FileSystem;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace LibX4.Tests.CatFileTests
{
    /// <summary>
    /// <see cref="CatFile"/> のテストクラス
    /// </summary>
    public class CatFileTest
    {
        private readonly string _BaseDir;

        public CatFileTest()
        {
            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? Environment.CurrentDirectory;

            while (!string.IsNullOrEmpty(path) && !(Path.GetDirectoryName(path) == Path.GetPathRoot(path)))
            {
                var file = Directory.GetFiles(path, "LibX4.Tests.csproj").FirstOrDefault();
                if (file is not null)
                {
                    path = Path.Combine(path, "TestData", nameof(CatFileTest));
                    break;
                }

                path = Path.GetDirectoryName(path);
            }

            _BaseDir = path ?? "";
        }

        private string MakePath(string path)
        {
            return Path.Combine(_BaseDir, path);
        }

        /// <summary>
        /// X4のバージョンを確認する
        /// </summary>
        [Fact]
        public void X4Version()
        {
            var cat = new CatFile(Path.Combine(_BaseDir, "VanillaEnvironment"));

            Assert.Equal("330", cat.Version);
        }



        #region バニラ環境のファイルを開く
        /// <summary>
        /// バニラ環境のファイルを開く
        /// </summary>
        [Fact]
        public async Task OpenVanillaFile1()
        {
            var cat = new CatFile(Path.Combine(_BaseDir, "VanillaEnvironment"));

            using var ms = await cat.OpenFileAsync("libraries/personal_infomation/personal_infomation.xml");

            using var fs = File.OpenRead(MakePath("VanillaEnvironmentOrig/01/libraries/personal_infomation/personal_infomation.xml"));

            Assert.True(IsSameStream(ms, fs));
        }


        /// <summary>
        /// バニラ環境のファイルを開く(ファイル名に空白文字あり)
        /// </summary>
        [Fact]
        public async Task OpenVanillaFile2()
        {
            var cat = new CatFile(Path.Combine(_BaseDir, "VanillaEnvironment"));

            using var ms = await cat.OpenFileAsync("libraries/personal_infomation/personal infomation.xml");

            using var fs = File.OpenRead(MakePath("VanillaEnvironmentOrig/01/libraries/personal_infomation/personal infomation.xml"));

            Assert.True(IsSameStream(ms, fs));
        }


        /// <summary>
        /// バニラ環境のファイルを開く(フォルダ名に空白文字あり)
        /// </summary>
        [Fact]
        public async Task OpenVanillaFile3()
        {
            var cat = new CatFile(Path.Combine(_BaseDir, "VanillaEnvironment"));

            using var ms = await cat.OpenFileAsync("libraries/personal infomation/personal_infomation.xml");

            using var fs = File.OpenRead(MakePath("VanillaEnvironmentOrig/01/libraries/personal infomation/personal_infomation.xml"));

            Assert.True(IsSameStream(ms, fs));
        }


        /// <summary>
        /// バニラ環境のファイルを開く(フォルダ名とファイル名に空白文字あり)
        /// </summary>
        [Fact]
        public async Task OpenVanillaFile4()
        {
            var cat = new CatFile(Path.Combine(_BaseDir, "VanillaEnvironment"));

            using var ms = await cat.OpenFileAsync("libraries/personal infomation/personal infomation.xml");

            using var fs = File.OpenRead(MakePath("VanillaEnvironmentOrig/01/libraries/personal infomation/personal infomation.xml"));

            Assert.True(IsSameStream(ms, fs));
        }


        /// <summary>
        /// バニラ環境のファイルを開く(存在しないファイル)
        /// </summary>
        [Fact]
        public async Task OpenVanillaFile5()
        {
            var ex = await Assert.ThrowsAsync<FileNotFoundException>(async () =>
            {
                var cat = new CatFile(Path.Combine(_BaseDir, "VanillaEnvironment"));

                using var ms = await cat.OpenFileAsync("libraries/not_exist.txt");
            });

            Assert.Contains("libraries/not_exist.txt", ex.Message);
        }


        /// <summary>
        /// バニラ環境のファイルを開く(存在しないファイル)
        /// </summary>
        [Fact]
        public async Task OpenVanillaFile6()
        {
            var cat = new CatFile(Path.Combine(_BaseDir, "VanillaEnvironment"));

            using var ms = await cat.TryOpenFileAsync("libraries/not_exist.txt");

            Assert.Null(ms);
        }


        /// <summary>
        /// バニラ環境のファイルを開く(数字が大きい方が優先される)
        /// </summary>
        [Fact]
        public async Task OpenVanillaFile7()
        {
            var cat = new CatFile(Path.Combine(_BaseDir, "VanillaEnvironment"));

            using var ms = await cat.OpenFileAsync("libraries/personal_infomation.xml");

            using var fs = File.OpenRead(MakePath("VanillaEnvironmentOrig/02/libraries/personal_infomation.xml"));

            Assert.True(IsSameStream(ms, fs));
        }
        #endregion



        #region Mod入でバニラ環境のファイルを開く
        /// <summary>
        /// Mod入の環境でバニラのファイルを開く
        /// </summary>
        [Fact]
        public async Task OpenVanillaFileWithMods1()
        {
            var cat = new CatFile(Path.Combine(_BaseDir, "ModEnvironment"));

            using var ms = await cat.OpenFileAsync("libraries/personal_infomation/personal_infomation.xml");

            using var fs = File.OpenRead(MakePath("ModEnvironmentOrig/01/libraries/personal_infomation/personal_infomation.xml"));

            Assert.True(IsSameStream(ms, fs));
        }


        /// <summary>
        /// Mod入の環境でバニラのファイルを開く(ファイル名に空白文字あり)
        /// </summary>
        [Fact]
        public async Task OpenVanillaFileWithMods2()
        {
            var cat = new CatFile(Path.Combine(_BaseDir, "ModEnvironment"));

            using var ms = await cat.OpenFileAsync("libraries/personal_infomation/personal infomation.xml");

            using var fs = File.OpenRead(MakePath("ModEnvironmentOrig/01/libraries/personal_infomation/personal infomation.xml"));

            Assert.True(IsSameStream(ms, fs));
        }


        /// <summary>
        /// Mod入の環境でバニラのファイルを開く(フォルダ名に空白文字あり)
        /// </summary>
        [Fact]
        public async Task OpenVanillaFileWithMods3()
        {
            var cat = new CatFile(Path.Combine(_BaseDir, "ModEnvironment"));

            using var ms = await cat.OpenFileAsync("libraries/personal infomation/personal_infomation.xml");

            using var fs = File.OpenRead(MakePath("ModEnvironmentOrig/01/libraries/personal infomation/personal_infomation.xml"));

            Assert.True(IsSameStream(ms, fs));
        }


        /// <summary>
        /// Mod入の環境でバニラのファイルを開く(フォルダ名とファイル名に空白文字あり)
        /// </summary>
        [Fact]
        public async Task OpenVanillaFileWithMods4()
        {
            var cat = new CatFile(Path.Combine(_BaseDir, "ModEnvironment"));

            using var ms = await cat.OpenFileAsync("libraries/personal infomation/personal infomation.xml");

            using var fs = File.OpenRead(MakePath("ModEnvironmentOrig/01/libraries/personal infomation/personal infomation.xml"));

            Assert.True(IsSameStream(ms, fs));
        }
        #endregion



        #region Modのファイルを開く
        /// <summary>
        /// Modのファイルを開く
        /// </summary>
        [Fact]
        public async Task OpenModFile1()
        {
            var cat = new CatFile(Path.Combine(_BaseDir, "ModEnvironment"));

            using var ms = await cat.OpenFileAsync("libraries/foo_mod_personal_infomation/personal_infomation.xml");

            using var fs = File.OpenRead(MakePath("ModEnvironmentOrig/extensions/foo_mod/ext_01/libraries/foo_mod_personal_infomation/personal_infomation.xml"));

            Assert.True(IsSameStream(ms, fs));
        }
        

        /// <summary>
        /// Modの環境でcatファイルに固められていないファイルを開く
        /// </summary>
        [Fact]
        public async Task OpenModFile2()
        {
            var cat = new CatFile(Path.Combine(_BaseDir, "PlaneFileModEnvironment"));

            using var ms = await cat.OpenFileAsync("libraries/foo_mod_personal_infomation/personal_infomation.xml");

            using var fs = File.OpenRead(MakePath("PlaneFileModEnvironment/extensions/foo_mod/libraries/foo_mod_personal_infomation/personal_infomation.xml"));

            Assert.True(IsSameStream(ms, fs));
        }
        #endregion


        #region Indexファイルに記載されているファイルを開く
        /// <summary>
        /// Indexファイルに記載されているファイルを開く
        /// </summary>
        [Fact]
        public async Task OpenIndexFile1()
        {
            var cat = new CatFile(Path.Combine(_BaseDir, "VanillaIndexXml"));

            var xml1 = await cat.OpenIndexXmlAsync("index/macros.xml", "foo_macro");

            var xml2 = XDocument.Load(MakePath("VanillaIndexXmlOrig/01/assets/fx/macros/foo_macro.xml"));

            Assert.True(XNode.DeepEquals(xml1, xml2));
        }


        /// <summary>
        /// Indexファイルに記載されていないファイルを開こうとする
        /// </summary>
        [Fact]
        public async Task OpenIndexFile2()
        {
            var ex = await Assert.ThrowsAsync<FileNotFoundException>(async () =>
            {
                var cat = new CatFile(Path.Combine(_BaseDir, "VanillaIndexXml"));

                var xml1 = await cat.OpenIndexXmlAsync("index/macros.xml", "not_exists_macro");
            });
        }


        /// <summary>
        /// Indexファイルに記載されているファイルを開く(Modのデータ)
        /// </summary>
        [Fact]
        public async Task OpenIndexFile3()
        {
            var cat = new CatFile(Path.Combine(_BaseDir, "ModIndexXml"));

            var xml1 = await cat.OpenIndexXmlAsync("index/macros.xml", "baz_macro");

            var xml2 = XDocument.Load(MakePath("ModIndexXmlOrig/extensions/baz_mod/ext_01/assets/fx/macros/baz_macro.xml"));

            Assert.True(XNode.DeepEquals(xml1, xml2));
        }
        #endregion



        /// <summary>
        /// 2つの <see cref="Stream"/> が同じかどうか判定する
        /// </summary>
        /// <param name="s1">判定対象1</param>
        /// <param name="s2">判定対象2</param>
        /// <returns>2つの <see cref="Stream"/> が同じか</returns>
        private bool IsSameStream(Stream s1, Stream s2)
        {
            if (s1.Length != s2.Length)
            {
                return false;
            }

            for (var i = 0L; i < s1.Length; i++)
            {
                if (s1.ReadByte() != s2.ReadByte())
                {
                    return false;
                }
            }

            return true;
        }
    }
}
