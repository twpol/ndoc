<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:MSHelp="http://msdn.microsoft.com/mshelp">
	<!-- -->
	<xsl:output method="html" indent="no" encoding="Windows-1252" version="3.2" doctype-public="-//W3C//DTD HTML 3.2 Final//EN" />
	<!-- -->
	<xsl:include href="common.xslt" />
	<!-- -->
	<xsl:param name='event-id' />
	<!-- -->
	<xsl:template match="/">
		<xsl:apply-templates select="ndoc/assembly/module/namespace/*/event[@id=$event-id]" />
	</xsl:template>
	
	<!-- -->
	
	<xsl:template match="event">
		<html dir="LTR">
			<xsl:call-template name="html-head">
				<xsl:with-param name="title" select="concat(../@name, '.', @name, ' Event')" />
				<xsl:with-param name="page-type" select="'event'"/>				
			</xsl:call-template>
			<body topmargin="0" id="bodyID" class="dtBODY">
				<object id="obj_cook" classid="clsid:59CC0C20-679B-11D2-88BD-0800361A1803" style="display:none;"></object>
				<xsl:call-template name="title-row">
					<xsl:with-param name="type-name">
						<xsl:value-of select="../@name" />.<xsl:value-of select="@name" /> Event
					</xsl:with-param>
				</xsl:call-template>
				<div id="nstext" valign="bottom">
					<xsl:call-template name="summary-section" />
					<xsl:call-template name="syntax-section"/>
					<xsl:variable name="type" select="@type" />
					<xsl:variable name="eventargs-id" select="concat('T:', //delegate[@id=concat('T:', $type)]/parameter[contains(@type, 'EventArgs')][1]/@type)" />
					<xsl:variable name="thisevent" select="//class[@id=$eventargs-id]" />
					<xsl:variable name="properties" select="$thisevent/property[@access='Public' and not(@static)]" />
					<xsl:variable name="properties-count" select="count($properties)" />
					<xsl:if test="$properties-count > 0">
						<h4 class="dtH4">Event Data</h4>
						<p>
							<xsl:text>The event handler receives an argument of type </xsl:text>
							<a>
								<xsl:attribute name="href">
									<xsl:call-template name="get-filename-for-type">
										<xsl:with-param name="id" select="$eventargs-id" />
									</xsl:call-template>
								</xsl:attribute>
								<xsl:value-of select="$thisevent/@name" />
							</a>
							<xsl:text> containing data related to this event. The following </xsl:text>
							<B>
								<xsl:value-of select="$thisevent/@name" />
							</B>
							<xsl:choose>
								<xsl:when test="$properties-count > 1">
									<xsl:text> properties provide </xsl:text>
								</xsl:when>
								<xsl:otherwise>
									<xsl:text> property provides </xsl:text>
								</xsl:otherwise>
							</xsl:choose>
							<xsl:text>information specific to this event.</xsl:text>
						</p>
						<div class="tablediv">
							<table class="dtTABLE" cellspacing="0">
								<tr valign="top">
									<th width="50%">Property</th>
									<th width="50%">Description</th>
								</tr>
								<xsl:apply-templates select="$properties">
									<xsl:sort select="@name" />
								</xsl:apply-templates>
							</table>
						</div>
					</xsl:if>
					<xsl:call-template name="implements-section" />
					<xsl:call-template name="remarks-section" />
					<xsl:call-template name="exceptions-section" />
					<xsl:call-template name="example-section" />
					<xsl:call-template name="requirements-section" />
					<xsl:call-template name="seealso-section">
						<xsl:with-param name="page">event</xsl:with-param>
					</xsl:call-template>

					<xsl:call-template name="footer-row">
						<xsl:with-param name="type-name">
							<xsl:value-of select="../@name" />.<xsl:value-of select="@name" /> Event
						</xsl:with-param>
					</xsl:call-template>
				</div>
			</body>
		</html>
	</xsl:template>
	
	<!-- -->
	
	<xsl:template match="property">
		<xsl:variable name="name" select="@name" />
		<xsl:if test="not(preceding-sibling::property[@name=$name])">
			<tr VALIGN="top">
				<xsl:choose>
					<xsl:when test="following-sibling::property[@name=$name]">
						<td width="50%">
							<a>
								<xsl:attribute name="href">
									<xsl:call-template name="get-filename-for-current-property-overloads" />
								</xsl:attribute>
								<xsl:value-of select="@name" />
							</a>
						</td>
						<td width="50%">Overloaded. <xsl:apply-templates select="documentation/summary/node()" mode="slashdoc" /></td>
					</xsl:when>
					<xsl:otherwise>
						<td width="50%">
							<xsl:choose>
								<xsl:when test="@declaringType">
									<xsl:variable name="declaring-type-id" select="concat('T:', @declaringType)" />
									<xsl:variable name="declaring-class" select="//class[@id=$declaring-type-id]" />
									<xsl:choose>
										<xsl:when test="$declaring-class">
											<a>
												<xsl:attribute name="href">
													<xsl:call-template name="get-filename-for-property" >
														<xsl:with-param name="property" select="$declaring-class/property[@name=$name]" />
													</xsl:call-template>
												</xsl:attribute>
												<xsl:value-of select="@name" />
											</a>
										</xsl:when>
										<xsl:when test="starts-with(@declaringType, 'System.')">
											<a>
												<xsl:attribute name="href">
													<xsl:call-template name="get-filename-for-system-property" />
												</xsl:attribute>
												<xsl:value-of select="@name" />
											</a>
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="@name" />
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<a>
										<xsl:attribute name="href">
											<xsl:call-template name="get-filename-for-current-property" />
										</xsl:attribute>
										<xsl:value-of select="@name" />
									</a>
								</xsl:otherwise>
							</xsl:choose>
						</td>
						<td width="50%">
							<xsl:apply-templates select="documentation/summary/node()" mode="nopara" />
						</td>
					</xsl:otherwise>
				</xsl:choose>
			</tr>
		</xsl:if>
	</xsl:template>
	<!-- -->
</xsl:stylesheet>
