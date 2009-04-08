<?xml version="1.0" encoding="UTF-8" ?>
<xsl:transform version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:NUtil="urn:NDocUtil" xmlns:ndoc="urn:ndoc-schema"
	exclude-result-prefixes="NUtil" >
	<!-- -->
	<xsl:param name="ndoc-vb-syntax" />
	<!-- -->
	<xsl:template name="vb-type">
		<xsl:param name="runtime-type" />
		<!-- Variable added to handle generic datatypes-->
		<xsl:variable name="type">
			<xsl:choose>
				<xsl:when test="contains($runtime-type, '`')">
					<xsl:value-of select="substring-before($runtime-type, '`')"/>
				</xsl:when>
				<xsl:when test="contains($runtime-type, '(')">
					<xsl:value-of select="substring-before($runtime-type, '(')"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$runtime-type"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="old-type">
			<xsl:choose>
				<xsl:when test="contains($type, '[')">
					<xsl:value-of select="substring-before($type, '[')" />
				</xsl:when>
				<xsl:when test="contains($type, '&amp;')">
					<xsl:value-of select="substring-before($type, '&amp;')" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$type" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="new-type">
			<xsl:choose>
				<xsl:when test="$old-type='System.Byte'">Byte</xsl:when>
				<xsl:when test="$old-type='System.Int16'">Short</xsl:when>
				<xsl:when test="$old-type='System.Int32'">Integer</xsl:when>
				<xsl:when test="$old-type='System.Int64'">Long</xsl:when>
				<xsl:when test="$old-type='System.Single'">Single</xsl:when>
				<xsl:when test="$old-type='System.Double'">Double</xsl:when>
				<xsl:when test="$old-type='System.Decimal'">Decimal</xsl:when>
				<xsl:when test="$old-type='System.String'">String</xsl:when>
				<xsl:when test="$old-type='System.Char'">Char</xsl:when>
				<xsl:when test="$old-type='System.Boolean'">Boolean</xsl:when>
				<xsl:when test="$old-type='System.DateTime'">Date</xsl:when>
				<xsl:when test="$old-type='System.Object'">Object</xsl:when>
				<xsl:otherwise>
					<xsl:call-template name="strip-namespace">
						<xsl:with-param name="name" select="$old-type" />
					</xsl:call-template>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:value-of select="$new-type" />
	</xsl:template>
	<!-- -->
	<xsl:template name="vb-type-syntax">
		<xsl:if test="$ndoc-vb-syntax">
			<div class="syntax">
				<span class="lang">[Visual&#160;Basic]</span>
				<br />
				<xsl:call-template name="vb-attributes" />
				<xsl:call-template name="vb-type-access">
					<xsl:with-param name="access" select="@access" />
					<xsl:with-param name="type" select="local-name()" />
				</xsl:call-template>
				<xsl:if test="@static = 'true'">
					<xsl:text>&#160;Shared</xsl:text>
				</xsl:if>
				<xsl:if test="@abstract = 'true'">
					<xsl:text>&#160;MustInherit</xsl:text>
				</xsl:if>
				<xsl:if test="@sealed = 'true'">
					<xsl:text>&#160;NotInheritable</xsl:text>
				</xsl:if>
				<xsl:text>&#160;</xsl:text>
				<xsl:choose>
					<xsl:when test="local-name() = 'class'">Class</xsl:when>
					<xsl:when test="local-name() = 'interface'">Interface</xsl:when>
					<xsl:when test="local-name() = 'structure'">Structure</xsl:when>
					<xsl:when test="local-name() = 'enumeration'">Enum</xsl:when>
					<xsl:when test="local-name() = 'delegate'">
						<xsl:text>Delegate&#160;</xsl:text>
						<xsl:choose>
							<xsl:when test="ndoc:returnType/@type = 'System.Void'">Sub</xsl:when>
							<xsl:otherwise>Function</xsl:otherwise>
						</xsl:choose>
					</xsl:when>
					<xsl:otherwise>ERROR</xsl:otherwise>
				</xsl:choose>
				<xsl:text>&#160;</xsl:text>
				<xsl:call-template name="get-displayname-vb">
					<xsl:with-param name="onlyWriteGenericLinks" select="true()"/>
				</xsl:call-template>
				<xsl:choose>
					<xsl:when test="local-name() != 'delegate'">
						<xsl:if test="ndoc:baseType">
							<br />
							<xsl:text>&#160;&#160;&#160;&#160;Inherits&#160;</xsl:text>
							<xsl:call-template name="get-displayname-vb">
								<xsl:with-param name="node" select="ndoc:baseType"/>
							</xsl:call-template>
						</xsl:if>
						<xsl:if test="ndoc:implementsClass[not(@inherited)] or ndoc:implements">
							<br />
							<xsl:text>&#160;&#160;&#160;&#160;Implements&#160;</xsl:text>
						</xsl:if>
						<xsl:for-each select="ndoc:implementsClass[not(@inherited)]">
							<xsl:call-template name="get-displayname-vb"/>
							<xsl:if test="position()!=last()">
								<xsl:text>, </xsl:text>
							</xsl:if>
						</xsl:for-each>
						<xsl:for-each select="ndoc:implements">
							<xsl:call-template name="get-displayname-vb"/>
							<xsl:if test="position()!=last()">
								<xsl:text>, </xsl:text>
							</xsl:if>
						</xsl:for-each>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="vb-parameters" />
					</xsl:otherwise>
				</xsl:choose>
			</div>
		</xsl:if>
	</xsl:template>
	<!-- Parameters template -->
	<xsl:template name="vb-parameters">
		<xsl:choose>
			<xsl:when test="ndoc:parameter">
				<xsl:text>( _</xsl:text>
				<br />
				<xsl:apply-templates select="ndoc:parameter" mode="vb" />
				<xsl:text>)</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>()</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:if test="ndoc:returnType/@type != 'System.Void'">
			<xsl:text>&#160;As&#160;</xsl:text>
			<xsl:call-template name="get-displayname-vb">
				<xsl:with-param name="node" select="ndoc:returnType"/>
			</xsl:call-template>
		</xsl:if>
		<xsl:if test="ndoc:implements">
			<xsl:variable name="member" select="local-name(..)"/>
			<xsl:text>&#160;_</xsl:text>
			<div>
				<xsl:text>&#160;&#160;&#160;&#160;Implements&#160;</xsl:text>
				<xsl:for-each select="ndoc:implements[not(@inherited)]">
					<xsl:call-template name="implements-member">
						<xsl:with-param name="member" select="$member" />
					</xsl:call-template>
					<xsl:if test="position()!=last()">
						<xsl:text>, </xsl:text>
					</xsl:if>
				</xsl:for-each>
			</div>
		</xsl:if>
	</xsl:template>
	<!-- Type access -->
	<xsl:template name="vb-type-access">
		<xsl:param name="access" />
		<xsl:param name="type" />
		<xsl:choose>
			<xsl:when test="$access='Public'">Public</xsl:when>
			<xsl:when test="$access='NotPublic'">Friend</xsl:when>
			<xsl:when test="$access='NestedPublic'">Public</xsl:when>
			<xsl:when test="$access='NestedFamily'">Protected</xsl:when>
			<xsl:when test="$access='NestedFamilyOrAssembly'">Protected Friend</xsl:when>
			<xsl:when test="$access='NestedAssembly'">Friend</xsl:when>
			<xsl:when test="$access='NestedPrivate'">Private</xsl:when>
			<xsl:otherwise>/* unknown */</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- Method access -->
	<xsl:template name="vb-method-access">
		<xsl:param name="access" />
		<xsl:choose>
			<xsl:when test="$access='Public'">Public</xsl:when>
			<xsl:when test="$access='Family'">Protected</xsl:when>
			<xsl:when test="$access='FamilyOrAssembly'">Protected Friend</xsl:when>
			<xsl:when test="$access='Assembly'">Friend</xsl:when>
			<xsl:when test="$access='Private'">Private</xsl:when>
			<xsl:otherwise>/* unknown */</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- Individual parameters -->
	<xsl:template match="ndoc:parameter" mode="vb">
		<xsl:text>&#160;&#160;&#160;</xsl:text>
		<xsl:if test="@optional = 'true'">
			<xsl:text>Optional </xsl:text>
		</xsl:if>
		<xsl:choose>
			<xsl:when test="@isParamArray = 'true'">
				<xsl:text>ParamArray </xsl:text>
			</xsl:when>
			<xsl:when test="@direction = 'ref' or @direction = 'out'">
				<xsl:text>ByRef </xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>ByVal </xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<i>
			<xsl:value-of select="@name" />
		</i>
		<xsl:text>&#160;As&#160;</xsl:text>
		<xsl:call-template name="get-displayname-vb"/>
		<xsl:if test="@optional = 'true'">
			<xsl:text> = </xsl:text>
			<xsl:if test="@type='System.String'">"</xsl:if>
			<xsl:value-of select="@defaultValue" />
			<xsl:if test="@type='System.String'">"</xsl:if>
		</xsl:if>
		<xsl:if test="position() != last()">
			<xsl:text>,</xsl:text>
		</xsl:if>
		<xsl:text>&#160;_</xsl:text>
		<br />
	</xsl:template>
	<!-- Member syntax -->
	<xsl:template name="vb-member-syntax">
		<xsl:if test="$ndoc-vb-syntax">
			<div class="syntax">
				<span class="lang">[Visual&#160;Basic]</span>
				<br />
				<xsl:call-template name="vb-attributes" />
				<xsl:choose>
					<xsl:when test="local-name() != 'operator'">
						<xsl:call-template name="vb-method-access">
							<xsl:with-param name="access" select="@access" />
						</xsl:call-template>
						<xsl:if test="not(parent::ndoc:interface or @interface)">
							<xsl:choose>
								<xsl:when test="@contract='Abstract'">
									<xsl:text>&#160;MustOverride</xsl:text>
								</xsl:when>
								<xsl:when test="@contract='Final'">
									<xsl:text>&#160;NotOverridable</xsl:text>
								</xsl:when>
								<xsl:when test="@contract='Override'">
									<xsl:text>&#160;Overrides</xsl:text>
								</xsl:when>
								<xsl:when test="@contract='Virtual'">
									<xsl:text>&#160;Overridable</xsl:text>
								</xsl:when>
							</xsl:choose>
							<xsl:if test="@overload">
								<xsl:text>&#160;Overloads</xsl:text>
							</xsl:if>
							<xsl:text>&#160;</xsl:text>
							<xsl:if test="@contract='Static'">
								<xsl:text>Shared&#160;</xsl:text>
							</xsl:if>
							<xsl:if test="@hiding='true'">
								<xsl:text>Shadows&#160;</xsl:text>
							</xsl:if>
						</xsl:if>
						<xsl:if test="parent::ndoc:interface or @interface">
							<xsl:text>&#160;</xsl:text>
						</xsl:if>
						<xsl:choose>
							<xsl:when test="ndoc:returnType/@type!='System.Void'">
								<xsl:text>Function</xsl:text>
							</xsl:when>
							<xsl:otherwise>
								<xsl:text>Sub</xsl:text>
							</xsl:otherwise>
						</xsl:choose>
						<xsl:text>&#160;</xsl:text>
						<xsl:choose>
							<xsl:when test="local-name() = 'constructor'">
								<xsl:text>New</xsl:text>
							</xsl:when>
							<xsl:otherwise>
								<!--<xsl:call-template name="strip-namespace">
                  <xsl:with-param name="name" select="@name" />
                </xsl:call-template>-->
								<xsl:call-template name="get-displayname-vb">
									<xsl:with-param name="node" select="."/>
									<xsl:with-param name="onlyWriteGenericLinks" select="true()"/>
								</xsl:call-template>
							</xsl:otherwise>
						</xsl:choose>
						<xsl:call-template name="vb-parameters" />
					</xsl:when>
					<!-- Operators -->
					<xsl:otherwise>
						<xsl:call-template name="vb-method-access">
							<xsl:with-param name="access" select="@access" />
						</xsl:call-template>
						<xsl:if test="@overload">
							<xsl:text>&#160;Overloads</xsl:text>
						</xsl:if>
						<xsl:text>&#160;Shared&#160;</xsl:text>
						<xsl:choose>
							<xsl:when test="@name = 'op_Explicit'">
								<xsl:text>Narrowing Operator&#160;</xsl:text>
								<xsl:call-template name="get-displayname-vb">
									<xsl:with-param name="node" select="ndoc:returnType" />
								</xsl:call-template>
							</xsl:when>
							<xsl:when test="@name = 'op_Implicit'">
								<xsl:text>Widening Operator&#160;</xsl:text>
								<xsl:call-template name="get-displayname-vb">
									<xsl:with-param name="node" select="ndoc:returnType" />
								</xsl:call-template>
							</xsl:when>
							<xsl:otherwise>
								<xsl:call-template name="vb-operator-name">
									<xsl:with-param name="name" select="@name" />
								</xsl:call-template>
							</xsl:otherwise>
						</xsl:choose>
						<xsl:call-template name="vb-parameters" />
					</xsl:otherwise>
				</xsl:choose>
			</div>
		</xsl:if>
	</xsl:template>
	<!-- Operator names -->
	<xsl:template name="vb-operator-name">
		<xsl:param name="name" />
		<xsl:choose>
			<xsl:when test="$name='op_UnaryPlus'">Operator +</xsl:when>
			<xsl:when test="$name='op_UnaryNegation'">Operator -</xsl:when>
			<xsl:when test="$name='op_LogicalNot'">Operator IsFalse</xsl:when>
			<xsl:when test="$name='op_OnesComplement'">Operator Not</xsl:when>
			<xsl:when test="$name='op_Addition'">Operator +</xsl:when>
			<xsl:when test="$name='op_Subtraction'">Operator -</xsl:when>
			<xsl:when test="$name='op_Multiply'">Operator *</xsl:when>
			<xsl:when test="$name='op_Division'">Operator /</xsl:when>
			<xsl:when test="$name='op_Modulus'">Operator Mod</xsl:when>
			<xsl:when test="$name='op_BitwiseAnd'">Operator And</xsl:when>
			<xsl:when test="$name='op_BitwiseOr'">Operator Or</xsl:when>
			<xsl:when test="$name='op_ExclusiveOr'">Operator Xor</xsl:when>
			<xsl:when test="$name='op_Equality'">Operator =</xsl:when>
			<xsl:when test="$name='op_Inequality'">Operator &lt;></xsl:when>
			<xsl:when test="$name='op_LessThan'">Operator &lt;</xsl:when>
			<xsl:when test="$name='op_GreaterThan'">Operator ></xsl:when>
			<xsl:when test="$name='op_LessThanOrEqual'">Operator &lt;=</xsl:when>
			<xsl:when test="$name='op_GreaterThanOrEqual'">Operator >=</xsl:when>
			<xsl:otherwise>ERROR</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- Field and event syntax -->
	<xsl:template name="vb-field-or-event-syntax">
		<xsl:if test="$ndoc-vb-syntax">
			<div class="syntax">
				<span class="lang">[Visual&#160;Basic]</span>
				<br />
				<xsl:call-template name="vb-attributes" />
				<xsl:if test="not(parent::ndoc:interface)">
					<xsl:call-template name="vb-method-access">
						<xsl:with-param name="access" select="@access" />
					</xsl:call-template>
					<xsl:text>&#160;</xsl:text>
				</xsl:if>
				<xsl:if test="@contract='Static'">
					<xsl:choose>
						<xsl:when test="@literal='true'">
							<xsl:text>Const&#160;</xsl:text>
						</xsl:when>
						<xsl:otherwise>
							<xsl:text>Shared&#160;</xsl:text>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:if>
				<xsl:if test="@initOnly='true'">
					<xsl:text>ReadOnly&#160;</xsl:text>
				</xsl:if>
				<xsl:if test="local-name() = 'event'">
					<xsl:text>Event&#160;</xsl:text>
				</xsl:if>
				<xsl:value-of select="@name" />
				<xsl:text>&#160;As&#160;</xsl:text>
				<xsl:call-template name="get-displayname-vb"/>
				<xsl:if test="@literal='true'">
					<xsl:text> = </xsl:text>
					<xsl:if test="@type='System.String'">
						<xsl:text>"</xsl:text>
					</xsl:if>
					<xsl:value-of select="@value" />
					<xsl:if test="@type='System.String'">
						<xsl:text>"</xsl:text>
					</xsl:if>
				</xsl:if>
			</div>
		</xsl:if>
	</xsl:template>
	<!-- Property syntax -->
	<xsl:template name="vb-property-syntax">
		<xsl:if test="$ndoc-vb-syntax">
			<xsl:call-template name="vb-attributes" />
			<xsl:call-template name="vb-method-access">
				<xsl:with-param name="access" select="@access" />
			</xsl:call-template>
			<xsl:text>&#160;</xsl:text>
			<xsl:choose>
				<xsl:when test="@contract='Abstract'">
					<xsl:text>MustOverride&#160;</xsl:text>
				</xsl:when>
				<xsl:when test="@contract='Final'">
					<xsl:text>NotOverridable&#160;</xsl:text>
				</xsl:when>
				<xsl:when test="@contract='Override'">
					<xsl:text>Overrides&#160;</xsl:text>
				</xsl:when>
				<xsl:when test="@contract='Virtual'">
					<xsl:text>Overridable&#160;</xsl:text>
				</xsl:when>
			</xsl:choose>
			<xsl:if test="@overload">
				<xsl:text>Overloads&#160;</xsl:text>
			</xsl:if>
			<xsl:if test="@contract='Static'">
				<xsl:text>Shared&#160;</xsl:text>
			</xsl:if>
			<xsl:if test="parameter">
				<xsl:text>Default&#160;</xsl:text>
			</xsl:if>
			<xsl:if test="@set = 'false'">
				<xsl:text>ReadOnly&#160;</xsl:text>
			</xsl:if>
			<xsl:if test="@get = 'false'">
				<xsl:text>WriteOnly&#160;</xsl:text>
			</xsl:if>
			<xsl:text>Property&#160;</xsl:text>
			<xsl:value-of select="@name" />
			<xsl:choose>
				<xsl:when test="ndoc:parameter">
					<xsl:call-template name="vb-parameters" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>()</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:text>&#160;As&#160;</xsl:text>
			<xsl:call-template name="get-displayname-vb"/>
			<xsl:if test="ndoc:implements">
				<xsl:variable name="member" select="local-name()" />
				<xsl:text>&#160;_</xsl:text>
				<div>
					<xsl:text>&#160;&#160;&#160;&#160;Implements&#160;</xsl:text>
					<xsl:for-each select="ndoc:implements[not(@inherited)]">
						<xsl:call-template name="implements-member">
							<xsl:with-param name="member" select="$member" />
						</xsl:call-template>
						<xsl:if test="position()!=last()">
							<xsl:text>, </xsl:text>
						</xsl:if>
					</xsl:for-each>
				</div>
			</xsl:if>
			<!-- If property has a get -->
			<xsl:if test="@get != 'false'">
				<br />
				<xsl:text>&#160;&#160;&#160;</xsl:text>
				<xsl:call-template name="vb-method-access">
					<xsl:with-param name="access" select="@get" />
				</xsl:call-template>
				<xsl:text>&#160;</xsl:text>
				<xsl:text>Get</xsl:text>
				<br />
				<xsl:text>&#160;&#160;&#160;End Get</xsl:text>
			</xsl:if>
			<xsl:if test="@set != 'false'">
				<br />
				<xsl:text>&#160;&#160;&#160;</xsl:text>
				<xsl:call-template name="vb-method-access">
					<xsl:with-param name="access" select="@set" />
				</xsl:call-template>
				<xsl:text>&#160;</xsl:text>
				<xsl:text>Set</xsl:text>
				<br />
				<xsl:text>&#160;&#160;&#160;End Set</xsl:text>
			</xsl:if>
			<br />
			<xsl:text>End Property</xsl:text>
		</xsl:if>
	</xsl:template>

	<!-- Attributes -->
	<xsl:template name="vb-attributes">
		<xsl:if test="$ndoc-document-attributes">
			<xsl:if test="ndoc:attribute">
				<xsl:for-each select="ndoc:attribute">
					<div class="attribute">
						<xsl:call-template name="vb-attribute">
							<xsl:with-param name="attname" select="@name" />
						</xsl:call-template>
					</div>
				</xsl:for-each>
			</xsl:if>
		</xsl:if>
	</xsl:template>
	<!-- Individual attribute -->
	<xsl:template name="vb-attribute">
		<xsl:param name="attname" />
		<xsl:text>&lt;</xsl:text>
		<xsl:if test="@target">
			<xsl:value-of select="@target" /> :
		</xsl:if>
		<xsl:call-template name="strip-namespace-and-attribute">
			<xsl:with-param name="name" select="@name" />
		</xsl:call-template>
		<xsl:if test="count(ndoc:property | ndoc:field) > 0">
			<xsl:text>(</xsl:text>
			<xsl:for-each select="ndoc:property | ndoc:field">
				<xsl:value-of select="@name" />
				<xsl:text>:=</xsl:text>
				<xsl:choose>
					<xsl:when test="@value">
						<xsl:if test="@type='System.String'">
							<xsl:text>"</xsl:text>
						</xsl:if>
						<xsl:choose>
							<xsl:when test="@type!='System.String'">
								<xsl:value-of select="NUtil:Replace(@value,'|',' Or ')" />
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="@value" />
							</xsl:otherwise>
						</xsl:choose>
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
		<xsl:text>&gt; _</xsl:text>
	</xsl:template>
	<!-- Get display name VB syntax  -->
	<xsl:template name="get-displayname-vb">
		<xsl:param name="node" select="."/>
		<xsl:param name="onlyWriteGenericLinks" select="false()"/>
		<xsl:choose>
			<xsl:when test="$onlyWriteGenericLinks = 'true'">
				<xsl:call-template name="write-type-link-vb">
					<xsl:with-param name="node" select="$node" />
					<xsl:with-param name="writelinks" select="false()"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="write-type-link-vb">
					<xsl:with-param name="node" select="$node" />
					<xsl:with-param name="writelinks" select="true()"/>
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:if test="$node/ndoc:genericargument">
			<xsl:text>(Of&#160;</xsl:text>
		</xsl:if>
		<xsl:for-each select="$node/ndoc:genericargument">
			<xsl:call-template name="get-genericarguments-vb" />
			<xsl:call-template name="get-genericconstraint-vb" />
			<xsl:if test="position()!=last()">
				<xsl:text>, </xsl:text>
			</xsl:if>
		</xsl:for-each>
		<xsl:if test="$node/ndoc:genericargument">
			<xsl:text>)</xsl:text>
		</xsl:if>
	</xsl:template>
	<!-- Generic parameters and arguments -->
	<xsl:template name="get-genericarguments-vb">
		<xsl:param name="node" select="." />
		<xsl:call-template name="write-type-link-vb">
			<xsl:with-param name="node" select="$node" />
		</xsl:call-template>
		<xsl:if test="$node/ndoc:genericargument">
			<xsl:text>(Of&#160;</xsl:text>
		</xsl:if>
		<xsl:for-each select="$node/ndoc:genericargument">
			<xsl:call-template name="get-genericarguments-vb" />
			<xsl:call-template name="get-genericconstraint-vb" />
			<xsl:if test="position()!=last()">
				<xsl:text>, </xsl:text>
			</xsl:if>
		</xsl:for-each>
		<xsl:if test="$node/ndoc:genericargument">
			<xsl:text>)</xsl:text>
		</xsl:if>
	</xsl:template>
	<!-- Get generic constraints -->
	<xsl:template name="get-genericconstraint-vb">
		<xsl:param name="node" select="." />
		<xsl:variable name="name" select="$node/@name" />
		<xsl:if test="$node/../ndoc:genericconstraints[@param=$name]">
			<xsl:text>&#160;As&#160;</xsl:text>
		</xsl:if>
		<xsl:if test="count($node/../ndoc:genericconstraints[@param=$name]/ndoc:constraint) > 1">
			<xsl:text>{</xsl:text>
		</xsl:if>
		<xsl:for-each select="$node/../ndoc:genericconstraints[@param=$name]/ndoc:constraint">
			<xsl:call-template name="write-type-link-vb" />
			<xsl:if test="position()!=last()">
				<xsl:text>, </xsl:text>
			</xsl:if>
		</xsl:for-each>
		<xsl:if test="count($node/../ndoc:genericconstraints[@param=$name]/ndoc:constraint) > 1">
			<xsl:text>}</xsl:text>
		</xsl:if>
	</xsl:template>
	<!-- Write link to type -->
	<xsl:template name="write-type-link-vb">
		<xsl:param name="node" select="."/>
		<xsl:param name="writelinks" select="true()"/>
		<xsl:if test="$node/@nullable = 'true'">
			<xsl:text>Nullable(Of&#160;</xsl:text>
		</xsl:if>
		<!-- Handle both types with ID attribute and those without fx. genericarguments -->
		<xsl:choose>
			<xsl:when test="(contains($node/@id, '.') or contains($node/@typeId, '.') or contains($node, '.') or (contains($node/@name, '.') and local-name($node) = 'genericargument')) and $writelinks = 'true'">
				<a>
					<xsl:choose>
						<!-- Handle the speciel case of fields, events, properties and parameters
            which uses typeId instead of id attribute -->
						<xsl:when test="local-name($node) = 'field' or local-name($node) = 'event'
                      or local-name($node) = 'property' or local-name($node) = 'parameter'">
							<xsl:attribute name="href">
								<xsl:call-template name="get-filename-for-type-name">
									<xsl:with-param name="type-name" select="substring-after($node/@typeId, ':')" />
								</xsl:call-template>
							</xsl:attribute>
							<xsl:call-template name="vb-type">
								<xsl:with-param name="runtime-type" select="substring-after($node/@typeId, ':')" />
							</xsl:call-template>
						</xsl:when>
						<!-- Handle special generic constraint case -->
						<xsl:when test="local-name($node) = 'constraint'">
							<xsl:attribute name="href">
								<xsl:call-template name="get-filename-for-type-name">
									<xsl:with-param name="type-name" select="." />
								</xsl:call-template>
							</xsl:attribute>
							<xsl:call-template name="vb-type">
								<xsl:with-param name="runtime-type" select="." />
							</xsl:call-template>
						</xsl:when>
						<xsl:when test="local-name($node) = 'genericargument'">
							<xsl:attribute name="href">
								<xsl:call-template name="get-filename-for-type-name">
									<xsl:with-param name="type-name" select="$node/@name" />
								</xsl:call-template>
							</xsl:attribute>
							<xsl:call-template name="vb-type">
								<xsl:with-param name="runtime-type" select="$node/@name" />
							</xsl:call-template>
						</xsl:when>
						<xsl:otherwise>
							<xsl:attribute name="href">
								<xsl:call-template name="get-filename-for-type-name">
									<xsl:with-param name="type-name" select="substring-after($node/@id, ':')" />
								</xsl:call-template>
							</xsl:attribute>
							<xsl:call-template name="vb-type">
								<xsl:with-param name="runtime-type" select="substring-after($node/@id, ':')" />
							</xsl:call-template>
						</xsl:otherwise>
					</xsl:choose>
				</a>
			</xsl:when>
			<xsl:when test="local-name($node) = 'constraint'">
				<xsl:choose>
					<xsl:when test="$node/text() = 'struct'">
						<xsl:text>Structure</xsl:text>
					</xsl:when>
					<xsl:when test="$node/text() = 'class'">
						<xsl:text>Class</xsl:text>
					</xsl:when>
					<xsl:when test="$node/text() = 'new'">
						<xsl:text>New</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="vb-type">
							<xsl:with-param name="runtime-type" select="." />
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:when test="local-name($node) = 'genericargument'">
				<xsl:call-template name="vb-type">
					<xsl:with-param name="runtime-type" select="$node/@name" />
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$node/@typeId != ''">
				<xsl:call-template name="vb-type">
					<xsl:with-param name="runtime-type" select="substring-after($node/@typeId, ':')" />
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$node/@id != ''">
				<xsl:call-template name="vb-type">
					<xsl:with-param name="runtime-type" select="substring-after($node/@id, ':')" />
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="vb-type">
					<xsl:with-param name="runtime-type" select="$node/@name" />
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:if test="$node/@nullable = 'true'">
			<xsl:text>)</xsl:text>
		</xsl:if>
	</xsl:template>
</xsl:transform>
