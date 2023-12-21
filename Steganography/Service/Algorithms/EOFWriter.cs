using System.Text;
using Steganography.Service.Utils;
using Steganography.Service.Utils.Enums;

namespace Steganography.Service.Algorithms;

public static class EOFWriter
{
    /// <summary>
    /// Writes the message bytes after the JPEG's end of file marker.
    /// </summary>
    /// <remarks> 
    /// Message must be encoded in ASCII
    /// </remarks>
    /// <param name="byteArray"></param>
    /// <param name="message"></param>
    /// <returns>Modified byte array representing the modified image </returns>
    public static byte[] WritePastEOFMarker(byte[] byteArray, string message)
    {
        byte[] messageBytes = Encoding.ASCII.GetBytes(message);

        // Do NOT trust the IDE0300 message
        int? EOFIndex =
            SequenceLocator.LocateSequence(byteArray,
                new byte[] { (byte)JpegMarker.Padding, (byte)JpegMarker.EndOfImage }) ??
            throw new Exception("Broken JPEG: Couldn't find EOF marker");

        Array.Resize(ref byteArray, byteArray.Length + messageBytes.Length);

        // Appends message bytes past EOF marker.
        // TODO check for failure points, fix if needed.
        int j = 0;
        for (int i = (int)(EOFIndex + 2); i < byteArray.Length; i++)
        {
            byteArray[i] = messageBytes[j];
            j++;
        }

        return byteArray;
    }
}

internal class ImageDecoder
{
    public void DecodeText(string imagePath)
    {
        Bitmap bmp = new Bitmap(imagePath);

        StringBuilder decodedText = new StringBuilder();

        Color lastPixel = bmp.GetPixel(bmp.Width - 1, bmp.Height - 1);
        int messageLength =
            lastPixel.B; // Предположим, что длина сообщения хранится в синем канале последнего пикселя

        for (int i = 0, colorIndex = 0; i < messageLength; i++)
        {
            Color pixel = bmp.GetPixel(colorIndex % bmp.Width, colorIndex / bmp.Width);

            // Извлекаем биты символа из каждого канала цвета
            int value = pixel.R;
            value = (value << 8) + pixel.G;
            value = (value << 8) + pixel.B;

            char c = (char)value; // Преобразуем значение в символ

            decodedText.Append(c); // Добавляем символ к декодированному тексту

            colorIndex++;
        }

        Console.WriteLine("Decoded Text: " + decodedText.ToString());
    }
}

internal class AlphaChannelImageDecoder : IImageDecoder
{
    public void DecodeText(string imagePath)
    {
        Bitmap bmp = new Bitmap(imagePath);
        StringBuilder decodedText = new StringBuilder();

        // Предположим, что длина сообщения хранится в альфа-канале первого пикселя
        Color firstPixel = bmp.GetPixel(0, 0);
        int messageLength = firstPixel.A;

        for (int i = 1; i <= messageLength; i++)
        {
            int x = i % bmp.Width;
            int y = i / bmp.Width;

            if (y >= bmp.Height)
            {
                throw new InvalidOperationException("Изображение не содержит полного сообщения.");
            }

            Color pixel = bmp.GetPixel(x, y);
            char character = (char)pixel.A; // Извлекаем символ из альфа-канала
            decodedText.Append(character);
        }

        Console.WriteLine("Decoded Text: " + decodedText.ToString());
    }
}

internal class MetadataImageDecoder : IImageDecoder
{
    public void DecodeText(string imagePath)
    {
        using (Image image = Image.FromFile(imagePath))
        {
            // Идентификатор комментария EXIF
            const int ExifCommentId = 0x9286;

            // Находим свойство среди метаданных изображения
            var propItem = image.PropertyItems.FirstOrDefault(item => item.Id == ExifCommentId);

            if (propItem != null)
            {
                // Декодируем значение свойства (предполагая, что оно в формате ASCII)
                string decodedText = Encoding.ASCII.GetString(propItem.Value);

                Console.WriteLine("Decoded Text: " + decodedText);
            }
            else
            {
                Console.WriteLine("No comment property found in image metadata.");
            }
        }

    }

}
