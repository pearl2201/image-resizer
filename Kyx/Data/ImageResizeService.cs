using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using SkiaSharp;
using ElectronNET.API;
using ElectronNET.API.Entities;

namespace Kyx.Data
{
    public class ImageResizeService
    {


        public ImageResizeService()
        {

        }

        public async Task<string> OnSelectFolder()
        {
            var mainWindow = Electron.WindowManager.BrowserWindows.First();
            var folders = await Electron.Dialog.ShowOpenDialogAsync(mainWindow, new ElectronNET.API.Entities.OpenDialogOptions() { Properties = new ElectronNET.API.Entities.OpenDialogProperty[] { ElectronNET.API.Entities.OpenDialogProperty.openDirectory } });
            if (folders.Length > 0)
            {
                return folders[0];
            }
            return String.Empty;
        }

        public async Task<string> OnSelectExportFolder()
        {
            var mainWindow = Electron.WindowManager.BrowserWindows.First();
            OpenDialogOptions options = new OpenDialogOptions()
            {
                Properties = new ElectronNET.API.Entities.OpenDialogProperty[] { ElectronNET.API.Entities.OpenDialogProperty.openDirectory }
            };
            var folders = await Electron.Dialog.ShowOpenDialogAsync(mainWindow, options);
            if (folders.Length > 0)
            {
                return folders[0];
            }
            return String.Empty;
        }

        public ResizeResult ResizeFolder(string directoryPath, string directoryExportPath, float width, float height)
        {
            if (!Directory.Exists(directoryPath))
            {
                return new ResizeResult()
                {
                    Success = false,
                    ErrorMessage = "Directory is not exists"
                };
            }

            if (!Directory.Exists(directoryExportPath))
            {
                if (Directory.CreateDirectory(directoryExportPath).Exists)
                {
                    return new ResizeResult()
                    {
                        Success = false,
                        ErrorMessage = "Cannot create output folder"
                    };
                }
            }

            foreach (var file in Directory.GetFiles(directoryPath))
            {
                if (file.EndsWith(".png") || file.EndsWith(".jpg"))
                {
                    try
                    {
                        var bitmap = SKBitmap.Decode(file);
                        Console.WriteLine("Image Path: " + file);
                        var info = new SKImageInfo((int)width, (int)height);
                        var surface = SKSurface.Create(info);
                        // the the canvas and properties
                        var canvas = surface.Canvas;

                        // make sure the canvas is blank
                        canvas.Clear(SKColors.Transparent);

                        var resizeFactor = 1f;
                        if (bitmap.Width * height / width > bitmap.Height)
                        {

                            resizeFactor = width / bitmap.Width;
                        }
                        else
                        {
                            resizeFactor = height / bitmap.Height;
                        }
                        Console.WriteLine($"OWidth: {bitmap.Width}, OHeight: {bitmap.Height}, ratio: {resizeFactor}, destWidth: {(int)(bitmap.Width * resizeFactor)}, destHeight: {(int)(bitmap.Height * resizeFactor)}");

                        var targetSize = (int)Math.Min(width, height);
                        SKImageInfo nearest = new SKImageInfo((int)(bitmap.Width * resizeFactor), (int)(bitmap.Height * resizeFactor));
                        //SKImageInfo nearest = new SKImageInfo(targetSize, targetSize);
                        var newImage = bitmap.Resize(nearest, SKFilterQuality.High);
                        canvas.DrawBitmap(newImage, new SKPoint((width - newImage.Width) / 2f, (height - newImage.Height) / 2f));
                        var image = surface.Snapshot();
                        //var image = SKImage.FromBitmap(toBitmap);
                        var data = image.Encode(SKEncodedImageFormat.Png, 90);



                        using (var stream = new FileStream(Path.Combine(directoryExportPath, Path.ChangeExtension(Path.GetFileName(file), ".png")), FileMode.OpenOrCreate, FileAccess.Write))
                            data.SaveTo(stream);

                        bitmap.Dispose();
                    }
                    catch (Exception e)
                    {
                        return new ResizeResult()
                        {
                            Success = false,
                            ErrorMessage = "Error on file: " + file + "\n Exception: " + e.ToString()
                        };
                    }

                }

            }


            return new ResizeResult()
            {
                Success = true,
                ErrorMessage = "Resize Success"
            };


        }

        public class ResizeResult
        {
            public bool Success { get; set; }

            public string ErrorMessage { get; set; }
        }
    }
}
