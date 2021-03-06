<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ndoc="urn:ndoc-schema"
                xmlns:NUtil="urn:NDocUtil"
	              exclude-result-prefixes="NUtil">
	<!-- Document attributes? -->
	<xsl:param name="ndoc-document-attributes" />
	<!-- Which attributes should be documented -->
	<xsl:param name="ndoc-documented-attributes" />
	<!-- C# Type syntax -->
	<xsl:template name="cs-type-syntax">
		<div class="syntax">
			<!-- VB syntax? -->
			<xsl:if test="$ndoc-vb-syntax">
				<span class="lang">[C#]</span>
			</xsl:if>
			<!-- Write attributes -->
			<xsl:call-template name="attributes" />
			<div>
				<!-- Does this type hide another type -->
				<xsl:if test="@hiding">
					<xsl:text>new&#160;</xsl:text>
				</xsl:if>
				<!-- Write type access modifier -->
				<xsl:call-template name="type-access">
					<xsl:with-param name="access" select="@access" />
					<xsl:with-param name="type" select="local-name()" />
				</xsl:call-template>
				<xsl:text>&#160;</xsl:text>
				<!-- Is this static? -->
				<xsl:if test="local-name() != 'interface' and @static = 'true'">
					<xsl:text>static&#160;</xsl:text>
				</xsl:if>
				<!-- Is this abstract? -->
				<xsl:if test="local-name() != 'interface' and @abstract = 'true'">
					<xsl:text>abstract&#160;</xsl:text>
				</xsl:if>
				<!-- Is this sealed? -->
				<xsl:if test="@sealed = 'true'">
					<xsl:text>sealed&#160;</xsl:text>
				</xsl:if>
				<xsl:choose>
					<!-- Is this a structure? -->
					<xsl:when test="local-name()='structure'">
						<xsl:text>struct</xsl:text>
					</xsl:when>
					<!-- Is this a enumeration? -->
					<xsl:when test="local-name()='enumeration'">
						<xsl:text>enum</xsl:text>
					</xsl:when>
					<!-- Otherwise just write the localname -->
					<xsl:otherwise>
						<xsl:value-of select="local-name()" />
					</xsl:otherwise>
				</xsl:choose>
				<xsl:text>&#160;</xsl:text>
				<!-- Is this a delegate? -->
				<xsl:if test="local-name()='delegate'">
					<!-- Writes datatype -->
					<xsl:call-template name="get-displayname-csharp">
						<xsl:with-param name="node" select="ndoc:returnType"/>
					</xsl:call-template>
					<xsl:text>&#160;</xsl:text>
				</xsl:if>
				<!-- Writes name -->
				<xsl:call-template name="get-displayname-csharp">
					<xsl:with-param name="onlyWriteGenericLinks" select="true()"/>
				</xsl:call-template>
				<!-- Is not a enumeration and not a delegate? -->
				<xsl:if test="local-name() != 'delegate'">
					<!-- Handel derivation -->
					<xsl:call-template name="derivation" />
				</xsl:if>
				<xsl:choose>
					<xsl:when test="local-name() = 'delegate'">
						<!-- Write parameters -->
						<xsl:call-template name="parameters">
							<xsl:with-param name="version">long</xsl:with-param>
							<xsl:with-param name="namespace-name" select="../@name" />
						</xsl:call-template>
						<xsl:if test="ndoc:genericconstraints">
							<xsl:call-template name="genericconstraints" />
						</xsl:if>
						<xsl:text>;</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<!-- Write generic constraints if there are any -->
						<xsl:if test="local-name() != 'delegate' and ndoc:genericconstraints">
							<xsl:call-template name="genericconstraints" />
						</xsl:if>
					</xsl:otherwise>
				</xsl:choose>
			</div>
		</div>
	</xsl:template>
	<!-- Generic constrains -->
	<xsl:template name="genericconstraints">
		<xsl:for-each select="ndoc:genericconstraints">
			<br />
			<xsl:text>where&#160;</xsl:text>
			<xsl:value-of select="@param"/>
			<xsl:text>&#160;:&#160;</xsl:text>
			<xsl:for-each select="ndoc:constraint">
				<xsl:call-template name="get-displayname-csharp"/>
				<xsl:if test="position() != last()">
					<xsl:text>,&#160;</xsl:text>
				</xsl:if>
			</xsl:for-each>
		</xsl:for-each>
	</xsl:template>
	<!-- Derivation -->
	<xsl:template name="derivation">
		<!-- Is this a derived class/interface? Either from a class or an interface -->
		<xsl:if test="ndoc:baseType or ndoc:implementsClass[not(@inherited)] or ndoc:implements">
			<b>
				<xsl:text> : </xsl:text>
				<xsl:if test="ndoc:baseType">
					<xsl:call-template name="get-displayname-csharp">
						<xsl:with-param name="node" select="ndoc:baseType"/>
					</xsl:call-template>
					<!-- If we also implements an interface -->
					<xsl:if test="ndoc:implementsClass[not(@inherited)]">
						<xsl:text>, </xsl:text>
					</xsl:if>
				</xsl:if>
				<!-- Iterate through all interfaces implemented by a class -->
				<xsl:for-each select="ndoc:implementsClass[not(@inherited)]">
					<xsl:call-template name="get-displayname-csharp"/>
					<!-- If there are more interfaces implemented -->
					<xsl:if test="position()!=last()">
						<xsl:text>, </xsl:text>
					</xsl:if>
				</xsl:for-each>
				<!-- Iterate through all interfaces implemented by an interface -->
				<xsl:for-each select="ndoc:implements">
					<xsl:call-template name="get-displayname-csharp"/>
					<!-- If there are more interfaces implemented -->
					<xsl:if test="position()!=last()">
						<xsl:text>, </xsl:text>
					</xsl:if>
				</xsl:for-each>
			</b>
		</xsl:if>
	</xsl:template>
	<!-- C# Member syntax -->
	<xsl:template name="cs-member-syntax">
		<div class="syntax">
			<!-- If VB syntax also should be written -->
			<xsl:if test="$ndoc-vb-syntax">
				<span class="lang">[C#]</span>
				<br />
			</xsl:if>
			<!-- Write attributes -->
			<xsl:call-template name="attributes" />
			<!-- If this member hides another member -->
			<xsl:if test="@hiding">
				<xsl:text>new&#160;</xsl:text>
			</xsl:if>
			<xsl:if test="not(parent::ndoc:interface or @interface)">
				<!-- If the member is not a constructor or is not static -->
				<xsl:if test="local-name()!='constructor'">
					<!-- Write method accessmodifier -->
					<xsl:call-template name="method-access">
						<xsl:with-param name="access" select="@access" />
					</xsl:call-template>
					<xsl:text>&#160;</xsl:text>
				</xsl:if>
				<!-- Handle operator overloaded methods -->
				<xsl:if test="local-name() = 'operator' and @overload">
					<xsl:text>override&#160;</xsl:text>
				</xsl:if>
				<!-- If the member is not final or normal -->
				<xsl:if test="@contract and @contract!='Normal' and @contract!='Final'">
					<!-- Write contract -->
					<xsl:call-template name="contract">
						<xsl:with-param name="contract" select="@contract" />
					</xsl:call-template>
					<xsl:text>&#160;</xsl:text>
				</xsl:if>
			</xsl:if>
			<xsl:choose>
				<!-- If this a constructor -->
				<xsl:when test="local-name()='constructor'">
					<xsl:call-template name="get-displayname-csharp">
						<xsl:with-param name="node" select=".."/>
					</xsl:call-template>
				</xsl:when>
				<xsl:otherwise>
					<!-- If the name is different from op_Explicit and op_Implicit -->
					<xsl:if test="@name != 'op_Explicit' and @name != 'op_Implicit'">
						<!-- Write link to datatype -->
						<xsl:call-template name="get-displayname-csharp">
							<xsl:with-param name="node" select="ndoc:returnType"/>
						</xsl:call-template>
						<xsl:text>&#160;</xsl:text>
					</xsl:if>
					<xsl:choose>
						<!-- If localname is operator -->
						<xsl:when test="local-name()='operator'">
							<xsl:choose>
								<!-- If this is a explicit conversion operator -->
								<xsl:when test="@name='op_Explicit'">
									<xsl:text>explicit operator </xsl:text>
									<!-- Write link to datatype -->
									<xsl:call-template name="get-displayname-csharp">
										<xsl:with-param name="node" select="ndoc:returnType"/>
									</xsl:call-template>
								</xsl:when>
								<!-- If this is a implicit conversion operator -->
								<xsl:when test="@name='op_Implicit'">
									<xsl:text>implicit operator </xsl:text>
									<!-- Write link to datatype -->
									<xsl:call-template name="get-displayname-csharp">
										<xsl:with-param name="node" select="ndoc:returnType"/>
									</xsl:call-template>
								</xsl:when>
								<!-- Otherwise write C# operator name -->
								<xsl:otherwise>
									<xsl:call-template name="csharp-operator-name">
										<xsl:with-param name="name" select="@name" />
									</xsl:call-template>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<!-- Hvis det ikke er en operator write the name -->
						<xsl:otherwise>
							<xsl:call-template name="get-displayname-csharp">
								<xsl:with-param name="node" select="." />
								<xsl:with-param name="onlyWriteGenericLinks" select="true()"/>
							</xsl:call-template>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:otherwise>
			</xsl:choose>
			<!-- Write parameters -->
			<xsl:call-template name="parameters">
				<xsl:with-param name="version">long</xsl:with-param>
				<xsl:with-param name="namespace-name" select="../../@name" />
			</xsl:call-template>
			<xsl:if test="ndoc:genericconstraints">
				<xsl:call-template name="genericconstraints" />
			</xsl:if>
			<xsl:text>;</xsl:text>
		</div>
	</xsl:template>

	<!-- C# Member Syntax in individual method overload list -->
	<xsl:template name="cs-member-syntax-overload">
		<!-- If this member hides another member -->
		<xsl:if test="@hiding">
			<xsl:text>new&#160;</xsl:text>
		</xsl:if>
		<!-- If the does not implement an interface -->
		<xsl:if test="local-name(parent::node()) != 'interface'">
			<!-- If this is not a constructor or the contract is not static -->
			<xsl:if test="(local-name()!='constructor') or (@contract!='Static')">
				<!-- Write method accessmodifier -->
				<xsl:call-template name="method-access">
					<xsl:with-param name="access" select="@access" />
				</xsl:call-template>
				<xsl:text>&#160;</xsl:text>
			</xsl:if>
			<!-- If the member has a contract and it is not final or normal -->
			<xsl:if test="@contract and @contract!='Normal' and @contract!='Final'">
				<!-- Write contract -->
				<xsl:call-template name="contract">
					<xsl:with-param name="contract" select="@contract" />
				</xsl:call-template>
				<xsl:text>&#160;</xsl:text>
			</xsl:if>
		</xsl:if>
		<xsl:choose>
			<!-- If the member is a constructor -->
			<xsl:when test="local-name()='constructor'">
				<xsl:value-of select="../@name" />
			</xsl:when>
			<!-- If the member is a operator -->
			<xsl:when test="local-name()='operator'">
				<!-- Write datatype -->
				<!--<xsl:call-template name="get-datatype">
					<xsl:with-param name="datatype" select="substring-after(ndoc:returnType/@typeId, ':')" />
				</xsl:call-template>-->
				<xsl:call-template name="get-displayname-csharp">
					<xsl:with-param name="node" select="ndoc:returnType" />
					<xsl:with-param name="onlyWriteGenericLinks" select="true()"/>
				</xsl:call-template>
				<!-- Write operator name-->
				<xsl:call-template name="operator-name">
					<xsl:with-param name="name">
						<xsl:value-of select="@name" />
					</xsl:with-param>
					<xsl:with-param name="from">
						<!--<xsl:call-template name="get-datatype">
							<xsl:with-param name="datatype" select="substring-after(ndoc:parameter/@typeId, ':')" />
						</xsl:call-template>-->
						<xsl:call-template name="get-displayname-csharp">
							<xsl:with-param name="node" select="ndoc:parameter" />
							<xsl:with-param name="onlyWriteGenericLinks" select="true()"/>
						</xsl:call-template>
					</xsl:with-param>
					<xsl:with-param name="to">
						<!--<xsl:call-template name="get-datatype">
							<xsl:with-param name="datatype" select="substring-after(ndoc:returnType/@typeId, ':')" />
						</xsl:call-template>-->
						<xsl:call-template name="get-displayname-csharp">
							<xsl:with-param name="node" select="ndoc:returnType" />
							<xsl:with-param name="onlyWriteGenericLinks" select="true()"/>
						</xsl:call-template>
					</xsl:with-param>
				</xsl:call-template>
			</xsl:when>
			<!-- Otherwise write datatype and name of the member -->
			<xsl:otherwise>
				<!--<xsl:call-template name="get-datatype">
					<xsl:with-param name="datatype" select="substring-after(ndoc:returnType/@typeId, ':')" />
				</xsl:call-template-->
				<xsl:call-template name="get-displayname-csharp">
					<xsl:with-param name="node" select="ndoc:returnType" />
					<xsl:with-param name="onlyWriteGenericLinks" select="true()"/>
				</xsl:call-template>
				<xsl:text>&#160;</xsl:text>
				<xsl:value-of select="@displayName" />
			</xsl:otherwise>
		</xsl:choose>
		<!-- If the member is not a conversion operator, write parameters in short mode -->
		<xsl:if test="@name!='op_Implicit' and @name!='op_Explicit'">
			<xsl:call-template name="parameters">
				<xsl:with-param name="version">short</xsl:with-param>
				<xsl:with-param name="namespace-name" select="../../@name" />
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<!-- Field or event syntax -->
	<xsl:template name="cs-field-or-event-syntax">
		<div class="syntax">
			<!-- Should VB syntax also be written -->
			<xsl:if test="$ndoc-vb-syntax">
				<span class="lang">[C#]</span>
				<br />
			</xsl:if>
			<!-- Write attributes -->
			<xsl:call-template name="attributes" />
			<!-- If it hides annother field or event -->
			<xsl:if test="@hiding">
				<xsl:text>new&#160;</xsl:text>
			</xsl:if>
			<!-- If class does not implement an interface -->
			<xsl:if test="not(parent::ndoc:interface)">
				<!-- Write method accessmodifier -->
				<xsl:call-template name="method-access">
					<xsl:with-param name="access" select="@access" />
				</xsl:call-template>
				<xsl:text>&#160;</xsl:text>
			</xsl:if>
			<!-- If contract is static -->
			<xsl:if test="@contract='Static'">
				<xsl:choose>
					<!-- If this is a constant -->
					<xsl:when test="@literal='true'">
						<xsl:text>const&#160;</xsl:text>
					</xsl:when>
					<!-- Otherwise this is just static -->
					<xsl:otherwise>
						<xsl:text>static&#160;</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:if>
			<!-- If this is readonly -->
			<xsl:if test="@initOnly='true'">
				<xsl:text>readonly&#160;</xsl:text>
			</xsl:if>
			<!-- If this is an event -->
			<xsl:if test="local-name() = 'event'">
				<xsl:text>event&#160;</xsl:text>
			</xsl:if>
			<!-- Write link to datatype -->
			<xsl:call-template name="get-displayname-csharp"/>
			<xsl:text>&#160;</xsl:text>
			<!-- Write name -->
			<xsl:value-of select="@name" />
			<!-- If this is a constant write assigned value-->
			<xsl:if test="@literal='true'">
				<xsl:text> = </xsl:text>
				<!-- If the value is a String write " -->
				<xsl:if test="@type='System.String'">
					<xsl:text>"</xsl:text>
				</xsl:if>
				<xsl:value-of select="@value" />
				<!-- If the value is a String write " -->
				<xsl:if test="@type='System.String'">
					<xsl:text>"</xsl:text>
				</xsl:if>
			</xsl:if>
			<xsl:text>;</xsl:text>
		</div>
	</xsl:template>
	<!-- C# Property Syntax -->
	<xsl:template name="cs-property-syntax">
		<xsl:param name="indent" select="true()" />
		<xsl:param name="display-names" select="true()" />
		<xsl:param name="link-types" select="true()" />
		<!-- Write attributes -->
		<xsl:call-template name="attributes" />
		<!-- If this property hides another -->
		<xsl:if test="@hiding">
			<xsl:text>new&#160;</xsl:text>
		</xsl:if>
		<!-- If the class does not implement an interface -->
		<xsl:if test="not(parent::interface)">
			<!-- Write accessmodifier -->
			<xsl:call-template name="method-access">
				<xsl:with-param name="access" select="@access" />
			</xsl:call-template>
			<xsl:text>&#160;</xsl:text>
		</xsl:if>
		<!-- If this is static -->
		<xsl:if test="@contract='Static'">
			<xsl:text>static&#160;</xsl:text>
		</xsl:if>
		<!-- If the class does not implement an interface -->
		<xsl:if test="not(parent::ndoc:interface)">
			<!-- If contract is not normal, static or final -->
			<xsl:if test="@contract!='Normal' and @contract!='Static' and @contract!='Final'">
				<!-- Write contract -->
				<xsl:call-template name="contract">
					<xsl:with-param name="contract" select="@contract" />
				</xsl:call-template>
				<xsl:text>&#160;</xsl:text>
			</xsl:if>
		</xsl:if>
		<xsl:choose>
			<!-- If we shoule write links to types -->
			<xsl:when test="$link-types">
				<xsl:call-template name="get-displayname-csharp"/>
			</xsl:when>
			<!-- Otherwise just write the type of the property -->
			<xsl:otherwise>
				<xsl:call-template name="get-displayname-csharp">
					<xsl:with-param name="onlyWriteGenericLinks" select="true()"/>
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:text>&#160;</xsl:text>
		<xsl:choose>
			<!-- If the property has parameters -->
			<xsl:when test="ndoc:parameter">
				<xsl:text>this[</xsl:text>
				<xsl:if test="$indent">
					<br />
				</xsl:if>
				<!-- Write all parameters -->
				<xsl:for-each select="ndoc:parameter">
					<xsl:if test="$indent">
						<xsl:text>&#160;&#160;&#160;</xsl:text>
					</xsl:if>
					<xsl:call-template name="get-displayname-csharp"/>
					<!-- If we should write the parameters names -->
					<xsl:if test="$display-names">
						<xsl:text>&#160;</xsl:text>
						<i>
							<xsl:value-of select="@name" />
						</i>
					</xsl:if>
					<!-- If have not written all parameters -->
					<xsl:if test="position() != last()">
						<xsl:text>,&#160;</xsl:text>
						<xsl:if test="$indent">
							<br />
						</xsl:if>
					</xsl:if>
				</xsl:for-each>
				<xsl:if test="$indent">
					<br />
				</xsl:if>
				<xsl:text>]</xsl:text>
			</xsl:when>
			<!-- Otherwise just write the name of the property-->
			<xsl:otherwise>
				<xsl:value-of select="@name" />
			</xsl:otherwise>
		</xsl:choose>
		<xsl:text>&#160;{</xsl:text>
		<!-- If property has a get -->
		<xsl:if test="@get!='false'">
			<xsl:text>&#160;</xsl:text>
			<xsl:call-template name="method-access">
				<xsl:with-param name="access" select="@get" />
			</xsl:call-template>
			<xsl:text>&#160;</xsl:text>
			<xsl:text>get;</xsl:text>
			<!-- If property does not have a set -->
			<xsl:if test="@set='false'">
				<xsl:text>&#160;</xsl:text>
			</xsl:if>
		</xsl:if>
		<!-- If property has a set -->
		<xsl:if test="@set!='false'">
			<xsl:text>&#160;</xsl:text>
			<xsl:call-template name="method-access">
				<xsl:with-param name="access" select="@set" />
			</xsl:call-template>
			<xsl:text>&#160;</xsl:text>
			<xsl:text>set;</xsl:text>
			<xsl:text>&#160;</xsl:text>
		</xsl:if>
		<xsl:text>}</xsl:text>
	</xsl:template>
	<!-- Parameters -->
	<xsl:template name="parameters">
		<xsl:param name="version" />
		<xsl:param name="namespace-name" />
		<!-- Write parameters -->
		<xsl:text>(</xsl:text>
		<xsl:if test="ndoc:parameter">
			<xsl:for-each select="ndoc:parameter">
				<!-- If this should be written in long mode -->
				<xsl:if test="$version='long'">
					<br />
					<xsl:text>&#160;&#160;&#160;</xsl:text>
				</xsl:if>
				<!-- Write modifiers -->
				<xsl:if test="@extension">this&#160;</xsl:if>
				<xsl:choose>
					<xsl:when test="@direction = 'ref'">ref&#160;</xsl:when>
					<xsl:when test="@direction = 'out'">out&#160;</xsl:when>
					<xsl:when test="@isParamArray = 'true'">params&#160;</xsl:when>
				</xsl:choose>
				<xsl:call-template name="get-displayname-csharp">
					<xsl:with-param name="onlyWriteGenericLinks">
						<xsl:if test="$version='short'">
							<xsl:value-of select="true()"/>
						</xsl:if>
					</xsl:with-param>
				</xsl:call-template>
				<!-- If this should be written in long mode, write name i italic -->
				<xsl:if test="$version='long'">
					<xsl:text>&#160;</xsl:text>
					<i>
						<xsl:value-of select="@name" />
					</i>
				</xsl:if>
				<!-- Has all parameters been written -->
				<xsl:if test="position()!= last()">
					<xsl:text>,</xsl:text>
				</xsl:if>
			</xsl:for-each>
			<!-- If this should be written in long mode -->
			<xsl:if test="$version='long'">
				<br />
			</xsl:if>
		</xsl:if>
		<xsl:text>)</xsl:text>
	</xsl:template>

	<xsl:template name="array" match="ndoc:array" mode="cs-syntax">
		<xsl:text>[</xsl:text>
		<!-- find a way to output ',' according to 'rank' attribute -->
		<xsl:call-template name="writechar">
			<xsl:with-param name="char" select="','"/>
			<xsl:with-param name="count" select="number(@rank)-1" />
		</xsl:call-template>
		<xsl:text>]</xsl:text>
		<xsl:apply-templates select="ndoc:array" mode="cs-syntax"/>
	</xsl:template>

	<xsl:template name="writechar">
		<xsl:param name="char" />
		<xsl:param name="count" />
		<xsl:if test="$count &gt; 0">
			<xsl:value-of select="$char" />
			<xsl:call-template name="writechar">
				<xsl:with-param name="char" select="$char" />
				<xsl:with-param name="count" select="($count)-1" />
			</xsl:call-template>
		</xsl:if>
	</xsl:template>
	
	<!--  -->
	<xsl:template name="get-datatype">
		<xsl:param name="datatype" />
		<!-- Variable added to handle generic datatypes-->
		<xsl:variable name="type">
			<xsl:choose>
				<xsl:when test="contains($datatype, '`')">
					<xsl:value-of select="substring-before($datatype, '`')"/>
				</xsl:when>
				<xsl:when test="contains($datatype, '(')">
					<xsl:value-of select="substring-before($datatype, '(')"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$datatype"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<!-- Removes namespace -->
		<xsl:call-template name="strip-namespace">
			<xsl:with-param name="name">
				<!-- Gets the C# type (System.String is string)-->
				<xsl:call-template name="csharp-type">
					<xsl:with-param name="runtime-type" select="$type" />
				</xsl:call-template>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>
	<!-- -->
	<!-- member.xslt is using this for title and h1.  should try and use parameters template above. -->
	<xsl:template name="get-param-list">
		<xsl:text>(</xsl:text>
		<xsl:for-each select="ndoc:parameter">
			<xsl:call-template name="strip-namespace">
				<xsl:with-param name="name" select="substring-after(@typeId, ':')" />
			</xsl:call-template>
			<xsl:if test="position()!=last()">
				<xsl:text>, </xsl:text>
			</xsl:if>
		</xsl:for-each>
		<xsl:text>)</xsl:text>
	</xsl:template>
	<!-- -->
	<!-- Attributes -->
	<xsl:template name="attributes">
		<!-- Should attributes be documented? -->
		<xsl:if test="$ndoc-document-attributes">
			<!-- Check if this is an attribute, and if so iterate over all of them -->
			<xsl:if test="ndoc:attribute">
				<xsl:for-each select="ndoc:attribute">
					<div class="attribute">
						<!-- Attribute template called with the name of the attribute -->
						<xsl:call-template name="attribute">
							<xsl:with-param name="attname" select="@name" />
						</xsl:call-template>
					</div>
				</xsl:for-each>
			</xsl:if>
		</xsl:if>
	</xsl:template>
	<!-- Attribute -->
	<xsl:template name="attribute">
		<!-- Name of attribute -->
		<xsl:param name="attname" />
		<xsl:text>[</xsl:text>
		<!-- Write target to outputstream -->
		<xsl:if test="@target">
			<xsl:value-of select="@target" /> :
		</xsl:if>
		<!-- Removes namespace and attributes -->
		<xsl:call-template name="strip-namespace-and-attribute">
			<xsl:with-param name="name" select="@name" />
		</xsl:call-template>
		<xsl:if test="count(ndoc:property | ndoc:field) > 0">
			<xsl:text>(</xsl:text>
			<xsl:for-each select="ndoc:property | ndoc:field">
				<xsl:value-of select="@name" />
				<xsl:text>=</xsl:text>
				<xsl:choose>
					<xsl:when test="@value">
						<xsl:if test="@type='System.String'">
							<xsl:text>"</xsl:text>
						</xsl:if>
						<xsl:value-of select="@value" />
						<xsl:if test="@type='System.String'">
							<xsl:text>"</xsl:text>
						</xsl:if>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>**UNKNOWN**</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:if test="position()!=last()">
					<xsl:text>, </xsl:text>
				</xsl:if>
			</xsl:for-each>
			<xsl:text>)</xsl:text>
		</xsl:if>
		<xsl:text>]</xsl:text>
	</xsl:template>
	<!-- Removes namespace and attribute -->
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
	<!-- Get the displayname of a type -->
	<xsl:template name="get-displayname-csharp">
		<xsl:param name="node" select="."/>
		<xsl:param name="onlyWriteGenericLinks" select="false()"/>
		<xsl:choose>
			<xsl:when test="$onlyWriteGenericLinks='true'">
				<xsl:call-template name="write-type-link">
					<xsl:with-param name="node" select="$node" />
					<xsl:with-param name="nolinks" select="true()"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="write-type-link">
					<xsl:with-param name="node" select="$node" />
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:if test="$node/ndoc:genericargument">
			<xsl:text>&lt;</xsl:text>
		</xsl:if>
		<xsl:for-each select="$node/ndoc:genericargument">
			<xsl:call-template name="get-genericarguments">
				<xsl:with-param name="node" select="."/>
				<xsl:with-param name="nolinks">
					<xsl:if test="$onlyWriteGenericLinks='true'">
						<xsl:value-of select="true()"/>
					</xsl:if>
				</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="position()!=last()">
				<xsl:text>, </xsl:text>
			</xsl:if>
		</xsl:for-each>
		<xsl:if test="$node/ndoc:genericargument">
			<xsl:text>&gt;</xsl:text>
		</xsl:if>
		<!-- append array syntax -->
		<xsl:apply-templates select="$node/ndoc:array" mode="cs-syntax" />
	</xsl:template>

	<!-- Generic parameters and arguments -->
	<xsl:template name="get-genericarguments">
		
		<xsl:param name="node" select="." />
		<xsl:param name="nolinks" select="false()"/>
		
		<xsl:call-template name="write-type-link">
			<xsl:with-param name="node" select="$node" />
			<xsl:with-param name="nolinks">
				<xsl:if test="$nolinks='true'">
					<xsl:value-of select="true()"/>
				</xsl:if>
			</xsl:with-param>
		</xsl:call-template>
		<xsl:if test="$node/ndoc:genericargument">
			<xsl:text>&lt;</xsl:text>
		</xsl:if>
		<xsl:for-each select="$node/ndoc:genericargument">
			<xsl:call-template name="get-genericarguments">
				<xsl:with-param name="node" select="."/>
				<xsl:with-param name="nolinks">
					<xsl:if test="$nolinks='true'">
						<xsl:value-of select="true()"/>
					</xsl:if>
				</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="position()!=last()">
				<xsl:text>, </xsl:text>
			</xsl:if>
		</xsl:for-each>
		<xsl:if test="$node/ndoc:genericargument">
			<xsl:text>&gt;</xsl:text>
		</xsl:if>
	</xsl:template>
	
	<!-- Write link to type -->
	<xsl:template name="write-type-link">
		<xsl:param name="node" select="."/>
		<xsl:param name="nolinks" select="false()"/>
		<!-- Handle both types with ID attribute and those without fx. genericarguments -->
		<xsl:choose>
			<xsl:when test="(contains($node/@id, '.') or contains($node/@typeId, '.') or contains($node, '.') or (contains($node/@name, '.') and local-name($node) = 'genericargument')) and not($nolinks='true')">
				<a>
					<xsl:choose>
						<!-- Handle the speciel case of fields, events, properties and parameters
            which uses typeId instead of id attribute -->
						<xsl:when test="$node/@typeId">
						<!--<xsl:when test="local-name($node) = 'field' or local-name($node) = 'event'
                      or local-name($node) = 'property' or local-name($node) = 'parameter' or local-name($node) = 'genericargument'">-->
							<xsl:attribute name="href">
								<xsl:call-template name="get-filename-for-type">
									<xsl:with-param name="id" select="$node/@typeId" />
									<xsl:with-param name="assemblyName" select="$node/@assembly" />
								</xsl:call-template>
							</xsl:attribute>
							<xsl:call-template name="get-datatype">
								<xsl:with-param name="datatype" select="substring-after($node/@typeId, ':')" />
							</xsl:call-template>
						</xsl:when>
						<!-- Handle special generic constraint case -->
						<xsl:when test="local-name($node) = 'constraint'">
							<xsl:attribute name="href">
								<xsl:call-template name="get-filename-for-type-name">
									<xsl:with-param name="type-name" select="." />
								</xsl:call-template>
							</xsl:attribute>
							<xsl:call-template name="get-datatype">
								<xsl:with-param name="datatype" select="." />
							</xsl:call-template>
						</xsl:when>
						<xsl:when test="local-name($node) = 'genericargument'">
							<xsl:attribute name="href">
								<xsl:call-template name="get-filename-for-type-name">
									<xsl:with-param name="type-name" select="$node/@name" />
								</xsl:call-template>
							</xsl:attribute>
							<xsl:call-template name="get-datatype">
								<xsl:with-param name="datatype" select="$node/@name" />
							</xsl:call-template>
						</xsl:when>
						<xsl:otherwise>
							<xsl:attribute name="href">
								<xsl:call-template name="get-filename-for-type-name">
									<xsl:with-param name="type-name" select="substring-after($node/@id, ':')" />
								</xsl:call-template>
							</xsl:attribute>
							<xsl:call-template name="get-datatype">
								<xsl:with-param name="datatype" select="substring-after($node/@id, ':')" />
							</xsl:call-template>
						</xsl:otherwise>
					</xsl:choose>
				</a>
			</xsl:when>
			<xsl:when test="local-name($node) = 'constraint'">
				<xsl:choose>
					<xsl:when test="$node/text() = 'struct'">
						<xsl:text>struct</xsl:text>
					</xsl:when>
					<xsl:when test="$node/text() = 'class'">
						<xsl:text>class</xsl:text>
					</xsl:when>
					<xsl:when test="$node/text() = 'new'">
						<xsl:text>new()</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="get-datatype">
							<xsl:with-param name="datatype" select="." />
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:when test="local-name($node) = 'genericargument'">
				<xsl:call-template name="get-datatype">
					<xsl:with-param name="datatype" select="$node/@name" />
				</xsl:call-template>
			</xsl:when>
			<!-- handle parameters of generic type arguments (both, class and method generic args) -->
			<xsl:when test="$node/@typeId != ''">
				<xsl:call-template name="get-datatype">
					<xsl:with-param name="datatype" select="substring-after($node/@typeId, ':')" />
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$node/@id != ''">
				<xsl:call-template name="get-datatype">
					<xsl:with-param name="datatype" select="substring-after($node/@id, ':')" />
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="get-datatype">
					<xsl:with-param name="datatype" select="$node/@name" />
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:if test="$node/@nullable = 'true'">
			<xsl:text>?</xsl:text>
		</xsl:if>
	</xsl:template>
</xsl:stylesheet>
