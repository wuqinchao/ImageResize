using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;

namespace ImageResize
{
    class Program
    {
        public class Options
        {
            [Option('f', "file", Required = true, HelpText = "需要处理的文件或目录。")]
            public string Files { get; set; }

            [Option('r', "recursive", Required = false, HelpText = "是否遍历子目录，在file为目录时生效，默认为false")]
            public bool Recursive { get; set; } = false;

            [Option('w', "width", Required = false, HelpText = "新图片宽，指定高度时0为等比例宽，高宽均不指定时为原图宽")]
            public int Width { get; set; } = 0;

            [Option('h', "height", Required = false, HelpText = "新图片高，指定宽度时0为等比例高，高宽均不指定时为原图高")]
            public int Height { get; set; } = 0;

            [Option('o', "override", Required = false, HelpText = "是否覆盖原有文件，默认false，如指定，则无论如何均会删除原图")]
            public bool Override { get; set; } = false;

            [Option('e', "ext", Required = false, HelpText = "输出图片的格式，默认Jpeg，值为MemoryBmp、Bmp、Emf、Wmf、Gif、Jpeg、Png、Tiff、Exif、Icon 之一")]
            public string Ext { get; set; } = "";

            [Option('q', "quiet", Required = false, HelpText = "是否静默处理, 不显示处理进度，默认为false")]
            public bool Quiet { get; set; } = false;
        }
        /// <summary>
        /// 是否静默处理, 不显示处理进度，默认为false
        /// </summary>
        private static bool Quiet = false;
        /// <summary>
        /// 输出文件默认后缀
        /// </summary>
        private static string outputExt = ".jpg";
        /// <summary>
        /// 输入文件默认格式
        /// </summary>
        private static ImageFormat outputFmt = ImageFormat.Jpeg;

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
        }

        private static void Run(Options option)
        {
            if(!string.IsNullOrWhiteSpace(option.Ext))
            {
                outputFmt = ParseImageFormat(option.Ext);
                outputExt = $".{option.Ext.ToLower()}";
            }
            Quiet = option.Quiet;
            if(Directory.Exists(option.Files))
            {
                FetchDir(option.Files, option);
            }
            else if(File.Exists(option.Files))
            {
                if(!".jpg.jpeg.gif.png.bmp".Contains(Path.GetExtension(option.Files).ToLower()))
                {
                    Console.WriteLine("不支持此文件格式，只接受 jpg jpeg gif png bmp");
                    return;
                }
                try
                {
                    Work(option.Files, option);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            else
            {
                Console.WriteLine("指定文件或目录不存在");
            }
        }

        private static void FetchDir(string root, Options option)
        {
            string[] pics = Directory.GetFiles(root)
                .Where(
                    x => x.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                       | x.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                       | x.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
                       | x.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase)
                       | x.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                ).ToArray();
            if(pics.Length>0)
            {
                foreach(string pic in pics)
                {
                    try
                    {
                        Work(pic, option);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Query();
                    }
                }
            }
            if(option.Recursive)
            {
                string[] dirs = Directory.GetDirectories(root);
                if(dirs.Length>0)
                {
                    foreach(string dir in dirs)
                    {
                        FetchDir(dir, option);
                    }
                }
            }
        }

        private static void Work(string file, Options option)
        {
            Bitmap target = null;
            int width = 0, height = 0;
            using (Image org = Image.FromFile(file))
            {
                if (option.Height == 0 && org.Width == option.Width && outputExt.ToLower() == Path.GetExtension(file).ToLower())
                {
                    Print($"{file}无需处理");
                    return;
                }
                else if (option.Width == 0 && org.Height == option.Height && outputExt.ToLower() == Path.GetExtension(file).ToLower())
                {
                    Print($"{file}无需处理");
                    return;
                }
                else if(option.Width == 0 && option.Height == 0)
                {
                    if(outputExt.ToLower() == Path.GetExtension(file).ToLower())
                    {
                        Print($"{file}无需处理");
                        return;
                    }
                    else // 只转换格式
                    {
                        width = org.Width;
                        height = org.Height;
                    }
                }
                else if(option.Width == 0 || option.Height == 0)
                {
                    if (option.Height == 0)
                    {
                        width = option.Width;
                        height = decimal.ToInt32(decimal.Parse(option.Width.ToString()) / decimal.Parse(org.Width.ToString()) * decimal.Parse(org.Height.ToString()));
                    }
                    if (option.Width == 0)
                    {
                        height = option.Height;
                        width = decimal.ToInt32(decimal.Parse(option.Height.ToString()) / decimal.Parse(org.Height.ToString()) * decimal.Parse(org.Width.ToString()));
                    }
                }
                else
                {
                    width = option.Width;
                    height = option.Height;
                }
                target = new Bitmap(width, height);
                var g = Graphics.FromImage(target);
                g.DrawImage(org, 
                    new Rectangle(0, 0, target.Width, target.Height), 
                    new Rectangle(0, 0, org.Width, org.Height), 
                    GraphicsUnit.Pixel);
                g.Dispose();
            }
            if(option.Override)
            {
                try
                {
                    File.Delete(file);
                    Print($"{file}已删除");
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Query();
                }
                
            }
            string path = Path.Combine(Path.GetDirectoryName(file), GetNewFileName(file, option.Override));
            target.Save(path, outputFmt);
            target.Dispose();
            Print($"{path}已处理完成");
        }
        public static void Print(string info)
        {
            if(!Quiet)
            {
                Console.WriteLine(info);
            }
        }
        public static void Query()
        {
            Console.WriteLine("是否继续? (yes/no, y/n)");
            var input = Console.ReadLine();
            if(input.ToLower()=="no" || input.ToLower() == "n")
            {
                Environment.Exit(1);
            }
        }
        public static string GetNewFileName(string file, bool over)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var num = 0;
            var ext = outputExt.ToLower() == ".jpeg" ? ".jpg" : outputExt.ToLower();
            if (!over)
            {
                var name_new = name + (num == 0 ? "" : num.ToString());
                while(File.Exists(Path.Combine(Path.GetDirectoryName(file), $"{name_new}{ext}")))
                {
                    num++;
                    name_new = name + (num == 0 ? "" : num.ToString());
                }
                name = name_new;
            }
            return $"{name}{ext}";
        }
        public static ImageFormat ParseImageFormat(string fmt)
        {
            return (ImageFormat)typeof(ImageFormat)
                    .GetProperty(fmt, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase)
                    .GetValue(null);
        }
    }
}
