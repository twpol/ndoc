<?xml version="1.0" encoding="UTF-8" ?>
<xsl:transform version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:template name="vb-type-syntax">
		<pre class="syntax">
			<span class="lang">[Visual Basic]</span>
			<br />
			<xsl:if test="@abstract = 'true'">
				<xsl:text>MustInherit&#160;</xsl:text>
			</xsl:if>
			<xsl:if test="@sealed = 'true'">
				<xsl:text>NotInheritable&#160;</xsl:text>
			</xsl:if>
			<xsl:call-template name="vb-type-access">
				<xsl:with-param name="access" select="@access" />
			</xsl:call-template>
			<xsl:text>&#160;</xsl:text>
			<xsl:choose>
				<xsl:when test="local-name() = 'class'">Class</xsl:when>
				<xsl:when test="local-name() = 'interface'">Interface</xsl:when>
				<xsl:when test="local-name() = 'structure'">Structure</xsl:when>
				<xsl:when test="local-name() = 'enumeration'">Enum</xsl:when>
				<xsl:otherwise>ERROR</xsl:otherwise>
			</xsl:choose>
			<xsl:text>&#160;</xsl:text>
			<xsl:value-of select="@name" />
			<xsl:if test="@baseType">
				<br />
				<xsl:text>&#160;&#160;&#160;Inherits&#160;</xsl:text>
				<xsl:value-of select="@baseType" />
			</xsl:if>
			<xsl:if test="implements">
				<br />
				<xsl:text>&#160;&#160;&#160;Implements&#160;</xsl:text>
				<xsl:for-each select="implements">
					<xsl:value-of select="." />
					<xsl:if test="position()!=last()">
						<xsl:text>, </xsl:text>
					</xsl:if>
				</xsl:for-each>
			</xsl:if>
		</pre>
	</xsl:template>
	<xsl:template name="vb-type-access">
		<xsl:param name="access" />
		<xsl:choose>
			<xsl:when test="$access='Public'">Public</xsl:when>
			<xsl:when test="$access='NotPublic'"></xsl:when>
			<xsl:when test="$access='NestedPublic'">Public</xsl:when>
			<xsl:when test="$access='NestedFamily'">Protected</xsl:when>
			<xsl:when test="$access='NestedFamilyOrAssembly'">Protected Friend</xsl:when>
			<xsl:when test="$access='NestedAssembly'">Friend</xsl:when>
			<xsl:when test="$access='NestedPrivate'">Private</xsl:when>
			<xsl:otherwise>ERROR</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
</xsl:transform>
