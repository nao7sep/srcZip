using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace srcZip
{
    public class ZipFileInfo: IDisposable
    {
        public readonly FileInfo File; // 保存先のパス

        public readonly DirectoryInfo BaseDirectory; // 圧縮するディレクトリー

        public readonly FileStream FileStream;

        public readonly ZipArchive ZipArchive;

        private bool mIsDisposed = false;

        /// <summary>
        /// ファイルが既に存在すると例外が飛ぶ。
        /// </summary>
        public ZipFileInfo (string filePath, string baseDirectoryPath)
        {
            File = new FileInfo (filePath);
            BaseDirectory = new DirectoryInfo (Shared.NormalizeDirectoryPath (baseDirectoryPath));
            FileStream = new FileStream (filePath, FileMode.CreateNew);

            // このままでエントリー名のエンコーディングは UTF-8 でちゃんと処理されるようだ

            // ZipHelper の GetEncodedTruncatedBytesFromString と GetEncoding も参考になる
            // エントリー名に c > 126 || c < 32 の文字が一つでもあれば UTF-8 が使われる
            // https://source.dot.net/#System.IO.Compression/System/IO/Compression/ZipHelper.cs

            ZipArchive = new ZipArchive (FileStream, ZipArchiveMode.Create, leaveOpen: false);
        }

        public void Add (string filePath, string? entryRelativePath = null)
        {
            // https://docs.microsoft.com/en-us/dotnet/api/system.io.compression.zipfileextensions.createentryfromfile
            ZipArchive.CreateEntryFromFile (filePath, entryRelativePath ?? Shared.ToRelativePath (filePath, BaseDirectory));
        }

        public void Dispose ()
        {
            if (mIsDisposed == false)
            {
                ZipArchive.Dispose ();
                FileStream.Dispose ();

                GC.SuppressFinalize (this);

                mIsDisposed = true;
            }
        }
    }
}
