using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace Gaeeyo
{
    public class FontList
    {
        private static List<string> names;

        public static List<string> GetList()
        {
            if (names == null) {
                names = new List<string>()
                {
                    "Arial",
                    "Arial Black",
                    "Arial Unicode MS",
                    "Calibri",
                    "Cambria",
                    "Cambria Math",
                    "Candara",
                    "Comic Sans MS",
                    "Consolas",
                    "Constantia",
                    "Corbel",
                    "Courier New",
                    "Georgia",
                    "Lucida Grande/Lucida Sans Unicode",
                    "Segoe UI",
                    "Symbol",
                    "Tahoma",
                    "Times New Roman",
                    "Trebuchet MS",
                    "Verdana",
                    "Wingdings",
                    "Wingdings 2",
                    "Wingdings 3",
                };
                addAsiaFonts();

                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.WinCE:
                        addAsiaWindowsFonts();
                        break;
                    case PlatformID.MacOSX:
                        addMacFonts();
                        break;
                }
            }
            return names;
        }

        private static void addAsiaFonts()
        {
            addFonts( new string[] {
                "Batang",
                "Meiryo",
                "MS Gothic",
                "MS Mincho",
                "MS PGothic",
                "MS PMincho",
                "PMingLiU",
                "SimSun",
            });
        }
        private static void addAsiaWindowsFonts()
        {
            addFonts(new string[] {
                "BatangChe",
                "DFKai-SB",
                "Dotum",
                "DutumChe",
                "FangSong",
                "GulimChe",
                "Gungsuh",
                "GungsuhChe",
                "KaiTi",
                "MS UI Gothic",
                "Malgun Gothic",
                "Microsoft JhengHei",
                "Microsoft YaHei",
                "MingLiU",
                "MingLiu-ExtB",
                "MingLiu_HKSCS",
                "MingLiu_HKSCS-ExtB",
                "NSimSun",
                "NSimSun-18030",
                "PMingLiu-ExtB",
                "SimHei",
                "SimSun-18030",
                "SimSun-ExtB",
            });
        }

        private static void addMacFonts()
        {
            addFonts(new string[] {
                "AppleGothic","Gulim","Hiragino Kaku Gothic Pro","STHeiti",
            });
        }

        private static void addFonts(string[] fonts)
        {
            foreach (string s in fonts)
            {
                names.Add(s);
            }
        }
    }
}
