<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:user="urn:my-scripts"
    xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
    exclude-result-prefixes="msxsl user"
>
<!--
	DO NOT pretty-indent the tags in this file
	Because most of these templates will be wrapped in html PRE tags
	the white space, as included, is necessary to get the correct
	formatting of the code syntax content
-->
	<xsl:include href="syntax-map.xslt" />
	<!-- -->
	<xsl:param name="ndoc-document-attributes" />
	<xsl:param name="ndoc-documented-attributes" />
	<!-- -->
	<xsl:template name="cs-syntax-header">	
		<xsl:param name="lang"/>
		
		<SPAN class="lang">[<xsl:value-of select="$lang"/>]<xsl:text>
</xsl:text></SPAN>
		<xsl:call-template name="attributes"/>	
	</xsl:template>	
	<!-- -->

	<xsl:template match="@* | node() | text()" mode="cs-syntax"/>
		
	<xsl:template match="enumeration" mode="cs-syntax">
		<xsl:param name="lang"/>

		<xsl:call-template name="cs-syntax-header">
			<xsl:with-param name="lang" select="$lang"/>		
		</xsl:call-template>
		<b>
			<xsl:apply-templates select="." mode="gc-type">
				<xsl:with-param name="lang" select="$lang"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="." mode="access">
				<xsl:with-param name="lang" select="$lang"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="." mode="keyword">
				<xsl:with-param name="lang" select="$lang"/>
			</xsl:apply-templates>			
			<xsl:value-of select="@name" /><xsl:text>
</xsl:text></b>	
	</xsl:template>

	<xsl:template match="structure | interface | class" mode="cs-syntax">
		<xsl:param name="lang"/>

		<xsl:call-template name="cs-syntax-header">
			<xsl:with-param name="lang" select="$lang"/>		
		</xsl:call-template>
		<b>
			<xsl:choose>
				<xsl:when test="$lang='Visual Basic'">
					<xsl:apply-templates select="." mode="abstract">
						<xsl:with-param name="lang" select="$lang"/>
					</xsl:apply-templates>
					<xsl:apply-templates select="." mode="sealed">
						<xsl:with-param name="lang" select="$lang"/>
					</xsl:apply-templates>				
					<xsl:apply-templates select="." mode="access">
						<xsl:with-param name="lang" select="$lang"/>
					</xsl:apply-templates>
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates select="." mode="access">
						<xsl:with-param name="lang" select="$lang"/>
					</xsl:apply-templates>
					<xsl:apply-templates select="." mode="gc-type">
						<xsl:with-param name="lang" select="$lang"/>
					</xsl:apply-templates>
					<xsl:apply-templates select="." mode="abstract">
						<xsl:with-param name="lang" select="$lang"/>
					</xsl:apply-templates>
					<xsl:apply-templates select="." mode="sealed">
						<xsl:with-param name="lang" select="$lang"/>
					</xsl:apply-templates>				
				</xsl:otherwise>
			</xsl:choose>
			<xsl:apply-templates select="." mode="keyword">
				<xsl:with-param name="lang" select="$lang"/>
			</xsl:apply-templates>
			<xsl:value-of select="@name" />
			<xsl:apply-templates select="." mode="derivation">
				<xsl:with-param name="lang" select="$lang"/>			
			</xsl:apply-templates>
<xsl:text>
</xsl:text></b>	
	</xsl:template>
	
	<xsl:template match="delegate" mode="cs-syntax">
		<xsl:param name="lang"/>

		<xsl:call-template name="cs-syntax-header">
			<xsl:with-param name="lang" select="$lang"/>		
		</xsl:call-template>		
		<b>
			<xsl:apply-templates select="." mode="access">
				<xsl:with-param name="lang" select="$lang"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="." mode="gc-type">
				<xsl:with-param name="lang" select="$lang"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="." mode="keyword">
				<xsl:with-param name="lang" select="$lang"/>
			</xsl:apply-templates>
			<!-- delegates can't be defined in JScript -->
			<xsl:if test="$lang != 'JScript'">
				<xsl:variable name="cs-type">
					<xsl:call-template name="get-datatype">
						<xsl:with-param name="datatype" select="@returnType" />
						<xsl:with-param name="lang" select="$lang" />
					</xsl:call-template>
				</xsl:variable>
				<xsl:call-template name="get-link-for-type-name">
					<xsl:with-param name="type-name" select="@returnType" />
					<xsl:with-param name="link-text" select="$cs-type" />
				</xsl:call-template>								
				<xsl:text>&#160;</xsl:text>
				<xsl:value-of select="@name" />
				<xsl:call-template name="parameters">
					<xsl:with-param name="include-type-links" select="true()"/>		
					<xsl:with-param name="lang" select="$lang"/>
					<xsl:with-param name="namespace-name" select="../@name" />
				</xsl:call-template>
			</xsl:if>
<xsl:text>
</xsl:text></b>	
	</xsl:template>

	
	<xsl:template match="constructor" mode="cs-inline-syntax">
		<xsl:param name="lang"/>
		<xsl:param name="href"/>

		<xsl:call-template name="cs-syntax-header">
			<xsl:with-param name="lang" select="$lang"/>		
		</xsl:call-template>
		<a href="{$href}">		
			<xsl:call-template name="cs-constructor-syntax">
				<xsl:with-param name="lang" select="$lang"/>		
				<xsl:with-param name="include-type-links" select="false()"/>		
			</xsl:call-template>
		</a>
	</xsl:template>
	
	<xsl:template match="constructor" mode="cs-syntax">		
		<xsl:param name="lang"/>

		<xsl:call-template name="cs-syntax-header">
			<xsl:with-param name="lang" select="$lang"/>		
		</xsl:call-template>
		<b>
			<xsl:call-template name="cs-constructor-syntax">
				<xsl:with-param name="lang" select="$lang"/>		
				<xsl:with-param name="include-type-links" select="true()"/>		
			</xsl:call-template><xsl:text>
</xsl:text></b>	
	</xsl:template>
	
	<xsl:template name="cs-constructor-syntax">
		<xsl:param name="lang"/>
		<xsl:param name="include-type-links"/>
	
		<xsl:call-template name="cs-member-syntax-prolog">
			<xsl:with-param name="lang" select="$lang"/>		
		</xsl:call-template>
		<xsl:call-template name="constructor-keyword">
			<xsl:with-param name="lang" select="$lang"/>		
		</xsl:call-template>
		<xsl:value-of select="../@name" />
		<xsl:call-template name="parameters">
			<xsl:with-param name="include-type-links" select="$include-type-links"/>		
			<xsl:with-param name="lang" select="$lang"/>
			<xsl:with-param name="namespace-name" select="../../@name" />
		</xsl:call-template>		
	</xsl:template>


	<xsl:template match="method" mode="cs-inline-syntax">
		<xsl:param name="lang"/>
		<xsl:param name="href"/>

		<xsl:call-template name="cs-syntax-header">
			<xsl:with-param name="lang" select="$lang"/>		
		</xsl:call-template>
		<a href="{$href}">
			<xsl:call-template name="cs-method-syntax">
				<xsl:with-param name="lang" select="$lang"/>
				<xsl:with-param name="include-type-links" select="false()"/>
			</xsl:call-template>
		</a>
	</xsl:template>
	
	<xsl:template match="method" mode="cs-syntax">
		<xsl:param name="lang"/>

		<xsl:call-template name="cs-syntax-header">
			<xsl:with-param name="lang" select="$lang"/>		
		</xsl:call-template>
		<b>
			<xsl:call-template name="cs-method-syntax">
				<xsl:with-param name="lang" select="$lang"/>
				<xsl:with-param name="include-type-links" select="true()"/>
			</xsl:call-template>
<xsl:text>
</xsl:text></b>	
	</xsl:template>

	<xsl:template name="cs-method-syntax">
		<xsl:param name="include-type-links"/>
		<xsl:param name="lang"/>

		<xsl:call-template name="cs-member-syntax-prolog">
			<xsl:with-param name="lang" select="$lang"/>							
		</xsl:call-template>
		<xsl:call-template name="method-start">
			<xsl:with-param name="include-type-links" select="$include-type-links"/>
			<xsl:with-param name="lang" select="$lang"/>
		</xsl:call-template>
		<xsl:value-of select="@name" />
		<xsl:call-template name="parameters">
			<xsl:with-param name="include-type-links" select="$include-type-links"/>		
			<xsl:with-param name="lang" select="$lang"/>
			<xsl:with-param name="namespace-name" select="../../@name" />
		</xsl:call-template>		
		<xsl:call-template name="method-end">
			<xsl:with-param name="include-type-links" select="$include-type-links"/>
			<xsl:with-param name="lang" select="$lang"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="cs-member-syntax-prolog">
		<xsl:param name="lang"/>
	
		<xsl:if test="not(parent::interface or @interface)">
			<xsl:choose>
				<xsl:when test="$lang='Visual Basic'">
					<xsl:if test="@contract and @contract!='Normal' and @contract!='Final'">
						<xsl:apply-templates select="." mode="contract">
							<xsl:with-param name="lang" select="$lang"/>
						</xsl:apply-templates>
					</xsl:if>				
					<xsl:if test="(local-name()!='constructor') or (@contract!='Static')">
						<xsl:apply-templates select="." mode="access">
							<xsl:with-param name="lang" select="$lang"/>
						</xsl:apply-templates>
					</xsl:if>
				</xsl:when>
				<xsl:otherwise>
					<xsl:if test="(local-name()!='constructor') or (@contract!='Static')">
						<xsl:apply-templates select="." mode="access">
							<xsl:with-param name="lang" select="$lang"/>
						</xsl:apply-templates>
					</xsl:if>
					<xsl:if test="@contract and @contract!='Normal' and @contract!='Final'">
						<xsl:apply-templates select="." mode="contract">
							<xsl:with-param name="lang" select="$lang"/>
						</xsl:apply-templates>
					</xsl:if>				
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>	
	</xsl:template>
	
	<xsl:template name="method-start">
		<xsl:param name="include-type-links"/>
		<xsl:param name="lang"/>
	
		<xsl:apply-templates select="." mode="method-open">
			<xsl:with-param name="lang" select="$lang"/>
		</xsl:apply-templates>
		<!-- VB and JScript declare the return type at the end of the declaration -->
		<xsl:if test="$lang != 'Visual Basic' and $lang != 'JScript'">
			<xsl:call-template name="return-type">
				<xsl:with-param name="include-type-links" select="$include-type-links"/>
				<xsl:with-param name="lang" select="$lang"/>
				<xsl:with-param name="type" select="@returnType"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>
	
	<xsl:template name="method-end">
		<xsl:param name="include-type-links"/>
		<xsl:param name="lang"/>
		
		<xsl:if test="$lang = 'Visual Basic' or $lang = 'JScript'">
			<xsl:call-template name="return-type">
				<xsl:with-param name="include-type-links" select="$include-type-links"/>
				<xsl:with-param name="lang" select="$lang"/>
				<xsl:with-param name="type" select="@returnType"/>
			</xsl:call-template>
		</xsl:if>
		<xsl:call-template name="statement-end">
			<xsl:with-param name="lang" select="$lang"/>			
		</xsl:call-template>			
	</xsl:template>

	<xsl:template name="return-type">
		<xsl:param name="lang"/>
		<xsl:param name="include-type-links"/>
		<xsl:param name="type"/>
		
		<xsl:variable name="cs-type">
			<xsl:call-template name="get-datatype">
				<xsl:with-param name="datatype" select="$type" />
				<xsl:with-param name="lang" select="$lang" />				
			</xsl:call-template>
		</xsl:variable>		
		
		<xsl:if test="$lang = 'Visual Basic' or $lang = 'JScript'">
			<xsl:if test="$type != 'System.Void'">
				<xsl:call-template name="param-seperator">
					<xsl:with-param name="lang" select="$lang" />
				</xsl:call-template>
			</xsl:if>
		</xsl:if>	

		<xsl:choose>
			<xsl:when test="$include-type-links = true()">
				<xsl:call-template name="get-link-for-type-name">
					<xsl:with-param name="type-name" select="$type" />
					<xsl:with-param name="link-text" select="$cs-type" />
				</xsl:call-template>					
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$cs-type"/>
			</xsl:otherwise>
		</xsl:choose>				
		<xsl:text>&#160;</xsl:text>	
	</xsl:template>

	<xsl:template match="property" mode="cs-syntax">
		<xsl:param name="lang"/>

		<xsl:call-template name="cs-syntax-header">
			<xsl:with-param name="lang" select="$lang"/>		
		</xsl:call-template>
		<b>
		<xsl:choose>
			<xsl:when test="$lang='C++' or $lang='JScript'">
				<xsl:call-template name="cs-property-syntax">
					<xsl:with-param name="lang" select="$lang"/>			
					<xsl:with-param name="include-type-links" select="true()"/>
					<xsl:with-param name="dir" select="'get'"/>
				</xsl:call-template>
<xsl:text>
</xsl:text>				
				<xsl:call-template name="cs-property-syntax">
					<xsl:with-param name="lang" select="$lang"/>			
					<xsl:with-param name="include-type-links" select="true()"/>
					<xsl:with-param name="dir" select="'set'"/>
				</xsl:call-template>			
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="cs-property-syntax">
					<xsl:with-param name="lang" select="$lang"/>			
					<xsl:with-param name="include-type-links" select="true()"/>
					<xsl:with-param name="dir" select="'both'"/>
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>		
<xsl:text>
</xsl:text></b>	
	</xsl:template>
	
	<xsl:template match="property" mode="cs-inline-syntax">
		<xsl:param name="href"/>
		<xsl:param name="lang"/>

		<xsl:call-template name="cs-syntax-header">
			<xsl:with-param name="lang" select="$lang"/>		
		</xsl:call-template>
		<a href="{$href}">
			<xsl:call-template name="cs-property-syntax">
				<xsl:with-param name="lang" select="$lang"/>			
				<xsl:with-param name="include-type-links" select="false()"/>
			</xsl:call-template>
		</a>
	</xsl:template>	
		
	<xsl:template match="field | event" mode="cs-syntax">
		<xsl:param name="lang"/>

		<xsl:call-template name="cs-syntax-header">
			<xsl:with-param name="lang" select="$lang"/>		
		</xsl:call-template>
		<b>
			<xsl:call-template name="cs-field-or-event-syntax">
				<xsl:with-param name="lang" select="$lang"/>
			</xsl:call-template>
<xsl:text>
</xsl:text></b>	
	</xsl:template>

	<xsl:template match="operator" mode="cs-inline-syntax">
		<xsl:param name="lang"/>
		<xsl:param name="href"/>

		<xsl:call-template name="cs-syntax-header">
			<xsl:with-param name="lang" select="$lang"/>		
		</xsl:call-template>
		<a href="{$href}">
			<xsl:call-template name="cs-operator-syntax">
				<xsl:with-param name="lang" select="$lang"/>
				<xsl:with-param name="include-type-links" select="false()"/>
			</xsl:call-template>
		</a>
	</xsl:template>
			
	<xsl:template match="operator" mode="cs-syntax">
		<xsl:param name="lang"/>
		<xsl:if test="@name != 'op_Explicit' and @name != 'op_Implicit'">

		</xsl:if>	
		<xsl:call-template name="cs-syntax-header">
			<xsl:with-param name="lang" select="$lang"/>		
		</xsl:call-template>
		<b>
			<xsl:call-template name="cs-operator-syntax">
				<xsl:with-param name="lang" select="$lang"/>
				<xsl:with-param name="include-type-links" select="true()"/>
			</xsl:call-template>
<xsl:text>
</xsl:text></b>	
	</xsl:template>
	
	<xsl:template name="cs-operator-syntax">
		<xsl:param name="include-type-links"/>
		<xsl:param name="lang"/>

		<xsl:call-template name="cs-member-syntax-prolog">
			<xsl:with-param name="lang" select="$lang"/>			
		</xsl:call-template>
		<xsl:choose>
			<xsl:when test="@name='op_Explicit'">
				<xsl:text>explicit operator </xsl:text>
				<!-- output the return type. this is duplicated code. -->
				<xsl:variable name="cs-type">
					<xsl:call-template name="get-datatype">
						<xsl:with-param name="datatype" select="@returnType" />
						<xsl:with-param name="lang" select="$lang" />						
					</xsl:call-template>
				</xsl:variable>
				<xsl:choose>
					<xsl:when test="$include-type-links = true()">
						<xsl:call-template name="get-link-for-type-name">
							<xsl:with-param name="type-name" select="@returnType" />
							<xsl:with-param name="link-text" select="$cs-type" />
						</xsl:call-template>					
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="cs-type"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:when test="@name='op_Implicit'">
				<xsl:text>implicit operator </xsl:text>
				<!-- output the return type. this is duplicated code. -->
				<xsl:variable name="cs-type">
					<xsl:call-template name="get-datatype">
						<xsl:with-param name="datatype" select="@returnType" />
						<xsl:with-param name="lang" select="$lang" />						
					</xsl:call-template>
				</xsl:variable>
				<xsl:choose>
					<xsl:when test="$include-type-links = true()">
						<xsl:call-template name="get-link-for-type-name">
							<xsl:with-param name="type-name" select="@returnType" />
							<xsl:with-param name="link-text" select="$cs-type" />
						</xsl:call-template>					
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="cs-type"/>
					</xsl:otherwise>
				</xsl:choose>									
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="csharp-operator-name">
					<xsl:with-param name="name" select="@name" />
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:call-template name="parameters">
			<xsl:with-param name="include-type-links" select="$include-type-links"/>
			<xsl:with-param name="lang" select="$lang"/>
			<xsl:with-param name="namespace-name" select="../../@name" />
		</xsl:call-template>		
	</xsl:template>
	<!-- -->
	<xsl:template match="structure | interface | class" mode="derivation">
		<xsl:param name="lang"/>
	
		<xsl:if test="@baseType!='' or implements[not(@inherited)]">
			<xsl:apply-templates select="." mode="inherits">
				<xsl:with-param name="lang" select="$lang"/>
			</xsl:apply-templates>
			<xsl:if test="@baseType!=''">
				<xsl:value-of select="@baseType" />
				<xsl:if test="implements[not(@inherited)]">
					<xsl:text>, </xsl:text>
				</xsl:if>
			</xsl:if>
			<xsl:for-each select="implements[not(@inherited)]">
				<xsl:value-of select="." />
				<xsl:if test="position()!=last()">
					<xsl:text>, </xsl:text>
				</xsl:if>
			</xsl:for-each>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="cs-field-or-event-syntax">
		<xsl:param name="include-type-links"/>
		<xsl:param name="lang"/>
	
		<xsl:if test="not(parent::interface)">
			<xsl:apply-templates select="." mode="access"/>
		</xsl:if>
		<xsl:if test="@contract='Static'">
			<xsl:choose>
				<xsl:when test="@literal='true'">
					<xsl:text>const&#160;</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>static&#160;</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
		<xsl:if test="@initOnly='true'">
			<xsl:text>readonly&#160;</xsl:text>
		</xsl:if>
		<xsl:if test="local-name() = 'event'">
			<xsl:text>event&#160;</xsl:text>
		</xsl:if>
		<xsl:variable name="cs-type">
			<xsl:call-template name="get-datatype">
				<xsl:with-param name="datatype" select="@type" />
				<xsl:with-param name="lang" select="$lang" />				
			</xsl:call-template>
		</xsl:variable>
		<xsl:call-template name="get-link-for-type-name">
			<xsl:with-param name="type-name" select="@type" />
			<xsl:with-param name="link-text" select="$cs-type" />
		</xsl:call-template>	
		<xsl:text>&#160;</xsl:text>
		<xsl:value-of select="@name" />
		<xsl:call-template name="statement-end">
			<xsl:with-param name="lang" select="$lang"/>
		</xsl:call-template>
	</xsl:template>
	
	<!-- -->
	<xsl:template name="cs-property-syntax">
		<xsl:param name="include-type-links"/>
		<xsl:param name="lang"/>
		<xsl:param name="dir"/>
		
		<xsl:call-template name="cs-member-syntax-prolog">
			<xsl:with-param name="lang" select="$lang"/>							
		</xsl:call-template>
		<xsl:apply-templates select="." mode="keyword">
			<xsl:with-param name="lang" select="$lang"/>
		</xsl:apply-templates>
		<xsl:if test="$lang != 'Visual Basic' and $lang != 'JScript' and $dir='both'">
			<xsl:call-template name="return-type">
				<xsl:with-param name="include-type-links" select="$include-type-links"/>
				<xsl:with-param name="lang" select="$lang"/>
				<xsl:with-param name="type" select="@type"/>
			</xsl:call-template>
		</xsl:if>
		<xsl:if test="$lang='C++'">
			<xsl:choose>
				<xsl:when test="$dir='set'">
					<xsl:call-template name="return-type">
						<xsl:with-param name="include-type-links" select="$include-type-links"/>
						<xsl:with-param name="lang" select="$lang"/>
						<xsl:with-param name="type" select="'System.Void'"/>
					</xsl:call-template>				
				</xsl:when>
				<xsl:otherwise>
					<xsl:call-template name="return-type">
						<xsl:with-param name="include-type-links" select="$include-type-links"/>
						<xsl:with-param name="lang" select="$lang"/>
						<xsl:with-param name="type" select="@type"/>
					</xsl:call-template>					
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
		<xsl:choose>
			<xsl:when test="parameter">
			
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="." mode="property-name">
					<xsl:with-param name="lang" select="$lang"/>
					<xsl:with-param name="dir" select="$dir"/>
				</xsl:apply-templates>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:if test="$lang='C++'">
			<xsl:choose>
				<xsl:when test="$dir='get'">();</xsl:when>
				<xsl:otherwise>
					<xsl:text>(</xsl:text>
					<xsl:call-template name="return-type">
						<xsl:with-param name="include-type-links" select="$include-type-links"/>
						<xsl:with-param name="lang" select="$lang"/>
						<xsl:with-param name="type" select="@type"/>
					</xsl:call-template>					
					<xsl:text>);</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
		<xsl:if test="$lang='C#'">
			<xsl:text>&#160;{</xsl:text>
			<xsl:if test="@get='true'">
				<xsl:text>get;</xsl:text>
				<xsl:if test="@set='true'">
					<xsl:text>&#160;</xsl:text>
				</xsl:if>
			</xsl:if>
			<xsl:if test="@set='true'">
				<xsl:text>set;</xsl:text>
			</xsl:if>
			<xsl:text>}</xsl:text>
		</xsl:if>
		<xsl:if test="$lang = 'Visual Basic'">
			<xsl:call-template name="return-type">
				<xsl:with-param name="include-type-links" select="$include-type-links"/>
				<xsl:with-param name="lang" select="$lang"/>
				<xsl:with-param name="type" select="@type"/>
			</xsl:call-template>
		</xsl:if>		
		<xsl:if test="$lang = 'JScript'">
			<xsl:choose>
				<xsl:when test="$dir='get'">
					<xsl:text>()</xsl:text>
					<xsl:call-template name="return-type">
						<xsl:with-param name="include-type-links" select="$include-type-links"/>
						<xsl:with-param name="lang" select="$lang"/>
						<xsl:with-param name="type" select="@type"/>
					</xsl:call-template>				
					<xsl:text>;</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>(</xsl:text>
					<xsl:variable name="cs-type">
						<xsl:call-template name="get-datatype">
							<xsl:with-param name="datatype" select="@type" />
							<xsl:with-param name="lang" select="$lang" />				
						</xsl:call-template>
					</xsl:variable>						
					<xsl:choose>	
						<xsl:when test="$include-type-links = true()">
							<xsl:call-template name="get-link-for-type-name">
								<xsl:with-param name="type-name" select="@type" />
								<xsl:with-param name="link-text" select="$cs-type" />
							</xsl:call-template>					
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="$cs-type"/>
						</xsl:otherwise>
					</xsl:choose>									
					<xsl:text>);</xsl:text>				
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
	</xsl:template>
	
	<xsl:template match="property[parameter]" mode="params">
		<xsl:param name="include-type-links"/>
		<xsl:param name="lang"/>

		<xsl:text>this[</xsl:text>
		<xsl:if test="$include-type-links = true()">
<xsl:text>
</xsl:text>
		</xsl:if>
		<xsl:for-each select="parameter">
			<xsl:if test="$include-type-links = true()">
				<xsl:text>&#160;&#160;&#160;</xsl:text>
			</xsl:if>
			<xsl:choose>
				<xsl:when test="$include-type-links = true()">
					<xsl:variable name="cs-type">
						<xsl:call-template name="get-datatype">
							<xsl:with-param name="datatype" select="@type" />
							<xsl:with-param name="lang" select="$lang" />									
						</xsl:call-template>
					</xsl:variable>
					<xsl:call-template name="get-link-for-type-name">
						<xsl:with-param name="type-name" select="@type" />
						<xsl:with-param name="link-text" select="$cs-type" />
					</xsl:call-template>	
				</xsl:when>
				<xsl:otherwise>
					<xsl:call-template name="get-datatype">
						<xsl:with-param name="datatype" select="@type" />
						<xsl:with-param name="lang" select="$lang" />
					</xsl:call-template>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:if test="$include-type-links = true()">
				<xsl:text>&#160;</xsl:text>
				<i>
					<xsl:value-of select="@name" />
				</i>
			</xsl:if>
			<xsl:if test="position() != last()">
				<xsl:text>,&#160;</xsl:text>
				<xsl:if test="$include-type-links = true()">						
<xsl:text>
</xsl:text>
				</xsl:if>
			</xsl:if>
		</xsl:for-each>
		<xsl:if test="$include-type-links = true()">
<xsl:text>
</xsl:text>				
		</xsl:if>
		<xsl:text>]</xsl:text>
	
	</xsl:template>
	<!-- -->
	<xsl:template name="value">
		<xsl:param name="type" />
		<xsl:variable name="namespace">
			<xsl:value-of select="concat(../../@name, '.')" />
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="contains($type, $namespace)">
				<xsl:value-of select="substring-after($type, $namespace)" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="lang-type">
					<xsl:with-param name="runtime-type" select="$type" />
					<xsl:with-param name="lang" select="'C#'"/>
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
		
	<!-- -->
	<xsl:template name="parameters">
		<xsl:param name="lang"/>
		<xsl:param name="namespace-name" />
		<xsl:param name="include-type-links"/>
		
		<xsl:text>(</xsl:text>
		<xsl:if test="parameter">
			<xsl:for-each select="parameter">
				<xsl:if test="$include-type-links=true()">
					<xsl:call-template name="statement-continue">
						<xsl:with-param name="lang" select="$lang"/>
					</xsl:call-template>
<xsl:text>
</xsl:text>
					<xsl:text>&#160;&#160;&#160;</xsl:text>
				</xsl:if>
				<xsl:apply-templates select="." mode="dir">
					<xsl:with-param name="lang" select="$lang"/>
				</xsl:apply-templates>
				<xsl:apply-templates select="." mode="param-array">
					<xsl:with-param name="lang" select="$lang"/>
				</xsl:apply-templates>
				<xsl:choose>
					<xsl:when test="$include-type-links=true()">
						<xsl:variable name="cs-type">
							<xsl:call-template name="get-datatype">
								<xsl:with-param name="datatype" select="@type" />
								<xsl:with-param name="lang" select="$lang" />
							</xsl:call-template>
						</xsl:variable>
						<xsl:if test="$include-type-links = true()">
							<xsl:call-template name="get-link-for-type-name">
								<xsl:with-param name="type-name" select="@type" />
								<xsl:with-param name="link-text" select="$cs-type" />
							</xsl:call-template>				
						</xsl:if>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="get-datatype">
							<xsl:with-param name="datatype" select="@type" />
							<xsl:with-param name="lang" select="$lang" />
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:if test="$include-type-links=true()">
					<xsl:text>&#160;</xsl:text>
					<i>
						<xsl:value-of select="@name" />
					</i>
				</xsl:if>
				<xsl:if test="position()!= last()">
					<xsl:text>,</xsl:text>
				</xsl:if>
			</xsl:for-each>
			<xsl:if test="$include-type-links=true()">
				<xsl:call-template name="statement-continue">
					<xsl:with-param name="lang" select="$lang"/>
				</xsl:call-template>
<xsl:text>
</xsl:text>
			</xsl:if>
		</xsl:if>
		<xsl:text>)</xsl:text>	
	</xsl:template>
	<!-- -->
	<xsl:template name="get-datatype">
		<xsl:param name="datatype" />
		<xsl:param name="lang"/>
		
		<xsl:variable name="type-temp">
			<xsl:call-template name="lang-type">
				<xsl:with-param name="runtime-type" select="$datatype" />
				<xsl:with-param name="lang" select="$lang"/>
			</xsl:call-template>				
		</xsl:variable>
		<xsl:call-template name="strip-namespace">
			<xsl:with-param name="name" select="$type-temp"/>
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
	<!-- ATTRIBUTES -->
	<xsl:template name="attributes">
		<xsl:if test="$ndoc-document-attributes">
			<xsl:if test="attribute">
				<xsl:for-each select="attribute">
					<div class="attribute"><xsl:call-template name="attribute">
						<xsl:with-param name="attname" select="@name" />
					</xsl:call-template></div>
				</xsl:for-each>
			</xsl:if>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="attribute">
		<xsl:param name="attname" />
		<xsl:if test="user:isAttributeWanted($ndoc-documented-attributes, @name)">			
			<xsl:text>[</xsl:text>
			<xsl:call-template name="strip-namespace-and-attribute">
				<xsl:with-param name="name" select="@name" />
			</xsl:call-template>
			<xsl:if test="count(property) > 0">
				<xsl:text>(</xsl:text>
				<xsl:for-each select="property">
					<xsl:if test="user:isPropertyWanted($ndoc-documented-attributes, @name) and @value!=''">
						<xsl:value-of select="@name" />
						<xsl:text>="</xsl:text>
						<xsl:value-of select="@value" />
						<xsl:text>"</xsl:text>
						<xsl:if test="position()!=last()"><xsl:text>, </xsl:text></xsl:if>
					</xsl:if>
				</xsl:for-each>
				<xsl:text>)</xsl:text>
			</xsl:if>
			<xsl:text>]</xsl:text>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="strip-namespace-and-attribute">
		<xsl:param name="name" />
		<xsl:choose>
			<xsl:when test="contains($name, '.')">
				<xsl:call-template name="strip-namespace-and-attribute">
					<xsl:with-param name="name" select="substring-after($name, '.')" />
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="substring-before(concat($name, '_____'), 'Attribute_____')" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<msxsl:script implements-prefix="user">
	<![CDATA[
		function isAttributeWanted(sParamWantedList, oElement)
		{
			var aWanted = (''+sParamWantedList).split('|');
			oElement.Current.MoveToFirstAttribute();
			var sAttributeType = ''+oElement.Current.Value;
			for(var i = 0; i != aWanted.length; i++)
			{
				var oAttribute = (''+aWanted[i]).split(',');
				if(sAttributeType.indexOf(""+oAttribute[0]) != -1)
				{
					return 'true';
				}
			}
			return '';
		}
		
		function isPropertyWanted(sParamWantedList, oElement)
		{
			var aWanted = (''+sParamWantedList).split('|');
			
			oElement.Current.MoveToFirstAttribute();
			var sPropertyType = ''+oElement.Current.Value;
			oElement.Current.MoveToParent();
			oElement.Current.MoveToParent();
			oElement.Current.MoveToFirstAttribute();
			var sAttributeType = ''+oElement.Current.Value;
			
			for(var i = 0; i != aWanted.length; i++)
			{
				var oAttribute = (''+aWanted[i]).split(',');
				if(sAttributeType.indexOf(""+oAttribute[0]) != -1)
				{
					if (oAttribute.length == 1)
					{
						return 'true';
					}
					else if (oAttribute.length != 0)
					{
						for(var j = 1; j != oAttribute.length; j++)
						{
							if(sPropertyType.indexOf(""+oAttribute[j]) != -1)
							{
								if (sPropertyType.length == oAttribute[j].length)
								{
									return 'true';
								}
							}
						}
					}
				}
			}
			return '';
		}
		]]>
    </msxsl:script>
	<!-- -->
</xsl:stylesheet>