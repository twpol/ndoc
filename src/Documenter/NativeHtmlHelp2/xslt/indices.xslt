<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:MSHelp="http://msdn.microsoft.com/mshelp">

	<!-- provide no-op override for all non-specified types -->
	<xsl:template match="* | node() | text()" mode="FIndex"/>
	<xsl:template match="* | node() | text()" mode="KIndex"/>
	<xsl:template match="* | node() | text()" mode="AIndex"/>
	
	<!-- this is just here until each type has it's own title logic -->
	<xsl:template match="* | node() | text()" mode="MSHelpTitle">
		<MSHelp:TOCTitle>
			<xsl:attribute name="Title"><xsl:value-of select="@id"/></xsl:attribute>
		</MSHelp:TOCTitle>
		<MSHelp:RLTitle>
			<xsl:attribute name="Title"><xsl:value-of select="@id"/></xsl:attribute>
		</MSHelp:RLTitle>	
	</xsl:template>
	
	<xsl:template match="ndoc" mode="MSHelpTitle">
		<xsl:param name="title" />
		<MSHelp:TOCTitle>
			<xsl:attribute name="Title"><xsl:value-of select="$title"/></xsl:attribute>
		</MSHelp:TOCTitle>
		<MSHelp:RLTitle>
			<xsl:attribute name="Title"><xsl:value-of select="concat( $title, ' Namespace' )"/></xsl:attribute>
		</MSHelp:RLTitle>	
	</xsl:template>
	
	<xsl:template match="enumeration" mode="MSHelpTitle">
		<xsl:param name="title" />
		<MSHelp:TOCTitle>
			<xsl:attribute name="Title"><xsl:value-of select="$title"/></xsl:attribute>
		</MSHelp:TOCTitle>
		<MSHelp:RLTitle>
			<xsl:attribute name="Title"><xsl:value-of select="$title"/></xsl:attribute>
		</MSHelp:RLTitle>	
	</xsl:template>
	
	
	<xsl:template match="enumeration" mode="FIndex">
		<xsl:call-template name="add-index-term">
			<xsl:with-param name="index">F</xsl:with-param>
			<xsl:with-param name="term" select="substring-after( @id, ':')"/>
		</xsl:call-template>
		<xsl:apply-templates select="field" mode="FIndex"/>
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
	
	<xsl:template match="namespace" mode="FIndex">
		<!-- TODO - look at how nested namespace appear -->
		<xsl:call-template name="add-index-term">
			<xsl:with-param name="index">F</xsl:with-param>
			<xsl:with-param name="term" select="@name"/>
		</xsl:call-template>
	</xsl:template>
	
	<xsl:template match="namespace" mode="KIndex">
		<!-- TODO - look at how nested namespace appear -->
		<xsl:call-template name="add-index-term">
			<xsl:with-param name="index">K</xsl:with-param>
			<xsl:with-param name="term" select="concat( @name, ' namespace' )"/>
		</xsl:call-template>
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
	
	
	<xsl:template match="enumeration | namespace" mode="AIndex">
		<MSHelp:Keyword Index="A">
			<xsl:attribute name="Term">
				<xsl:call-template name="get-filename-for-type">
					<xsl:with-param name="id" select="@id"/>
				</xsl:call-template>
			</xsl:attribute>
		</MSHelp:Keyword>			
	</xsl:template>		
	
	<xsl:template name="add-index-term">
		<xsl:param name="index"/>
		<xsl:param name="term"/>
		<MSHelp:Keyword>
			<xsl:attribute name="Index"><xsl:value-of select="$index"/></xsl:attribute>
			<xsl:attribute name="Term"><xsl:value-of select="$term"/></xsl:attribute>
		</MSHelp:Keyword>
	</xsl:template>	
	
</xsl:stylesheet>

  