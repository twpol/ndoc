<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:NUtil="urn:ndoc-sourceforge-net:documenters.NativeHtmlHelp2.xsltUtilities"
	xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
	exclude-result-prefixes="NUtil" >
	<!-- -->
	<xsl:output method="html" indent="no" encoding="utf-8" version="3.2" doctype-public="-//W3C//DTD HTML 3.2 Final//EN" />
	<!-- -->
	<xsl:include href="common.xslt" />
	<!-- -->
	<xsl:param name='member-id' />
	<!-- -->
	<xsl:template match="/">
		<xsl:apply-templates select="ndoc/assembly/module/namespace/*/*[@id=$member-id][1]" />
	</xsl:template>
	<!-- -->
	<xsl:template match="method | constructor | property | operator">
		<xsl:variable name="type">
			<xsl:choose>
				<xsl:when test="local-name(..)='interface'">Interface</xsl:when>
				<xsl:when test="local-name(..)='structure'">Structure</xsl:when>
				<xsl:otherwise>Class</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="childType">
			<xsl:choose>
				<xsl:when test="local-name()='method'">Method</xsl:when>
				<xsl:when test="local-name()='constructor'">Constructor</xsl:when>
				<xsl:when test="local-name()='operator'">
			    <xsl:call-template name="operator-name">
				    <xsl:with-param name="name">
				      <xsl:value-of select="@name" />
				    </xsl:with-param>
			    </xsl:call-template>
				</xsl:when>
				<xsl:otherwise>Property</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="memberName" select="@name" />
		<html dir="LTR">
			<xsl:call-template name="html-head">
				<xsl:with-param name="title">
					<xsl:choose>
						<xsl:when test="local-name()='constructor' or local-name()='operator'">
							<xsl:value-of select="../@name" />
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="@name" />
						</xsl:otherwise>
					</xsl:choose>
					<xsl:text>&#32;</xsl:text>
					<xsl:value-of select="$childType" />
				</xsl:with-param>
				<xsl:with-param name="page-type" select="$childType"/>	
				<xsl:with-param name="overload-page" select="true()"/>			
			</xsl:call-template>
			<body topmargin="0" id="bodyID" class="dtBODY">
				<object id="obj_cook" classid="clsid:59CC0C20-679B-11D2-88BD-0800361A1803" style="display:none;"></object>
				<xsl:call-template name="title-row">
					<xsl:with-param name="type-name">
						<xsl:value-of select="../@name" />
						<xsl:if test="local-name()='method' or local-name()='property' ">
							<xsl:text>.</xsl:text>
							<xsl:value-of select="@name" />
						</xsl:if>
						<xsl:text>&#160;</xsl:text>
						<xsl:value-of select="$childType" />
					</xsl:with-param>
				</xsl:call-template>
				<div id="nstext" valign="bottom">
					<xsl:call-template name="overloads-summary-section" />
					<h4 class="dtH4">Overload List</h4>
					<xsl:for-each select="parent::node()/*[@name=$memberName]">
						<xsl:sort order="ascending" select="@id"/>
						<xsl:choose>
							<xsl:when test="@declaringType and starts-with(@declaringType, 'System.')">
								<p>
									<xsl:text>Inherited from </xsl:text>
									<xsl:variable name="link-text">
										<xsl:call-template name="strip-namespace">
											<xsl:with-param name="name" select="@declaringType" />
										</xsl:call-template>
									</xsl:variable>
									<xsl:call-template name="get-link-for-type-name">
										<xsl:with-param name="type-name" select="@declaringType" />
										<xsl:with-param name="link-text" select="$link-text" />
									</xsl:call-template>									
									<xsl:text>.</xsl:text>
								</p>
								<blockquote class="dtBlock">
									<xsl:variable name="text">
										<xsl:apply-templates select="self::node()" mode="syntax" />
									</xsl:variable>
									<xsl:call-template name="get-xlink-for-system-member">
										<xsl:with-param name="text" select="@name"/>
										<xsl:with-param name="member" select="."/>
									</xsl:call-template>
								</blockquote>
							</xsl:when>
							<xsl:when test="@declaringType">
								<p>
									<xsl:text>Inherited from </xsl:text>
									<xsl:variable name="link-text">
										<xsl:call-template name="strip-namespace">
											<xsl:with-param name="name" select="@declaringType" />
										</xsl:call-template>
									</xsl:variable>
									<xsl:call-template name="get-link-for-type-name">
										<xsl:with-param name="type-name" select="@declaringType" />
										<xsl:with-param name="link-text" select="$link-text" />
									</xsl:call-template>										
									<xsl:text>.</xsl:text>
								</p>
								<blockquote class="dtBlock">
									<xsl:choose>
										<!-- not sure this block can ever get executed since constructors aren't inherited -->
										<xsl:when test="local-name()='constructor'">
											<a href="{NUtil:GetCustructorOverloadHRef( string( @declaringType ) )}">
												<xsl:apply-templates select="self::node()" mode="syntax" />
											</a>										
										</xsl:when>
										<xsl:otherwise>
											<a href="{NUtil:GetMemberOverloadHRef( string( @declaringType ), string( @name ) )}">
												<xsl:apply-templates select="self::node()" mode="syntax" />
											</a>										
										</xsl:otherwise>
									</xsl:choose>
								</blockquote>
							</xsl:when>
							<xsl:otherwise>
								<p>
									<xsl:call-template name="summary-with-no-paragraph">
										<xsl:with-param name="member" select="." />
									</xsl:call-template>
								</p>
								<blockquote class="dtBlock">
										<xsl:apply-templates select="self::node()" mode="syntax">
											<xsl:with-param name="href" select="NUtil:GetMemberHRef( . )"/>
										</xsl:apply-templates>								
								</blockquote>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:for-each>
					<xsl:call-template name="overloads-remarks-section" />
					<xsl:call-template name="overloads-example-section" />
					<xsl:call-template name="seealso-section">
						<xsl:with-param name="page">memberoverload</xsl:with-param>
					</xsl:call-template>

					<xsl:call-template name="footer-row">
						<xsl:with-param name="type-name">
							<xsl:value-of select="../@name" />
							<xsl:if test="local-name()='method' or local-name()='property' ">
								<xsl:text>.</xsl:text>
								<xsl:value-of select="@name" />
							</xsl:if>
							<xsl:text>&#160;</xsl:text>
							<xsl:value-of select="$childType" />
						</xsl:with-param>
					</xsl:call-template>
				</div>
			</body>
		</html>
	</xsl:template>
	<!-- -->
	<xsl:template match="constructor | method | operator" mode="syntax">
		<xsl:param name="href"/>
		<xsl:apply-templates select="." mode="cs-inline-syntax">
			<xsl:with-param name="lang" select="'Visual Basic'"/>
			<xsl:with-param name="href" select="$href"/>
		</xsl:apply-templates>
		<br/>
		<xsl:apply-templates select="." mode="cs-inline-syntax">
			<xsl:with-param name="lang" select="'C#'"/>
			<xsl:with-param name="href" select="$href"/>
		</xsl:apply-templates>
		<br/>
		<xsl:apply-templates select="." mode="cs-inline-syntax">
			<xsl:with-param name="lang" select="'C++'"/>
			<xsl:with-param name="href" select="$href"/>
		</xsl:apply-templates>				
		<br/>
		<xsl:apply-templates select="." mode="cs-inline-syntax">
			<xsl:with-param name="lang" select="'JScript'"/>
			<xsl:with-param name="href" select="$href"/>
		</xsl:apply-templates>		
	</xsl:template>
	<!-- -->
	<xsl:template match="property" mode="syntax">
		<xsl:call-template name="cs-property-syntax">
			<xsl:with-param name="indent" select="false()" />
			<xsl:with-param name="display-names" select="false()" />
			<xsl:with-param name="link-types" select="false()" />
		</xsl:call-template>
	</xsl:template>
	<!-- -->
</xsl:stylesheet>
