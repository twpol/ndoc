<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
	xmlns:NUtil="urn:ndoc-sourceforge-net:documenters.NativeHtmlHelp2.xsltUtilities"
	exclude-result-prefixes="NUtil" >

	<xsl:template name="get-link-for-interface-method">
		<xsl:param name="declaring-type-name"/>		
		<xsl:param name="method"/>
		<xsl:param name="interface-method"/>
		<xsl:param name="link-text"/>
		<xsl:choose>
			<xsl:when test="$interface-method">
				<xsl:call-template name="get-link-for-member">
					<xsl:with-param name="declaring-type-name" select="$declaring-type-name"/>
					<xsl:with-param name="member" select="$interface-method"/>
					<xsl:with-param name="link-text" select="$link-text"/>		
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="get-link-for-member">
					<xsl:with-param name="declaring-type-name" select="$declaring-type-name"/>
					<xsl:with-param name="member" select="$method"/>
					<xsl:with-param name="link-text" select="$link-text"/>		
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>	
	</xsl:template>
		
	<xsl:template name="get-link-for-interface-event">
		<xsl:param name="declaring-type-name"/>		
		<xsl:param name="event"/>
		<xsl:param name="interface-event"/>
		<xsl:param name="link-text"/>
		<xsl:choose>
			<xsl:when test="$interface-event">
				<xsl:call-template name="get-link-for-member">
					<xsl:with-param name="declaring-type-name" select="$declaring-type-name"/>
					<xsl:with-param name="member" select="$interface-event"/>
					<xsl:with-param name="link-text" select="$link-text"/>		
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="get-link-for-member">
					<xsl:with-param name="declaring-type-name" select="$declaring-type-name"/>
					<xsl:with-param name="member" select="$event"/>
					<xsl:with-param name="link-text" select="$link-text"/>		
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>	
	</xsl:template>
   
	<xsl:template name="get-link-for-interface-property">
		<xsl:param name="declaring-type-name"/>		
		<xsl:param name="property"/>
		<xsl:param name="interface-property"/>
		<xsl:param name="link-text"/>
		<xsl:choose>
			<xsl:when test="$interface-property">
				<xsl:call-template name="get-link-for-member">
					<xsl:with-param name="declaring-type-name" select="$declaring-type-name"/>
					<xsl:with-param name="member" select="$interface-property"/>
					<xsl:with-param name="link-text" select="$link-text"/>		
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="get-link-for-member">
					<xsl:with-param name="declaring-type-name" select="$declaring-type-name"/>
					<xsl:with-param name="member" select="$property"/>
					<xsl:with-param name="link-text" select="$link-text"/>		
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>   
   
	<xsl:template name="get-link-for-member">
		<xsl:param name="declaring-type-name"/>		
		<xsl:param name="member"/>
		<xsl:param name="link-text"/>
		<xsl:choose>
			<xsl:when test="starts-with($declaring-type-name, 'System.')">
				<xsl:call-template name="get-xlink-for-system-member">
					<xsl:with-param name="member" select="$member"/>
					<xsl:with-param name="text" select="$link-text"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<a href="{NUtil:GetMemberHRef( $member )}">			
					<xsl:value-of select="$link-text"/> 
				</a>
			</xsl:otherwise>
		</xsl:choose>		
	</xsl:template>

	<xsl:template name="get-link-for-type-name">
		<xsl:param name="type-name"/>
		<xsl:param name="link-text"/>

		<xsl:call-template name="get-link-for-type">
			<xsl:with-param name="type-id" select="concat( 'T:', $type-name )"/>
			<xsl:with-param name="link-text" select="$link-text"/>
		</xsl:call-template>
	</xsl:template>  
	
	<xsl:template name="get-link-for-type">
		<xsl:param name="type-id"/>
		<xsl:param name="link-text"/>

		<xsl:choose>
			<xsl:when test="contains( $type-id, ':System.' )">
				<xsl:call-template name="get-xlink-for-system-type">
					<xsl:with-param name="type-id" select="$type-id"/>
					<xsl:with-param name="link-text" select="$link-text"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<a href="{NUtil:GetHRef( string( $type-id ) )}">		
					<xsl:value-of select="$link-text"/> 
				</a>
			</xsl:otherwise>
		</xsl:choose>	
	</xsl:template>   
	
	<xsl:template name="get-xlink-for-system-type">
		<xsl:param name="type-id"/>
		<xsl:param name="link-text"/>

		<xsl:variable name="text">
			<xsl:choose>
				<xsl:when test="string-length($link-text) != 0">
					<xsl:value-of select="$link-text"/>								
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="substring-after( $type-id, ':' )"/>				
				</xsl:otherwise>
			</xsl:choose>	
		</xsl:variable>
		<xsl:call-template name="get-xlink">
			<xsl:with-param name="a-index" select="NUtil:GetHRef( $type-id )"/>
			<xsl:with-param name="link-text" select="$text"/>
		</xsl:call-template>		
	</xsl:template>
	
	<xsl:template name="get-xlink">
		<xsl:param name="a-index"/>
		<xsl:param name="link-text"/>
		<MSHelp:link keywords="{$a-index}" indexMoniker="!DefaultAssociativeIndex" namespace="{$ndoc-sdk-doc-base-url}" tabindex="0">
			<xsl:value-of select="$link-text"/>				
		</MSHelp:link>		
	</xsl:template>


	<xsl:template name="get-xlink-for-system-member">
		<xsl:param name="text"/>
		<xsl:param name="member"/>
		
		<!-- this is the last place where links are being resolved in xslt -->
		<xsl:variable name="a-index">
			<xsl:value-of select="concat('frlrf', translate($member/@declaringType, '.[,]', ''), 'Class', translate($member/@name, '.[,]', ''), 'Topic' )" />				
		</xsl:variable>
		<xsl:call-template name="get-xlink">
			<xsl:with-param name="a-index" select="$a-index"/>
			<xsl:with-param name="link-text" select="$text"/>
		</xsl:call-template>				
	</xsl:template>
	
</xsl:stylesheet>