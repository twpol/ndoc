<?xml version="1.0" encoding="UTF-8" ?>
<xsl:transform version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<!-- -->
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
				<xsl:when test="local-name() = 'delegate'">
					<xsl:text>Delegate&#160;</xsl:text>
					<xsl:choose>
						<xsl:when test="@returnType = 'System.Void'">Sub</xsl:when>
						<xsl:otherwise>Function</xsl:otherwise>
					</xsl:choose>
				</xsl:when>
				<xsl:otherwise>ERROR</xsl:otherwise>
			</xsl:choose>
			<xsl:text>&#160;</xsl:text>
			<xsl:value-of select="@name" />
			<xsl:choose>
				<xsl:when test="local-name() != 'delegate'">
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
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>( _</xsl:text>
					<br />
					<xsl:apply-templates select="parameter" mode="vb" />
					<xsl:text>)</xsl:text>
					<xsl:if test="@returnType != 'System.Void'">
						<xsl:text>&#160;As&#160;</xsl:text>
						<xsl:call-template name="strip-namespace">
							<xsl:with-param name="name" select="@returnType" />
						</xsl:call-template>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>
		</pre>
	</xsl:template>
	<!-- -->
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
	<!-- -->
	<xsl:template match="parameter" mode="vb">
		<xsl:text>&#160;&#160;&#160;</xsl:text>
		<xsl:text>ByVal </xsl:text>
		<xsl:value-of select="@name" />
		<xsl:text>&#160;As&#160;</xsl:text>
		<xsl:call-template name="strip-namespace">
			<xsl:with-param name="name" select="@type" />
		</xsl:call-template>
		<xsl:if test="position() != last()">
			<xsl:text>,</xsl:text>
		</xsl:if>
		<xsl:text>&#160;_</xsl:text>
		<br />
	</xsl:template>
	<!-- -->
</xsl:transform>
