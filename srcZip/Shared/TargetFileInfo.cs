using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace srcZip
{
    public class TargetFileInfo
    {
        public readonly string FilePath;

        // 今のところ不要だが、ZipArchive.CreateEntryFromFile と整合させておく
        public readonly string? EntryRelativePath;

        public TargetFileInfo (string filePath, string? entryRelativePath = null)
        {
            FilePath = filePath;
            EntryRelativePath = entryRelativePath;
        }
    }
}
