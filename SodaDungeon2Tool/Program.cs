﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SodaDungeon2Tool.Utils;

namespace SodaDungeon2Tool
{
    public class Program
    {
        public static IntPtr sodaGame;
        public static bool shutDownOnFinish = false;
        static void Main(string[] args)
        {
            //1264x720
            while (Process.GetProcessesByName("SodaDungeon2").Length < 1)
            {
                WriteToConsole.Error("Could not detect the Game!");
                WriteToConsole.Text("Please make sure, that the game is running. Press Enter once you have started the game.");
                Console.ReadLine();
            }

            sodaGame = Process.GetProcessesByName("SodaDungeon2")[0].MainWindowHandle;
            Configuration config = new Configuration();
            config.Save();
            while (true)
            {
                WriteToConsole.Text("What would you like to Do? Type the coresponding Number.");
                string shutdownStatus = (shutDownOnFinish) ? "#Col:Green#Enabled#" : "#Col:Red#Disabled#";
                WriteToConsole.Text($"\t#Col:Green#1# : Start\n\t#Col:Green#2# : change your Configuration\n\t#Col:Green#3# : Change option 'Shutdown on finish' currently set to: {shutdownStatus}\n\t#Col:Green#4# : Exit");
                string userInput = Console.ReadLine();
                if (userInput == "1")
                {
                    RunTool(config);
                }
                else if (userInput == "2")
                {
                    ChangeConfiguration(config);
                }
                else if(userInput == "3")
                {
                    shutDownOnFinish = !shutDownOnFinish;
                }
                else if (userInput == "4")
                {
                    break;
                }
                else
                {
                    WriteToConsole.Error("Could not Read Input!");
                    continue;
                }
                Console.Clear();
            }
        }

        private static void ChangeConfiguration(Configuration config)
        {
            while (true)
            {
                WriteToConsole.Text("Current Settings:\n\t" + config.ToString());
                WriteToConsole.Text("Which Setting would you like to change?");
                WriteToConsole.Text("\t#Col:Green#1# : Check Interval\n\t#Col:Green#2# : Notify on finish\n\t#Col:Green#3# : Number of Notifications\n\t#Col:Green#4# : Back to Main Menu");
                string userInput = Console.ReadLine();
                if (userInput == "1")
                {
                    Console.WriteLine("Please Enter the Number of Seconds:");
                    try
                    {
                        config.sleepTimerInSeconds = int.Parse(Console.ReadLine());
                    }
                    catch(FormatException ex)
                    {
                        WriteToConsole.Error("Could not Read Input!");
                        continue;
                    }
                }
                else if (userInput == "2")
                {
                    config.notifyOnFinish = !config.notifyOnFinish;
                }
                else if (userInput == "3")
                {
                    Console.WriteLine("Please Enter the Number of Notifications:");
                    try
                    {
                        config.numberOfNotifications = int.Parse(Console.ReadLine());
                    }
                    catch (FormatException ex)
                    {
                        WriteToConsole.Error("Could not Read Input!");
                        continue;
                    }
                }
                else if (userInput == "4")
                {
                    config.Save();
                    return;
                }
                else
                {
                    WriteToConsole.Error("Could not Read Input!"); ;
                    continue;
                }
                Console.Clear();
            }
        }

        private static void RunTool(Configuration config)
        {
            while (true)
            {
                Bitmap image = TakeScreenshot();
                string time = DateTime.Now.ToString("t");
                if (HasExitButton(image))
                {
                    WriteToConsole.Text($"{time} - The Run #Col:Green#ended!#");
                    if (config.notifyOnFinish == true)
                    {
                        for(int i = 0; i < config.numberOfNotifications; i++)
                        {
                            Console.Beep();
                            Thread.Sleep(300);
                        }
                    }
                    if (shutDownOnFinish == true)
                    {
                        Process.Start("shutdown", "/s /t 0");
                    }
                    break;

                }
                WriteToConsole.Text($"{time} - The Run #Col:Red#did not end yet!#");
                Thread.Sleep(config.sleepTimerInSeconds * 1000);
            }
            WriteToConsole.Text("#Col:Green#Done!# Press any Key to get Back to the Main Menu.");
            Console.ReadKey();
        }

        private static bool HasExitButton(Bitmap image)
        {
            //[A=255, R=44, G=86, B=153] 524, 640
            Color[] colorFields = new Color[4];
            Color target = Color.FromArgb(255, 44, 86, 153);
            
            colorFields[0] = image.GetPixel(524, 640);
            colorFields[1] = image.GetPixel(750, 640);
            colorFields[2] = image.GetPixel(750, 660);
            colorFields[3] = image.GetPixel(524, 660);

            for (int i = 0; i < 4; i++)
            {
                if(colorFields[i].ToArgb() != target.ToArgb())
                {
                    return false;
                }
            }
            return true;
        }

        private static Bitmap TakeScreenshot()
        {
            bool wasMinimized = Window.IsMinimized(sodaGame);
            Window.Restore(sodaGame);
            Bitmap image = ScreenCapture.CaptureWindow(sodaGame);
            if (wasMinimized == true) Window.Minimize(sodaGame);
            return ResizeImage(image, 1264, 720);
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
