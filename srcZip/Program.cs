namespace srcZip
{
    internal class Program
    {
        static void Main (string [] args)
        {
            try
            {
                int xErrorCount = 0;
                List <string> xPaths = new List <string> ();

                foreach (string xArg in args)
                {
                    string xTrimmed = xArg.Trim ();

                    if (xTrimmed.Length > 0)
                    {
                        if (Path.IsPathFullyQualified (xTrimmed) == false)
                        {
                            xErrorCount ++;
                            Console.WriteLine ("不正なパスです: " + xTrimmed);
                        }

                        else if (Directory.Exists (xTrimmed) == false)
                        {
                            xErrorCount ++;
                            Console.WriteLine ("ディレクトリーが存在しません: " + xTrimmed);
                        }

                        else
                        {
                            if (xPaths.Contains (xTrimmed, StringComparer.OrdinalIgnoreCase) == false)
                                xPaths.Add (xTrimmed);
                        }
                    }
                }

                if (xErrorCount > 0)
                    return; // finally へ

                if (xPaths.Count == 0)
                {
                    Console.WriteLine ("圧縮するディレクトリーをプログラムの実行ファイルにドラッグ＆ドロップしてください。");
                    return;
                }

                foreach (string xPath in xPaths)
                {
                    // いつの間にかなくなっているかもしれないので存在を再チェック

                    if (Directory.Exists (xPath))
                    {
                        try
                        {
                            Console.WriteLine ("ディレクトリーの圧縮を開始しました: " + xPath);

                            TargetDirectoryManager xDirectoryManager = new TargetDirectoryManager (xPath);
                            xDirectoryManager.Preprocess ();
                            xDirectoryManager.Scan ();

                            if (xDirectoryManager.Files.Count == 0)
                            {
                                Console.WriteLine ("圧縮するファイルがありません: " + xPath);
                                continue;
                            }

                            // Windows のエクスプローラーでは "Hoge   .txt" を作れる
                            // Mac の Finder では "   Hoge   .txt" も作れる
                            // しかし、ファイル名の先頭や末尾に空白系文字を入れるのはユーザーの自由なので Trim しない

                            string xZipFilePath = Path.Join (Path.GetDirectoryName (xPath),
                                $"{Path.GetFileName (xPath)}-{xDirectoryManager.GetLastLastWriteTimeUtc ().ToString ("yyyyMMdd'T'HHmmss'Z'")}.zip");

                            if (Directory.Exists (xZipFilePath) || File.Exists (xZipFilePath))
                            {
                                Console.WriteLine ("ZIP ファイルを作成できません: " + xZipFilePath);
                                continue;
                            }

                            // ZIP ファイルを新たに作れるなら、ファイルパスリストの方のファイルは上書き前提でよい

                            using (ZipFileInfo xZipFile = new ZipFileInfo (xZipFilePath, xPath))
                            {
                                PathListFileInfo xListFile = new PathListFileInfo (Path.ChangeExtension (xZipFilePath, ".txt"), xPath);

                                foreach (TargetFileInfo xFile in xDirectoryManager.Files)
                                {
                                    xZipFile.Add (xFile.FilePath!);
                                    xListFile.Add (xFile.FilePath!);
                                }

                                xListFile.Save ();
                            }

                            Console.WriteLine ("ディレクトリーの圧縮に成功しました: " + xPath);
                        }

                        catch (Exception xException)
                        {
                            Console.WriteLine ("ディレクトリーの圧縮に失敗しました:" + xPath);
                            Console.WriteLine (xException.ToString ().TrimEnd ());
                        }
                    }
                }
            }

            catch (Exception xException)
            {
                Console.WriteLine ("エラーが発生しました:");
                Console.WriteLine (xException.ToString ().TrimEnd ());
            }

            finally
            {
                Console.Write ("プログラムを終了するには、任意のキーを押してください: ");
                Console.ReadKey (true);
                Console.WriteLine ();
            }
        }
    }
}
