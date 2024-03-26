using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using System.Drawing;

class Program
{
    static async Task Main(string[] args)
    {
        string containerUrl = "https://inversionrecruitment.blob.core.windows.net/find-the-code";
        string connectionString = "<your_storage_account_connection_string>";

        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("find-the-code");

        // Create a directory to store downloaded images
        string downloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DownloadedImages");
        Directory.CreateDirectory(downloadDirectory);

        // Download all PNG files
        await DownloadImages(containerClient, downloadDirectory);

        // Solve the puzzle
        int[,] solvedPuzzle = SolvePuzzle(downloadDirectory);
        int answer = ExtractAnswer(solvedPuzzle);

        Console.WriteLine("The number represented by the solved image is: " + answer);
    }

    static async Task DownloadImages(BlobContainerClient containerClient, string downloadDirectory)
    {
        await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
        {
            BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);
            string downloadFilePath = Path.Combine(downloadDirectory, blobItem.Name);
            await blobClient.DownloadToAsync(downloadFilePath);
        }
    }

    static int[,] SolvePuzzle(string downloadDirectory)
    {
        int[,] puzzle = new int[30, 40];

        foreach (string filePath in Directory.GetFiles(downloadDirectory))
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            int index = int.Parse(fileName);

            using (Bitmap bitmap = new Bitmap(filePath))
            {
                int row = (index - 1) / 40;
                int col = (index - 1) % 40;

                for (int i = 0; i < bitmap.Width; i++)
                {
                    for (int j = 0; j < bitmap.Height; j++)
                    {
                        puzzle[row * bitmap.Height + j, col * bitmap.Width + i] = bitmap.GetPixel(i, j).ToArgb();
                    }
                }
            }
        }

        return puzzle;
    }

    static int ExtractAnswer(int[,] solvedPuzzle)
    {
        // Assuming the black border is 10 pixels wide
        int startX = 10;
        int endX = 10 + 20; // Width of the answer region
        int startY = 10;
        int endY = 10 + 20; // Height of the answer region

        string answerStr = "";

        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                Color color = Color.FromArgb(solvedPuzzle[y, x]);
                // Assuming the number is black and the background is white
                if (color.GetBrightness() < 0.5) // Considering brightness to detect black
                {
                    answerStr += "1";
                }
                else
                {
                    answerStr += "0";
                }
            }
        }

        return Convert.ToInt32(answerStr, 2);
    }
}
