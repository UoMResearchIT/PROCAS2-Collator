﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PROCAS2.Resources {
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
    public class HL7Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal HL7Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PROCAS2.Resources.HL7Resources", typeof(HL7Resources).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error setting consent flag for patient {0}..
        /// </summary>
        public static string CONSENT_NOT_SET {
            get {
                return ResourceManager.GetString("CONSENT_NOT_SET", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There has been a problem creating the response header..
        /// </summary>
        public static string HEADER_CREATION_ERROR {
            get {
                return ResourceManager.GetString("HEADER_CREATION_ERROR", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The message is not an ORU^R01..
        /// </summary>
        public static string NOT_ORUR01 {
            get {
                return ResourceManager.GetString("NOT_ORUR01", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error with OBX type {0}. Record not created..
        /// </summary>
        public static string OBX_ERROR {
            get {
                return ResourceManager.GetString("OBX_ERROR", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ID {0} does not exist.
        /// </summary>
        public static string PATIENT_NOT_EXISTS {
            get {
                return ResourceManager.GetString("PATIENT_NOT_EXISTS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error creating the risk letter for patient {0}..
        /// </summary>
        public static string RISK_LETTER_NOT_CREATED {
            get {
                return ResourceManager.GetString("RISK_LETTER_NOT_CREATED", resourceCulture);
            }
        }
    }
}
