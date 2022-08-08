using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace srcZip
{
    public class PathListFileInfo
    {
        public readonly FileInfo File; // 保存先のパス
        public readonly DirectoryInfo BaseDirectory; // 圧縮するディレクトリー
        public readonly List <string> RelativePaths = new List <string> (); // ここに / 区切りの相対パスが入る

        public PathListFileInfo (string filePath, string baseDirectoryPath)
        {
            File = new FileInfo (filePath);
            BaseDirectory = new DirectoryInfo (Shared.NormalizeDirectoryPath (baseDirectoryPath));
        }

        public bool Contains (string absolutePath)
        {
            return RelativePaths.Contains (Shared.ToRelativePath (absolutePath, BaseDirectory), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 以前は行われた重複チェックが今では行われない
        /// </summary>
        public void Add (string absolutePath)
        {
            RelativePaths.Add (Shared.ToRelativePath (absolutePath, BaseDirectory));
        }

        public void Save ()
        {
            if (Directory.Exists (File.DirectoryName) == false)
                Directory.CreateDirectory (File.DirectoryName!); // null にならない

            System.IO.File.WriteAllLines (File.FullName, RelativePaths, Encoding.UTF8);
        }
    }
}
