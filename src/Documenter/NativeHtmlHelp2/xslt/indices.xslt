<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:MSHelp="http://msdn.microsoft.com/mshelp"	
	xmlns:NUtil="urn:ndoc-sourceforge-net:documenters.NativeHtmlHelp2.xsltUtilities"
	exclude-result-prefixes="NUtil" >
<!-- good for debugging 
<xsl:output indent="yes"/>-->

	<!-- provide no-op override for all non-specified types -->
	<xsl:template match="@* | node() | text()" mode="FIndex"/>
	<xsl:template match="@* | node() | text()" mode="KIndex"/>
	<xsl:template match="@* | node() | text()" mode="AIndex"/>
	<xsl:template match="@* | node() | text()" mode="AIndex-hierarchy"/>
	
	<!-- this is just here until each type has it's own title logic -->
	<xsl:template match="* | node() | text()" mode="MSHelpTitle">
		<MSHelp:TOCTitle Title="{@id}"/>
		<MSHelp:RLTitle Title="{@id}"/>
	</xsl:template>
	
	<xsl:template match="ndoc" mode="MSHelpTitle">
		<xsl:param name="title" />
		<MSHelp:TOCTitle Title="{$title}"/>
		<MSHelp:RLTitle Title="{concat( $title, ' Namespace' )}"/>
	</xsl:template>
	
	<xsl:template match="enumeration | delegate | constructor" mode="MSHelpTitle">
		<xsl:param name="title" />
		<MSHelp:TOCTitle Title="{$title}"/>
		<MSHelp:RLTitle Title="{$title}"/>	
	</xsl:template>
	
	<xsl:template match="field | property | method | event | operator" mode="MSHelpTitle">
		<xsl:param name="title" />
		<MSHelp:TOCTitle Title="{$title}"/>
		<MSHelp:RLTitle Title="{concat( parent::node()/@name, '.', $title )}"/>	
	</xsl:template>	
	
	<xsl:template match="class | interface | structure" mode="MSHelpTitle">
		<xsl:param name="title" />
		<xsl:param name="page-type"/>
		<xsl:choose>
			<xsl:when test="$page-type='type' or $page-type='hierarchy' or $page-type='Members'">
				<MSHelp:TOCTitle Title="{$title}"/>
			</xsl:when>
			<xsl:otherwise>
				<MSHelp:TOCTitle Title="{$page-type}"/>
			</xsl:otherwise>
		</xsl:choose>
		<MSHelp:RLTitle Title="{$title}"/>
	</xsl:template>
<!--		
	<xsl:template match="operator" mode="MSHelpTitle">
		<xsl:param name="title" />
		<xsl:param name="page-type"/>	
		<MSHelp:TOCTitle Title="{$page-type}"/>	
		<MSHelp:RLTitle Title="{$title}"/>
	</xsl:template>
-->		
		
	<xsl:template match="ndoc" mode="AIndex">
		<xsl:call-template name="add-a-index">
			<xsl:with-param name="filename" select="NUtil:GetNamespaceHRef( string( $namespace ) )"/>
		</xsl:call-template>		
	</xsl:template>
	<xsl:template match="ndoc" mode="AIndex-hierarchy">
		<xsl:call-template name="add-a-index">
			<xsl:with-param name="filename" select="NUtil:GetNamespaceHierarchyHRef( string( $namespace ) )"/>
		</xsl:call-template>		
	</xsl:template>
	<xsl:template match="enumeration | delegate" mode="AIndex">
		<xsl:call-template name="add-a-index">	
			<xsl:with-param name="filename" select="NUtil:GetTypeHRef( string( @id ) )"/>
		</xsl:call-template>
	</xsl:template>
	<xsl:template match="field | event | property" mode="AIndex">
		<xsl:call-template name="add-a-index">
			<xsl:with-param name="filename" select="NUtil:GetMemberHRef( . )"/>
		</xsl:call-template>
	</xsl:template>
	<xsl:template match="method" mode="AIndex">
		<xsl:param name="overload-page"/>
		<xsl:variable name="filename">
			<xsl:choose>
				<xsl:when test="$overload-page=true()">
					<!-- need to deal with inherited overloads -->
					<xsl:value-of select="NUtil:GetMemberOverloadsHRef( string( ../@id ), string( @name ) )"/>								
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="NUtil:GetMemberHRef( . )"/>								
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:call-template name="add-a-index">
			<xsl:with-param name="filename" select="$filename"/>
		</xsl:call-template>
	</xsl:template>	
	<xsl:template match="operator" mode="AIndex">
		<xsl:param name="overload-page"/>
		<xsl:variable name="filename">
			<xsl:choose>
				<xsl:when test="$overload-page=true()">
					<xsl:value-of select="NUtil:GetMemberOverloadsHRef( string( ../@id ), string( @name ) )"/>								
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="NUtil:GetMemberHRef( . )"/>								
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:call-template name="add-a-index">
			<xsl:with-param name="filename" select="$filename"/>
		</xsl:call-template>
	</xsl:template>
	<xsl:template match="constructor" mode="AIndex">
		<xsl:param name="overload-page"/>
		<xsl:variable name="filename">
			<xsl:choose>
				<xsl:when test="$overload-page=true()">
					<xsl:value-of select="NUtil:GetCustructorOverloadHRef( string( ../@id ) )"/>								
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="NUtil:GetCustructorHRef( . )"/>				
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:call-template name="add-a-index">
			<xsl:with-param name="filename" select="$filename"/>
		</xsl:call-template>
	</xsl:template>	
	<xsl:template match="class | interface | structure" mode="AIndex">
		<xsl:param name="page-type"/>		
		
		<xsl:variable name="filename">
			<xsl:choose>
				<xsl:when test="$page-type='Members'">
					<xsl:value-of select="NUtil:GetTypeMembersHRef( string( @id ) )"/>				
				</xsl:when>
				<xsl:when test="$page-type='Properties'">
					<xsl:value-of select="NUtil:GetTypePropertiesHRef( string( @id ) )"/>				
				</xsl:when>
				<xsl:when test="$page-type='Events'">
					<xsl:value-of select="NUtil:GetTypeEventsHRef( string( @id ) )"/>				
				</xsl:when>
				<xsl:when test="$page-type='Operators'">
					<xsl:value-of select="NUtil:GetTypeOperatorsHRef( string( @id ) )"/>				
				</xsl:when>
				<xsl:when test="$page-type='Methods'">
					<xsl:value-of select="NUtil:GetTypeMethodsHRef( string( @id ) )"/>				
				</xsl:when>
				<xsl:when test="$page-type='Fields'">
					<xsl:value-of select="NUtil:GetTypeFieldsHRef( string( @id ) )"/>				
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="NUtil:GetTypeHRef( string( @id ) )"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		
		<xsl:call-template name="add-a-index">
			<xsl:with-param name="filename" select="$filename"/>
		</xsl:call-template>
	</xsl:template>
					
	<xsl:template name="filename-to-aindex">
		<xsl:param name="filename"/>
		<!-- there is a bug in this line in that if a type has ".html" in its full name this will fail to produce the correct result -->
		<xsl:value-of select="substring-before( $filename, '.html' )"/>
	</xsl:template>
	
	<xsl:template name="add-a-index">
		<xsl:param name="filename"/>
		
		<xsl:variable name="aindex">
			<xsl:call-template name="filename-to-aindex">
				<xsl:with-param name="filename" select="$filename"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:call-template name="add-index-term">
			<xsl:with-param name="index">A</xsl:with-param>
			<xsl:with-param name="term" select="$aindex"/>
		</xsl:call-template>		
	</xsl:template>
		
		
	<xsl:template match="ndoc" mode="FIndex">
		<xsl:param name="title"/>
		<xsl:call-template name="add-index-term">
			<xsl:with-param name="index">F</xsl:with-param>
			<xsl:with-param name="term" select="$title"/>
		</xsl:call-template>
	</xsl:template>		
	
	<xsl:template match="delegate" mode="FIndex">
		<xsl:call-template name="add-index-term">
			<xsl:with-param name="index">F</xsl:with-param>
			<xsl:with-param name="term" select="substring-after( @id, ':' )"/>
		</xsl:call-template>
	</xsl:template>	

	<xsl:template match="enumeration" mode="FIndex">
		<xsl:call-template name="add-index-term">
			<xsl:with-param name="index">F</xsl:with-param>
			<xsl:with-param name="term" select="substring-after( @id, ':' )"/>
		</xsl:call-template>
		<xsl:apply-templates select="field" mode="FIndex"/>
	</xsl:template>	

	<xsl:template match="class | structure | interface" mode="FIndex">
		<xsl:param name="title"/>
		<xsl:param name="page-type"/>

		<xsl:if test="$page-type='Members' or $page-type='type'">
			<xsl:call-template name="add-index-term">
				<xsl:with-param name="index">F</xsl:with-param>
				<xsl:with-param name="term" select="substring-after( @id, ':' )"/>
			</xsl:call-template>
		</xsl:if>			
	</xsl:template>	

	<xsl:template match="enumeration/field" mode="FIndex">
		<xsl:call-template name="add-index-term">
			<xsl:with-param name="index">F</xsl:with-param>
			<xsl:with-param name="term" select="substring-after( @id, ':')"/>
		</xsl:call-template>
		<xsl:call-template name="add-index-term">
			<xsl:with-param name="index">F</xsl:with-param>
			<xsl:with-param name="term" select="concat( parent::node()/@name, '.', @name )"/>
		</xsl:call-template>		
	</xsl:template>	
	
	<xsl:template match="constructor" mode="FIndex">
		<xsl:param name="overload-page"/>
		
		<xsl:if test="$overload-page=true() or not(@overload)">
			<xsl:call-template name="add-index-term">
				<xsl:with-param name="index">F</xsl:with-param>
				<xsl:with-param name="term" select="concat( substring-after( parent::node()/@id, ':' ), '.', parent::node()/@name )"/>
			</xsl:call-template>
			<xsl:call-template name="add-index-term">
				<xsl:with-param name="index">F</xsl:with-param>
				<xsl:with-param name="term" select="concat( parent::node()/@name, '.', parent::node()/@name )"/>
			</xsl:call-template>
			<xsl:call-template name="add-index-term">
				<xsl:with-param name="index">F</xsl:with-param>
				<xsl:with-param name="term" select="concat( substring-after( parent::node()/@id, ':' ), '.New' )"/>
			</xsl:call-template>
			<xsl:call-template name="add-index-term">
				<xsl:with-param name="index">F</xsl:with-param>
				<xsl:with-param name="term" select="concat( parent::node()/@name, '.New' )"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>	
	
	<xsl:template match="field | property | method | event" mode="FIndex">
		<xsl:param name="overload-page"/>

		<xsl:if test="$overload-page=true() or not(@overload)">
			<xsl:call-template name="add-index-term">
				<xsl:with-param name="index">F</xsl:with-param>
				<xsl:with-param name="term" select="concat( substring-after( parent::node()/@id, ':' ), '.', @name )"/>
			</xsl:call-template>
			<xsl:call-template name="add-index-term">
				<xsl:with-param name="index">F</xsl:with-param>
				<xsl:with-param name="term" select="concat( parent::node()/@name, '.', @name )"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>		
	
	<xsl:template match="ndoc" mode="KIndex">
		<xsl:param name="title" />
		<xsl:if test="contains( $title, '.' )">
			<xsl:call-template name="add-index-term">
				<xsl:with-param name="index">K</xsl:with-param>
				<xsl:with-param name="term" select="concat( substring-after( $title, '.' ), ' Namespace' )"/>
			</xsl:call-template>		
		</xsl:if>
		<xsl:call-template name="add-index-term">
			<xsl:with-param name="index">K</xsl:with-param>
			<xsl:with-param name="term" select="concat( $title, ' Namespace' )"/>
		</xsl:call-template>		
	</xsl:template>	
	
		
	<xsl:template match="class | interface | structure" mode="KIndex">
		<xsl:param name="title"/>
		<xsl:param name="page-type"/>		
		<xsl:choose>
			<xsl:when test="$page-type='Members'">
				<xsl:call-template name="add-index-term">
					<xsl:with-param name="index">K</xsl:with-param>
					<xsl:with-param name="term" select="concat( @name, ' ', local-name() )"/>
				</xsl:call-template>					
				<xsl:call-template name="add-index-term">
					<xsl:with-param name="index">K</xsl:with-param>
					<xsl:with-param name="term" select="concat( @name, ' ', local-name(), ', all members' )"/>
				</xsl:call-template>					
				<xsl:call-template name="add-index-term">
					<xsl:with-param name="index">K</xsl:with-param>
					<xsl:with-param name="term" select="concat( substring-after( @id, ':' ), ' ', local-name() )"/>
				</xsl:call-template>					
			</xsl:when>
			<xsl:when test="$page-type='Properties'">
				<xsl:call-template name="add-index-term">
					<xsl:with-param name="index">K</xsl:with-param>
					<xsl:with-param name="term" select="concat( @name, ' ', local-name(), ', properties' )"/>
				</xsl:call-template>									
			</xsl:when>
			<xsl:when test="$page-type='Events'">
				<xsl:call-template name="add-index-term">
					<xsl:with-param name="index">K</xsl:with-param>
					<xsl:with-param name="term" select="concat( @name, ' ', local-name(), ', events' )"/>
				</xsl:call-template>									
			</xsl:when>
			<xsl:when test="$page-type='Operators'">
				<xsl:call-template name="add-index-term">
					<xsl:with-param name="index">K</xsl:with-param>
					<xsl:with-param name="term" select="concat( @name, ' ', local-name(), ', operators' )"/>
				</xsl:call-template>									
			</xsl:when>
			<xsl:when test="$page-type='Methods'">
				<xsl:call-template name="add-index-term">
					<xsl:with-param name="index">K</xsl:with-param>
					<xsl:with-param name="term" select="concat( @name, ' ', local-name(), ', methods' )"/>
				</xsl:call-template>									
			</xsl:when>
			<xsl:when test="$page-type='Fields'">
				<xsl:call-template name="add-index-term">
					<xsl:with-param name="index">K</xsl:with-param>
					<xsl:with-param name="term" select="concat( @name, ' ', local-name(), ', fields' )"/>
				</xsl:call-template>									
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="add-index-term">
					<xsl:with-param name="index">K</xsl:with-param>
					<xsl:with-param name="term" select="concat( @name, ' ', local-name(), ', about ', @name, ' ', local-name() )"/>
				</xsl:call-template>					
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>	
	
	<xsl:template match="constructor" mode="KIndex">
		<xsl:param name="overload-page"/>

		<xsl:if test="$overload-page=true() or not(@overload)">
			<xsl:call-template name="add-index-term">
				<xsl:with-param name="index">K</xsl:with-param>
				<xsl:with-param name="term" select="concat( parent::node()/@name, ' ', local-name( parent::node() ), ', ', local-name() )"/>
			</xsl:call-template>				
			<xsl:call-template name="add-index-term">
				<xsl:with-param name="index">K</xsl:with-param>
				<xsl:with-param name="term" select="concat( substring-after( parent::node()/@id, ':' ), ' ', local-name() )"/>
			</xsl:call-template>	
		</xsl:if>			
	</xsl:template>	
	
	<xsl:template match="field | property | method | event" mode="KIndex">
		<xsl:param name="overload-page" />

		<xsl:if test="$overload-page=true() or not(@overload)">
			<xsl:call-template name="add-index-term">
				<xsl:with-param name="index">K</xsl:with-param>
				<xsl:with-param name="term" select="concat( parent::node()/@name, '.', @name, ' ', local-name() )"/>
			</xsl:call-template>				
			<xsl:call-template name="add-index-term">
				<xsl:with-param name="index">K</xsl:with-param>
				<xsl:with-param name="term" select="concat( @name, ' ', local-name() )"/>
			</xsl:call-template>	
		</xsl:if>			
	</xsl:template>	
			
	<xsl:template match="enumeration" mode="KIndex">
		<xsl:apply-templates select="field" mode="KIndex"/>
		<xsl:call-template name="add-index-term">
			<xsl:with-param name="index">K</xsl:with-param>
			<xsl:with-param name="term" select="concat( substring-after( @id, ':'), ' enumeration' )"/>
		</xsl:call-template>		
	</xsl:template>	
	
	<xsl:template match="enumeration/field" mode="KIndex">
		<xsl:call-template name="add-index-term">
			<xsl:with-param name="index">K</xsl:with-param>
			<xsl:with-param name="term" select="concat( @name, ' enumeration member' )"/>
		</xsl:call-template>		
	</xsl:template>
	
		<xsl:template match="delegate" mode="KIndex">
		<xsl:call-template name="add-index-term">
			<xsl:with-param name="index">K</xsl:with-param>
			<xsl:with-param name="term" select="concat( @name, ' delegate' )"/>
		</xsl:call-template>		
		<xsl:call-template name="add-index-term">
			<xsl:with-param name="index">K</xsl:with-param>
			<xsl:with-param name="term" select="concat( substring-after( @id, ':' ), ' delegate' )"/>
		</xsl:call-template>		
	</xsl:template>
	
	
	<xsl:template name="add-index-term">
		<xsl:param name="index"/>
		<xsl:param name="term"/>
		<MSHelp:Keyword Index="{$index}" Term="{$term}"/>
	</xsl:template>	
	
</xsl:stylesheet>

  