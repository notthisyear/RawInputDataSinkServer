using System.Runtime.InteropServices;

namespace RawInputDataSinkServer
{
    delegate nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam);

    [StructLayout(LayoutKind.Sequential)]
    readonly struct WNDCLASS
    {
        /// <summary>
        /// The class style(s). This member can be any combination of the Class Styles.
        /// </summary>
        public uint Style { get; init; }

        /// <summary>
        /// A pointer to the window procedure. You must use the CallWindowProc function to call the window procedure. For more information, see WindowProc.
        /// </summary>
        public WndProc WndProc { get; init; }
        /// <summary>
        /// The number of extra bytes to allocate following the window-class structure. The system initializes the bytes to zero.
        /// </summary>
        public int ClsExtra { get; init; }

        /// <summary>
        /// The number of extra bytes to allocate following the window instance. The system initializes the bytes to zero. If an application uses WNDCLASS to register a dialog box created by using the CLASS directive in the resource file, it must set this member to DLGWINDOWEXTRA.
        /// </summary>
        public int WndExtra { get; init; }

        /// <summary>
        /// A handle to the instance that contains the window procedure for the class.
        /// </summary>
        public nint Instance { get; init; }

        /// <summary>
        /// A handle to the class icon. This member must be a handle to an icon resource. If this member is NULL, the system provides a default icon.
        /// </summary>
        public nint Icon { get; init; }

        /// <summary>
        /// A handle to the class cursor. This member must be a handle to a cursor resource. If this member is NULL, an application must explicitly set the cursor shape whenever the mouse moves into the application's window.
        /// </summary>
        public nint Cursor { get; init; }

        /// <summary>
        /// A handle to the class background brush. This member can be a handle to the physical brush to be used for painting the background, or it can be a color value. A color value must be one of the following standard system colors (the value 1 must be added to the chosen color).
        /// </summary>
        public nint Background { get; init; }

        /// <summary>
        /// The resource name of the class menu, as the name appears in the resource file. If you use an integer to identify the menu, use the MAKEINTRESOURCE macro. If this member is NULL, windows belonging to this class have no default menu.
        /// </summary>
        public string MenuName { get; init; }

        /// <summary>
        /// <para>A pointer to a null-terminated string or is an atom. If this parameter is an atom, it must be a class atom created by a previous call to the RegisterClass or RegisterClassEx function. The atom must be in the low-order word of lpszClassName; the high-order word must be zero.</para>
        ///<para>If lpszClassName is a string, it specifies the window class name. The class name can be any name registered with RegisterClass or RegisterClassEx, or any of the predefined control-class names.</para>
        ///<para>The maximum length for lpszClassName is 256. If lpszClassName is greater than the maximum length, the RegisterClass function will fail.</para>
        /// </summary>
        public nint ClassName { get; init; }
    }
}
