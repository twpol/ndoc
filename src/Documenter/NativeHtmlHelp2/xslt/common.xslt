<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:NUtil="urn:ndoc-sourceforge-net:documenters.NativeHtmlHelp2.xsltUtilities"
	xmlns:NHtmlProvider="urn:NDocExternalHtml"
	xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
	exclude-result-prefixes="NUtil NHtmlProvider" >
	<xsl:include href="tags.xslt" />
	<xsl:include href="indices.xslt" />
	<xsl:include href="XLinks.xslt" />
	<!-- -->
	<xsl:param name="ndoc-title" />
	<xsl:param name="ndoc-net-framework-version"/>
	<xsl:param name="ndoc-version"/>
	<xsl:param name="ndoc-omit-syntax"/>
	<!-- -->
	<!--
	 | no-op extensibility templates
	 +-->
	<xsl:template match="node()|@*|text()" mode="preliminary-section"/>
	<xsl:template match="node()|@*|text()" mode="summary-section" />
	<xsl:template match="node()|@*|text()" mode="thread-safety-section" />
	<xsl:template match="node()|@*|text()" mode="syntax-section"/>
	<xsl:template match="node()|@*|text()" mode="value-section"/>
	<xsl:template match="node()|@*|text()" mode="parameter-section" />
	<xsl:template match="node()|@*|text()" mode="returnvalue-section" />
	<xsl:template match="node()|@*|text()" mode="implements-section" />
	<xsl:template match="node()|@*|text()" mode="remarks-section" />
	<xsl:template match="node()|@*|text()" mode="obsolete-section" />
	<xsl:template match="node()|@*|text()" mode="events-section" />
	<xsl:template match="node()|@*|text()" mode="exceptions-section" />
	<xsl:template match="node()|@*|text()" mode="example-section" />
	<xsl:template match="node()|@*|text()" mode="member-requirements-section" />
	<xsl:template match="node()|@*|text()" mode="type-requirements-section" />
	<xsl:template match="node()|@*|text()" mode="seealso-section"/>
	<xsl:template match="node()|@*|text()" mode="enumeration-members-section"/>
	<xsl:template match="node()|@*|text()" mode="footer-row"/>
	<xsl:template match="node()|@*|text()" mode="title-row"/>
	<xsl:template match="node()|@*|text()" mode="overloads-remarks-section"/>
	<xsl:template match="node()|@*|text()" mode="overloads-example-section"/>
	<xsl:template match="node()|@*|text()" mode="overloads-summary-section"/>
	
	<!-- -->
	
	<xsl:template name="parameter-topic">
		<dl>
		<xsl:for-each select="parameter">
			<xsl:variable name="name" select="@name" />
			<dt>
				<i>
					<xsl:value-of select="@name" />
				</i>
			</dt>
			<dd>
				<xsl:apply-templates select="parent::node()/documentation/param[@name=$name]/node()" mode="slashdoc" />
			</dd>
		</xsl:for-each>
		</dl>
	</xsl:template>
	<!-- -->
	<xsl:template name="type-mixed">
		<xsl:choose>
			<xsl:when test="local-name()='constructor' or local-name()='property' or local-name()='field' or local-name()='method' or local-name()='event' or local-name()='operator'">
				<xsl:choose>
					<xsl:when test="local-name(..)='interface'">Interface</xsl:when>
					<xsl:when test="local-name(..)='structure'">Structure</xsl:when>
					<xsl:otherwise>Class</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:choose>
					<xsl:when test="local-name()='interface'">Interface</xsl:when>
					<xsl:when test="local-name()='structure'">Structure</xsl:when>
					<xsl:otherwise>Class</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template name="type-element">
		<xsl:choose>
			<xsl:when test="local-name()='constructor' or local-name()='field' or local-name()='property' or local-name()='method' or local-name()='event' or local-name()='operator'">
				<xsl:value-of select="local-name(..)" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="local-name()" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template name="type-name">
		<xsl:choose>
			<xsl:when test="local-name()='constructor' or local-name()='field' or local-name()='property' or local-name()='method' or local-name()='event' or local-name()='operator'">
				<xsl:value-of select="../@name" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="@name" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template name="type-id">
		<xsl:choose>
			<xsl:when test="local-name()='constructor' or local-name()='field' or local-name()='property' or local-name()='method' or local-name()='event' or local-name()='operator'">
				<xsl:value-of select="../@id" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="@id" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template name="namespace-name">
		<xsl:choose>
			<xsl:when test="local-name()='constructor' or local-name()='field' or local-name()='property' or local-name()='method' or local-name()='event' or local-name()='operator'">
				<xsl:value-of select="../../@name" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="../@name" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template name="seealso-section">
		<xsl:param name="page" />
		<xsl:variable name="typeMixed">
			<xsl:call-template name="type-mixed" />
		</xsl:variable>
		<xsl:variable name="typeElement">
			<xsl:call-template name="type-element" />
		</xsl:variable>
		<xsl:variable name="typeName">
			<xsl:call-template name="type-name" />
		</xsl:variable>
		<xsl:variable name="typeID">
			<xsl:call-template name="type-id" />
		</xsl:variable>
		<xsl:variable name="namespaceName">
			<xsl:call-template name="namespace-name" />
		</xsl:variable>
		<h4 class="dtH4">See Also</h4>
		<p>
			<xsl:if test="$page!='type' and $page!='enumeration' and $page!='delegate'">
				<a href="{NUtil:GetLocalCRef( string( $typeID ) ) }">
					<xsl:value-of select="concat($typeName, ' ', $typeMixed)" />
				</a>
				<xsl:text> | </xsl:text>
			</xsl:if>
			<xsl:if test="(constructor|field|property|method|operator|event) and $page!='members' and $page!='enumeration' and $page!='delegate' and $page!='methods' and $page!='properties' and $page!='fields' and $page!='events'">
				<a href="{NUtil:GetOverviewHRef( string( $typeID ), 'Members' )}">
					<xsl:value-of select="$typeName" />
					<xsl:text> Members</xsl:text>
				</a>
				<xsl:text> | </xsl:text>
			</xsl:if>
			<a href="{NUtil:GetNamespaceHRef( string( $namespaceName ) )}">
				<xsl:value-of select="$namespaceName" />
				<xsl:text> Namespace</xsl:text>
			</a>
			<xsl:if test="$page='member' or $page='property'">
				<xsl:variable name="memberName" select="@name" />
				<xsl:if test="count(parent::node()/*[@name=$memberName]) &gt; 1">
					<xsl:choose>
						<xsl:when test="local-name()='constructor'">
 							<xsl:text> | </xsl:text>
							<a href="{NUtil:GetOverviewHRef( string( ../@id ), 'Constructor' )}">
								<xsl:value-of select="$typeName" />
								<xsl:text> Constructor Overload List</xsl:text>
							</a>
						</xsl:when>
						<xsl:when test="local-name()='operator'">
							<xsl:if test="@name!='op_Implicit' and @name!='op_Explicit'">
   							<xsl:text> | </xsl:text>
							<a href="{NUtil:GetMemberOverloadsHRef( string( ../@id ), string( @name ) )}">
									<xsl:value-of select="$typeName" />
									<xsl:text> </xsl:text>
									<xsl:call-template name="operator-name">
										<xsl:with-param name="name"><xsl:value-of select="@name" /></xsl:with-param>
									</xsl:call-template>
									<xsl:text> Overload List</xsl:text>
								</a>
						  </xsl:if>
						</xsl:when>

						<xsl:when test="local-name()='property'">
	 						<xsl:text> | </xsl:text>
							<a href="{NUtil:GetMemberOverloadsHRef( string( ../@id ), string( @name ) )}">
								<xsl:value-of select="concat($typeName, '.', @name)" />
								<xsl:text> Overload List</xsl:text>
							</a>
						</xsl:when>
						<xsl:when test="local-name()='method'">
	 						<xsl:text> | </xsl:text>
							<a href="{NUtil:GetMemberOverloadsHRef( string( ../@id ), string( @name ) )}">
								<xsl:value-of select="concat($typeName, '.', @name)" />
								<xsl:text> Overload List</xsl:text>
							</a>
						</xsl:when>
					</xsl:choose>
				</xsl:if>
			</xsl:if>
			<xsl:if test="documentation//seealso">
				<xsl:for-each select="documentation//seealso">
					<xsl:text> | </xsl:text>
					<xsl:choose>
						<xsl:when test="@cref">
							<xsl:call-template name="get-a-href">
								<xsl:with-param name="cref" select="@cref"/>
							</xsl:call-template>
						</xsl:when>
						<xsl:when test="@href">
							<a href="{@href}">
								<xsl:value-of select="." />
							</a>
						</xsl:when>
					</xsl:choose>
				</xsl:for-each>
			</xsl:if>
			<xsl:apply-templates select="documentation/node()" mode="seealso-section"/>
		</p>
	</xsl:template>
	<!-- -->
	<xsl:template name="output-paragraph">
		<xsl:param name="nodes" />
		<xsl:choose>
			<xsl:when test="not($nodes/self::para | $nodes/self::p)">
				<p>
					<xsl:apply-templates select="$nodes" mode="slashdoc" />
				</p>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="$nodes" mode="slashdoc" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template match="para | p" mode="no-para">
		<xsl:apply-templates mode="slashdoc" />
	</xsl:template>
	<!-- -->
	<xsl:template match="note" mode="no-para">
		<blockquote class="dtBlock">
			<b>Note</b>
			<xsl:text>&#160;&#160;&#160;</xsl:text>
			<xsl:apply-templates mode="slashdoc" />
		</blockquote>
	</xsl:template>
	<!-- -->
	<xsl:template match="node()" mode="no-para">
		<xsl:apply-templates select="." mode="slashdoc" />
	</xsl:template>
	<!-- -->
	<xsl:template name="obsolete-section">
		<xsl:if test="./obsolete">
			<P><FONT color="red"><B>NOTE: This <xsl:value-of select="name()"/> is now obsolete.</B></FONT></P>
			<xsl:if test="./obsolete!=''">
				<P><B><xsl:value-of select="obsolete"/></B></P>
			</xsl:if>
			<xsl:if test="./documentation/obsolete!=''">
				<xsl:call-template name="output-paragraph">
					<xsl:with-param name="nodes" select="(documentation/obsolete)[1]/node()" />
				</xsl:call-template>
			</xsl:if>
			<xsl:apply-templates select="documentation/node()" mode="obsolete-section"/>
			<HR/>
		</xsl:if>
	</xsl:template>
	<xsl:template name="obsolete-inline">
		<xsl:if test="./obsolete">
			<FONT color="red"><B>Obsolete. </B></FONT>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="summary-section">
		<div id="allHistory" class="saveHistory" onsave="saveAll()" onload="loadAll()"></div>
		<xsl:call-template name="obsolete-section"/>
		<xsl:call-template name="output-paragraph">
			<xsl:with-param name="nodes" select="(documentation/summary)[1]/node()" />
		</xsl:call-template>
		<xsl:apply-templates select="documentation/node()" mode="summary-section"/>
	</xsl:template>
	<!-- -->
	<xsl:template name="summary-with-no-paragraph">
		<xsl:param name="member" select="." />
		<xsl:apply-templates select="($member/documentation/summary)[1]/node()" mode="no-para" />
		<xsl:if test="not(($member/documentation/summary)[1]/node())">&#160;</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="overloads-summary-section">
		<xsl:variable name="memberName" select="@name" />
		<div id="allHistory" class="saveHistory" onsave="saveAll()" onload="loadAll()"></div>
		<xsl:choose>
			<xsl:when test="parent::node()/*[@name=$memberName]/documentation/overloads/summary">
				<xsl:call-template name="output-paragraph">
					<xsl:with-param name="nodes" select="(parent::node()/*[@name=$memberName]/documentation/overloads/summary)[1]/node()" />
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="parent::node()/*[@name=$memberName]/documentation/overloads">
				<xsl:call-template name="output-paragraph">
					<xsl:with-param name="nodes" select="(parent::node()/*[@name=$memberName]/documentation/overloads)[1]/node()" />
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="summary-section" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template name="overloads-summary-with-no-paragraph">
		<xsl:param name="overloads" select="." />
		<xsl:variable name="memberName" select="@name" />
		<xsl:choose>
			<xsl:when test="$overloads/../*[@name=$memberName]/documentation/overloads/summary">
				<xsl:apply-templates select="($overloads/../*[@name=$memberName]/documentation/overloads/summary)[1]/node()" mode="no-para" />
			</xsl:when>
			<xsl:when test="$overloads/../*[@name=$memberName]/documentation/overloads">
				<xsl:apply-templates select="($overloads/../*[@name=$memberName]/documentation/overloads)[1]/node()" mode="no-para" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="summary-with-no-paragraph">
					<xsl:with-param name="member" select="$overloads" />
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template name="overloads-remarks-section">
		<xsl:variable name="memberName" select="@name" />
		<xsl:if test="parent::node()/*[@name=$memberName]/documentation/overloads/remarks">
			<h4 class="dtH4">Remarks</h4>
			<p>
				<xsl:call-template name="output-paragraph">
					<xsl:with-param name="nodes" select="(parent::node()/*[@name=$memberName]/documentation/overloads/remarks)[1]/node()" />
				</xsl:call-template>
			</p>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="overloads-example-section">
		<xsl:variable name="memberName" select="@name" />
		<xsl:if test="parent::node()/*[@name=$memberName]/documentation/overloads/example">
			<h4 class="dtH4">Example</h4>
			<p>
				<xsl:call-template name="output-paragraph">
					<xsl:with-param name="nodes" select="(parent::node()/*[@name=$memberName]/documentation/overloads/example)[1]/node()" />
				</xsl:call-template>
			</p>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="parameter-section">
		<xsl:choose>
			<xsl:when test="substring(@name,1,3)='op_'">
				<xsl:if test="documentation/param">
					<H4 class="dtH4">Arguments <SPAN class="lang">[Visual&#160;Basic,&#160;JScript]</SPAN> </H4>
					<xsl:call-template name="parameter-topic" />
				</xsl:if>
				<xsl:if test="documentation/param">
					<H4 class="dtH4">Parameters <SPAN class="lang">[C#,&#160;C++]</SPAN> </H4>
					<xsl:call-template name="parameter-topic" />
				</xsl:if>
			</xsl:when>
			<xsl:when test="documentation/param">
				<h4 class="dtH4">Parameters</h4>
				<xsl:call-template name="parameter-topic" />
			</xsl:when>
		</xsl:choose>
		<xsl:apply-templates select="documentation/node()" mode="parameter-section"/>			
	</xsl:template>
	<!-- -->
	<xsl:template name="returnvalue-section">
		<xsl:if test="documentation/returns">
			<h4 class="dtH4">Return Value</h4>
			<p>
				<xsl:apply-templates select="documentation/returns/node()" mode="slashdoc" />
			</p>
			<xsl:apply-templates select="documentation/node()" mode="returnvalue-section"/>			
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="implements-section">
		<xsl:if test="implements">
			<xsl:variable name="member" select="local-name()" />
			<h4 class="dtH4">Implements</h4>
			<xsl:for-each select="implements">
				<xsl:call-template name="get-link-for-implements-member">
					<xsl:with-param name="id" select="@id" />
					<xsl:with-param name="link-text" select="concat( @interface, '.', @name )"/>	
				</xsl:call-template>
			</xsl:for-each>
			<xsl:apply-templates select="documentation/node()" mode="implements-section"/>			
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="remarks-section">
		<xsl:if test="documentation/remarks">
			<h4 class="dtH4">Remarks</h4>
			<xsl:variable name="first-element" select="local-name(documentation/remarks/*[1])" />
			<xsl:choose>
				<xsl:when test="$first-element!='para' and $first-element!='p'">
					<p>
						<xsl:apply-templates select="documentation/remarks/node()" mode="slashdoc" />
					</p>
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates select="documentation/remarks/node()" mode="slashdoc" />
				</xsl:otherwise>
			</xsl:choose>
			<xsl:apply-templates select="documentation/node()" mode="remarks-section"/>			
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="value-section">
		<xsl:if test="documentation/value">
			<h4 class="dtH4">Property Value</h4>
			<p>
				<xsl:apply-templates select="documentation/value/node()" mode="slashdoc" />
			</p>
			<xsl:apply-templates select="documentation/node()" mode="value-section"/>			
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="events-section">
		<xsl:if test="documentation/event">
			<h4 class="dtH4">Events</h4>
			<div class="tablediv">
				<table class="dtTABLE" cellspacing="0">
					<tr valign="top">
						<th width="50%">Event Type</th>
						<th width="50%">Reason</th>
					</tr>
					<xsl:for-each select="documentation/event">
						<xsl:sort select="@name" />
						<tr valign="top">
							<td width="50%">
								<xsl:call-template name="get-a-href">
									<xsl:with-param name="cref" select="@cref" />
								</xsl:call-template>
							</td>
							<td width="50%">
								<xsl:apply-templates select="./node()" mode="slashdoc" />
								<xsl:if test="not(./node())">&#160;</xsl:if>
							</td>
						</tr>
					</xsl:for-each>
				</table>
			</div>
			<xsl:apply-templates select="documentation/node()" mode="events-section"/>			
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="exceptions-section">
		<xsl:if test="documentation/exception">
			<h4 class="dtH4">Exceptions</h4>
			<div class="tablediv">
				<table class="dtTABLE" cellspacing="0">
					<tr valign="top">
						<th width="50%">Exception Type</th>
						<th width="50%">Condition</th>
					</tr>
					<xsl:for-each select="documentation/exception">
						<xsl:sort select="@name" />
						<tr valign="top">
							<td width="50%">
								<xsl:call-template name="get-a-href">
									<xsl:with-param name="cref" select="@cref" />
									<xsl:with-param name="ignore-text" select="true()" />									
								</xsl:call-template>
							</td>
							<td width="50%">
								<xsl:apply-templates select="./node()" mode="slashdoc" />
								<xsl:if test="not(./node())">&#160;</xsl:if>
							</td>
						</tr>
					</xsl:for-each>
				</table>
			</div>
			<xsl:apply-templates select="documentation/node()" mode="exceptions-section"/>			
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="example-section">
		<xsl:if test="documentation/example">
			<h4 class="dtH4">Example</h4>
			<p>
				<xsl:apply-templates select="documentation/example/node()" mode="slashdoc" />
			</p>
			<xsl:apply-templates select="documentation/node()" mode="example-section"/>			
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="thread-safety-section">
		<H4 class="dtH4">Thread Safety</H4>
		<xsl:choose>
			<xsl:when test="documentation/threadsafety[@static='true'][@instance='true']">
				<P>This type is safe for multithreaded operations.</P>
			</xsl:when>
			<xsl:when test="documentation/threadsafety[@static='false'][@instance='false']">
				<P>This type is <b>not</b> safe for multithreaded operations.</P>
			</xsl:when>
			<xsl:when test="documentation/threadsafety[@static='false'][@instance='true']">
				<!-- not sure this makes sense but... -->
				<P>Public static (Shared in Visual Basic) members of this type are not guaranteed 
				to be safe for multithreaded operations. Instance members are not guaranteed to be 
				thread-safe.</P>
			</xsl:when>
			<xsl:otherwise><!-- static=true instance=false-->
				<P>Public static (Shared in Visual Basic) members of this type are safe 
				for multithreaded operations. Instance members are not guaranteed to be 
				thread-safe.</P>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:apply-templates select="documentation/threadsafety/node()" mode="slashdoc" />
		<xsl:apply-templates select="documentation/node()" mode="thread-safety-section"/>			
	</xsl:template>	
	<!-- -->
	<xsl:template name="enumeration-members-section">
		<xsl:if test="field">
			<h4 class="dtH4">Members</h4>
			<div class="tablediv">
				<table class="dtTABLE" cellspacing="0">
					<tr valign="top">
						<th width="50%">Member Name</th>
						<th width="50%">Description</th>
					</tr>
					<xsl:for-each select="field">
						<xsl:text>&#10;</xsl:text>
						<tr valign="top">
							<td width="50%">
								<b>
									<xsl:value-of select="@name" />
								</b>
							</td>
							<td width="50%">
								<xsl:apply-templates select="documentation/summary/node()" mode="slashdoc" />
								<xsl:if test="not(documentation/summary/node())">&#160;</xsl:if>
							</td>
						</tr>
					</xsl:for-each>
				</table>
			</div>
			<xsl:apply-templates select="documentation/node()" mode="enumeration-members-section"/>						
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-lang">
		<xsl:param name="lang" />
		<xsl:choose>
			<xsl:when test="$lang = 'VB' or $lang='Visual Basic'">
				<xsl:value-of select="'Visual&#160;Basic'" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$lang" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- get-a-href -->
	<xsl:template name="get-a-href">
		<xsl:param name="cref" />
		<xsl:param name="ignore-text"/>
		
		<xsl:variable name="filename">
			<xsl:value-of select="NUtil:GetLocalCRef( $cref )"/>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="$filename=''">
				<xsl:call-template name="get-xlink">
					<xsl:with-param name="a-index" select="NUtil:GetAIndex( $cref )"/>
					<xsl:with-param name="link-text" select="string(NUtil:GetName($cref))"/>	
					<xsl:with-param name="ns-key" select="substring-after( $cref, ':' )"/>										
				</xsl:call-template>	
			</xsl:when>
			<xsl:otherwise>
				<a>
					<xsl:attribute name="href"><xsl:value-of select="$filename"/></xsl:attribute> 			
					<xsl:choose>
						<xsl:when test="node() and $ignore-text!=true()">
							<xsl:value-of select="." />
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="NUtil:GetName($cref)" />
						</xsl:otherwise>
					</xsl:choose>
				</a>				
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->

	<xsl:template name="html-head">
		<xsl:param name="title" />
		<xsl:param name="page-type"/>
		<xsl:param name="overload-page"/>
		<head>
			<title>
				<xsl:value-of select="$title" />
			</title>
			<xml>
				<xsl:apply-templates select="." mode="MSHelpTitle">
					<xsl:with-param name="title" select="$title"/>
					<xsl:with-param name="page-type" select="$page-type"/>						
				</xsl:apply-templates>
				
				<xsl:choose>
					<xsl:when test="$page-type!='hierarchy'">
						<xsl:apply-templates select="." mode="KIndex">
							<xsl:with-param name="title" select="$title"/>
							<xsl:with-param name="page-type" select="$page-type"/>		
							<xsl:with-param name="overload-page" select="$overload-page"/>				
						</xsl:apply-templates>
						
						<xsl:apply-templates select="." mode="FIndex">
							<xsl:with-param name="title" select="$title"/>
							<xsl:with-param name="page-type" select="$page-type"/>
							<xsl:with-param name="overload-page" select="$overload-page"/>
						</xsl:apply-templates>
						
						<xsl:apply-templates select="." mode="AIndex">
							<xsl:with-param name="page-type" select="$page-type"/>
							<xsl:with-param name="overload-page" select="$overload-page"/>				
						</xsl:apply-templates>
					</xsl:when>
					
					<xsl:otherwise>
						<xsl:apply-templates select="ndoc" mode="AIndex-hierarchy"/>		
					</xsl:otherwise>
				</xsl:choose>
				
				<!--MSHelp:Attr Name="DocSet" Value="NETFramework"/-->
				<xsl:for-each select="/ndoc/docSet">
					<MSHelp:Attr Name="DocSet" Value="{text()}"/>					
				</xsl:for-each>
				<MSHelp:Attr Name="TopicType" Value="kbSyntax"/>
				<MSHelp:Attr Name="DevLang" Value="CSharp"/>
				<MSHelp:Attr Name="DevLang" Value="VB"/>
				<MSHelp:Attr Name="DevLang" Value="C++"/>
				<MSHelp:Attr Name="DevLang" Value="JScript"/>
				<MSHelp:Attr Name="DevLang" Value="VJ#"/>
				<MSHelp:Attr Name="Technology" Value="WFC"/>
				<MSHelp:Attr Name="Technology" Value="ManagedC"/>
				<MSHelp:Attr Name="TechnologyVers" Value="kbWFC"/>
				<MSHelp:Attr Name="TechnologyVers" Value="kbManagedC"/>
				<MSHelp:Attr Name="Locale" Value="kbEnglish"/>
				<!-- Microsoft reserves priority of 1 and 2 for framework types -->
				<MSHelp:Attr Name="HelpPriority" Value="3"/>	
			</xml>
			<SCRIPT SRC="dtuelink.js"></SCRIPT>
		</head>
	</xsl:template>
	
	<!-- -->
	<xsl:template name="title-row">
		<xsl:param name="type-name" />
		<div id="nsbanner">		
			<xsl:variable name="headerHtml" select="NHtmlProvider:GetHeaderHtml(string($type-name))" />			
			<xsl:choose>
				<xsl:when test="$headerHtml=''">
					<div id="bannerrow1">
						<table class="bannerparthead" cellspacing="0"><tr id="hdr">
							<td class="runninghead" nowrap="true">
								<xsl:value-of select="$ndoc-title" />
							</td>
							<td class="product" nowrap="true">
							</td>
						</tr></table>
					</div>
					<div id="TitleRow">
						<h1 class="dtH1">
							<xsl:value-of select="$type-name" />
						</h1>
					</div>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$headerHtml" disable-output-escaping="yes" />
				</xsl:otherwise>
			</xsl:choose>
			<xsl:if test="ancestor-or-self::node()/documentation/preliminary | /ndoc/preliminary">
				<xsl:call-template name="preliminary-section"/>
			</xsl:if>					
			<xsl:apply-templates select="documentation/node()" mode="title-row"/>
		</div>
	</xsl:template>
	<!-- -->
	<xsl:template name="footer-row">
		<xsl:param name="type-name" />
		
		<xsl:call-template name="version-section" />		
				
		<xsl:variable name="assembly-name">
			<xsl:value-of select="ancestor-or-self::assembly/./@name" />
		</xsl:variable>
		<xsl:variable name="assembly-version">
			<xsl:value-of select="ancestor-or-self::assembly/./@version" />
		</xsl:variable>
		<xsl:variable name="footerHtml" select="NHtmlProvider:GetFooterHtml(string($assembly-name), string($assembly-version), string($type-name))" />
		<xsl:variable name="copyright-rtf">
			<xsl:call-template name="copyright-notice" />
		</xsl:variable>
		<xsl:variable name="version-rtf">
			<xsl:call-template name="generated-from-assembly-version" />
		</xsl:variable>
		<xsl:if test="string($copyright-rtf) or string($version-rtf) or string($footerHtml) or /ndoc/feedbackEmail">
			<hr />
			<div id="footer">
				<xsl:apply-templates select="/ndoc/feedbackEmail">
					<xsl:with-param name="page" select="$type-name"/>
				</xsl:apply-templates>
				<xsl:choose>
					<xsl:when test="$footerHtml=''">
						<p>
							<xsl:copy-of select="$copyright-rtf" />
						</p>
						<p>
							<xsl:copy-of select="$version-rtf" />
						</p>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="$footerHtml" disable-output-escaping="yes" />
					</xsl:otherwise>
				</xsl:choose>
				<xsl:apply-templates select="documentation/node()" mode="footer-row"/>	
			</div>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="preliminary-section">
		<p class="topicstatus">
			<!-- use the appropriate custom text if present, otherwise just the feault -->
			<xsl:choose>
				<xsl:when test="documentation/preliminary[text()]">
					<xsl:value-of select="documentation/preliminary"/>
				</xsl:when>
				<xsl:when test="ancestor::node()/documentation/preliminary[text()]">
					<xsl:value-of select="ancestor::node()/documentation/preliminary"/>				
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>[This is preliminary documentation and subject to change.]</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</p>
		<xsl:apply-templates select="documentation/node()" mode="preliminary-section"/>			
	</xsl:template>
	<!-- -->	
	<xsl:template name="copyright-notice">
		<xsl:variable name="copyright-text">
			<xsl:value-of select="/ndoc/copyright/@text" />
		</xsl:variable>
		<xsl:variable name="copyright-href">
			<xsl:value-of select="/ndoc/copyright/@href" />
		</xsl:variable>
		<xsl:if test="$copyright-text != ''">
			<a>
				<xsl:if test="$copyright-href != ''">
					<xsl:attribute name="href">
						<xsl:value-of select="$copyright-href" />
					</xsl:attribute>
				</xsl:if>
				<xsl:value-of select="/ndoc/copyright/@text" />
			</a>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="generated-from-assembly-version">
		<xsl:variable name="assembly-name">
			<xsl:value-of select="ancestor-or-self::assembly/./@name" />
		</xsl:variable>
		<xsl:variable name="assembly-version">
			<xsl:value-of select="ancestor-or-self::assembly/./@version" />
		</xsl:variable>
		<xsl:if test="$assembly-version != ''">
			<xsl:text>Generated from assembly </xsl:text>
			<xsl:value-of select="$assembly-name" />
			<xsl:text> [</xsl:text>
			<xsl:value-of select="$assembly-version" />
			<xsl:text>]</xsl:text>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="operator-name">
		<xsl:param name="name" />
		<xsl:param name="from" />
		<xsl:param name="to" />
		<xsl:choose>
			<xsl:when test="$name='op_UnaryPlus'">Unary Plus Operator</xsl:when>
			<xsl:when test="$name='op_UnaryNegation'">Unary Negation Operator</xsl:when>
			<xsl:when test="$name='op_LogicalNot'">Logical Not Operator</xsl:when>
			<xsl:when test="$name='op_OnesComplement'">Ones Complement Operator</xsl:when>
			<xsl:when test="$name='op_Increment'">Increment Operator</xsl:when>
			<xsl:when test="$name='op_Decrement'">Decrement Operator</xsl:when>
			<xsl:when test="$name='op_True'">True Operator</xsl:when>
			<xsl:when test="$name='op_False'">False Operator</xsl:when>
			<xsl:when test="$name='op_Addition'">Addition Operator</xsl:when>
			<xsl:when test="$name='op_Subtraction'">Subtraction Operator</xsl:when>
			<xsl:when test="$name='op_Multiply'">Multiplication Operator</xsl:when>
			<xsl:when test="$name='op_Division'">Division Operator</xsl:when>
			<xsl:when test="$name='op_Modulus'">Modulus Operator</xsl:when>
			<xsl:when test="$name='op_BitwiseAnd'">Bitwise And Operator</xsl:when>
			<xsl:when test="$name='op_BitwiseOr'">Bitwise Or Operator</xsl:when>
			<xsl:when test="$name='op_ExclusiveOr'">Exclusive Or Operator</xsl:when>
			<xsl:when test="$name='op_LeftShift'">Left Shift Operator</xsl:when>
			<xsl:when test="$name='op_RightShift'">Right Shift Operator</xsl:when>
			<xsl:when test="$name='op_Equality'">Equality Operator</xsl:when>
			<xsl:when test="$name='op_Inequality'">Inequality Operator</xsl:when>
			<xsl:when test="$name='op_LessThan'">Less Than Operator</xsl:when>
			<xsl:when test="$name='op_GreaterThan'">Greater Than Operator</xsl:when>
			<xsl:when test="$name='op_LessThanOrEqual'">Less Than Or Equal Operator</xsl:when>
			<xsl:when test="$name='op_GreaterThanOrEqual'">Greater Than Or Equal Operator</xsl:when>
			<xsl:when test="$name='op_Implicit' or $name='op_Explicit'">
				<xsl:call-template name="strip-namespace">
					<xsl:with-param name="name" select="$from" />
				</xsl:call-template>
				<xsl:text> to </xsl:text>
				<xsl:call-template name="strip-namespace">
					<xsl:with-param name="name" select="$to" />
				</xsl:call-template>
				<xsl:text> Conversion</xsl:text>
			</xsl:when>
			<xsl:otherwise>ERROR</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template name="csharp-operator-name">
		<xsl:param name="name" />

		<xsl:text>operator&#160;</xsl:text>
		<xsl:call-template name="operator-symbol">
			<xsl:with-param name="name" select="$name"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="operator-symbol">
		<xsl:param name="name" />
		<xsl:choose>
			<xsl:when test="$name='op_UnaryPlus'">+</xsl:when>
			<xsl:when test="$name='op_UnaryNegation'">-</xsl:when>
			<xsl:when test="$name='op_LogicalNot'">!</xsl:when>
			<xsl:when test="$name='op_OnesComplement'">~</xsl:when>
			<xsl:when test="$name='op_Increment'">++</xsl:when>
			<xsl:when test="$name='op_Decrement'">--</xsl:when>
			<xsl:when test="$name='op_True'">true</xsl:when>
			<xsl:when test="$name='op_False'">false</xsl:when>
			<xsl:when test="$name='op_Addition'">+</xsl:when>
			<xsl:when test="$name='op_Subtraction'">-</xsl:when>
			<xsl:when test="$name='op_Multiply'">*</xsl:when>
			<xsl:when test="$name='op_Division'">/</xsl:when>
			<xsl:when test="$name='op_Modulus'">%</xsl:when>
			<xsl:when test="$name='op_BitwiseAnd'">&amp;</xsl:when>
			<xsl:when test="$name='op_BitwiseOr'">|</xsl:when>
			<xsl:when test="$name='op_ExclusiveOr'">^</xsl:when>
			<xsl:when test="$name='op_LeftShift'">&lt;&lt;</xsl:when>
			<xsl:when test="$name='op_RightShift'">>></xsl:when>
			<xsl:when test="$name='op_Equality'">==</xsl:when>
			<xsl:when test="$name='op_Inequality'">!=</xsl:when>
			<xsl:when test="$name='op_LessThan'">&lt;</xsl:when>
			<xsl:when test="$name='op_GreaterThan'">></xsl:when>
			<xsl:when test="$name='op_LessThanOrEqual'">&lt;=</xsl:when>
			<xsl:when test="$name='op_GreaterThanOrEqual'">>=</xsl:when>
			<xsl:otherwise>operator </xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-namespace">
		<xsl:param name="name" />
		<xsl:param name="namespace" />
		<xsl:choose>
			<xsl:when test="contains($name, '.')">
				<xsl:call-template name="get-namespace">
					<xsl:with-param name="name" select="substring-after($name, '.')" />
					<xsl:with-param name="namespace" select="concat($namespace, substring-before($name, '.'), '.')" />
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="substring($namespace, 1, string-length($namespace) - 1)" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template name="strip-namespace">
		<xsl:param name="name" />
		<xsl:choose>
			<xsl:when test="contains($name, '.')">
				<xsl:call-template name="strip-namespace">
					<xsl:with-param name="name" select="substring-after($name, '.')" />
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$name" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	
	<xsl:template name="type-requirements-section">
		<h4 class="dtH4">Requirements</h4>
		<p>
			<b>Namespace: </b>
			<a href="{NUtil:GetNamespaceHRef( string( ../@name ) )}">
				<xsl:value-of select="../@name" />
			</a>
		</p>
		<xsl:choose>
			<xsl:when test="documentation/platform">
				<xsl:apply-templates select="documentation/platform"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="/ndoc/platform"/>			
			</xsl:otherwise>
		</xsl:choose>
		<p>
			<b>Assembly: </b>
			<xsl:value-of select="../../../@name" /> (in <xsl:value-of select="../../@name" />)
		</p>
		<xsl:call-template name="permsissions"/>
		<xsl:apply-templates select="documentation/node()" mode="type-requirements-section"/>								
	</xsl:template>
	
	<xsl:template name="member-requirements-section">
		<xsl:if test="documentation/permission | documentation/platform | parent::node()/documentation/platform | /ndoc/platform">
			<h4 class="dtH4">Requirements</h4>
			<xsl:call-template name="member-platform"/>
			<xsl:call-template name="permsissions"/>
			<xsl:apply-templates select="documentation/node()" mode="member-requirements-section"/>									
		</xsl:if>
	</xsl:template>
	
	<xsl:template name="permsissions">
		<xsl:if test="documentation/permission">
			<p>
				<b>.NET Framework Security: </b>
				<ul class="permissions">
					<xsl:apply-templates select="documentation/permission"/>
				</ul>	
			</p>
			<xsl:apply-templates select="documentation/node()" mode="permissions-section"/>									
		</xsl:if>
	</xsl:template>
	
	<xsl:template match="documentation/permission">
		<li>
			<xsl:call-template name="get-link-for-type">
				<xsl:with-param name="type" select="@cref" />
				<xsl:with-param name="link-text" select="substring-after(@cref, 'T:')"/>
			</xsl:call-template>	
			<xsl:text>&#160;</xsl:text>
			<xsl:apply-templates mode="slashdoc" />
		</li>	
	</xsl:template>
	
			
	<xsl:template name="member-platform">
		<xsl:choose>
			<!-- first see if the member has a platform specified -->
			<xsl:when test="documentation/platform">
				<xsl:apply-templates select="documentation/platform"/>			
			</xsl:when>
			<!-- then look in the containing type -->
			<xsl:when test="parent::node()/documentation/platform">
				<xsl:apply-templates select="parent::node()/documentation/platform"/>			
			</xsl:when>
			<!-- otherwise use the project defaults -->
			<xsl:otherwise>
				<xsl:apply-templates select="/ndoc/platform"/>			
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="ndoc/platform">
		<p>
			<b class="le">Platforms: </b>
			<xsl:apply-templates select="os"/>
			<xsl:apply-templates select="frameworks"/>	
		</p>
	</xsl:template>
	<xsl:template match="documentation/platform">
		<p>
			<b class="le">Platforms: </b>
			<!-- this next part will first look for os and framework support locally
					if it can't find it, then it looks to the project defaults -->
			<xsl:choose>
				<xsl:when test="os">
					<xsl:apply-templates select="os"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates select="/ndoc/platform/os"/>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:choose>
				<xsl:when test="frameworks">
					<xsl:apply-templates select="frameworks"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates select="/ndoc/platform/frameworks"/>
				</xsl:otherwise>
			</xsl:choose>		
		</p>
	</xsl:template>
		
	<xsl:template match="os">
		<xsl:choose>
			<xsl:when test="@predefined = 'all'">
				<xsl:text>Windows 98, Windows NT 4.0, Windows Millennium Edition, Windows 2000, Windows XP Home Edition, Windows XP Professional, Windows Server 2003 family</xsl:text>
			</xsl:when>
			<xsl:when test="@predefined = 'nt5plus'">
				<xsl:text>Windows 2000, Windows XP Home Edition, Windows XP Professional, Windows Server 2003 family</xsl:text>
			</xsl:when>
			<xsl:when test="@predefined = 'enterprise'">
				<xsl:text>Windows 2000, Windows XP Professional, Windows Server 2003 family</xsl:text>
			</xsl:when>			
		</xsl:choose>	
		<xsl:if test="string-length(.)">
			<xsl:text>, </xsl:text>
			<xsl:value-of select="."/>
		</xsl:if>
	</xsl:template>
	
	<xsl:template match="frameworks">
		<xsl:if test="count( node()[text()='true'] | custom ) != 0">, </xsl:if>
		<xsl:apply-templates select="compact | mono | custom"/>
	</xsl:template>	
	
	<xsl:template match="mono"/>
	<xsl:template match="mono[text() = 'true']">
		<xsl:text>MONO Open Source Framework</xsl:text>
		<xsl:if test="position()!=last()">, </xsl:if>
	</xsl:template>		
	
	<xsl:template match="compact"/>
	<xsl:template match="compact[text() = 'true']">
		<xsl:text>.NET Compact Framework</xsl:text>
		<xsl:if test="position()!=last()">, </xsl:if>
	</xsl:template>		
	
	<xsl:template match="frameworks/custom">
		<xsl:value-of select="."/>
		<xsl:if test="position()!=last()">, </xsl:if>
	</xsl:template>		
	
	<!-- -->	
	<xsl:template name="version-section">
		<DIV CLASS="footer">
			<p>
				<center>
					<font color="red">
						<i>
							<xsl:if test="string-length( $ndoc-net-framework-version ) != 0">
								Syntax based on .NET Framework version <xsl:value-of select="$ndoc-net-framework-version"/>.<br/>
							</xsl:if>							
							<xsl:if test="string-length( $ndoc-version ) != 0">
								Documentation version <xsl:value-of select="$ndoc-version"/>.
							</xsl:if>
						</i>
					</font>
				</center>
			</p>	
		</DIV>
	</xsl:template>
	
	<xsl:template match="feedbackEmail">
		<xsl:param name="page"/>
		<p>
			<a>
				<xsl:attribute name="href">
					<xsl:text>mailto:</xsl:text>
					<xsl:value-of select="."/>
					<xsl:text>?subject=</xsl:text>
					<xsl:value-of select="$ndoc-title" />
					<xsl:text> </xsl:text>
					<xsl:value-of select="$ndoc-version" />
					<xsl:text> Documentation Feedback: </xsl:text>
					<xsl:value-of select="$page"/>
				</xsl:attribute>
				<xsl:text>Send comments on this topic.</xsl:text>
			</a>
		</p>
	</xsl:template>
</xsl:stylesheet>
