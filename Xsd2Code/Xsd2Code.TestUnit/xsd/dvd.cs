namespace XSDCodeGen {
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using System.Collections;
    using System.Xml.Schema;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.1318")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://tempuri.org/Test.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://tempuri.org/Test.xsd", IsNullable=false)]
    public partial class Dvdtheque : System.ComponentModel.INotifyPropertyChanged {
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        private List <DvdthequeDvds> dvdsField;
        
        public Dvdtheque() {
            if ((this.dvdsField == null)) {
                this.dvdsField = new List <DvdthequeDvds>();
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("dvds")]
        public List<DvdthequeDvds> dvds {
            get {
                return this.dvdsField;
            }
            set {
                this.dvdsField = value;
                OnPropertyChanged("dvds");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        private void OnPropertyChanged(string info) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.1318")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://tempuri.org/Test.xsd")]
    public partial class DvdthequeDvds : System.ComponentModel.INotifyPropertyChanged {
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        private string titreField;
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        private DvdthequeDvdsActeurs acteursField;
        
        public DvdthequeDvds() {
            if ((this.acteursField == null)) {
                this.acteursField = new DvdthequeDvdsActeurs();
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string Titre {
            get {
                return this.titreField;
            }
            set {
                this.titreField = value;
                OnPropertyChanged("Titre");
            }
        }
        
        /// <remarks/>
        public DvdthequeDvdsActeurs Acteurs {
            get {
                return this.acteursField;
            }
            set {
                this.acteursField = value;
                OnPropertyChanged("Acteurs");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        private void OnPropertyChanged(string info) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.1318")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://tempuri.org/Test.xsd")]
    public partial class DvdthequeDvdsActeurs : System.ComponentModel.INotifyPropertyChanged {
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        private string nomField;
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        private string prenomField;
        
        public DvdthequeDvdsActeurs() {
        }
        
        /// <remarks/>
        public string Nom {
            get {
                return this.nomField;
            }
            set {
                this.nomField = value;
                OnPropertyChanged("Nom");
            }
        }
        
        /// <remarks/>
        public string prenom {
            get {
                return this.prenomField;
            }
            set {
                this.prenomField = value;
                OnPropertyChanged("prenom");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        private void OnPropertyChanged(string info) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
