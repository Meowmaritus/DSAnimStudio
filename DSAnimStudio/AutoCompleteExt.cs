using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSAnimStudio
{
    /// <summary>
    /// From: https://www.codeproject.com/Articles/3792/C-does-Shell-Part-4
    /// Note: The UCOMIEnumString interface is deprecated in .NET as of 2018!
    /// </summary>
    public class AutoCompleteExt
    {
        public static void DisableAutoSuggest(TextBox tb)
        {
            tb.AutoCompleteCustomSource = null;
            AutoCompleteExt.Disable(tb.Handle);
        }

        public static void DisableAutoSuggest(ComboBox tb)
        {
            tb.AutoCompleteCustomSource = null;
            AutoCompleteExt.Disable(tb.Handle);
        }

        public static void EnableAutoSuggest(TextBox tb, string[] suggestions)
        {
            // Create and assign the autocomplete source
            // Try to enable a more advanced settings for AutoComplete via the WinShell interface
            try
            {
                var source = new SourceCustomList() { StringList = suggestions.ToArray() };
                // For options descriptions see: 
                // https://docs.microsoft.com/en-us/windows/desktop/api/shldisp/ne-shldisp-_tagautocompleteoptions
                var options = AUTOCOMPLETEOPTIONS.ACO_UPDOWNKEYDROPSLIST | AUTOCOMPLETEOPTIONS.ACO_USETAB |
                              /* AUTOCOMPLETEOPTIONS.ACO_AUTOAPPEND | */ AUTOCOMPLETEOPTIONS.ACO_AUTOSUGGEST | AUTOCOMPLETEOPTIONS.ACO_WORD_FILTER;
                AutoCompleteExt.Enable(tb.Handle, source, options);
            }
            catch (Exception)
            {
                // Incase of an error, let's fall back to the default
                var source = new AutoCompleteStringCollection();
                source.AddRange(suggestions);
                tb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                tb.AutoCompleteSource = AutoCompleteSource.CustomSource;
                tb.AutoCompleteCustomSource = source;
            }
        }

        public static void EnableAutoSuggest(ComboBox tb, string[] suggestions)
        {
            // Create and assign the autocomplete source
            // Try to enable a more advanced settings for AutoComplete via the WinShell interface
            try
            {
                var source = new SourceCustomList() { StringList = suggestions.ToArray() };
                // For options descriptions see: 
                // https://docs.microsoft.com/en-us/windows/desktop/api/shldisp/ne-shldisp-_tagautocompleteoptions
                var options = AUTOCOMPLETEOPTIONS.ACO_UPDOWNKEYDROPSLIST | AUTOCOMPLETEOPTIONS.ACO_USETAB |
                              /* AUTOCOMPLETEOPTIONS.ACO_AUTOAPPEND | */ AUTOCOMPLETEOPTIONS.ACO_AUTOSUGGEST | AUTOCOMPLETEOPTIONS.ACO_WORD_FILTER;
                AutoCompleteExt.Enable(tb.Handle, source, options);
            }
            catch (Exception)
            {
                // Incase of an error, let's fall back to the default
                var source = new AutoCompleteStringCollection();
                source.AddRange(suggestions);
                tb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                tb.AutoCompleteSource = AutoCompleteSource.CustomSource;
                tb.AutoCompleteCustomSource = source;
            }
        }

        public static Guid CLSID_AutoComplete = new Guid("{00BB2763-6A77-11D0-A535-00C04FD7D062}");

        private static object GetAutoComplete()
        {
            return Activator.CreateInstance(Type.GetTypeFromCLSID(CLSID_AutoComplete));
        }

        public static void Enable(IntPtr controlHandle, SourceCustomList items, AUTOCOMPLETEOPTIONS options)
        {
            if (controlHandle == IntPtr.Zero)
                return;

            IAutoComplete2 iac = null;
            try
            {
                iac = (IAutoComplete2)GetAutoComplete();
                int ret = iac.Init(controlHandle, items, "", "");
                ret = iac.SetOptions(options);
                ret = iac.Enable(true);
            }
            finally
            {
                if (iac != null)
                    Marshal.ReleaseComObject(iac);
            }
        }

        public static void Disable(IntPtr controlHandle)
        {
            if (controlHandle == IntPtr.Zero)
                return;

            IAutoComplete2 iac = null;
            try
            {
                iac = (IAutoComplete2)GetAutoComplete();
                iac.Enable(false);
            }
            finally
            {
                if (iac != null)
                    Marshal.ReleaseComObject(iac);
            }
        }
    }
    /// <summary>
    /// From https://www.pinvoke.net/default.aspx/Interfaces.IAutoComplete2
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("EAC04BC0-3791-11D2-BB95-0060977B464C")]
    public interface IAutoComplete2
    {
        [PreserveSig]
        int Init(
            // Handle to the window for the system edit control that is to
            // have autocompletion enabled. 
            IntPtr hwndEdit,

            // Pointer to the IUnknown interface of the string list object that
            // is responsible for generating candidates for the completed 
            // string. The object must expose an IEnumString interface. 
            [MarshalAs(UnmanagedType.IUnknown)] object punkACL,

            // Pointer to an optional null-terminated Unicode string that gives
            // the registry path, including the value name, where the format 
            // string is stored as a REG_SZ value. The autocomplete object 
            // first looks for the path under HKEY_CURRENT_USER . If it fails,
            // it then tries HKEY_LOCAL_MACHINE . For a discussion of the 
            // format string, see the definition of pwszQuickComplete.
            [MarshalAs(UnmanagedType.LPWStr)] string pwszRegKeyPath,

            // Pointer to an optional string that specifies the format to be
            // used if the user enters some text and presses CTRL+ENTER. Set
            // this parameter to NULL to disable quick completion. Otherwise,
            // the autocomplete object treats pwszQuickComplete as a sprintf 
            // format string, and the text in the edit box as its associated 
            // argument, to produce a new string. For example, set 
            // pwszQuickComplete to "http://www. %s.com/". When a user enters
            // "MyURL" into the edit box and presses CTRL+ENTER, the text in 
            // the edit box is updated to "http://www.MyURL.com/". 
            [MarshalAs(UnmanagedType.LPWStr)] string pwszQuickComplete
        );

        // Enables or disables autocompletion.
        [PreserveSig]
        int Enable(bool value);

        // Sets the current autocomplete options.
        [PreserveSig]
        int SetOptions(AUTOCOMPLETEOPTIONS dwFlag);

        // Retrieves the current autocomplete options.
        [PreserveSig]
        int GetOptions(out AUTOCOMPLETEOPTIONS pdwFlag);
    }

    /// <summary>
    ///   Specifies values used by IAutoComplete2::GetOptions and 
    ///   "IAutoComplete2.SetOptions" for options surrounding autocomplete.
    /// </summary>
    /// <remarks>
    ///   [AUTOCOMPLETEOPTIONS Enumerated Type ()]
    ///   http://msdn.microsoft.com/en-us/library/bb762479.aspx
    /// </remarks>
    [Flags]
    public enum AUTOCOMPLETEOPTIONS
    {
        /// <summary>Do not autocomplete.</summary>
        ACO_NONE = 0x0000,

        /// <summary>Enable the autosuggest drop-down list.</summary>
        ACO_AUTOSUGGEST = 0x0001,

        /// <summary>Enable autoappend.</summary>
        ACO_AUTOAPPEND = 0x0002,

        /// <summary>Add a search item to the list of 
        /// completed strings. When the user selects 
        /// this item, it launches a search engine.</summary>
        ACO_SEARCH = 0x0004,

        /// <summary>Do not match common prefixes, such as 
        /// "www." or "http://".</summary>
        ACO_FILTERPREFIXES = 0x0008,

        /// <summary>Use the TAB key to select an 
        /// item from the drop-down list.</summary>
        ACO_USETAB = 0x0010,

        /// <summary>Use the UP ARROW and DOWN ARROW keys to 
        /// display the autosuggest drop-down list.</summary>
        ACO_UPDOWNKEYDROPSLIST = 0x0020,

        /// <summary>Normal windows display text left-to-right 
        /// (LTR). Windows can be mirrored to display languages 
        /// such as Hebrew or Arabic that read right-to-left (RTL). 
        /// Typically, control text is displayed in the same 
        /// direction as the text in its parent window. If 
        /// ACO_RTLREADING is set, the text reads in the opposite 
        /// direction from the text in the parent window.</summary>
        ACO_RTLREADING = 0x0040,

        /// <summary>[Windows Vista and later]. If set, the 
        /// autocompleted suggestion is treated as a phrase 
        /// for search purposes. The suggestion, Microsoft 
        /// Office, would be treated as "Microsoft Office" 
        /// (where both Microsoft AND Office must appear in 
        /// the search results).</summary>
        ACO_WORD_FILTER = 0x0080,

        /// <summary>[Windows Vista and later]. Disable prefix 
        /// filtering when displaying the autosuggest dropdown. 
        /// Always display all suggestions.</summary>
        ACO_NOPREFIXFILTERING = 0x0100
    }

    /// <summary>
    /// Implements the https://docs.microsoft.com/en-us/windows/desktop/api/objidl/nn-objidl-ienumstring
    /// interface for autoccomplete
    /// </summary>
    public class SourceCustomList : UCOMIEnumString
    {

        public string[] StringList;
        private int currentPosition = 0;

        public int Next(
                int celt,     // Number of elements being requested.
                string[] rgelt, // Array of size celt (or larger) of the 
                                // elements of interest. The type of this 
                                // parameter depends on the item being
                                // enumerated.  
                out int pceltFetched) // Pointer to the number of elements actually
                                      // supplied in rgelt. The Caller can pass 
                                      // in NULL if  celt is 1. 
        {
            pceltFetched = 0;
            while ((currentPosition <= StringList.Length - 1) && (pceltFetched < celt))
            {
                rgelt[pceltFetched] = StringList[currentPosition];
                pceltFetched++;
                currentPosition++;
            }

            if (pceltFetched == celt)
                return 0;    // S_OK;
            else
                return 1;    // S_FALSE;
        }

        /// <summary>
        /// This method skips the next specified number of elements in the enumeration sequence.
        /// </summary>
        /// <param name="celt"></param>
        /// <returns></returns>
        public int Skip(
            int celt)                    // Number of elements to be skipped. 
        {
            currentPosition += celt;
            if (currentPosition <= StringList.Length - 1)
                return 0;
            else
                return 1;
        }

        // This method resets the enumeration sequence to the beginning.
        public Int32 Reset()
        {
            currentPosition = 0;
            return 0;
        }

        // This method creates another enumerator that contains the same enumeration 
        // state as the current one. Using this function, a client can record a 
        // particular point in the enumeration sequence and return to that point at a 
        // later time. The new enumerator supports the same interface as the original one.
        public void Clone(
                out UCOMIEnumString ppenum)         // Address of the IEnumString pointer  
                                                    // variable that receives the interface 
                                                    // pointer to the enumeration object. If 
                                                    // the method  is unsuccessful, the value
                                                    // of this output variable is undefined. 
        {
            SourceCustomList clone = new SourceCustomList
            {
                currentPosition = currentPosition,
                StringList = (String[])StringList.Clone()
            };
            ppenum = clone;
        }
    }
}
