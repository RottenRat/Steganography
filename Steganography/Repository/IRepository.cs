using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Collections.Generic;

using System.Threading.Tasks;

namespace Steganography.Repository
{
    public interface IRepository
    {
  
        Bitmap LoadImageToBitmap(string imagePath);
        string SaveImageFromBitmap(Bitmap image);
        byte[] LoadImageToBytes(string imageFolder, string imagePath);
        string SaveImageFromBytes(byte[] image);
        List<string> GetImageFilesInDirectory(string directoryPath);
    }
}
