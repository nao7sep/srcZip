using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace srcZip
{
    public class ParamsFileInfo
    {
        public readonly FileInfo File; // 読み込むファイルのパス

        private List <ParamInfo>? mParams = null;

        public List <ParamInfo> Params
        {
            get
            {
                if (mParams == null)
                    Load ();

                return mParams!;
            }
        }

        public ParamsFileInfo (string path)
        {
            File = new FileInfo (path);
        }

        public void Load ()
        {
            mParams = new List <ParamInfo> ();

            if (File.Exists)
            {
                ParamKind? xKind = null;

                // インデントに対応するため行頭の空白系文字も削る

                foreach (string xLine in System.IO.File.ReadAllLines (File.FullName, Encoding.UTF8).
                    Select (x => x.Trim ()).Where (x => x.Length > 0 && x.StartsWith ("//") == false))
                {
                    if (xLine [0] == '[' && xLine.Length >= 3 && xLine [xLine.Length - 1] == ']')
                    {
                        string xKindString = xLine.Substring (1, xLine.Length - 2);

                        if (Enum.TryParse <ParamKind> (xKindString, ignoreCase: true, out ParamKind xResult))
                            xKind = xResult;

                        else throw new FormatException ("パラメーターの種類を認識できません: " + xKindString);
                    }

                    else
                    {
                        if (xKind == null)
                            throw new FormatException ("パラメーターの種類が不明です。");

                        ParamInfo xParam = new ParamInfo (xKind.Value);
                        xParam.Values.AddRange (xLine.Split ('>', StringSplitOptions.TrimEntries));
#if DEBUG
                        Console.WriteLine ("パラメーターの種類: " + xParam.Kind);

                        foreach (string xValue in xParam.Values)
                            Console.WriteLine ("\x20\x20\x20\x20" + xValue);
#endif
                        mParams.Add (xParam);
                    }
                }
            }
        }
    }
}
