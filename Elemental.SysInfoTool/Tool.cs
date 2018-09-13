using Elemental;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

class Tool
{
    public static void Main(string[] args)
    {
        var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        var isOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        if (isWindows)
            EnableConsoleColor();

        ColorConsole.SetForeground(0x40, 0x80, 0x20);
        Value("Tool Version", typeof(Tool).Assembly.GetName().Version.ToString());
        Header("Machine");
        Value("Architecture", RuntimeInformation.ProcessArchitecture.ToString());
        Value("Runtime Version", RuntimeEnvironment.GetSystemVersion());


        Value("MachineName", Environment.MachineName);
        var os =
            isWindows
            ? "Windows"
            : isLinux
            ? "Linux"
            : isOSX
            ? "OSX"
            : "Other";
        Value("OS", os);
        Value("OSVersion", Environment.OSVersion.ToString());

        Header("Time");
        Value("UTC Time", DateTime.UtcNow.ToString());
        Value("Local Time", DateTime.Now.ToString());
        Value("TimeZone", TimeZoneInfo.Local.StandardName);

        Header("Region/Culture");
        Value("Region", RegionInfo.CurrentRegion.Name);
        Value("Culture", CultureInfo.CurrentCulture.Name);
        Value("UICulture", CultureInfo.CurrentUICulture.Name);

        Header("User");
        Value("Domain", Environment.UserDomainName);
        Value("User", Environment.UserName);

        Header("Network");

        {
            bool first = true;
            foreach (var net in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (net.OperationalStatus == OperationalStatus.Up && net.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    if (first) first = false;
                    else Console.WriteLine();
                    Value("Type", net.NetworkInterfaceType.ToString());
                    Value("Description", net.Description);
                    var props = net.GetIPProperties();
                    foreach (var addr in props.UnicastAddresses)
                    {
                        Value(addr.Address.AddressFamily.ToString(), addr.Address.ToString());
                    }
                }
            }
        }

        Header("Environment");

        foreach (var kvp in Environment.GetEnvironmentVariables().Cast<DictionaryEntry>().OrderBy(kvp => kvp.Key))
        {
            var key = (string)kvp.Key;
            var value = (string)kvp.Value;

            if (StringComparer.OrdinalIgnoreCase.Equals(key, "LS_COLORS"))
            {
                LSColors(key, value);
            }
            else
            {
                if (SplitEnvVars.Contains(key))
                {
                    var separator = isWindows ? ';' : ':';
                    var values = value.Split(separator);
                    Value(key, values, EnvVarWidth);
                }
                else
                {
                    Value(key, value, EnvVarWidth);
                }
            }
        }

        ColorConsole.SetDefaults();
    }

    static void LSColors(string key, string value)
    {
        var items = value.Split(':');
        ColorConsole.SetForeground(0x60, 0xc0, 0x60);
        Console.Write(String.Format("{0," + EnvVarWidth + "}", key));

        ColorConsole.SetForeground(0xa0, 0xa0, 0xa0);
        Console.Write(": ");

        ColorConsole.SetForeground(0xe0, 0xe0, 0xe0);

        bool first = true;
        foreach (var item in items)
        {
            if (first) first = false;
            else
            {
                Console.Write(new string(' ', EnvVarWidth + 2));
            }

            Console.Write($"{item,-20}");
            Console.Write(" ");
            var idx = item.IndexOf('=');
            if (idx > 0)
            {
                var itemKey = item.Substring(0, idx);
                var itemVal = item.Substring(idx + 1);
                var parts = itemVal.Split(';');
                foreach (var part in parts)
                {
                    ColorConsole.SetCode(part);
                }

                Console.Write("(Color)");

                ColorConsole.SetDefaults();
                if (LSKeys.TryGetValue(itemKey, out string desc))
                {
                    Console.Write(" ");
                    Console.Write(desc);
                }
            }

            Console.WriteLine();
        }

        ColorConsole.SetDefaults();
    }

    const int HeaderWidth = 80;
    const int DefaultNameWidth = 16;
    const int EnvVarWidth = 32;
    const char HeaderChar = '-';
    const int PrePadCount = 3;
    static readonly string PrePad = new string(HeaderChar, PrePadCount);

    // environment variables to split.
    static HashSet<string> SplitEnvVars = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Path", "PSModulePath", "PathEXT" };

    static Dictionary<string, string> LSKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        // from http://www.bigsoft.co.uk/blog/2008/04/11/configuring-ls_colors
        ["no"] = "normal",
        ["fi"] = "file",
        ["di"] = "directory",
        ["ln"] = "symlink",
        ["pi"] = "pipe",
        ["do"] = "door",
        ["bd"] = "block device",
        ["cd"] = "character device",
        ["or"] = "orphan",
        ["so"] = "socket",
        ["su"] = "setuid",
        ["sg"] = "setgid",
        ["tw"] = "sticky other writable",
        ["ow"] = "other writable",
        ["st"] = "sticky",
        ["ex"] = "executable",
        ["mi"] = "missing",
        ["lc"] = "left code",
        ["rc"] = "right code",
        ["ec"] = "end code",
    };
    static void EnableConsoleColor()
    {
        try
        {
            ColorConsole.EnableColorMode();
        }
        catch (Exception) {
            // this might throw on linux/osx
            // but we don't need to call
        }
    }

    static void Header(string heading)
    {
        Console.WriteLine();
        ColorConsole.SetForeground(0x40, 0xa0, 0xf0);

        var l = HeaderWidth - 2 - PrePadCount - heading.Length;

        Console.Write(PrePad);
        Console.Write(' ');
        Console.Write(heading);
        Console.Write(' ');
        if (l > 0)
        {
            Console.Write(new string(HeaderChar, l));
        }
        Console.WriteLine();
        ColorConsole.SetDefaults();
    }

    static void Value(string name, string value, int width = DefaultNameWidth)
    {
        Value(name, new string[] { value }, width);
    }

    static void Value(string name, IEnumerable<string> values, int width = DefaultNameWidth)
    {
        ColorConsole.SetForeground(0x60, 0xc0, 0x60);
        Console.Write(String.Format("{0," + width + "}", name));

        ColorConsole.SetForeground(0xa0, 0xa0, 0xa0);
        Console.Write(": ");

        ColorConsole.SetForeground(0xe0, 0xe0, 0xe0);

        bool first = true;
        foreach(var value in values)
        {
            if (first) first = false;
            else
            {
                Console.Write(new string(' ', width + 2));
            }
            Console.Write(value);
            Console.WriteLine();
        }
        
        ColorConsole.SetDefaults();
    }
}
