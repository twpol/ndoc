<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:NUtil="urn:NDocUtil"
	exclude-result-prefixes="NUtil" xmlns:ndoc="urn:ndoc-schema">
	<!-- -->
	<xsl:template match="/">
		<xsl:apply-templates select="ndoc:ndoc/ndoc:assembly[@name=$assembly-name]/ndoc:module/ndoc:namespace/ndoc:*[@id=$type-id]" />
	</xsl:template>
	<!-- -->
	<xsl:template match="ndoc:class">
		<xsl:call-template name="type-members">
			<xsl:with-param name="type">Class</xsl:with-param>
		</xsl:call-template>
	</xsl:template>
	<!-- -->
	<xsl:template match="ndoc:interface">
		<xsl:call-template name="type-members">
			<xsl:with-param name="type">Interface</xsl:with-param>
		</xsl:call-template>
	</xsl:template>
	<!-- -->
	<xsl:template match="ndoc:structure">
		<xsl:call-template name="type-members">
			<xsl:with-param name="type">Structure</xsl:with-param>
		</xsl:call-template>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-big-member-plural">
		<xsl:param name="member" />
		<xsl:choose>
			<xsl:when test="$member='field'">Fields</xsl:when>
			<xsl:when test="$member='property'">Properties</xsl:when>
			<xsl:when test="$member='event'">Events</xsl:when>
			<xsl:when test="$member='operator'">
				<xsl:choose>
					<xsl:when test="*[local-name()=$member and @name!='op_Implicit' and @name!='op_Explicit']">
						<xsl:choose>
							<xsl:when test="*[local-name()=$member and (@name='op_Implicit' or @name='op_Explicit')]">
							Operators and Type Conversions
							</xsl:when>
							<xsl:otherwise>Operators</xsl:otherwise>
						</xsl:choose>
					</xsl:when>
					<xsl:otherwise>Type Conversions</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>Methods</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-small-member-plural">
		<xsl:param name="member" />
		<xsl:choose>
			<xsl:when test="$member='field'">fields</xsl:when>
			<xsl:when test="$member='property'">properties</xsl:when>
			<xsl:when test="$member='event'">events</xsl:when>
			<xsl:when test="$member='operator'">
				<xsl:choose>
					<xsl:when test="*[local-name()=$member and @name!='op_Implicit' and @name!='op_Explicit']">
						<xsl:choose>
							<xsl:when test="*[local-name()=$member and (@name='op_Implicit' or @name='op_Explicit')]">
							operators and type conversions
							</xsl:when>
							<xsl:otherwise>operators</xsl:otherwise>
						</xsl:choose>
					</xsl:when>
					<xsl:otherwise>type conversions</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>methods</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template name="public-static-section">
		<xsl:param name="member" />
		<xsl:if test="*[local-name()=$member and @access='Public' and @contract='Static']">
			<h4 class="dtH4">
				<xsl:text>Public Static </xsl:text>
				<xsl:if test="$ndoc-vb-syntax">
  				<xsl:text>(Shared) </xsl:text>
  			</xsl:if>
				<xsl:call-template name="get-big-member-plural">
					<xsl:with-param name="member" select="$member" />
				</xsl:call-template>
			</h4>
			<div class="tablediv">
				<table class="dtTABLE" cellspacing="0">
					<xsl:choose>
						<!-- since operators must be public static this is 
							 the only place we need to do this -->
						<xsl:when test="$member='operator'">
							<xsl:apply-templates select="*[local-name()=$member and @name!='op_Implicit' and @name!='op_Explicit']">
								<xsl:sort select="@id" />
							</xsl:apply-templates>
							<xsl:apply-templates select="*[local-name()=$member and (@name='op_Implicit' or @name='op_Explicit')]">
								<xsl:sort select="@id" />
							</xsl:apply-templates>
						</xsl:when>
						<xsl:otherwise>
							<xsl:apply-templates select="*[local-name()=$member and @access='Public' and @contract='Static']">
								<xsl:sort select="@name" />
							</xsl:apply-templates>
						</xsl:otherwise>
					</xsl:choose>
				</table>
			</div>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="protected-static-section">
		<xsl:param name="member" />
		<xsl:if test="*[local-name()=$member and @access='Family' and @contract='Static']">
			<h4 class="dtH4">
				<xsl:text>Protected Static </xsl:text>
				<xsl:if test="$ndoc-vb-syntax">
  				<xsl:text>(Shared) </xsl:text>
  			</xsl:if>
				<xsl:call-template name="get-big-member-plural">
					<xsl:with-param name="member" select="$member" />
				</xsl:call-template>
			</h4>
			<div class="tablediv">
				<table class="dtTABLE" cellspacing="0">
					<xsl:apply-templates select="*[local-name()=$member and @access='Family' and @contract='Static']">
						<xsl:sort select="@name" />
					</xsl:apply-templates>
				</table>
			</div>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="protected-internal-static-section">
		<xsl:param name="member" />
		<xsl:if test="*[local-name()=$member and @access='FamilyOrAssembly' and @contract='Static']">
			<h4 class="dtH4">
				<xsl:text>Protected Internal Static </xsl:text>
				<xsl:if test="$ndoc-vb-syntax">
  				<xsl:text>(Shared) </xsl:text>
  			</xsl:if>
				<xsl:call-template name="get-big-member-plural">
					<xsl:with-param name="member" select="$member" />
				</xsl:call-template>
			</h4>
			<div class="tablediv">
				<table class="dtTABLE" cellspacing="0">
					<xsl:apply-templates select="*[local-name()=$member and @access='FamilyOrAssembly' and @contract='Static']">
						<xsl:sort select="@dname" />
					</xsl:apply-templates>
				</table>
			</div>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="internal-static-section">
		<xsl:param name="member" />
		<xsl:if test="*[local-name()=$member and @access='Assembly' and @contract='Static']">
			<h4 class="dtH4">
				<xsl:text>Internal Static </xsl:text>
				<xsl:if test="$ndoc-vb-syntax">
  				<xsl:text>(Shared) </xsl:text>
  			</xsl:if>
				<xsl:call-template name="get-big-member-plural">
					<xsl:with-param name="member" select="$member" />
				</xsl:call-template>
			</h4>
			<div class="tablediv">
				<table class="dtTABLE" cellspacing="0">
					<xsl:apply-templates select="*[local-name()=$member and @access='Assembly' and @contract='Static']">
						<xsl:sort select="@name" />
					</xsl:apply-templates>
				</table>
			</div>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="private-static-section">
		<xsl:param name="member" />
		<xsl:if test="*[local-name()=$member and @access='Private' and @contract='Static']">
			<h4 class="dtH4">
				<xsl:text>Private Static </xsl:text>
				<xsl:if test="$ndoc-vb-syntax">
  				<xsl:text>(Shared) </xsl:text>
  			</xsl:if>
				<xsl:call-template name="get-big-member-plural">
					<xsl:with-param name="member" select="$member" />
				</xsl:call-template>
			</h4>
			<div class="tablediv">
				<table class="dtTABLE" cellspacing="0">
					<xsl:apply-templates select="*[local-name()=$member and @access='Private' and @contract='Static']">
						<xsl:sort select="@name" />
					</xsl:apply-templates>
				</table>
			</div>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="public-instance-section">
		<xsl:param name="member" />
		<xsl:if test="*[local-name()=$member and @access='Public' and not(@contract='Static')]">
			<h4 class="dtH4">
				<xsl:text>Public Instance </xsl:text>
				<xsl:call-template name="get-big-member-plural">
					<xsl:with-param name="member" select="$member" />
				</xsl:call-template>
			</h4>
			<div class="tablediv">
				<table class="dtTABLE" cellspacing="0">
					<xsl:apply-templates select="*[local-name()=$member and @access='Public' and not(@contract='Static')]">
						<xsl:sort select="@name" />
					</xsl:apply-templates>
				</table>
			</div>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="protected-instance-section">
		<xsl:param name="member" />
		<xsl:if test="*[local-name()=$member and @access='Family' and not(@contract='Static')]">
			<h4 class="dtH4">
				<xsl:text>Protected Instance </xsl:text>
				<xsl:call-template name="get-big-member-plural">
					<xsl:with-param name="member" select="$member" />
				</xsl:call-template>
			</h4>
			<div class="tablediv">
				<table class="dtTABLE" cellspacing="0">
					<xsl:apply-templates select="*[local-name()=$member and @access='Family' and not(@contract='Static')]">
						<xsl:sort select="@name" />
					</xsl:apply-templates>
				</table>
			</div>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="protected-internal-instance-section">
		<xsl:param name="member" />
		<xsl:if test="*[local-name()=$member and @access='FamilyOrAssembly' and not(@contract='Static')]">
			<h4 class="dtH4">
				<xsl:text>Protected Internal Instance </xsl:text>
				<xsl:call-template name="get-big-member-plural">
					<xsl:with-param name="member" select="$member" />
				</xsl:call-template>
			</h4>
			<div class="tablediv">
				<table class="dtTABLE" cellspacing="0">
					<xsl:apply-templates select="*[local-name()=$member and @access='FamilyOrAssembly' and not(@contract='Static')]">
						<xsl:sort select="@name" />
					</xsl:apply-templates>
				</table>
			</div>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="internal-instance-section">
		<xsl:param name="member" />
		<xsl:if test="*[local-name()=$member and @access='Assembly' and not(@contract='Static')]">
			<h4 class="dtH4">
				<xsl:text>Internal Instance </xsl:text>
				<xsl:call-template name="get-big-member-plural">
					<xsl:with-param name="member" select="$member" />
				</xsl:call-template>
			</h4>
			<div class="tablediv">
				<table class="dtTABLE" cellspacing="0">
					<xsl:apply-templates select="*[local-name()=$member and @access='Assembly' and not(@contract='Static')]">
						<xsl:sort select="@name" />
					</xsl:apply-templates>
				</table>
			</div>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="private-instance-section">
		<xsl:param name="member" />
		<xsl:if test="*[local-name()=$member and @access='Private' and not(@contract='Static') and not(@interface)]">
			<h4 class="dtH4">
				<xsl:text>Private Instance </xsl:text>
				<xsl:call-template name="get-big-member-plural">
					<xsl:with-param name="member" select="$member" />
				</xsl:call-template>
			</h4>
			<div class="tablediv">
				<table class="dtTABLE" cellspacing="0">
					<xsl:apply-templates select="*[local-name()=$member and @access='Private' and not(@contract='Static') and not(@interface)]">
						<xsl:sort select="@name" />
					</xsl:apply-templates>
				</table>
			</div>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="explicit-interface-implementations">
		<xsl:param name="member" />
		<xsl:if test="*[(local-name()='property' or local-name()='method'or local-name()='event') and @access='Private' and not(@contract='Static') and @interface]">
			<h4 class="dtH4">
				<xsl:text>Explicit Interface Implementations</xsl:text>
			</h4>
			<div class="tablediv">
				<table class="dtTABLE" cellspacing="0">
					<xsl:apply-templates select="*[(local-name()='property' or local-name()='method'or local-name()='event') and @access='Private' and not(@contract='Static') and @interface]">
						<xsl:sort select="@name" />
					</xsl:apply-templates>
				</table>
			</div>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="images">
		<xsl:param name="access" />
		<xsl:param name="local-name" />
		<xsl:param name="contract" />
		<xsl:choose>
			<xsl:when test="$access='Public'">
				<img>
					<xsl:attribute name="src">
						<xsl:text>pub</xsl:text>
						<xsl:value-of select="$local-name"/>
						<xsl:text>.gif</xsl:text>
					</xsl:attribute>
				</img>
			</xsl:when>
			<xsl:when test="$access='Family'">
				<img>
					<xsl:attribute name="src">
						<xsl:text>prot</xsl:text>
						<xsl:value-of select="$local-name"/>
						<xsl:text>.gif</xsl:text>
					</xsl:attribute>
				</img>
			</xsl:when>
			<xsl:when test="$access='Private'">
				<img>
					<xsl:attribute name="src">
						<xsl:text>priv</xsl:text>
						<xsl:value-of select="$local-name"/>
						<xsl:text>.gif</xsl:text>
					</xsl:attribute>
				</img>
			</xsl:when>
			<xsl:when test="$access='Assembly' or $access='FamilyOrAssembly'">
				<img>
					<xsl:attribute name="src">
						<xsl:text>int</xsl:text>
						<xsl:value-of select="$local-name"/>
						<xsl:text>.gif</xsl:text>
					</xsl:attribute>
				</img>
			</xsl:when>
		</xsl:choose>
		<xsl:if test="$contract='Static'">
			<img src="static.gif" />
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template match="ndoc:property[@declaringType]">
		<xsl:variable name="id" select="@id" />
		<xsl:variable name="name" select="@name" />
		<xsl:variable name="declaring-id" select="@declaringId" />
		<xsl:variable name="declaring-assembly" select="@declaringAssembly" />
		<xsl:variable name="declaring-type-id" select="concat('T:', @declaringType)" />
		<xsl:text>&#10;</xsl:text>
		<tr VALIGN="top">
			<xsl:variable name="declaring-class" select="ancestor::ndoc:ndoc/ndoc:assembly[@name=$declaring-assembly]//ndoc:class[@id=$declaring-type-id]" />
			<xsl:choose>
				<xsl:when test="$declaring-class">
					<td width="50%">
						<xsl:call-template name="images">
							<xsl:with-param name="access" select="@access" />
							<xsl:with-param name="contract" select="@contract" />
							<xsl:with-param name="local-name" select="local-name()" />
						</xsl:call-template>
						<a>
							<xsl:attribute name="href">
								<xsl:call-template name="get-filename-for-property">
									<xsl:with-param name="property" select="$declaring-class/ndoc:property[@id=$declaring-id]" />
								</xsl:call-template>
							</xsl:attribute>
							<xsl:value-of select="@name" />
						</a>
						<xsl:text> (inherited from </xsl:text>
						<b>
							<xsl:call-template name="get-datatype">
								<xsl:with-param name="datatype" select="@declaringType" />
							</xsl:call-template>
						</b>
						<xsl:text>)</xsl:text>
					</td>
					<td width="50%">
						<xsl:call-template name="obsolete-inline"/>
						<xsl:call-template name="summary-with-no-paragraph">
							<xsl:with-param name="member" select="$declaring-class/ndoc:property[@id=$declaring-id]" />
						</xsl:call-template>
					</td>
				</xsl:when>
				<xsl:otherwise>
					<td width="50%">
						<xsl:call-template name="images">
							<xsl:with-param name="access" select="@access" />
							<xsl:with-param name="contract" select="@contract" />
							<xsl:with-param name="local-name" select="local-name()" />
						</xsl:call-template>
						<xsl:value-of select="@name" />
						<xsl:text> (inherited from </xsl:text>
						<b>
							<xsl:value-of select="@declaringType" />
						</b>
						<xsl:text>)</xsl:text>
					</td>
					<td width="50%">
						<xsl:call-template name="obsolete-inline"/>
						<xsl:call-template name="summary-with-no-paragraph" />
					</td>
				</xsl:otherwise>
			</xsl:choose>
		</tr>
	</xsl:template>
	<!-- -->
	<xsl:template match="ndoc:property[@declaringType and starts-with(@declaringType, 'System.')]">
		<xsl:text>&#10;</xsl:text>
		<tr VALIGN="top">
			<td width="50%">
				<xsl:call-template name="images">
					<xsl:with-param name="access" select="@access" />
					<xsl:with-param name="contract" select="@contract" />
					<xsl:with-param name="local-name" select="local-name()" />
				</xsl:call-template>
				<a>
					<xsl:attribute name="href">
						<xsl:call-template name="get-filename-for-system-property" />
					</xsl:attribute>
					<xsl:value-of select="@name" />
				</a>
				<xsl:text> (inherited from </xsl:text>
				<b>
					<xsl:call-template name="strip-namespace">
						<xsl:with-param name="name" select="@declaringType" />
					</xsl:call-template>
				</b>
				<xsl:text>)</xsl:text>
			</td>
			<td width="50%">
				<xsl:call-template name="obsolete-inline"/>
				<xsl:call-template name="summary-with-no-paragraph" />
			</td>
		</tr>
	</xsl:template>
	<!-- -->
	<xsl:template match="ndoc:field[@declaringType]">
		<xsl:variable name="id" select="@id" />
		<xsl:variable name="name" select="@name" />
		<xsl:variable name="declaring-id" select="@declaringId" />
		<xsl:variable name="declaring-assembly" select="@declaringAssembly" />
		<xsl:variable name="declaring-type-id" select="concat('T:', @declaringType)" />
		<xsl:text>&#10;</xsl:text>
		<tr VALIGN="top">
			<xsl:variable name="declaring-class" select="ancestor::ndoc:ndoc/ndoc:assembly[@name=$declaring-assembly]//ndoc:class[@id=$declaring-type-id]" />
			<xsl:choose>
				<xsl:when test="$declaring-class">
					<td width="50%">
						<xsl:call-template name="images">
							<xsl:with-param name="access" select="@access" />
							<xsl:with-param name="contract" select="@contract" />
							<xsl:with-param name="local-name" select="local-name()" />
						</xsl:call-template>
						<a>
							<xsl:attribute name="href">
								<xsl:call-template name="get-filename-for-field">
									<xsl:with-param name="field" select="$declaring-class/ndoc:field[@id=$declaring-id]" />
								</xsl:call-template>
							</xsl:attribute>
							<xsl:value-of select="@name" />
						</a>
						<xsl:text> (inherited from </xsl:text>
						<b>
							<xsl:call-template name="get-datatype">
								<xsl:with-param name="datatype" select="@declaringType" />
							</xsl:call-template>
						</b>
						<xsl:text>)</xsl:text>
					</td>
					<td width="50%">
						<xsl:call-template name="obsolete-inline"/>
						<xsl:call-template name="summary-with-no-paragraph">
							<xsl:with-param name="member" select="$declaring-class/ndoc:field[@id=$declaring-id]" />
						</xsl:call-template>
					</td>
				</xsl:when>
				<xsl:otherwise>
					<td width="50%">
						<xsl:call-template name="images">
							<xsl:with-param name="access" select="@access" />
							<xsl:with-param name="contract" select="@contract" />
							<xsl:with-param name="local-name" select="local-name()" />
						</xsl:call-template>
						<xsl:value-of select="@name" />
						<xsl:text> (inherited from </xsl:text>
						<b>
							<xsl:value-of select="@declaringType" />
						</b>
						<xsl:text>)</xsl:text>
					</td>
					<td width="50%">
						<xsl:call-template name="obsolete-inline"/>
						<xsl:call-template name="summary-with-no-paragraph" />
					</td>
				</xsl:otherwise>
			</xsl:choose>
		</tr>
	</xsl:template>
	<!-- -->
	<xsl:template match="ndoc:field[@declaringType and starts-with(@declaringType, 'System.')]">
		<xsl:text>&#10;</xsl:text>
		<tr VALIGN="top">
			<td width="50%">
				<xsl:call-template name="images">
					<xsl:with-param name="access" select="@access" />
					<xsl:with-param name="contract" select="@contract" />
					<xsl:with-param name="local-name" select="local-name()" />
				</xsl:call-template>
				<a>
					<xsl:attribute name="href">
						<xsl:call-template name="get-filename-for-system-field" />
					</xsl:attribute>
					<xsl:value-of select="@name" />
				</a>
				<xsl:text> (inherited from </xsl:text>
				<b>
					<xsl:call-template name="strip-namespace">
						<xsl:with-param name="name" select="@declaringType" />
					</xsl:call-template>
				</b>
				<xsl:text>)</xsl:text>
			</td>
			<td width="50%">
				<xsl:call-template name="obsolete-inline"/>
				<xsl:call-template name="summary-with-no-paragraph" />
			</td>
		</tr>
	</xsl:template>
	
	<!-- -->
	<xsl:template match="ndoc:method[@declaringType]">
		<xsl:variable name="id" select="@id" />
		<xsl:variable name="name" select="@name" />
		<xsl:variable name="displayName" select="NUtil:ToGeneralGenericFormat(@displayName)" />
		<xsl:variable name="contract" select="@contract" />
		<xsl:variable name="access" select="@access" />
		<xsl:variable name="declaring-assembly" select="@declaringAssembly" />
		<xsl:variable name="declaring-type" select="@declaringType" />
		<xsl:variable name="declaring-id" select="@declaringId" />
		<xsl:variable name="declaring-type-id" select="concat('T:', @declaringType)" />
		<xsl:if test="not(NUtil:HasSimilarOverloads(concat($name,':',$declaring-type,':',$access,':',($contract='Static'))))">
			<xsl:text>&#10;</xsl:text>
			<tr VALIGN="top">
				<xsl:variable name="declaring-class" select="ancestor::ndoc:ndoc/ndoc:assembly[@name=$declaring-assembly]//ndoc:class[@id=$declaring-type-id]" />
				<xsl:choose>
					<xsl:when test="$declaring-class">
						<xsl:choose>
							<xsl:when test="following-sibling::method[(@name=$name) and (@declaringType=$declaring-type) and (@declaringAssembly=$declaring-assembly) and (@access=$access) and (($contract='Static' and @contract='Static') or ($contract!='Static' and @contract!='Static'))]">
								<td width="50%">
									<xsl:call-template name="images">
										<xsl:with-param name="access" select="$access" />
										<xsl:with-param name="contract" select="$contract" />
										<xsl:with-param name="local-name" select="local-name()" />
									</xsl:call-template>
									<a>
										<xsl:attribute name="href">
											<xsl:call-template name="get-filename-for-method">
												<xsl:with-param name="method" select="$declaring-class/ndoc:method[@id=$declaring-id]" />
											</xsl:call-template>
										</xsl:attribute>
										<xsl:value-of select="$displayName" />
									</a>
									<xsl:text> (inherited from </xsl:text>
									<b>
										<xsl:call-template name="get-datatype">
											<xsl:with-param name="datatype" select="@declaringType" />
										</xsl:call-template>
									</b>
									<xsl:text>)</xsl:text>
								</td>
								<td width="50%">
									<xsl:text>Overloaded. </xsl:text>
									<xsl:call-template name="overloads-summary-with-no-paragraph">
										<xsl:with-param name="overloads" select="$declaring-class/ndoc:method[@id=$declaring-id]" />
									</xsl:call-template>
								</td>
							</xsl:when>
							<xsl:otherwise>
								<td width="50%">
									<xsl:call-template name="images">
										<xsl:with-param name="access" select="$access" />
										<xsl:with-param name="contract" select="$contract" />
										<xsl:with-param name="local-name" select="local-name()" />
									</xsl:call-template>
									<a>
										<xsl:attribute name="href">
											<xsl:call-template name="get-filename-for-method">
												<xsl:with-param name="method" select="$declaring-class/ndoc:method[@id=$declaring-id]" />
											</xsl:call-template>
										</xsl:attribute>
										<xsl:value-of select="$displayName" />
									</a>
									<xsl:text> (inherited from </xsl:text>
									<b>
										<xsl:call-template name="get-datatype">
											<xsl:with-param name="datatype" select="@declaringType" />
										</xsl:call-template>
									</b>
									<xsl:text>)</xsl:text>
								</td>
								<td width="50%">
									<xsl:if test="@overload">
										<xsl:text>Overloaded. </xsl:text>
									</xsl:if>
									<xsl:if test="not(@overload)">
										<xsl:call-template name="obsolete-inline"/>
									</xsl:if>
									<xsl:call-template name="summary-with-no-paragraph">
										<xsl:with-param name="member" select="$declaring-class/ndoc:method[@id=$declaring-id]" />
									</xsl:call-template>
								</td>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:when>
					<xsl:otherwise>
						<td width="50%">
							<xsl:call-template name="images">
								<xsl:with-param name="access" select="$access" />
								<xsl:with-param name="contract" select="$contract" />
								<xsl:with-param name="local-name" select="local-name()" />
							</xsl:call-template>
							<xsl:value-of select="$displayName" />
							<xsl:text> (inherited from </xsl:text>
							<b>
								<xsl:value-of select="@declaringType" />
							</b>
							<xsl:text>)</xsl:text>
						</td>
						<td width="50%">
							<xsl:if test="@overload">
								<xsl:text>Overloaded. </xsl:text>
							</xsl:if>
							<xsl:if test="not(@overload)">
								<xsl:call-template name="obsolete-inline"/>
							</xsl:if>
							<xsl:call-template name="summary-with-no-paragraph" />
						</td>
					</xsl:otherwise>
				</xsl:choose>
			</tr>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template match="ndoc:method[@declaringType and starts-with(@declaringType, 'System.')]">
		<xsl:variable name="name" select="@name" />
		<xsl:variable name="contract" select="@contract" />
		<xsl:variable name="access" select="@access" />
		<xsl:variable name="declaringType" select="@declaringType" />
		<xsl:if test="not(NUtil:HasSimilarOverloads(concat($name,':',$declaringType,':',$access,':',($contract='Static'))))">
			<xsl:text>&#10;</xsl:text>
			<tr VALIGN="top">
				<td width="50%">
					<xsl:call-template name="images">
						<xsl:with-param name="access" select="$access" />
						<xsl:with-param name="contract" select="$contract" />
						<xsl:with-param name="local-name" select="local-name()" />
					</xsl:call-template>
					<a>
						<xsl:attribute name="href">
							<xsl:call-template name="get-filename-for-system-method" />
						</xsl:attribute>
						<xsl:value-of select="@name" />
					</a>
					<xsl:text> (inherited from </xsl:text>
					<b>
						<xsl:call-template name="strip-namespace">
							<xsl:with-param name="name" select="@declaringType" />
						</xsl:call-template>
					</b>
					<xsl:text>)</xsl:text>
				</td>
				<td width="50%">
					<xsl:if test="@overload">
						<xsl:text>Overloaded. </xsl:text>
					</xsl:if>
					<xsl:if test="not(@overload)">
						<xsl:call-template name="obsolete-inline"/>
					</xsl:if>
					<xsl:call-template name="summary-with-no-paragraph" />
				</td>
			</tr>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template match="ndoc:event[@declaringType]">
		<xsl:variable name="id" select="@id" />
		<xsl:variable name="name" select="@name" />
		<xsl:variable name="declaring-id" select="@declaringId" />
		<xsl:variable name="declaring-assembly" select="@declaringAssembly" />
		<xsl:variable name="declaring-type-id" select="concat('T:', @declaringType)" />
		<xsl:text>&#10;</xsl:text>
		<tr VALIGN="top">
			<xsl:variable name="declaring-class" select="ancestor::ndoc:ndoc/ndoc:assembly[@name=$declaring-assembly]//ndoc:class[@id=$declaring-type-id]" />
			<xsl:choose>
				<xsl:when test="$declaring-class">
					<td width="50%">
						<xsl:call-template name="images">
							<xsl:with-param name="access" select="@access" />
							<xsl:with-param name="contract" select="@contract" />
							<xsl:with-param name="local-name" select="local-name()" />
						</xsl:call-template>
						<a>
							<xsl:attribute name="href">
								<xsl:call-template name="get-filename-for-event">
									<xsl:with-param name="event" select="$declaring-class/ndoc:event[@id=$declaring-id]" />
								</xsl:call-template>
							</xsl:attribute>
							<xsl:value-of select="@name" />
						</a>
						<xsl:text> (inherited from </xsl:text>
						<b>
							<xsl:call-template name="get-datatype">
								<xsl:with-param name="datatype" select="@declaringType" />
							</xsl:call-template>
						</b>
						<xsl:text>)</xsl:text>
					</td>
					<td width="50%">
						<xsl:call-template name="obsolete-inline"/>
						<xsl:call-template name="summary-with-no-paragraph">
							<xsl:with-param name="member" select="$declaring-class/ndoc:event[@id=$declaring-id]" />
						</xsl:call-template>
					</td>
				</xsl:when>
				<xsl:otherwise>
					<td width="50%">
						<xsl:call-template name="images">
							<xsl:with-param name="access" select="@access" />
							<xsl:with-param name="contract" select="@contract" />
							<xsl:with-param name="local-name" select="local-name()" />
						</xsl:call-template>
						<xsl:value-of select="@name" />
						<xsl:text> (inherited from </xsl:text>
						<b>
							<xsl:value-of select="@declaringType" />
						</b>
						<xsl:text>)</xsl:text>
					</td>
					<td width="50%">
						<xsl:call-template name="obsolete-inline"/>
						<xsl:call-template name="summary-with-no-paragraph" />
					</td>
				</xsl:otherwise>
			</xsl:choose>
		</tr>
	</xsl:template>
	<!-- -->
	<xsl:template match="ndoc:event[@declaringType and starts-with(@declaringType, 'System.')]">
		<xsl:text>&#10;</xsl:text>
		<tr VALIGN="top">
			<td width="50%">
				<xsl:call-template name="images">
					<xsl:with-param name="access" select="@access" />
					<xsl:with-param name="contract" select="@contract" />
					<xsl:with-param name="local-name" select="local-name()" />
				</xsl:call-template>
				<a>
					<xsl:attribute name="href">
						<xsl:call-template name="get-filename-for-system-event" />
					</xsl:attribute>
					<xsl:value-of select="@name" />
				</a>
				<xsl:text> (inherited from </xsl:text>
				<b>
					<xsl:call-template name="strip-namespace">
						<xsl:with-param name="name" select="@declaringType" />
					</xsl:call-template>
				</b>
				<xsl:text>)</xsl:text>
			</td>
			<td width="50%">
				<xsl:call-template name="obsolete-inline"/>
				<xsl:call-template name="summary-with-no-paragraph" />
			</td>
		</tr>
	</xsl:template>
	<!-- -->
	<xsl:template match="ndoc:field[not(@declaringType)]|ndoc:property[not(@declaringType)]|ndoc:event[not(@declaringType)]|ndoc:method[not(@declaringType)]|ndoc:operator">
		<xsl:variable name="member" select="local-name()" />
		<xsl:variable name="name" select="@name" />
		<xsl:variable name="displayName">
			<xsl:choose>
				<xsl:when test="NUtil:ToGeneralGenericFormat(@displayName)">
					<xsl:value-of select="@name" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="@name" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="contract" select="@contract" />
		<xsl:variable name="access" select="@access" />
		<xsl:if test="@name='op_Implicit' or @name='op_Explicit' or not(NUtil:HasSimilarOverloads(concat($name,'::',$access,':',($contract='Static'))))">
			<xsl:text>&#10;</xsl:text>
			<tr VALIGN="top">
				<xsl:choose>
					<xsl:when test="@name!='op_Implicit' and @name!='op_Explicit' and following-sibling::*[(local-name()=$member) and (@name=$name) and (@access=$access) and (not(@declaringType)) and (($contract='Static' and @contract='Static') or ($contract!='Static' and @contract!='Static'))]">
						<td width="50%">
							<xsl:call-template name="images">
								<xsl:with-param name="access" select="@access" />
								<xsl:with-param name="contract" select="@contract" />
								<xsl:with-param name="local-name" select="local-name()" />
							</xsl:call-template>
							<a>
								<xsl:attribute name="href">
									<xsl:call-template name="get-filename-for-individual-member-overloads">
										<xsl:with-param name="member">
											<xsl:value-of select="$member" />
										</xsl:with-param>
									</xsl:call-template>
								</xsl:attribute>
								<xsl:choose>
									<xsl:when test="local-name()='operator'">
										<xsl:call-template name="operator-name">
											<xsl:with-param name="name" select="$name" />
											<xsl:with-param name="from" select="ndoc:parameter/@type"/>
											<xsl:with-param name="to" select="ndoc:returnType/@type" />
										</xsl:call-template>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="$displayName" />
									</xsl:otherwise>
								</xsl:choose>
							</a>
						</td>
						<td width="50%">
							<xsl:text>Overloaded. </xsl:text>
							<xsl:call-template name="overloads-summary-with-no-paragraph" />
						</td>
					</xsl:when>
					<xsl:otherwise>
						<td width="50%">
							<xsl:call-template name="images">
								<xsl:with-param name="access" select="@access" />
								<xsl:with-param name="contract" select="@contract" />
								<xsl:with-param name="local-name" select="local-name()" />
							</xsl:call-template>
							<a>
								<xsl:attribute name="href">
									<xsl:call-template name="get-filename-for-individual-member">
										<xsl:with-param name="member">
											<xsl:value-of select="$member" />
										</xsl:with-param>
									</xsl:call-template>
								</xsl:attribute>
								<xsl:choose>
									<xsl:when test="local-name()='operator'">
										<xsl:call-template name="operator-name">
											<xsl:with-param name="name" select="@name" />
											<xsl:with-param name="from" select="ndoc:parameter/@type"/>
											<xsl:with-param name="to" select="ndoc:returnType/@type" />
										</xsl:call-template>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="$displayName" />
									</xsl:otherwise>
								</xsl:choose>
							</a>
						</td>
						<td width="50%">
							<xsl:if test="@overload and @name!='op_Implicit' and @name!='op_Explicit'">
								<xsl:text>Overloaded. </xsl:text>
							</xsl:if>
							<xsl:if test="not(@overload)">
								<xsl:call-template name="obsolete-inline"/>
							</xsl:if>
							<xsl:call-template name="summary-with-no-paragraph" />
						</td>
					</xsl:otherwise>
				</xsl:choose>
			</tr>
		</xsl:if>
	</xsl:template>
	<!-- -->
</xsl:stylesheet>
