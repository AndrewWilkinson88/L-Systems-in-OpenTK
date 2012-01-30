using System;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace GeometricModeling
{
	public class LSystems : GameWindow
	{	
		//These are coordinates for the camera, x,z being spatial locations
		//and angle being which they're facing, with lx, and lz being used for
		//the target and are determined by angle.
		public float lx = 0.0f;
        public float lz = 1.0f;
        public float x = 0.0f;
        public float z = 0.0f;
		public float y = 0.0f;
        public float angle = 0.0f;
		
		//Me playing with shaders
		public int shaderProgram = 0;
		
		/* 
		 * The L-System Code
		 * L-Systems are defined by a starting string a generator string(s), 
		 * how many iterations to run the generator and a definition for 
		 * each symbol. The symbols I use are:
		 * f  move forward pen up
		 * F  move forward pen Down
		 * +  turn positive ang
		 * -  turn negative ang
		 * [  push relative position
		 * ]  pop to previous position
		*/
		public String start = "F-F-F-F";
		public String[] next = new String[10];
		
		public float ang = 90.0f;
		public int timer = 0;
		public int iter = 0;
		
		/*
		 * Basic constructor
		 * Uses the default graphicsMode as well as the default graphics context
		 * which is fine for the purposes of this assignment
		*/
		public LSystems()
			: base(800,600, OpenTK.Graphics.GraphicsMode.Default, "Geometric Modeling Project" )
		{
			VSync = VSyncMode.On;
		}
		
		
		/*
		 * This is called when the program starts
		 * analogous to the GlutInit callback
		 */
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(0.1f, 0.2f, 0.5f, 0.0f);
			
			shaderProgram = GL.CreateProgram();
			
			int vert = GL.CreateShader(ShaderType.VertexShader);
			int frag = GL.CreateShader(ShaderType.FragmentShader);
			
			//These simple shaders are obtained from:
			//http://www.starstonesoftware.com/OpenGL/
			String vertSource = @"
				#version 330
				
				in vec4 vVertex;
				in vec4 vColor;
				
				out vec4 vVaryingColor;
				
				void main(void)
				{
					vVaryingColor = vColor;
					gl_Position = vVertex;
				} ";

			String fragSource = @"
				#version 330
				
				out vec4 vFragColor
				in vec4 vVaryingColor
				
				void main(void)
				{
					vFragColor = vVaryingColor;
				} ";

			GL.CompileShader(vert);
			GL.CompileShader(frag);
			
			int testVal;

			//check to see if vertex shader compilation worked
			GL.GetShader(vert, ShaderParameter.CompileStatus, out testVal);
			if( testVal == 1 ) //compiling failed
			{
				String infoLog;
				GL.GetShaderInfoLog( vert, out infoLog ); 
				System.Console.WriteLine( infoLog );
				GL.DeleteShader(vert);
				GL.DeleteShader(frag);
				Exit();
			}
			
			//check to see if fragment shader compilation worked
			GL.GetShader(frag, ShaderParameter.CompileStatus, out testVal);
			if( testVal == 1 ) //compiling failed
			{
				String infoLog;
				GL.GetShaderInfoLog( frag, out infoLog ); 
				System.Console.WriteLine( infoLog );
				GL.DeleteShader(vert);
				GL.DeleteShader(frag);
				Exit();
			}
			
			GL.AttachShader(shaderProgram, vert);
			GL.AttachShader(shaderProgram, frag);
			
			GL.BindAttribLocation( shaderProgram, 0, "vVertex" );
			GL.BindAttribLocation( shaderProgram, 1, "vColor" );
			GL.BindAttribLocation( shaderProgram, 2, "vFragColor" );
			GL.BindAttribLocation( shaderProgram, 3, "vVaryingColor" );
			
			GL.LinkProgram( shaderProgram );
			GL.DeleteShader( vert );
			GL.DeleteShader( frag );
			
			GL.GetProgram(shaderProgram, ProgramParameter.LinkStatus, out testVal);
			if( testVal == 1 ) //linking failed
			{
				String infoLog;
				GL.GetProgramInfoLog( shaderProgram, out infoLog );
				GL.DeleteProgram( shaderProgram );
				Exit();
			}
			GL.UseProgram( shaderProgram );
        }
		
		/*
		 * This function is called whenever the window is resize
		 * anaglous to GlutReshape callback
		 */
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
			//ClienRectangle grabs the window coords to the screen from the OS
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
			
			//constructs the base prjection frustrum
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }
		
		/*
		 * This function is called every time a frame is updated
		 * used as a catch all for keyboard input, mouse input, 
		 * and other things you want to look out for while the program
		 * is running but necessarily re-displaying anything
		 */
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (Keyboard[Key.Escape]) //kill the main program
                Exit();
			if (Keyboard[Key.A]) //turn left
				angle += 0.05f;
            if (Keyboard[Key.D]) //turn right
				angle -= 0.05f;
			if (Keyboard[Key.W]) //move forward
			{
            	x = x + lx * 0.05f;		
				z = z + lz * 0.05f;
			}
            if (Keyboard[Key.S]) //move backward
			{
				x = x - lx * 0.05f;		
				z = z - lz * 0.05f;
			}
			if (Keyboard[Key.Z]) //move up
			{
				y = y - 0.05f;		
			}
			if (Keyboard[Key.C]) //move down
			{
				y = y + 0.05f;		
			}
            if (Keyboard[Key.Q]) //pan left
			{
				x = x + (float)Math.Sin ((Math.PI / 2) + angle) * 0.05f;
				z = z + (float)Math.Cos ((Math.PI / 2) + angle) * 0.05f;
			}
            if (Keyboard[Key.E]) //pan right
			{
				x = x - (float)Math.Sin ((Math.PI / 2) + angle) * 0.05f;
				z = z - (float)Math.Cos ((Math.PI / 2) + angle) * 0.05f;
			}
			if (Keyboard[Key.T])
			{
				timer = start.Length;	
			}
			
			// LSystem type 1
			if (Keyboard[Key.Number1])
			{
				timer = 0;
				start = "F-F-F-F";
				next[0] = "FF-FFF+F+FF-F";
				ang = 90.0f;
				iter = 4;
				for( int n = 0; n < iter; n++ )
					start = start.Replace("F", next[0]);
			}
			 
			//LSystem type 2
			if( Keyboard[Key.Number2])
			{
				timer = 0;
				start = "L";
				next[0] = "L+R++R-L--LL-R+";
				next[1] = "-L+RR++R+L--L-R";
				ang = 60.0f;
				iter = 4;
				String temp = "";
				
				for( int n=0; n<iter; n++){
					for( int i=0; i<start.Length; i++){
						if( start[i] == 'L' )
							temp += next[0];
						else if(start[i] == 'R')
							temp += next[1];
						else
							temp += start[i];
					}
					start = temp;
					temp = "";
				}
				start = start.Replace('L', 'F');
				start = start.Replace('R', 'F');
			}
			
			//LSystem type 3
			if (Keyboard[Key.Number3])
			{
				timer = 0;
				start = "F+F+F+F";
				next[0] = "F+f-FF+F+FF+Ff+FF-f+FF-F-FF-Ff-FFF";
				next[1] = "ffffff";
				ang = 90.0f;
				iter = 2;
				String temp = "";
				
				for( int n=0; n<iter; n++){
					for( int i=0; i<start.Length; i++){
						if( start[i] == 'F' )
							temp += next[0];
						else if(start[i] == 'f')
							temp += next[1];
						else
							temp += start[i];
					}
					start = temp;
					temp = "";
				}
			}
				
			//LSystem type 4
			if (Keyboard[Key.Number4])
			{
				timer = 0;
				start = "L";
				next[0] = "L+R+";
				next[1] = "-L-R";
				ang = 90.0f;
				iter = 10;
				
				String foo = "";
				
				for( int n=0; n<iter; n++){
					for( int i=0; i<start.Length; i++){
						if( start[i] == 'L' )
							foo += next[0];
						else if(start[i] == 'R')
							foo += next[1];
						else
							foo += start[i];
					}
					start = foo;
					foo = "";
				}
				start = start.Replace('L', 'F');
				start = start.Replace('R', 'F');
			}
			
			// Tree Type 1			
			if (Keyboard[Key.Number5])
			{
				timer = 0;
				start = "F";
				next[0] = "F[+F]F[-F]F";
				ang = 25.7f;
				iter = 5;
				
				for( int n = 0; n < iter; n++ )
				{
					start = start.Replace("F", next[0]);
				}
			}
			
			// Tree Type 2
			if (Keyboard[Key.Number6])
			{
				timer = 0;
				start = "F";
				next[0] = "F[+F+F]F[-FF]F";
				ang = 20.0f;
				iter = 5;
				
				for( int n = 0; n < iter; n++ )
				{
					start = start.Replace("F", next[0]);
				}
			}
			
			// Tree Type 3
			if (Keyboard[Key.Number7])
			{
				timer = 0;
				start = "F";
				next[0] = "FF-[-F+F+F]+[+F-F-F]";
				ang = 22.5f;
				iter = 4;
				
				for( int n = 0; n < iter; n++ )
				{
					start = start.Replace("F", next[0]);
				}				
			}	
        }
		
		/*
		 * This function is called whenever the window is told to redraw
		 * anything, analogous to the glutDisplay function.
		 */
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.Color3( 1.0, 1.0, 1.0);
			
			//Handle Modelview Matrix
			lx = (float)Math.Sin(angle);
			lz = (float)Math.Cos(angle);
			
			Vector3 eye =    new Vector3( x, y, z);
            Vector3 target = new Vector3(x+lx, y, z+lz);

            Matrix4 modelview = Matrix4.LookAt(eye, target,  Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
			
			for( int n=0; n < start.Length; n++ )
			{
				if(start[n] == '[')
					GL.PushMatrix();
				if(start[n] == ']')
					GL.PopMatrix();
				if(start[n] == 'f')
					GL.Translate(0.0f, 0.1f, 0.0f);	
				if(start[n]	== 'F' && n < timer){
					GL.Translate(0.0f, 0.1f, 0.0f);
					GL.Begin(BeginMode.Lines);
						GL.Vertex3( 0.0f, -0.1f, 10.0f );
						GL.Vertex3( 0.0f, 0.0f, 10.0f);
					GL.End();
				}
				if(start[n] == '-')
					GL.Rotate( -ang, Vector3d.UnitZ );
				if(start[n] == '+')
					GL.Rotate( ang, Vector3d.UnitZ );
			}
			
			if( (timer+3) >= start.Length )
				timer = start.Length;
			else
				timer+=3;
			
			//Display buffer to screen
            SwapBuffers();
        }
		
		protected override void OnUnload(EventArgs e)
		{
			GL.DeleteProgram(shaderProgram);
			base.OnUnload(e);
		}
		
		[STAThread]
        static void Main()
        {
            // The 'using' idiom guarantees proper resource cleanup.
            // We request 30 UpdateFrame events per second, and unlimited
            // RenderFrame events (as fast as the computer can handle).
            using (LSystems main = new LSystems())
            {
                main.Run(30.0);
            }
        }
	}
}

