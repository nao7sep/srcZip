using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace srcZip
{
    public class TargetDirectoryManager
    {
        public readonly DirectoryInfo BaseDirectory; // 圧縮するディレクトリー

        public readonly ParamsFileInfo ParamsFile; // srcZip.txt のパス | あるとは限らない

        public readonly List <TargetFileInfo> Files = new List <TargetFileInfo> (); // 最終的に圧縮されるファイル

        public TargetDirectoryManager (string baseDirectoryPath)
        {
            string xNormalized = Shared.NormalizeDirectoryPath (baseDirectoryPath);
            BaseDirectory = new DirectoryInfo (xNormalized);
            ParamsFile = new ParamsFileInfo (Path.Join (xNormalized, "srcZip.txt"));
        }

        // Preprocess や Scan が複数回呼ばれた場合の動作は不定

        public void Preprocess ()
        {
            foreach (ParamInfo xParam in ParamsFile.Params)
            {
                if (xParam.Kind == ParamKind.Exclude)
                {
                    // 何もしない
                }

                else if (xParam.Kind == ParamKind.Copy)
                {
                    if (xParam.Values.Count == 2 &&
                        string.IsNullOrEmpty (xParam.Values [0]) == false && Path.IsPathFullyQualified (xParam.Values [0]) == false &&
                        string.IsNullOrEmpty (xParam.Values [1]) == false && Path.IsPathFullyQualified (xParam.Values [1]) == false)
                    {
                        string xSourcePath = Path.Join (BaseDirectory.FullName, xParam.Values [0]),
                            xDestPath = Path.Join (BaseDirectory.FullName, xParam.Values [1]);

                        if (Directory.Exists (xSourcePath))
                        {
                            Shared.CopyDirectory (xSourcePath, xDestPath);

                            Console.WriteLine ("ディレクトリーをコピーしました。");
                            Console.WriteLine ("\x20\x20\x20\x20コピー元: " + xParam.Values [0]);
                            Console.WriteLine ("\x20\x20\x20\x20コピー先: " + xParam.Values [1]);
                        }

                        else if (File.Exists (xSourcePath))
                        {
                            // xDestPath は絶対にフルパス
                            string xDirectoryPath = Path.GetDirectoryName (xDestPath)!;

                            if (Directory.Exists (xDirectoryPath) == false)
                                Directory.CreateDirectory (xDirectoryPath);

                            File.Copy (xSourcePath, xDestPath, overwrite: true);

                            Console.WriteLine ("ファイルをコピーしました。");
                            Console.WriteLine ("\x20\x20\x20\x20コピー元: " + xParam.Values [0]);
                            Console.WriteLine ("\x20\x20\x20\x20コピー先: " + xParam.Values [1]);
                        }

                        else throw new InvalidDataException ("コピーするディレクトリーまたはファイルが存在しません: " + xParam.Values [0]);
                    }
                }

                else if (xParam.Kind == ParamKind.Delete)
                {
                    if (xParam.Values.Count == 1 &&
                        string.IsNullOrEmpty (xParam.Values [0]) == false && Path.IsPathFullyQualified (xParam.Values [0]) == false)
                    {
                        string xTargetPath = Path.Join (BaseDirectory.FullName, xParam.Values [0]);

                        // Visual Studio がロックしているなどで消せずにエラーになることがある
                        // Delete は Copy 前に使うもので、他のところでは Exclude が適する

                        // ディレクトリーの圧縮では、何か一つでも問題があれば最終的な ZIP ファイルの信頼性が低下する
                        // 上でのコピーも下での削除も、一度でも失敗すれば例外が投げられて Preprocess が打ち切りになるべき

                        if (Directory.Exists (xTargetPath))
                        {
                            File.SetAttributes (xTargetPath, FileAttributes.Directory);
                            Directory.Delete (xTargetPath, recursive: true);

                            Console.WriteLine ("ディレクトリーを削除しました: " + xParam.Values [0]);
                        }

                        else if (File.Exists (xTargetPath))
                        {
                            File.SetAttributes (xTargetPath, FileAttributes.Normal);
                            File.Delete (xTargetPath);

                            Console.WriteLine ("ファイルを削除しました: " + xParam.Values [0]);
                        }

                        // 「あれば消す」なので、なくても問題でない
                    }
                }

                else throw new InvalidDataException ();
            }
        }

        public void Scan ()
        {
            // 多少の高速化のために Exclude のところだけ抜き出す

            var xExcludedRelativePaths = ParamsFile.Params.Where (x =>
            {
                return x.Kind == ParamKind.Exclude && x.Values.Count == 1 &&
                    string.IsNullOrEmpty (x.Values [0]) == false && Path.IsPathFullyQualified (x.Values [0]) == false;
            }).
            Select (x => x.Values [0]);

            List <FileInfo> xFiles = new List <FileInfo> ();

            bool iIsExcluded (string absolutePath)
            {
                return xExcludedRelativePaths.Contains (Shared.ToRelativePath (absolutePath, BaseDirectory), StringComparer.OrdinalIgnoreCase);
            }

            void iScan (DirectoryInfo directory)
            {
                // ここでディレクトリーを除外するかどうか見ると、BaseDirectory を除外するかどうかの判断の処理になって落ちる
                // ディレクトリーの除外については、サブディレクトリーに関してのみ判断される

                // ディレクトリーやファイルは、最後に一括でなく、各ディレクトリー内においてそれぞれ個別にソートされる
                // そうすると、ディレクトリーから先にスキャンしていった構造のリストが得られる

                foreach (DirectoryInfo xSubdirectory in directory.GetDirectories ().OrderBy (x => x.Name, StringComparer.OrdinalIgnoreCase))
                {
                    if (iIsExcluded (xSubdirectory.FullName))
                    {
                        Console.WriteLine ("ディレクトリーが除外されました: " + Shared.ToRelativePath (xSubdirectory.FullName, BaseDirectory));
                        continue;
                    }

                    // 今のところ、空のディレクトリーのエントリーを ZIP ファイルに追加する考えはない
                    // Git でも無視されるため
                    // アーカイブして他の環境で復元する必要のあるディレクトリーならダミーのファイルを入れるとよい

                    iScan (xSubdirectory);
                }

                foreach (FileInfo xFile in directory.GetFiles ().OrderBy (x => x.Name, StringComparer.OrdinalIgnoreCase))
                {
                    if (iIsExcluded (xFile.FullName))
                    {
                        Console.WriteLine ("ファイルが除外されました: " + Shared.ToRelativePath (xFile.FullName, BaseDirectory));
                        continue;
                    }

                    // 古いコードには重複チェックが不要とのコメントが含まれていたが一応
                    // 自分が詳しくない OS のマイナーな機能により同じパスが得られる可能性も想定
                    // Files にはパスの重複がないことを保証しなければならない

                    if (xFiles.Any (x => x.FullName.Equals (xFile.FullName, StringComparison.OrdinalIgnoreCase)) == false)
                        xFiles.Add (xFile);

                    else throw new InvalidDataException ("ファイルのパスが重複しています: " + xFile.FullName);
                }
            }

            iScan (BaseDirectory);

            // entryRelativePath はデフォルトの null に
            Files.AddRange (xFiles.Select (x => new TargetFileInfo (x.FullName)));
        }

        public DateTime GetLastLastWriteTimeUtc ()
        {
            return Files.Select (x => File.GetLastWriteTimeUtc (x.FilePath)).Max ();
        }
    }
}
