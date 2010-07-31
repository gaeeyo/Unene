/*(c) Copyright SilverlightChina
 *Author By: 八爪熊
 *From:http://SilverlightChina.com
 *Use Gif File in Silverlight 3  - (Silverlight 3 使用Gif图片)
 *Mail & MSN:qbaby0427@hotmail.com
 *Thanks for Herberth updeted to Silverlight 3 RTW
 *Version 1.03
 */

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
using System.IO;
using System.Windows.Media.Imaging;
using System.Collections.Generic;


namespace GifImageLib
{
    class GifAnimation
    {
        public GifAnimation()
        {

        }
        /// <summary>
        ///  File read status: No errors.| 文件读取无错误
        /// </summary>
        public static readonly int STATUS_OK = 0;

        /// <summary>
        /// File read status: Error decoding file (may be partially decoded) | 解码时出现错误
        /// </summary>
        public static readonly int STATUS_FORMAT_ERROR = 1;

        /// <summary>
        /// File read status: Unable to open source. | 文件打开出错
        /// </summary>
        public static readonly int STATUS_OPEN_ERROR = 2;

        protected Stream inStream;
        protected int status;

        /// <summary>
        /// 图片宽/full image width
        /// </summary>
        protected int width;
        /// <summary>
        /// 图片高/full image height
        /// </summary>
        protected int height; 

        public double Width
        {
            get
            {
                return Convert.ToDouble(width);
            }
        }
        public double Height
        {
            get
            {
                return Convert.ToDouble(height);
            }
        }

        /// <summary>
        /// global color table used | 应用全局色彩表
        /// </summary>
        protected bool gctFlag; 
        /// <summary>
        /// size of global color table | 全局色彩表大小
        /// </summary>
        protected int gctSize;  

        /// <summary>
        /// 循环次数 | iterations; 0 = repeat forever
        /// </summary>
        public int loopCount = 1; 

        /// <summary>
        /// global color table | 全局色彩表
        /// </summary>
        protected int[] gct; 
        /// <summary>
        /// local color table  | 局部色彩表
        /// </summary>
        protected int[] lct; 
        /// <summary>
        /// active color table | 当前色彩表
        /// </summary>
        protected int[] act; 

        /// <summary>
        /// background color index | 背景色索引
        /// </summary>
        protected int bgIndex; 
        /// <summary>
        /// background color | 背景色
        /// </summary>
        protected int bgColor; 
        /// <summary>
        ///  previous bg color | 上一背景色 
        /// </summary>
        protected int lastBgColor; 
        /// <summary>
        /// pixel aspect ratio
        /// </summary>
        protected int pixelAspect; 

        /// <summary>
        /// local color table flag | 局部色彩标识
        /// </summary>
        protected bool lctFlag;
        /// <summary>
        /// interlace flag | 交错显示标识
        /// </summary>
        protected bool interlace; 

        /// <summary>
        /// local color table size | 局部色彩表大小
        /// </summary>
        protected int lctSize;  

        /// <summary>
        /// 层图片信息
        /// </summary>
        protected int ix, iy, iw, ih; 

        /// <summary>
        /// current frame | 当前针
        /// </summary>
        protected WriteableBitmap image; 
        protected WriteableBitmap bitmap;
        /// <summary>
        /// previous frame | 前一针
        /// </summary>
        protected WriteableBitmap lastImage; 

        /// <summary>
        /// current data block | 当前信息区
        /// </summary>
        protected byte[] block = new byte[256];  
        /// <summary>
        /// block size | 信息区大小
        /// </summary>
        protected int blockSize = 0;  

        // last graphic control extension info
        protected int dispose = 0;
        // 0=no action; 1=leave in place; 2=restore to bg; 3=restore to prev
        protected int lastDispose = 0;
        protected bool transparency = false; // use transparent color
        protected int delay = 0; // delay in milliseconds
        protected int transIndex; // transparent color index

        /// <summary>
        /// max decoder pixel stack size | 编码块最大长度
        /// </summary>
        protected static readonly int MaxStackSize = 4096;

        // LZW decoder working arrays | LZW解码算法应用数组
        protected short[] prefix;
        protected byte[] suffix;
        protected byte[] pixelStack;
        protected byte[] pixels;

        /// <summary>
        /// frames read from current file | 图片针信息
        /// </summary>
        public List<GifFrame> frames; 
        protected int frameCount;

        public class GifFrame
        {
            public GifFrame(WriteableBitmap im, int del, int transIndex)
            {
                image = im;
                delay = del;
                TransIndex = transIndex;
            }
            public WriteableBitmap image;
            public int delay;
            public int TransIndex;
        }



        /// <summary>
        /// Gets display duration for specified frame.
        /// </summary>
        /// <param name="n">index of frame</param>
        /// <returns>delay in milliseconds </returns>
        public int GetDelay(int n)
        {
            //
            delay = -1;
            if ((n >= 0) && (n < frameCount))
            {
                delay = ((GifFrame)frames[n]).delay ;
            }
            return delay;
        }


        /// <summary>
        /// Gets the number of frames read from file.
        /// </summary>
        /// <returns>frame count</returns>
        public int GetFrameCount()
        {
            return frameCount;
        }


        /// <summary>
        /// Gets the first (or only) image read.
        /// </summary>
        /// <returns>BufferedWriteableBitmap containing first frame, or null if none.</returns>
        public WriteableBitmap GetImage()
        {
            return GetFrame(0);
        }


        /// <summary>
        /// Gets the "Netscape" iteration count, if any.
        /// A count of 0 means repeat indefinitiely.
        /// </summary>
        /// <returns>iteration count if one was specified, else 1.</returns>
        public int GetLoopCount()
        {
            return loopCount;
        }

        ColorTable TmpColorTable = new ColorTable();


        /// <summary>
        /// Creates new frame image from current data(and previous frames as specified by their disposition codes).
        /// </summary>
        /// <param name="bitmap">frame image</param>
        /// <returns>frame data</returns>
        int[] GetPixels(WriteableBitmap bitmap)
        {
            int[] pixels = new int[width * height];
            int count = 0;
            //bitmap.Lock();
            for (int th = 0; th < height; th++)
            {
                for (int tw = 0; tw < width; tw++)
                {
                    pixels[count] = bitmap.Pixels[th * width + tw];
                    count++;
                }
            }
            bitmap.Invalidate();
            //bitmap.Unlock();
            return pixels;
            //int[] pixels = new int[3 * width * height];
            //int count = 0;
            //bitmap.Lock();
            //for (int th = 0; th < height; th++)
            //{
            //    for (int tw = 0; tw < width; tw++)
            //    {
            //        TmpColorTable.pixelColor = bitmap[th * width + tw];
            //        pixels[count] = TmpColorTable.Red;
            //        count++;
            //        pixels[count] = TmpColorTable.Green;
            //        count++;
            //        pixels[count] = TmpColorTable.Blue;
            //        count++;
            //    }
            //}
            //bitmap.Invalidate();
            //bitmap.Unlock();
            //return pixels;
        }

        /// <summary>
        /// Translate Color to Color Detail
        /// </summary>
        public class ColorTable
        {
            public Color color
            {
                get
                {
                    return Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
                }
            }
            public int pixelColor
            {
                get
                {
                    byte[] colorbyte = new byte[4];
                    colorbyte[0] = Convert.ToByte(Red);
                    colorbyte[1] = Convert.ToByte(Green);
                    colorbyte[2] = Convert.ToByte(Blue);
                    colorbyte[3] = Convert.ToByte(Alpha);
                    return BitConverter.ToInt32(colorbyte, 0);
                }
                set
                {
                    byte[] colorbyte = new byte[4];
                    colorbyte = BitConverter.GetBytes(value);
                    r = Convert.ToInt32(colorbyte[0]);
                    g = Convert.ToInt32(colorbyte[1]);
                    b = Convert.ToInt32(colorbyte[2]);
                    a = Convert.ToInt32(colorbyte[3]);
                }
            }
            uint ColorAddress = 0;
            int r
            {
                get
                {
                    return Convert.ToInt32((ColorAddress & 0xFF0000) >> 16);
                }
                set
                {
                    ColorAddress = ColorAddress & 0xff00ffff;
                    uint iaddress = Convert.ToUInt32(value) << 16;
                    ColorAddress = ColorAddress | iaddress;
                }
            }
            int g
            {
                get
                {
                    return Convert.ToInt32((ColorAddress & 0xFF00) >> 8);
                }
                set
                {
                    ColorAddress = ColorAddress & 0xffff00ff;
                    uint iaddress = Convert.ToUInt32(value) << 8;
                    ColorAddress = ColorAddress | iaddress;
                }
            }
            int b
            {
                get
                {
                    return Convert.ToInt32(ColorAddress & 0xFF);
                }
                set
                {
                    ColorAddress = ColorAddress & 0xffffff00;
                    uint iaddress = Convert.ToUInt32(value);
                    ColorAddress = ColorAddress | iaddress;
                }
            }
            int a
            {
                get
                {
                    return Convert.ToInt32((ColorAddress & 0xFF000000) >> 24);
                }
                set
                {
                    ColorAddress = ColorAddress & 0x00ffffff;
                    uint iaddress = Convert.ToUInt32(value) << 24;
                    ColorAddress = ColorAddress | iaddress;
                }
            }
            public int Red
            {
                get
                {
                    return r;
                }
            }
            public int Green
            {
                get
                {
                    return g;
                }
            }
            public int Blue
            {
                get
                {
                    return b;
                }
            }
            public int Alpha
            {
                get
                {
                    return a;
                }
            }
            public ColorTable()
            {
                r = 0;
                g = 0;
                b = 0;
            }
            public ColorTable(int Red, int Green, int Blue)
            {
                r = Red;
                g = Green;
                b = Blue;
                a = 255;
            }
            public ColorTable(byte[] inData)
            {
                r = Convert.ToInt32(inData[0]);
                g = Convert.ToInt32(inData[1]);
                b = Convert.ToInt32(inData[2]);
                a = 255;
            }
            public ColorTable(byte Read, byte Green, byte Blue)
            {
                r = Convert.ToInt32(Read);
                g = Convert.ToInt32(Green);
                b = Convert.ToInt32(Blue);
                a = 255;
            }
        }
        void SetPixels(int[] pixels)
        {
            int count = 0;
            //bitmap.Lock();
            for (int th = 0; th < height; th++)
            {
                for (int tw = 0; tw < width; tw++)
                {
                    bitmap.Pixels[th * width + tw] = pixels[count++];
                }
            }
            bitmap.Invalidate();
            //bitmap.Unlock();
        }

        /// <summary>
        /// Set current frame data in current frame image 
        /// </summary>
        protected void SetPixels()
        {

            int[] dest = GetPixels(bitmap);

            // fill in starting image contents based on last image's dispose code
            if (lastDispose > 0)
            {
                if (lastDispose == 3)
                {
                    // use image before last
                    int n = frameCount - 2;
                    if (n > 0)
                    {
                        lastImage = GetFrame(n - 1);
                    }
                    else
                    {
                        lastImage = null;
                    }
                }

                if (lastImage != null && lastDispose == 1)
                {

                    int[] prev = GetPixels(lastImage);
                    Array.Copy(prev, 0, dest, 0, width * height);
                    // copy pixels

                    if (lastDispose == 2)
                    {
                        // fill last image rect area with background color

                        Color c;
                        if (transparency)
                        {
                            c = Color.FromArgb(0, 0, 0, 0); 	// assume background is transparent
                        }
                        else
                        {
                            TmpColorTable.pixelColor = lastBgColor;
                            c = Color.FromArgb((byte)TmpColorTable.Alpha, (byte)TmpColorTable.Red, (byte)TmpColorTable.Green, (byte)TmpColorTable.Blue);

                        }
                    }
                }
            }

            // copy each source line to the appropriate place in the destination
            int pass = 1;
            int inc = 8;
            int iline = 0;
            for (int i = 0; i < ih; i++)
            {
                int line = i;
                if (interlace)
                {
                    if (iline >= ih)
                    {
                        pass++;
                        switch (pass)
                        {
                            case 2:
                                iline = 4;
                                break;
                            case 3:
                                iline = 2;
                                inc = 4;
                                break;
                            case 4:
                                iline = 1;
                                inc = 2;
                                break;
                        }
                    }
                    line = iline;
                    iline += inc;
                }
                line += iy;
                if (line < height)
                {
                    int k = line * width;
                    int dx = k + ix; // start of line in dest
                    int dlim = dx + iw; // end of dest line
                    if ((k + width) < dlim)
                    {
                        dlim = k + width; // past dest edge
                    }
                    int sx = i * iw; // start of line in source
                    while (dx < dlim)
                    {
                        // map color and insert in destination
                        int index = ((int)pixels[sx++]) & 0xff;
                        int c = act[index];
                        if (c != 0)
                        {
                            dest[dx] = c;
                        }
                        dx++;
                    }
                }
            }
            SetPixels(dest);
        }


        /// <summary>
        /// Gets the image contents of frame n.
        /// </summary>
        /// <param name="n">frame count</param>
        /// <returns> BufferedWriteableBitmap representation of frame, or null if n is invalid.</returns>
        public WriteableBitmap GetFrame(int n)
        {
            WriteableBitmap im = null;
            if ((n >= 0) && (n < frameCount))
            {
                im = ((GifFrame)frames[n]).image;
            }
            return im;
        }

        /// <summary>
        /// Reads GIF image from stream
        /// </summary>
        /// <param name="inStream">Stream containing GIF file.</param>
        /// <returns>read status code (0 = no errors)</returns>
        public int Read(Stream inStream)
        {
            Init();
            if (inStream != null)
            {
                this.inStream = inStream;
                ReadHeader();
                if (!Error())
                {
                    ReadContents();
                    if (frameCount < 0)
                    {
                        status = STATUS_FORMAT_ERROR;
                    }
                }
                inStream.Close();
            }
            else
            {
                status = STATUS_OPEN_ERROR;
            }
            lastImage = null;
            return status;
        }





        /// <summary>
        /// Decodes LZW image data into pixel array.
        /// </summary>
        protected void DecodeImageData()
        {
            int NullCode = -1;
            int npix = iw * ih;
            int available,
                clear,
                code_mask,
                code_size,
                end_of_information,
                in_code,
                old_code,
                bits,
                code,
                count,
                i,
                datum,
                data_size,
                first,
                top,
                bi,
                pi;

            if ((pixels == null) || (pixels.Length < npix))
            {
                pixels = new byte[npix]; // allocate new pixel array
            }
            if (prefix == null) prefix = new short[MaxStackSize];
            if (suffix == null) suffix = new byte[MaxStackSize];
            if (pixelStack == null) pixelStack = new byte[MaxStackSize + 1];

            //  Initialize GIF data stream decoder.

            data_size = Read();
            clear = 1 << data_size;
            end_of_information = clear + 1;
            available = clear + 2;
            old_code = NullCode;
            code_size = data_size + 1;
            code_mask = (1 << code_size) - 1;
            for (code = 0; code < clear; code++)
            {
                prefix[code] = 0;
                suffix[code] = (byte)code;
            }

            //  Decode GIF pixel stream.

            datum = bits = count = first = top = pi = bi = 0;

            for (i = 0; i < npix; )
            {
                if (top == 0)
                {
                    if (bits < code_size)
                    {
                        //  Load bytes until there are enough bits for a code.
                        if (count == 0)
                        {
                            // Read a new data block.
                            count = ReadBlock();
                            if (count <= 0)
                                break;
                            bi = 0;
                        }
                        datum += (((int)block[bi]) & 0xff) << bits;
                        bits += 8;
                        bi++;
                        count--;
                        continue;
                    }

                    //  Get the next code.

                    code = datum & code_mask;
                    datum >>= code_size;
                    bits -= code_size;

                    //  Interpret the code

                    if ((code > available) || (code == end_of_information))
                        break;
                    if (code == clear)
                    {
                        //  Reset decoder.
                        code_size = data_size + 1;
                        code_mask = (1 << code_size) - 1;
                        available = clear + 2;
                        old_code = NullCode;
                        continue;
                    }
                    if (old_code == NullCode)
                    {
                        pixelStack[top++] = suffix[code];
                        old_code = code;
                        first = code;
                        continue;
                    }
                    in_code = code;
                    if (code == available)
                    {
                        pixelStack[top++] = (byte)first;
                        code = old_code;
                    }
                    while (code > clear)
                    {
                        pixelStack[top++] = suffix[code];
                        code = prefix[code];
                    }
                    first = ((int)suffix[code]) & 0xff;

                    //  Add a new string to the string table,

                    if (available >= MaxStackSize)
                        break;
                    pixelStack[top++] = (byte)first;
                    prefix[available] = (short)old_code;
                    suffix[available] = (byte)first;
                    available++;
                    if (((available & code_mask) == 0)
                        && (available < MaxStackSize))
                    {
                        code_size++;
                        code_mask += available;
                    }
                    old_code = in_code;
                }

                //  Pop a pixel off the pixel stack.

                top--;
                pixels[pi++] = pixelStack[top];
                i++;
            }

            for (i = pi; i < npix; i++)
            {
                pixels[i] = 0; // clear missing pixels
            }

        }


        /// <summary>
        /// Returns true if an error was encountered during reading/decoding
        /// </summary>
        /// <returns></returns>
        protected bool Error()
        {
            return status != STATUS_OK;
        }


        /// <summary>
        ///  Initializes or re-initializes reader
        /// </summary>
        protected void Init()
        {
            status = STATUS_OK;
            frameCount = 0;
            frames = new List<GifFrame>();
            gct = null;
            lct = null;
        }


        /// <summary>
        /// Reads a single byte from the input stream.
        /// </summary>
        /// <returns>a single byte</returns>
        protected int Read()
        {
            int curByte = 0;
            try
            {
                curByte = inStream.ReadByte();
            }
            catch (IOException /*e*/)
            {
                status = STATUS_FORMAT_ERROR;
            }
            return curByte;
        }
        

        /// <summary>
        /// Reads next variable length block from input.
        /// </summary>
        /// <returns> number of bytes stored in "buffer"</returns>
        protected int ReadBlock()
        {
            blockSize = Read();
            int n = 0;
            if (blockSize > 0)
            {
                try
                {
                    int count = 0;
                    while (n < blockSize)
                    {
                        count = inStream.Read(block, n, blockSize - n);
                        if (count == -1)
                            break;
                        n += count;
                    }
                }
                catch (IOException /*e*/)
                {
                }

                if (n < blockSize)
                {
                    status = STATUS_FORMAT_ERROR;
                }
            }
            return n;
        }


        /// <summary>
        /// Reads color table as 256 RGB integer values
        /// </summary>
        /// <param name="ncolors">ncolors int number of colors to read</param>
        /// <returns>int array containing 256 colors (packed ARGB with full alpha)</returns>
        protected int[] ReadColorTable(int ncolors)
        {
            int nbytes = 3 * ncolors;
            int[] tab = null;
            byte[] c = new byte[nbytes];
            int n = 0;
            try
            {
                n = inStream.Read(c, 0, c.Length);
            }
            catch (IOException /*e*/)
            {
            }
            if (n < nbytes)
            {
                status = STATUS_FORMAT_ERROR;
            }
            else
            {
                tab = new int[256]; // max size to avoid bounds checks
                int i = 0;
                int j = 0;
                while (i < ncolors)
                {
                    int r = ((int)c[j++]) & 0xff;
                    int g = ((int)c[j++]) & 0xff;
                    int b = ((int)c[j++]) & 0xff;
                    tab[i++] = (int)(0xff000000 | ((uint)r << 16) | ((uint)g << 8) | (uint)b);
                }
            }
            return tab;
        }


        /// <summary>
        ///  Main file parser.  Reads GIF content blocks.
        /// </summary>
        protected void ReadContents()
        {
            // read GIF file content blocks
            bool done = false;
            while (!(done || Error()))
            {
                int code = Read();
                switch (code)
                {

                    case 0x2C: // image separator
                        ReadImage();
                        break;

                    case 0x21: // extension
                        code = Read();
                        switch (code)
                        {
                            case 0xf9: // graphics control extension
                                ReadGraphicControlExt();
                                break;

                            case 0xff: // application extension
                                ReadBlock();
                                String app = "";
                                for (int i = 0; i < 11; i++)
                                {
                                    app += (char)block[i];
                                }
                                if (app.Equals("NETSCAPE2.0"))
                                {
                                    ReadNetscapeExt();
                                }
                                else
                                    Skip(); // don't care
                                break;

                            default: // uninteresting extension
                                Skip();
                                break;
                        }
                        break;

                    case 0x3b: // terminator
                        done = true;
                        break;

                    case 0x00: // bad byte, but keep going and see what happens
                        break;

                    default:
                        status = STATUS_FORMAT_ERROR;
                        break;
                }
            }
        }


        /// <summary>
        /// Reads Graphics Control Extension values
        /// </summary>
        protected void ReadGraphicControlExt()
        {
            Read(); // block size
            int packed = Read(); // packed fields
            dispose = (packed & 0x1c) >> 2; // disposal method
            if (dispose == 0)
            {
                dispose = 1; // elect to keep old image if discretionary
            }
            transparency = (packed & 1) != 0;
            delay = ReadShort() * 10; // delay in milliseconds
            if (delay == 0)
            {
                delay = 100;
            }
            transIndex = Read(); // transparent color index
            Read(); // block terminator
        }


        /// <summary>
        /// Reads GIF file header information.
        /// </summary>
        protected void ReadHeader()
        {
            String id = "";
            for (int i = 0; i < 6; i++)
            {
                id += (char)Read();
            }
            if (!id.StartsWith("GIF"))
            {
                status = STATUS_FORMAT_ERROR;
                return;
            }

            ReadLSD();
            if (gctFlag && !Error())
            {
                gct = ReadColorTable(gctSize);
                bgColor = gct[bgIndex];
            }
        }


        /// <summary>
        /// Reads next frame image
        /// </summary>
        protected void ReadImage()
        {
            ix = ReadShort(); // (sub)image position & size
            iy = ReadShort();
            iw = ReadShort();
            ih = ReadShort();

            int packed = Read();
            lctFlag = (packed & 0x80) != 0; // 1 - local color table flag
            interlace = (packed & 0x40) != 0; // 2 - interlace flag
            // 3 - sort flag
            // 4-5 - reserved
            lctSize = 2 << (packed & 7); // 6-8 - local color table size

            if (lctFlag)
            {
                lct = ReadColorTable(lctSize); // read table
                act = lct; // make local table active
            }
            else
            {
                act = gct; // make global table active
                if (bgIndex == transIndex)
                    bgColor = 0;
            }
            int save = 0;
            if (transparency)
            {
                save = act[transIndex];
                act[transIndex] = 0; // set transparent color if specified
            }

            if (act == null)
            {
                status = STATUS_FORMAT_ERROR; // no color table defined
            }

            if (Error()) return;

            DecodeImageData(); // decode pixel data
            Skip();

            if (Error()) return;

            frameCount++;

            bitmap = new WriteableBitmap(width, height);
            image = bitmap;
            SetPixels(); // transfer pixel data to image

            frames.Add(new GifFrame(bitmap, delay, transIndex)); // add image to frame list

            if (transparency)
            {
                act[transIndex] = save;
            }
            ResetFrame();

        }


        /// <summary>
        /// Reads Logical Screen Descriptor
        /// </summary>
        protected void ReadLSD()
        {

            // logical screen size
            width = ReadShort();
            height = ReadShort();

            // packed fields
            int packed = Read();
            gctFlag = (packed & 0x80) != 0; // 1   : global color table flag
            // 2-4 : color resolution
            // 5   : gct sort flag
            gctSize = 2 << (packed & 7); // 6-8 : gct size

            bgIndex = Read(); // background color index
            pixelAspect = Read(); // pixel aspect ratio
        }


        /// <summary>
        /// Reads Netscape extenstion to obtain iteration count
        /// </summary>
        protected void ReadNetscapeExt()
        {
            do
            {
                ReadBlock();
                if (block[0] == 1)
                {
                    // loop count sub-block
                    int b1 = ((int)block[1]) & 0xff;
                    int b2 = ((int)block[2]) & 0xff;
                    loopCount = (b2 << 8) | b1;
                }
            } while ((blockSize > 0) && !Error());
        }

        /// <summary>
        /// Reads next 16-bit value, LSB first
        /// </summary>
        /// <returns></returns>
        protected int ReadShort()
        {
            // read 16-bit value, LSB first
            return Read() | (Read() << 8);
        }


        /// <summary>
        /// Resets frame state for reading next image.
        /// </summary>
        protected void ResetFrame()
        {
            lastDispose = dispose;
            lastImage = image;
            lastBgColor = bgColor;
            /*bool transparency = false;*/
            /*int delay = 0;*/
            lct = null;
        }


        /// <summary>
        /// Skips variable length blocks up to and including next zero length block.
        /// </summary>
        protected void Skip()
        {
            do
            {
              ReadBlock();
            } while ((blockSize > 0) && !Error());
        }
    }
}
