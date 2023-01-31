using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerHandler.Classes
{
    public delegate void ProgressChangeDelegate(double Percentage, ref bool Cancel);
    public delegate void ProgressChangeNoCancelDelegate(double Percentage);
    public delegate void Completedelegate();
    public class CustomFileCopier
    {


        public string SourceFilePath { get; set; }
        public string DestDirPath { get; set; }

        public event ProgressChangeDelegate OnProgressChanged;
        public event ProgressChangeNoCancelDelegate OnProgressChangedNoCancel;
        public event Completedelegate OnComplete;
        public CustomFileCopier(string Source, string DestDir)
        {
            this.SourceFilePath = Source;
            this.DestDirPath = DestDir;

            OnProgressChangedNoCancel += delegate { };
            OnProgressChanged += delegate { };
            OnComplete += delegate { };
        }
        public void Copy()
        {
            byte[] buffer = new byte[1024 * 1024]; // 1MB buffer
            bool cancelFlag = false;

            using (FileStream source = new FileStream(SourceFilePath, FileMode.Open, FileAccess.Read))
            {
                long fileLength = source.Length;
                string fileName = Path.GetFileName(SourceFilePath);

                using (FileStream dest = new FileStream(Path.Combine(DestDirPath, fileName), FileMode.OpenOrCreate, FileAccess.Write))
                {
                    long totalBytes = 0;
                    int currentBlockSize = 0;

                    while ((currentBlockSize = source.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        totalBytes += currentBlockSize;
                        double percentage = (double)totalBytes * 100.0 / fileLength;

                        dest.Write(buffer, 0, currentBlockSize);

                        cancelFlag = false;
                        OnProgressChanged(percentage, ref cancelFlag);
                        OnProgressChangedNoCancel(percentage);
                        if (cancelFlag == true)
                        {
                            // Delete dest file here
                            break;
                        }
                    }
                }
            }

            OnComplete();
        }
    }
}
