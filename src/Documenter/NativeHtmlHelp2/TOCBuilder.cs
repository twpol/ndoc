using System;

using NDoc.Documenter.NativeHtmlHelp2.HxProject;
using NDoc.Documenter.NativeHtmlHelp2.Engine;

namespace NDoc.Documenter.NativeHtmlHelp2
{
	/// <summary>
	/// Orchestrates building the tabel of contents file base on HTMLFactory events
	/// </summary>
	public class TOCBuilder : IDisposable
	{
		private TOCFile toc = null;

		private HtmlFactory factory = null;

		/// <summary>
		/// Contruct a enw instance of the TOCBuilder class
		/// </summary>
		/// <param name="_toc">The table of contents file to write to</param>
		/// <param name="_factory">The HTMLFactory creating each file to be added</param>
		public TOCBuilder( TOCFile _toc, HtmlFactory _factory )
		{
			if ( _toc == null )
				throw new NullReferenceException( "The TOCFile cannot be null" );

			if ( _factory == null )
				throw new NullReferenceException( "The HtmlFactory annot be null" );

			toc = _toc;
			factory = _factory;

			toc.Open();

			// connect to factory events
			// this is so we can build the TOC as we go
			factory.TopicStart += new FileEventHandler(factory_TopicStart);
			factory.TopicEnd += new EventHandler(factory_TopicEnd);
			factory.AddFileToTopic += new FileEventHandler(factory_AddFileToTopic);
		}

		/// <summary>
		/// Finalizer
		/// </summary>
		~TOCBuilder()
		{
			Dispose( false );
		}

		private void factory_TopicStart(object sender, FileEventArgs e)
		{
			// this assumes that all content files are going in a directory named
			// "html" (relative to the location of the HxT
			toc.OpenNode( "/html/" + e.File );
		}

		private void factory_TopicEnd(object sender, EventArgs e)
		{
			toc.CloseNode();
		}

		private void factory_AddFileToTopic(object sender, FileEventArgs args)
		{
			// this assumes that all content files are going in a directory named
			// "html" (relative to the location of the HxT
			toc.InsertNode( "/html/" + args.File );
		}

		/// <summary>
		/// Disposes the TOCBuilder instance
		/// </summary>
		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		/// <summary>
		/// Disposes teh TOCBuilder instance
		/// </summary>
		/// <param name="disposing">Was this method called from the Dispsose() method</param>
		protected virtual void Dispose( bool disposing )
		{
			if ( disposing )
			{
				if ( factory != null )
				{
					factory.TopicStart -= new FileEventHandler(factory_TopicStart);
					factory.TopicEnd -= new EventHandler(factory_TopicEnd);
					factory.AddFileToTopic -= new FileEventHandler(factory_AddFileToTopic);
				}

				if ( toc != null )
					toc.Close();
			}
		}
	}
}
