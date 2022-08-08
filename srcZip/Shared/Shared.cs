using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace srcZip
{
    public static class Shared
    {
        public static string NormalizeDirectoryPath (string absolutePath)
        {
            // C:\, C:\Hoge, C:\Hoge\, C:\Hoge\\ のいずれも問題なし
            return Path.GetDirectoryName (Path.Join (absolutePath, "Hoge"))!;
        }

        /// <summary>
        /// baseDirectory.FullName が NormalizeDirectoryPath を通っていること。
        /// </summary>
        public static string ToRelativePath (string absolutePath, DirectoryInfo baseDirectory)
        {
            // さまざまなパスがある
            // DOS では A:PICTURE.JPG も通るようで、全てに対応することはできない
            // https://en.wikipedia.org/wiki/Path_(computing)

            FileInfo xFile = new FileInfo (absolutePath);
            DirectoryInfo? xDirectory = xFile.Directory;

            Stack <string> xFragments = new Stack <string> ();
            xFragments.Push (xFile.Name);

            while (xDirectory != null)
            {
                // Directory/Parent プロパティーにより得られるパスは、GetDirectoryName を通す NormalizeDirectoryPath によるものと一致するはず
                // 大文字・小文字を区別してもよさそうだが、Windows ではレジストリーキーの大文字・小文字が揺れるなどもあるので区別しない

                // / の使用は、ArchivingUtils.EntryFromPath の実装およびコメントに基づく
                // https://source.dot.net/#System.IO.Compression.ZipFile/Archiving.Utils.cs

                if (xDirectory.FullName.Equals (baseDirectory.FullName, StringComparison.OrdinalIgnoreCase))
                    return string.Join ('/', xFragments); // Mac/Linux 形式のパスに

                xFragments.Push (xDirectory.Name);

                xDirectory = xDirectory.Parent;
            }

            // absolutePath と baseDirectory が同一プロセス内で同様の方法により取得されたなら起こらないはず
            throw new InvalidDataException ("そんなバナナ！");
        }

        public static void CopyDirectory (string sourcePath, string destPath)
        {
            void iCopyDirectory (DirectoryInfo sourceDirectory, DirectoryInfo destDirectory)
            {
                foreach (DirectoryInfo xSubdirectory in sourceDirectory.GetDirectories ())
                    iCopyDirectory (xSubdirectory, destDirectory.CreateSubdirectory (xSubdirectory.Name));

                foreach (FileInfo xFile in sourceDirectory.GetFiles ())
                    xFile.CopyTo (Path.Join (destDirectory.FullName, xFile.Name), overwrite: true);
            }

            if (Directory.Exists (destPath) == false)
                Directory.CreateDirectory (destPath);

            iCopyDirectory (new DirectoryInfo (sourcePath), new DirectoryInfo (destPath));
        }
    }
}
