<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
	xmlns:NUtil="urn:ndoc-sourceforge-net:documenters.NativeHtmlHelp2.xsltUtilities"
	exclude-result-prefixes="NUtil" >
	
	<xsl:template name="get-link-for-interface-method">	
		<xsl:param name="method"/>
		<xsl:param name="interface-method"/>
		<xsl:param name="link-text"/>
		<xsl:choose>
			<xsl:when test="$interface-method">
				<xsl:call-template name="get-link-for-member">
					<xsl:with-param name="member" select="$interface-method"/>
					<xsl:with-param name="link-text" select="$link-text"/>		
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="get-link-for-member">
					<xsl:with-param name="member" select="$method"/>
					<xsl:with-param name="link-text" select="$link-text"/>		
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>	
	</xsl:template>
		
	<xsl:template name="get-link-for-interface-event">		
		<xsl:param name="event"/>
		<xsl:param name="interface-event"/>
		<xsl:param name="link-text"/>
		<xsl:choose>
			<xsl:when test="$interface-event">
				<xsl:call-template name="get-link-for-member">
					<xsl:with-param name="member" select="$interface-event"/>
					<xsl:with-param name="link-text" select="$link-text"/>		
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="get-link-for-member">
					<xsl:with-param name="member" select="$event"/>
					<xsl:with-param name="link-text" select="$link-text"/>		
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>	
	</xsl:template>
   
	<xsl:template name="get-link-for-interface-property">	
		<xsl:param name="property"/>
		<xsl:param name="interface-property"/>
		<xsl:param name="link-text"/>
		<xsl:choose>
			<xsl:when test="$interface-property">
				<xsl:call-template name="get-link-for-member">
					<xsl:with-param name="member" select="$interface-property"/>
					<xsl:with-param name="link-text" select="$link-text"/>		
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="get-link-for-member">
					<xsl:with-param name="member" select="$property"/>
					<xsl:with-param name="link-text" select="$link-text"/>		
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>   
   
	<xsl:template name="get-link-for-member">		
		<xsl:param name="member"/>
		<xsl:param name="link-text"/>
		
		<xsl:variable name="mid">
			<xsl:choose>
				<xsl:when test="$member/@declaringType">
					<xsl:value-of select="NUtil:Replace( $member/@id, substring-after( $member/../@id, ':' ), $member/@declaringType )"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$member/@id"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable> 

		<xsl:variable name="cref">
			<xsl:value-of select="NUtil:GetLocalCRef( $member/@id )"/>
		</xsl:variable>
		
		<xsl:choose>
			<xsl:when test="$cref=''">
				<xsl:variable name="a-index" select="NUtil:GetAIndex( $mid )"/>
				<xsl:call-template name="get-xlink">
					<xsl:with-param name="a-index" select="$a-index"/>
					<xsl:with-param name="link-text" select="$link-text"/>
					<xsl:with-param name="ns-key" select="$member/@declaringType"/>			
				</xsl:call-template>					
			</xsl:when>
			<xsl:otherwise>
				<xsl:choose>
					<xsl:when test="$member[@declaringType]">
						<a href="{NUtil:GetInheritedMemberHRef( string( $mid ), $member )}">			
							<xsl:value-of select="$link-text"/> 
						</a>					
					</xsl:when>
					<xsl:otherwise>
						<a>
							<xsl:attribute name="href"><xsl:value-of select="$cref"/></xsl:attribute> 			
							<xsl:value-of select="$link-text"/> 
						</a>					
					</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>		
	</xsl:template>
	
	<xsl:template name="get-link-for-member-overload">		
		<xsl:param name="member"/>
		<xsl:param name="link-text"/>
		
		<xsl:variable name="mid">
			<xsl:choose>
				<xsl:when test="$member/@declaringType">
					<xsl:value-of select="NUtil:Replace( $member/@id, substring-after( $member/../@id, ':' ), $member/@declaringType )"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$member/@id"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable> 

		<xsl:variable name="cref">
			<xsl:value-of select="NUtil:GetLocalCRef( $mid )"/>
		</xsl:variable>
		
		<xsl:choose>
			<xsl:when test="$cref=''">
				<xsl:variable name="a-index" select="NUtil:GetAIndex( $mid )"/>
				<xsl:call-template name="get-xlink">
					<xsl:with-param name="a-index" select="$a-index"/>
					<xsl:with-param name="link-text" select="$link-text"/>
					<xsl:with-param name="ns-key" select="$member/@declaringType"/>			
				</xsl:call-template>					
			</xsl:when>
			<xsl:otherwise>
				<xsl:choose>
					<xsl:when test="$member[@declaringType]">
						<a href="{NUtil:GetInheritedMemberOverloadHRef( string( $mid ), $member )}">			
							<xsl:value-of select="$link-text"/> 
						</a>					
					</xsl:when>
					<xsl:otherwise>
						<a href="{NUtil:GetMemberOverloadHRef( $member )}">			
							<xsl:value-of select="$link-text"/> 
						</a>					
					</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>		
	</xsl:template>

	<xsl:template name="get-link-for-type-name">
		<xsl:param name="type-name"/>
		<xsl:param name="link-text"/>

		<xsl:call-template name="get-link-for-type">
			<xsl:with-param name="type" select="concat( 'T:', $type-name )"/>
			<xsl:with-param name="link-text" select="$link-text"/>
		</xsl:call-template>
	</xsl:template>  
	
	<xsl:template name="get-link-for-type">
		<xsl:param name="type"/>
		<xsl:param name="link-text"/>

		<xsl:variable name="cref">
			<xsl:value-of select="NUtil:GetLocalCRef( $type/@id )"/>
		</xsl:variable>
		
		<xsl:choose>
			<xsl:when test="$cref=''">
				<xsl:call-template name="get-xlink-for-foreign-type">
					<xsl:with-param name="type" select="$type"/>
					<xsl:with-param name="link-text" select="$link-text"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<a>
					<xsl:attribute name="href"><xsl:value-of select="$cref"/></xsl:attribute> 			
					<xsl:value-of select="$link-text"/> 
				</a>
			</xsl:otherwise>
		</xsl:choose>	
	</xsl:template>   
	
	<xsl:template name="get-xlink-for-foreign-type">
		<xsl:param name="type"/>
		<xsl:param name="link-text"/>

		<xsl:variable name="text">
			<xsl:choose>
				<xsl:when test="string-length($link-text) != 0">
					<xsl:value-of select="$link-text"/>								
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="substring-after( $type, ':' )"/>				
				</xsl:otherwise>
			</xsl:choose>	
		</xsl:variable>
		<xsl:call-template name="get-xlink">
			<xsl:with-param name="a-index" select="NUtil:GetAIndex( $type )"/>
			<xsl:with-param name="link-text" select="$text"/>
			<xsl:with-param name="ns-key" select="$type"/>
		</xsl:call-template>		
	</xsl:template>
	
	<xsl:template name="get-xlink">
		<xsl:param name="a-index"/>
		<xsl:param name="link-text"/>
		<xsl:param name="ns-key"/>

		<MSHelp:link keywords="{$a-index}" indexMoniker="!DefaultAssociativeIndex" namespace="{NUtil:GetHelpNamespace( $ns-key )}" tabindex="0">
			<xsl:value-of select="$link-text"/>				
		</MSHelp:link>		
	</xsl:template>

	
</xsl:stylesheet>