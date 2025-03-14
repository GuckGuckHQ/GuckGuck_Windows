using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuckGuck;
public class Constants
{
	public static string AppName = "GuckGuck";
	public static string AppVersion = "0.0.1";
#if DEBUG
	public static string BaseUrl = "https://localhost:7179";
#else
	public static string BaseUrl = "http://guckguck.runasp.net";
#endif
}
