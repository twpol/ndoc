using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace NDoc.UsersGuideStager
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class App
	{
		private static int exitCode = 0;
		private static string err = "";

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[MTAThread]
		static int Main(string[] args)
		{
			Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

			// HACK
			// in order to use mshtml to parse the content files we need to have a message loop
			// here we create window so mshtml can use its message loop
			// this window is never shown. The actual work for the application is 
			// initiated and completed in the window's OnLoad override
			Application.Run( new MessageLoopForm( args ) );

			if ( exitCode != 0 )
			{
				Trace.WriteLine( err );
				Console.WriteLine( err );
			}
			return exitCode;
		}

		private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			exitCode = 1;
			err = e.Exception.Message;

			Application.ExitThread();
		}
	}
}
