using System;

using NDoc.Documenter.NativeHtmlHelp2.HxProject;
using NDoc.Documenter.NativeHtmlHelp2.Engine;

namespace NDoc.Documenter.NativeHtmlHelp2
{
	/// <summary>
	/// Summary description for TOCBuilder.
	/// </summary>
	public class TOCBuilder : IDisposable
	{
		private TOCFile toc = null;

		private HtmlFactory factory = null;

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

		~TOCBuilder()
		{
			Dispose( false );
		}

		private void factory_TopicStart(object sender, FileEventArgs e)
		{
			toc.OpenNode( "/html/" + e.File );
		}

		private void factory_TopicEnd(object sender, EventArgs e)
		{
			toc.CloseNode();
		}

		private void factory_AddFileToTopic(object sender, FileEventArgs args)
		{
			toc.InsertNode( "/html/" + args.File );
		}

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

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
