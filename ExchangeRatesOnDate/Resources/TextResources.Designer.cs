﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ExchangeRatesOnDate.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class TextResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal TextResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ExchangeRatesOnDate.Resources.TextResources", typeof(TextResources).Assembly);
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
        ///   Looks up a localized string similar to ExchangeRatesOnDate_Bot is running!.
        /// </summary>
        internal static string BotIsRunning {
            get {
                return ResourceManager.GetString("BotIsRunning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Receiving updates finished..
        /// </summary>
        internal static string Finish {
            get {
                return ResourceManager.GetString("Finish", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to To get the exchange rate against the ruble, enter the currency code and the required date, separated by a space. For example: &quot;USD 15.11.2021&quot;..
        /// </summary>
        internal static string Instruction {
            get {
                return ResourceManager.GetString("Instruction", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Fail on making out the date..
        /// </summary>
        internal static string InvalidDate {
            get {
                return ResourceManager.GetString("InvalidDate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Press &quot;Esc&quot; to stop..
        /// </summary>
        internal static string MessageToStop {
            get {
                return ResourceManager.GetString("MessageToStop", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0}, 1 {1} =  {2} RUB.
        /// </summary>
        internal static string Reply {
            get {
                return ResourceManager.GetString("Reply", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Receiving updates started..
        /// </summary>
        internal static string StartReceivingUpdates {
            get {
                return ResourceManager.GetString("StartReceivingUpdates", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Telegram API Error:
        ///[{0}]
        ///{1}.
        /// </summary>
        internal static string TelegramApiError {
            get {
                return ResourceManager.GetString("TelegramApiError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown currency code..
        /// </summary>
        internal static string UnknownCurrencyCode {
            get {
                return ResourceManager.GetString("UnknownCurrencyCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown update type: {0}.
        /// </summary>
        internal static string UnknownUpdateType {
            get {
                return ResourceManager.GetString("UnknownUpdateType", resourceCulture);
            }
        }
    }
}