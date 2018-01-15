using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Cloo;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Template {

    class Game
    {
	    // when GLInterop is set to true, the fractal is rendered directly to an OpenGL texture
	    bool GLInterop = false;
	    // load the OpenCL program; this creates the OpenCL context
	    static OpenCLProgram ocl = new OpenCLProgram( "../../program.cl" );
	    // find the kernel named 'device_function' in the program
	    OpenCLKernel kernel = new OpenCLKernel( ocl, "device_function" );
	    // create a regular buffer; by default this resides on both the host and the device
	    OpenCLBuffer<int> buffer = new OpenCLBuffer<int>( ocl, 512 * 512 );
	    // create an OpenGL texture to which OpenCL can send data
	    OpenCLImage<int> image = new OpenCLImage<int>( ocl, 512, 512 );
	    public Surface screen;
        static uint[] temp = { 1, 2, 3, 4, 5 };
        OpenCLBuffer<uint> tempbuffer = new OpenCLBuffer<uint>(ocl, temp);
	    Stopwatch timer = new Stopwatch();
	    float t = 21.5f;
        
        public void Init()
	    {
		    // nothing here
	    }
	    public void Tick()
	    {
		    GL.Finish();
		    // clear the screen
		    screen.Clear( 0 );
		    // do opencl stuff
		    if (GLInterop) kernel.SetArgument( 0, image );
				      else kernel.SetArgument( 0, buffer );
		    kernel.SetArgument( 1, t );
            kernel.SetArgument( 2, tempbuffer );
            
		    t += 0.1f;
            // execute kernel
            long[] workSize = { 512, 512 };
            long[] localSize = { 32, 4 };



            // NO INTEROP PATH:
            // Use OpenCL to fill a C# pixel array, encapsulated in an
            // OpenCLBuffer<int> object (buffer). After filling the buffer, it
            // is copied to the screen surface, so the template code can show
            // it in the window.
            // execute the kernel
            kernel.Execute( workSize, localSize );
		    // get the data from the device to the host
		    tempbuffer.CopyFromDevice();
            for (int i = 0; i < temp.Length; i++)
                Console.WriteLine(temp[i]);
		    // plot pixels using the data on the host
		    for( int y = 0; y < 512; y++ ) for( int x = 0; x < 512; x++ )
		    {
			    screen.pixels[x + y * screen.width] = buffer[x + y * 512];
		    }
	    }
	    public void Render() 
	    {
		    // use OpenGL to draw a quad using the texture that was filled by OpenCL
		    if (GLInterop)
		    {
			    GL.LoadIdentity();
			    GL.BindTexture( TextureTarget.Texture2D, image.OpenGLTextureID );
			    GL.Begin( PrimitiveType.Quads );
			    GL.TexCoord2( 0.0f, 1.0f ); GL.Vertex2( -1.0f, -1.0f );
			    GL.TexCoord2( 1.0f, 1.0f ); GL.Vertex2(  1.0f, -1.0f );
			    GL.TexCoord2( 1.0f, 0.0f ); GL.Vertex2(  1.0f,  1.0f );
			    GL.TexCoord2( 0.0f, 0.0f ); GL.Vertex2( -1.0f,  1.0f );
			    GL.End();
		    }
	    }
    }

} // namespace Template