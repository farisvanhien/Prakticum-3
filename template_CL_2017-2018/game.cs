using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Cloo;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.IO;


namespace Template {

    class Game
    {
        #region Member Variables
        int generation = 0;
        // two buffers for the pattern: simulate reads 'second', writes to 'pattern'
        static uint[] pattern;
        static uint[] second;
        uint pw, ph; // note: pw is in uints; width in bits is 32 this value.

        // mouse handling: dragging functionality
        uint xoffset = 0, yoffset = 0;
        bool lastLButtonState = false;
        int dragXStart, dragYStart, offsetXStart, offsetYStart;

	    // load the OpenCL program; this creates the OpenCL context
	    static OpenCLProgram ocl = new OpenCLProgram( "../../program.cl" );
	    // find the kernel named 'device_function' in the program
	    OpenCLKernel kernel = new OpenCLKernel( ocl, "device_function" );
	    // create a regular buffer; by default this resides on both the host and the device
	    OpenCLBuffer<int> buffer = new OpenCLBuffer<int>( ocl, 512 * 512 );
	    public Surface screen;

        OpenCLBuffer<uint> patternbuffer;
        OpenCLBuffer<uint> secondbuffer;

        uint zoom = 1; //zoomfactor used to zoom in and zoom out

        Stopwatch timer = new Stopwatch();
        #endregion
        #region Bit Helpers
        // helper function for setting one bit in the pattern buffer
        void BitSet(uint x, uint y) { pattern[y * pw + (x >> 5)] |= 1U << (int)(x & 31); }
        // helper function for getting one bit from the secondary pattern buffer
        uint GetBit(uint x, uint y) { return (second[y * pw + (x >> 5)] >> (int)(x & 31)) & 1U; }
        #endregion

        public void Init()
	    {
            StreamReader sr = new StreamReader("../../data/turing_js_r.rle");
            uint state = 0, n = 0, x = 0, y = 0;
            while (true)
            {
                String line = sr.ReadLine();
                if (line == null) break; // end of file
                int pos = 0;
                if (line[pos] == '#') continue;
                else if (line[pos] == 'x') // header
                {
                    String[] sub = line.Split(new char[] { '=', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    pw = (UInt32.Parse(sub[1]) + 31) / 32;
                    ph = UInt32.Parse(sub[3]);
                    pattern = new uint[pw * ph];
                    second = new uint[pw * ph];
                }
                else while (pos < line.Length)
                    {
                        Char c = line[pos++];
                        if (state == 0) if (c < '0' || c > '9') { state = 1; n = Math.Max(n, 1); } else n = (uint)(n * 10 + (c - '0'));
                        if (state == 1) // expect other character
                        {
                            if (c == '$') { y += n; x = 0; } // newline
                            else if (c == 'o') for (int i = 0; i < n; i++) BitSet(x++, y); else if (c == 'b') x += n;
                            state = n = 0;
                        }
                    }
                // swap buffers
                for (int i = 0; i < pw * ph; i++) second[i] = pattern[i];
            }
            patternbuffer = new OpenCLBuffer<uint>(ocl,(int)(pw * ph));
            secondbuffer = new OpenCLBuffer<uint>(ocl, second);
        }
	    public void Tick()
	    {
            timer.Restart();    //Start timer
            OpenCLTick();       //OpenCL work
            DrawScreenZoom();   //Draw the screen with the ability to zoom in/out

            /*============================
            //Old Initial methods for CPU
            //Kept for debugging purposes
            Simulate();         //CPU for debug purposes
            DrawScreen();       //Draw the screen normally
            ============================*/

            // report performance
            Console.WriteLine("generation " + generation++ + ": " + timer.ElapsedMilliseconds + "ms");
        }
        // Computes the simulation via OpenCL
        void OpenCLTick()
        {
            for (int i = 0; i < patternbuffer.Length; i++) patternbuffer[i] = 0;
            patternbuffer.CopyToDevice();
            secondbuffer.CopyToDevice();

            // Set openCL arguments
            kernel.SetArgument(0, patternbuffer);
            kernel.SetArgument(1, secondbuffer);
            kernel.SetArgument(2, pw);
            kernel.SetArgument(3, ph);
            // Execute kernel
            long[] workSize = { pw*32,ph };

            // NO INTEROP PATH:
            // Use OpenCL to fill a C# pixel array, encapsulated in an
            // OpenCLBuffer<int> object (buffer). After filling the buffer, it
            // is copied to the screen surface, so the template code can show
            // it in the window.
            // execute the kernel
            kernel.Execute(workSize);
            // get the data from the device to the host
            patternbuffer.CopyFromDevice();
            // swap buffers
            for (int i = 0; i < pw * ph; i++) second[i] = patternbuffer[i];
        }
        public void SetMouseState(int x, int y, bool pressed)
        {
            if (pressed)
            {
                if (lastLButtonState)
                {
                    int deltax = x - dragXStart, deltay = y - dragYStart;
                    xoffset = (uint)Math.Min(pw * 32 - screen.width, Math.Max(0, offsetXStart - deltax));
                    yoffset = (uint)Math.Min(ph - screen.height, Math.Max(0, offsetYStart - deltay));
                }
                else
                {
                    dragXStart = x;
                    dragYStart = y;
                    offsetXStart = (int)xoffset;
                    offsetYStart = (int)yoffset;
                    lastLButtonState = true;
                }
            }
            else lastLButtonState = false;
        }
        // Adjusts the zoomfactor by incrementing/decremnting
        public void SetZoom(bool adj) 
        {
            if (adj) zoom++;
            else if (zoom > 1) zoom--; // make sure that the zoom is never smaller than the original
        }
        // Draws pixel on the screen with ability to zoom
        public void DrawScreenZoom()
        {
            // clear the screen
            screen.Clear(0);
            // you essentially draw a smaller segment of the field
            for (uint y = 0; y < screen.height / zoom; y++)
            {
                for (uint x = 0; x < screen.width / zoom; x++)
                {
                    if (GetBit(x + xoffset, y + yoffset) == 1)
                    {
                        // draw a white square per white bit
                        for (uint i = 0; i < zoom; i++)
                        {
                            for (uint j = 0; j < zoom; j++)
                            {
                                screen.Plot(x * zoom + i, y * zoom + j, 0xffffff);
                            }
                        }
                    }
                }
            }
        }

        #region Old/Original Methods
        // SIMULATE
        // Takes the pattern in array 'second', and applies the rules of Game of Life to produce the next state
        // in array 'pattern'. At the end, the result is copied back to 'second' for the next generation.
        void Simulate() //The original sequential simulate
        {
            // clear destination pattern
            for (int i = 0; i < pw * ph; i++) pattern[i] = 0;
            // process all pixels, skipping one pixel boundary
            uint w = pw * 32, h = ph;
            for (uint y = 1; y < h - 1; y++)
                for (uint x = 1; x < w - 1; x++)
                {
                    // count active neighbors
                    uint n = GetBit(x - 1, y - 1) + GetBit(x, y - 1) + GetBit(x + 1, y - 1) + GetBit(x - 1, y) +
                             GetBit(x + 1, y) + GetBit(x - 1, y + 1) + GetBit(x, y + 1) + GetBit(x + 1, y + 1);
                    if ((GetBit(x, y) == 1 && n == 2) || n == 3) BitSet(x, y);
                }
            // swap buffers
            for (int i = 0; i < pw * ph; i++) second[i] = pattern[i];
        }
        public void DrawScreen()
        {
            // clear the screen
            screen.Clear(0);
            for (uint y = 0; y < screen.height; y++)
            {
                for (uint x = 0; x < screen.width; x++)
                {
                    if (GetBit(x + xoffset, y + yoffset) == 1)
                    {
                        screen.Plot(x, y, 0xffffff);
                    }
                }
            }
        }
        #endregion
    }

} // namespace Template