namespace ExcelClone.Resources.Localization {
    using System;
    using System.Resources;
    using System.Globalization;

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class AppResources {
        
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        internal AppResources() {
        }

        internal static ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    ResourceManager temp = new ResourceManager("ExcelClone.Resources.Localization.AppResources", typeof(AppResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        internal static CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        internal static string OpenFile {
            get {
                return ResourceManager.GetString("OpenFile", resourceCulture);
            }
        }

        internal static string SaveFileAs {
            get {
                return ResourceManager.GetString("SaveFileAs", resourceCulture);
            }
        }

        internal static string UnexpectedToken {
            get {
                return ResourceManager.GetString("UnexpectedToken", resourceCulture);
            }
        }

        internal static string UnknownPrefixFunction {
            get {
                return ResourceManager.GetString("UnknownPrefixFunction", resourceCulture);
            }
        }

        internal static string UnknownInfixFunction {
            get {
                return ResourceManager.GetString("UnknownInfixFunction", resourceCulture);
            }
        }

        internal static string UnexpectedEOE {
            get {
                return ResourceManager.GetString("UnexpectedEOE", resourceCulture);
            }
        }

        internal static string UnexpectedValue {
            get {
                return ResourceManager.GetString("UnexpectedValue", resourceCulture);
            }
        }

        internal static string UnexpectedTokenEOE {
            get {
                return ResourceManager.GetString("UnexpectedTokenEOE", resourceCulture);
            }
        }

        internal static string UnexpectedCharacter {
            get {
                return ResourceManager.GetString("UnexpectedCharacter", resourceCulture);
            }
        }

        internal static string CannotDivideBy0 {
            get {
                return ResourceManager.GetString("CannotDivideBy0", resourceCulture);
            }
        }

        internal static string ExpectsExactlyNArguments {
            get {
                return ResourceManager.GetString("ExpectsExactlyNArguments", resourceCulture);
            }
        }

        internal static string ExpectsAtLeastNNumberArguments {
            get {
                return ResourceManager.GetString("ExpectsAtLeastNNumberArguments", resourceCulture);
            }
        }

        internal static string CannotConvertTo {
            get {
                return ResourceManager.GetString("CannotConvertTo", resourceCulture);
            }
        }

        internal static string OperatorDefinedOnlyForNumbers {
            get {
                return ResourceManager.GetString("OperatorDefinedOnlyForNumbers", resourceCulture);
            }
        }

        internal static string UnaryOperatorDefinedOnlyForNumbers {
            get {
                return ResourceManager.GetString("UnaryOperatorDefinedOnlyForNumbers", resourceCulture);
            }
        }

        internal static string OK {
            get {
                return ResourceManager.GetString("OK", resourceCulture);
            }
        }

        internal static string Error {
            get {
                return ResourceManager.GetString("Error", resourceCulture);
            }
        }

        internal static string SavingResult {
            get {
                return ResourceManager.GetString("SavingResult", resourceCulture);
            }
        }

        internal static string EmptyFile {
            get {
                return ResourceManager.GetString("EmptyFile", resourceCulture);
            }
        }

        internal static string InvalidFileHeader {
            get {
                return ResourceManager.GetString("InvalidFileHeader", resourceCulture);
            }
        }

        internal static string FileSavedSuccessfully {
            get {
                return ResourceManager.GetString("FileSavedSuccessfully", resourceCulture);
            }
        }

        internal static string FileSavingError {
            get {
                return ResourceManager.GetString("FileSavingError", resourceCulture);
            }
        }

    }
}