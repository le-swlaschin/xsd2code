﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Xsd2Code.TestUnit.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Xsd2Code.TestUnit.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;!--Created with Liquid XML Studio (http://www.liquid-technologies.com)--&gt;
        ///&lt;xs:schema attributeFormDefault=&quot;unqualified&quot; elementFormDefault=&quot;qualified&quot; id=&quot;Actor&quot; xmlns:xs=&quot;http://www.w3.org/2001/XMLSchema&quot;&gt;
        ///  &lt;xs:element name=&quot;Actor&quot;&gt;
        ///    &lt;xs:annotation&gt;
        ///      &lt;xs:documentation&gt;
        ///        Actor pépé class include firstname and lastname (ûàéçè).
        ///      &lt;/xs:documentation&gt;
        ///    &lt;/xs:annotation&gt;
        ///    &lt;xs:complexType&gt;
        ///      &lt;xs:sequence&gt;
        ///        &lt;xs:element name=&quot;f [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Actor {
            get {
                return ResourceManager.GetString("Actor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;!--Created with Liquid XML Studio (http://www.liquid-technologies.com)--&gt;
        ///&lt;xs:schema xmlns:mstns=&quot;http://tempuri.org/ArrayOfArray.xsd&quot; elementFormDefault=&quot;qualified&quot; targetNamespace=&quot;http://tempuri.org/ArrayOfArray.xsd&quot; id=&quot;ArrayOfArray&quot; xmlns:xs=&quot;http://www.w3.org/2001/XMLSchema&quot;&gt;
        ///  &lt;xs:element name=&quot;root&quot;&gt;
        ///    &lt;xs:complexType&gt;
        ///      &lt;xs:sequence minOccurs=&quot;0&quot;&gt;
        ///        &lt;xs:element minOccurs=&quot;0&quot; maxOccurs=&quot;unbounded&quot; name=&quot;Element&quot; type=&quot;mstns:element&quot; /&gt;
        ///     [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string ArrayOfArray {
            get {
                return ResourceManager.GetString("ArrayOfArray", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;xs:schema targetNamespace=&quot;http://tempuri.org/EtudeData.xsd&quot; xmlns:NameSpace1=&quot;http://www.axilog.fr&quot; xmlns:xs=&quot;http://www.w3.org/2001/XMLSchema&quot; xmlns:mstns=&quot;http://tempuri.org/EtudeData.xsd&quot; xmlns=&quot;http://tempuri.org/EtudeData.xsd&quot; elementFormDefault=&quot;qualified&quot; id=&quot;EtudeData&quot;&gt;
        ///  &lt;xs:element name=&quot;ROOT&quot;&gt;
        ///    &lt;xs:annotation&gt;
        ///      &lt;xs:documentation&gt;Element racine de l&apos;etude&lt;/xs:documentation&gt;
        ///    &lt;/xs:annotation&gt;
        ///    &lt;xs:complexType&gt;
        ///      &lt;xs:sequence&gt;
        ///       [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Circular {
            get {
                return ResourceManager.GetString("Circular", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;!--Created with Liquid XML Studio - FREE Community Edition (http://www.liquid-technologies.com)--&gt;
        ///&lt;xs:schema xmlns:mstns=&quot;http://tempuri.org/CircularClassReference.xsd&quot; elementFormDefault=&quot;qualified&quot; targetNamespace=&quot;http://tempuri.org/CircularClassReference.xsd&quot; id=&quot;CircularClassReference&quot; xmlns:xs=&quot;http://www.w3.org/2001/XMLSchema&quot;&gt;
        ///  &lt;xs:complexType name=&quot;Circular&quot;&gt;
        ///    &lt;xs:sequence&gt;
        ///      &lt;xs:element name=&quot;circular&quot; type=&quot;mstns:Circular&quot; /&gt;
        ///      &lt;xs:eleme [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CircularClassReference {
            get {
                return ResourceManager.GetString("CircularClassReference", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;!--Created with Liquid XML Studio Designer Edition (http://www.liquid-technologies.com)--&gt;
        ///&lt;xs:schema attributeFormDefault=&quot;unqualified&quot; elementFormDefault=&quot;qualified&quot; id=&quot;Dvd&quot; xmlns:xs=&quot;http://www.w3.org/2001/XMLSchema&quot;&gt;
        ///  &lt;xs:include schemaLocation=&quot;Actor.xsd&quot; /&gt;
        ///  &lt;xs:element name=&quot;DvdCollection&quot;&gt;
        ///    &lt;xs:complexType&gt;
        ///      &lt;xs:sequence maxOccurs=&quot;unbounded&quot;&gt;
        ///        &lt;xs:element name=&quot;Dvds&quot; type=&quot;dvd&quot; /&gt;
        ///      &lt;/xs:sequence&gt;
        ///    &lt;/xs:complexType&gt;
        ///  &lt;/xs: [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string dvd {
            get {
                return ResourceManager.GetString("dvd", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;!--Created with Liquid XML Studio - FREE Community Edition (http://www.liquid-technologies.com)--&gt;
        ///&lt;xs:schema xmlns:mstns=&quot;http://tempuri.org/Gender.xsd&quot; elementFormDefault=&quot;qualified&quot; targetNamespace=&quot;http://tempuri.org/Gender.xsd&quot; id=&quot;Gender&quot; xmlns:xs=&quot;http://www.w3.org/2001/XMLSchema&quot;&gt;
        ///  &lt;xs:element name=&quot;Root&quot;&gt;
        ///    &lt;xs:complexType&gt;
        ///      &lt;xs:sequence&gt;
        ///        &lt;xs:element name=&quot;GenderElement&quot; type=&quot;mstns:ksgender&quot; /&gt;
        ///      &lt;/xs:sequence&gt;
        ///      &lt;xs:attribut [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Gender {
            get {
                return ResourceManager.GetString("Gender", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;xs:schema xmlns:xs=&quot;http://www.w3.org/2001/XMLSchema&quot; elementFormDefault=&quot;qualified&quot;&gt;
        ///
        ///  &lt;xs:element name=&quot;root&quot; type=&quot;rootType&quot;/&gt;
        ///
        ///  &lt;xs:element name=&quot;Image&quot;&gt;
        ///    &lt;xs:complexType&gt;
        ///      &lt;xs:simpleContent&gt;
        ///        &lt;xs:extension base=&quot;xs:base64Binary&quot;&gt;
        ///          &lt;xs:attribute name=&quot;fileName&quot; type=&quot;xs:string&quot; use=&quot;optional&quot;/&gt;
        ///        &lt;/xs:extension&gt;
        ///      &lt;/xs:simpleContent&gt;
        ///    &lt;/xs:complexType&gt;
        ///  &lt;/xs:element&gt;
        ///
        ///  &lt;xs:complexType name=&quot;rootType&quot;&gt;
        ///    &lt;x [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string hexBinary {
            get {
                return ResourceManager.GetString("hexBinary", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;UTF-8&quot;?&gt;
        ///&lt;xs:schema xmlns:xs=&quot;http://www.w3.org/2001/XMLSchema&quot; elementFormDefault=&quot;qualified&quot; attributeFormDefault=&quot;unqualified&quot;&gt;
        ///  &lt;xs:element name=&quot;Root&quot;&gt;
        ///    &lt;xs:annotation&gt;
        ///      &lt;xs:documentation&gt;Doc of root root element&lt;/xs:documentation&gt;
        ///    &lt;/xs:annotation&gt;
        ///    &lt;xs:complexType&gt;
        ///      &lt;xs:sequence&gt;
        ///        &lt;xs:element name=&quot;SubElement&quot; maxOccurs=&quot;unbounded&quot;&gt;
        ///          &lt;xs:annotation&gt;
        ///            &lt;xs:documentation&gt;Doc of unbounded SubElement&lt;/xs:documentation&gt;        /// [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Hierarchical {
            get {
                return ResourceManager.GetString("Hierarchical", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;xs:schema id=&quot;Inheritance&quot;
        ///    targetNamespace=&quot;http://tempuri.org/Inheritance.xsd&quot;
        ///    elementFormDefault=&quot;qualified&quot;
        ///    xmlns=&quot;http://tempuri.org/Inheritance.xsd&quot;
        ///    xmlns:mstns=&quot;http://tempuri.org/Inheritance.xsd&quot;
        ///    xmlns:xs=&quot;http://www.w3.org/2001/XMLSchema&quot;
        ///&gt;
        ///  &lt;xs:element name=&quot;RootElement&quot;&gt;
        ///    &lt;xs:complexType&gt;
        ///          &lt;xs:sequence&gt;
        ///      &lt;xs:element name=&quot;Garage&quot; type=&quot;Garage&quot;/&gt;
        ///    &lt;/xs:sequence&gt;
        ///    
        ///    &lt;/xs:complexType&gt;
        ///  &lt;/xs:element&gt; [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Inheritance {
            get {
                return ResourceManager.GetString("Inheritance", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;xs:schema elementFormDefault=&quot;qualified&quot; xmlns:xs=&quot;http://www.w3.org/2001/XMLSchema&quot;&gt;
        ///  &lt;xs:element name=&quot;Address&quot;&gt;
        ///    &lt;xs:complexType&gt;
        ///      &lt;xs:attribute name=&quot;name&quot; type=&quot;xs:string&quot; use=&quot;required&quot;/&gt;
        ///      &lt;xs:attribute name=&quot;street&quot; type=&quot;xs:string&quot; use=&quot;required&quot;/&gt;
        ///      &lt;xs:attribute name=&quot;zip&quot; type=&quot;xs:string&quot; use=&quot;required&quot;/&gt;
        ///      &lt;xs:attribute name=&quot;city&quot; type=&quot;xs:string&quot; use=&quot;required&quot;/&gt;
        ///    &lt;/xs:complexType&gt;
        ///  &lt;/xs:element&gt;
        ///  &lt;xs:element name=&quot;Ad [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string LazyLoading {
            get {
                return ResourceManager.GetString("LazyLoading", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;xs:schema xmlns:xs=&quot;http://www.w3.org/2001/XMLSchema&quot; xmlns:mailxml_base=&quot;http://idealliance.org/maildat/Specs/md091/mailxml60a/base&quot; xmlns:mailxml=&quot;http://idealliance.org/maildat/Specs/md091/mailxml60a/mailxml&quot; attributeFormDefault=&quot;qualified&quot; elementFormDefault=&quot;qualified&quot; targetNamespace=&quot;http://idealliance.org/maildat/Specs/md091/mailxml60a/mailxml&quot; version=&quot;mailxml60a120308&quot; &gt;
        ///  &lt;xs:import schemaLocation=&quot;mailxml_base_120108.xsd&quot; namespace=&quot;http://idealliance.org/maildat/Specs/md091/mailxml60a/base&quot;/ [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string mailxml_base {
            get {
                return ResourceManager.GetString("mailxml_base", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;xs:schema xmlns:xs=&quot;http://www.w3.org/2001/XMLSchema&quot; xmlns:mailxml_base=&quot;http://idealliance.org/maildat/Specs/md091/mailxml60a/base&quot; attributeFormDefault=&quot;unqualified&quot; elementFormDefault=&quot;qualified&quot; targetNamespace=&quot;http://idealliance.org/maildat/Specs/md091/mailxml60a/base&quot; version=&quot;MAILxmlBase60a120108&quot;&gt;
        ///  &lt;xs:simpleType name=&quot;actionCodeType&quot;&gt;
        ///    &lt;xs:annotation&gt;
        ///      &lt;xs:documentation&gt;Action Code for USPS&lt;/xs:documentation&gt;
        ///    &lt;/xs:annotation&gt;
        ///    &lt;xs:restriction base=&quot;xs:string&quot;&gt;
        ///      &lt;xs:enu [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string mailxml_base_120108 {
            get {
                return ResourceManager.GetString("mailxml_base_120108", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;!--Created with Liquid XML Studio Designer Edition (http://www.liquid-technologies.com)--&gt;
        ///&lt;xs:schema xmlns:mstns=&quot;http://tempuri.org/Gender.xsd&quot; elementFormDefault=&quot;qualified&quot; targetNamespace=&quot;http://tempuri.org/Gender.xsd&quot; id=&quot;Gender&quot; xmlns:xs=&quot;http://www.w3.org/2001/XMLSchema&quot;&gt;
        ///  &lt;xs:element name=&quot;Root&quot;&gt;
        ///    &lt;xs:complexType&gt;
        ///      &lt;xs:sequence&gt;
        ///        &lt;xs:element name=&quot;Gender&quot; type=&quot;mstns:ksgender&quot; minOccurs=&quot;0&quot; maxOccurs=&quot;1&quot;&gt;
        ///          &lt;xs:annotation&gt;
        ///   [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string PropertyNameSpecified {
            get {
                return ResourceManager.GetString("PropertyNameSpecified", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;!--Created with Liquid XML Studio - FREE Community Edition (http://www.liquid-technologies.com)--&gt;
        ///&lt;xs:schema xmlns:mstns=&quot;http://tempuri.org/XMLSchema1.xsd&quot; elementFormDefault=&quot;qualified&quot; targetNamespace=&quot;http://tempuri.org/XMLSchema1.xsd&quot; id=&quot;XMLSchema1&quot; xmlns:xs=&quot;http://www.w3.org/2001/XMLSchema&quot;&gt;
        ///  &lt;xs:element name=&quot;MyWrapper&quot;&gt;
        ///    &lt;xs:complexType&gt;
        ///      &lt;xs:sequence&gt;
        ///        &lt;xs:element name=&quot;MyElement&quot; type=&quot;mstns:MyType&quot; /&gt;
        ///      &lt;/xs:sequence&gt;
        ///    &lt;/x [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string StackOverFlow {
            get {
                return ResourceManager.GetString("StackOverFlow", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;!--Created with Liquid XML Studio (http://www.liquid-technologies.com)--&gt;
        ///&lt;xs:schema xmlns=&quot;http://www.portalfiscal.inf.br/nfe&quot; xmlns:ds=&quot;http://www.w3.org/2000/09/xmldsig#&quot; attributeFormDefault=&quot;unqualified&quot; elementFormDefault=&quot;qualified&quot; targetNamespace=&quot;http://www.portalfiscal.inf.br/nfe&quot; xmlns:xs=&quot;http://www.w3.org/2001/XMLSchema&quot;&gt;
        ///  &lt;xs:element name=&quot;StaffMember&quot; type=&quot;TActor&quot;&gt;
        ///    &lt;xs:annotation&gt;
        ///      &lt;xs:documentation&gt;A staff member&lt;/xs:documentation&gt;
        ///  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string TestAnnotations {
            get {
                return ResourceManager.GetString("TestAnnotations", resourceCulture);
            }
        }
    }
}
