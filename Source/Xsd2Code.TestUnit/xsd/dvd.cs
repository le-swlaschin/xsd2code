/// ------------------------------------------------------------------------------
/// XSD2Code generation options. ** Must be at the begining of the file **
/// <NameSpace>XSD2Code.Test</NameSpace><Collection>List</Collection><codeType>CSharp</codeType><EnableDataBinding>False</EnableDataBinding><HidePrivateFieldInIDE>False</HidePrivateFieldInIDE>
/// ------------------------------------------------------------------------------
namespace XSD2Code.Test {
    using System;
    using System.Diagnostics;
    using System.Xml.Serialization;
    using System.Collections;
    using System.Xml.Schema;
    using System.ComponentModel;
    using System.Collections.Generic;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "1.0.3138.31512")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://tempuri.org/Test.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://tempuri.org/Test.xsd", IsNullable=false)]
    public partial class DvdCollection {
        
        private List <dvds> dvdsField;
        
        // Constructor
        public DvdCollection() {
            if ((this.dvdsField == null)) {
                this.dvdsField = new List <dvds>();
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("dvds")]
        public List<dvds> dvds {
            get {
                return this.dvdsField;
            }
            set {
                this.dvdsField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "1.0.3138.31512")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://tempuri.org/Test.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://tempuri.org/Test.xsd", IsNullable=false)]
    public partial class dvds {
        
        private string titleField;
        
        private string styleField;
        
        private List <Actor> actorField;
        
        private System.Nullable<bool> showField;
        
        // Constructor
        public dvds() {
            if ((this.actorField == null)) {
                this.actorField = new List <Actor>();
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string title {
            get {
                return this.titleField;
            }
            set {
                this.titleField = value;
            }
        }
        
        /// <remarks/>
        public string style {
            get {
                return this.styleField;
            }
            set {
                this.styleField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Actor")]
        public List<Actor> Actor {
            get {
                return this.actorField;
            }
            set {
                this.actorField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public System.Nullable<bool> show {
            get {
                return this.showField;
            }
            set {
                this.showField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "1.0.3138.31512")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://tempuri.org/Test.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://tempuri.org/Test.xsd", IsNullable=false)]
    public partial class Actor {
        
        private string firstnameField;
        
        private string lastnameField;
        
        /// <remarks/>
        public string firstname {
            get {
                return this.firstnameField;
            }
            set {
                this.firstnameField = value;
            }
        }
        
        /// <remarks/>
        public string lastname {
            get {
                return this.lastnameField;
            }
            set {
                this.lastnameField = value;
            }
        }
    }
}
