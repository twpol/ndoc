<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<!-- -->
	<xsl:template name="cs-type-syntax">
		<pre class="syntax">
			<span class="lang">[C#]</span>
			<br />
			<xsl:call-template name="type-access">
				<xsl:with-param name="access" select="@access" />
			</xsl:call-template>
			<xsl:text>&#160;</xsl:text>
			<xsl:if test="local-name() != 'interface' and @abstract = 'true'">abstract&#160;</xsl:if>
			<xsl:if test="@sealed = 'true'">
				<xsl:text>sealed&#160;</xsl:text>
			</xsl:if>
			<xsl:choose>
				<xsl:when test="local-name()='structure'">
					<xsl:text>struct</xsl:text>
				</xsl:when>
				<xsl:when test="local-name()='enumeration'">
					<xsl:text>enum</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="local-name()" />
				</xsl:otherwise>
			</xsl:choose>
			<xsl:text>&#160;</xsl:text>
			<xsl:if test="local-name()='delegate'">
				<xsl:call-template name="get-datatype">
					<xsl:with-param name="datatype" select="@returnType" />
				</xsl:call-template>
				<xsl:text>&#160;</xsl:text>
			</xsl:if>
			<xsl:value-of select="@name" />
			<xsl:if test="local-name() != 'enumeration' and local-name() != 'delegate'">
				<xsl:call-template name="derivation" />
			</xsl:if>
			<xsl:if test="local-name() = 'delegate'">
				<xsl:call-template name="parameters">
					<xsl:with-param name="version">long</xsl:with-param>
					<xsl:with-param name="namespace-name" select="../@name" />
				</xsl:call-template>
			</xsl:if>
		</pre>
	</xsl:template>
	<!-- -->
	<xsl:template name="derivation">
		<xsl:if test="@baseType!='' or implements">
			<xsl:text> : </xsl:text>
			<xsl:if test="@baseType!=''">
				<xsl:value-of select="@baseType" />
				<xsl:if test="implements">
					<xsl:text>, </xsl:text>
				</xsl:if>
			</xsl:if>
			<xsl:for-each select="implements">
				<xsl:value-of select="." />
				<xsl:if test="position()!=last()">
					<xsl:text>, </xsl:text>
				</xsl:if>
			</xsl:for-each>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="cs-member-syntax">
		<pre class="syntax">
			<span class="lang">[C#]</span>
			<br />
			<xsl:if test="not(parent::interface)">
				<xsl:call-template name="method-access">
					<xsl:with-param name="access" select="@access" />
				</xsl:call-template>
				<xsl:text>&#160;</xsl:text>
				<xsl:if test="@contract and @contract!='Normal'">
					<xsl:call-template name="contract">
						<xsl:with-param name="contract" select="@contract" />
					</xsl:call-template>
					<xsl:text>&#160;</xsl:text>
				</xsl:if>
			</xsl:if>
			<xsl:choose>
				<xsl:when test="local-name()='constructor'">
					<xsl:value-of select="../@name" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:if test="@name != 'op_Explicit' and @name != 'op_Implicit'">
						<!-- output the return type. this is duplicated code. -->
						<a>
							<xsl:attribute name="href">
								<xsl:call-template name="get-filename-for-type-name">
									<xsl:with-param name="type-name" select="@returnType" />
								</xsl:call-template>
							</xsl:attribute>
							<xsl:call-template name="get-datatype">
								<xsl:with-param name="datatype" select="@returnType" />
							</xsl:call-template>
						</a>
						<xsl:text>&#160;</xsl:text>
					</xsl:if>
					<xsl:choose>
						<xsl:when test="local-name()='operator'">
							<xsl:choose>
								<xsl:when test="@name='op_Explicit'">
									<xsl:text>explicit operator </xsl:text>
									<!-- output the return type. this is duplicated code. -->
									<a>
										<xsl:attribute name="href">
											<xsl:call-template name="get-filename-for-type-name">
												<xsl:with-param name="type-name" select="@returnType" />
											</xsl:call-template>
										</xsl:attribute>
										<xsl:call-template name="get-datatype">
											<xsl:with-param name="datatype" select="@returnType" />
										</xsl:call-template>
									</a>
								</xsl:when>
								<xsl:when test="@name='op_Implicit'">
									<xsl:text>implicit operator </xsl:text>
									<!-- output the return type. this is duplicated code. -->
									<a>
										<xsl:attribute name="href">
											<xsl:call-template name="get-filename-for-type-name">
												<xsl:with-param name="type-name" select="@returnType" />
											</xsl:call-template>
										</xsl:attribute>
										<xsl:call-template name="get-datatype">
											<xsl:with-param name="datatype" select="@returnType" />
										</xsl:call-template>
									</a>
								</xsl:when>
								<xsl:otherwise>
									<xsl:call-template name="csharp-operator-name">
										<xsl:with-param name="name" select="@name" />
									</xsl:call-template>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="@name" />
						</xsl:otherwise>
					</xsl:choose>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:call-template name="parameters">
				<xsl:with-param name="version">long</xsl:with-param>
				<xsl:with-param name="namespace-name" select="../../@name" />
			</xsl:call-template>
		</pre>
	</xsl:template>
	<!-- -->
	<xsl:template name="member-syntax2">
		<xsl:call-template name="method-access">
			<xsl:with-param name="access" select="@access" />
		</xsl:call-template>
		<xsl:text>&#160;</xsl:text>
		<xsl:choose>
			<xsl:when test="local-name()='constructor'">
				<xsl:value-of select="../@name" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="get-datatype">
					<xsl:with-param name="datatype" select="@returnType" />
				</xsl:call-template>
				<xsl:text>&#160;</xsl:text>
				<xsl:value-of select="@name" />
			</xsl:otherwise>
		</xsl:choose>
		<xsl:call-template name="parameters">
			<xsl:with-param name="version">short</xsl:with-param>
			<xsl:with-param name="namespace-name" select="../../@name" />
		</xsl:call-template>
	</xsl:template>
	<!-- -->
	<xsl:template name="field-or-event-syntax">
		<pre class="syntax">
			<xsl:call-template name="method-access">
				<xsl:with-param name="access" select="@access" />
			</xsl:call-template>
			<xsl:text>&#160;</xsl:text>
			<xsl:if test="@contract='Static'">
				<xsl:text>static&#160;</xsl:text>
			</xsl:if>
			<xsl:if test="@initOnly='true'">
				<xsl:text>readonly&#160;</xsl:text>
			</xsl:if>
			<a>
				<xsl:attribute name="href">
					<xsl:call-template name="get-filename-for-type-name">
						<xsl:with-param name="type-name" select="@type" />
					</xsl:call-template>
				</xsl:attribute>
				<xsl:call-template name="get-datatype">
					<xsl:with-param name="datatype" select="@type" />
				</xsl:call-template>
			</a>
			<xsl:text>&#160;</xsl:text>
			<xsl:value-of select="@name" />
			<xsl:text>;</xsl:text>
		</pre>
	</xsl:template>
	<!-- -->
	<xsl:template name="property-syntax">
		<xsl:param name="indent" select="true()" />
		<xsl:param name="display-names" select="true()" />
		<xsl:param name="link-types" select="true()" />
		<xsl:if test="not(parent::interface)">
			<xsl:call-template name="method-access">
				<xsl:with-param name="access" select="@access" />
			</xsl:call-template>
			<xsl:text>&#160;</xsl:text>
		</xsl:if>
		<xsl:if test="@contract='Static'">
			<xsl:text>static&#160;</xsl:text>
		</xsl:if>
		<xsl:choose>
			<xsl:when test="$link-types">
				<a>
					<xsl:attribute name="href">
						<xsl:call-template name="get-filename-for-type-name">
							<xsl:with-param name="type-name" select="@type" />
						</xsl:call-template>
					</xsl:attribute>
					<xsl:call-template name="value">
						<xsl:with-param name="type" select="@type" />
					</xsl:call-template>
				</a>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="value">
					<xsl:with-param name="type" select="@type" />
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:text>&#160;</xsl:text>
		<xsl:choose>
			<xsl:when test="@name='Item'">
				<xsl:text>this[</xsl:text>
				<xsl:if test="$indent">&#10;</xsl:if>
				<xsl:for-each select="parameter">
					<xsl:if test="$indent">
						<xsl:text>&#160;&#160;&#160;</xsl:text>
					</xsl:if>
					<xsl:choose>
						<xsl:when test="$link-types">
							<a>
								<xsl:attribute name="href">
									<xsl:call-template name="get-filename-for-type-name">
										<xsl:with-param name="type-name" select="@type" />
									</xsl:call-template>
								</xsl:attribute>
								<xsl:call-template name="csharp-type">
									<xsl:with-param name="runtime-type" select="@type" />
								</xsl:call-template>
							</a>
						</xsl:when>
						<xsl:otherwise>
							<xsl:call-template name="csharp-type">
								<xsl:with-param name="runtime-type" select="@type" />
							</xsl:call-template>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:if test="$display-names">
						<xsl:text>&#160;</xsl:text>
						<xsl:value-of select="@name" />
					</xsl:if>
					<xsl:if test="position() != last()">
						<xsl:text>,&#160;</xsl:text>
						<xsl:if test="$indent">&#10;</xsl:if>
					</xsl:if>
				</xsl:for-each>
				<xsl:if test="$indent">&#10;</xsl:if>
				<xsl:text>]</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="@name" />
			</xsl:otherwise>
		</xsl:choose>
		<xsl:text>&#160;{</xsl:text>
		<xsl:if test="@get='true'">
			<xsl:if test="not(parent::interface)">
				<xsl:if test="@contract!='Normal' and @contract!='Static'">
					<xsl:call-template name="contract">
						<xsl:with-param name="contract" select="@contract" />
					</xsl:call-template>
					<xsl:text>&#160;</xsl:text>
				</xsl:if>
			</xsl:if>
			<xsl:text>get;</xsl:text>
			<xsl:if test="@set='true'">
				<xsl:text>&#160;</xsl:text>
			</xsl:if>
		</xsl:if>
		<xsl:if test="@set='true'">
			<xsl:if test="not(parent::interface)">
				<xsl:if test="@contract!='Normal' and @contract!='Static'">
					<xsl:call-template name="contract">
						<xsl:with-param name="contract" select="@contract" />
					</xsl:call-template>
					<xsl:text>&#160;</xsl:text>
				</xsl:if>
			</xsl:if>
			<xsl:text>set;</xsl:text>
		</xsl:if>
		<xsl:text>}</xsl:text>
	</xsl:template>
	<!-- -->
	<xsl:template name="parameters">
		<xsl:param name="version" />
		<xsl:param name="namespace-name" />
		<xsl:text>(</xsl:text>
		<xsl:if test="parameter">
			<xsl:for-each select="parameter">
				<xsl:if test="$version='long'">
					<br />
					<xsl:text>&#160;&#160;&#160;</xsl:text>
				</xsl:if>
				<xsl:choose>
					<xsl:when test="@direction = 'ref'">ref&#160;</xsl:when>
					<xsl:when test="@direction = 'out'">out&#160;</xsl:when>
  			  <xsl:when test="@isParamArray = 'true'">params&#160;</xsl:when>
				</xsl:choose>
				<xsl:choose>
					<xsl:when test="$version='long'">
						<a>
							<xsl:attribute name="href">
								<xsl:call-template name="get-filename-for-type-name">
									<xsl:with-param name="type-name" select="@type" />
								</xsl:call-template>
							</xsl:attribute>
							<xsl:call-template name="get-datatype">
								<xsl:with-param name="datatype" select="@type" />
							</xsl:call-template>
						</a>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="get-datatype">
							<xsl:with-param name="datatype" select="@type" />
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:if test="$version='long'">
					<xsl:text>&#160;</xsl:text>
					<i>
						<xsl:value-of select="@name" />
					</i>
				</xsl:if>
				<xsl:if test="position()!= last()">
					<xsl:text>,</xsl:text>
				</xsl:if>
			</xsl:for-each>
			<xsl:if test="$version='long'">
				<br />
			</xsl:if>
		</xsl:if>
		<xsl:text>);</xsl:text>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-datatype">
		<xsl:param name="datatype" />
		<xsl:call-template name="strip-namespace">
			<xsl:with-param name="name">
				<xsl:call-template name="csharp-type">
					<xsl:with-param name="runtime-type" select="$datatype" />
				</xsl:call-template>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>
	<!-- -->
	<!-- member.xslt is using this for title and h1.  should try and use parameters template above. -->
	<xsl:template name="get-param-list">
		<xsl:text>(</xsl:text>
		<xsl:for-each select="parameter">
			<xsl:call-template name="strip-namespace">
				<xsl:with-param name="name" select="@type" />
			</xsl:call-template>
			<xsl:if test="position()!=last()">
				<xsl:text>, </xsl:text>
			</xsl:if>
		</xsl:for-each>
		<xsl:text>)</xsl:text>
	</xsl:template>
	<!-- -->
</xsl:stylesheet>
