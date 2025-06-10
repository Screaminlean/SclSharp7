//------------------------------------------------------------------------------
// If you are compiling for UWP verify that WINDOWS_UWP or NETFX_CORE are 
// defined into Project Properties->Build->Conditional compilation symbols
//------------------------------------------------------------------------------
#if WINDOWS_UWP || NETFX_CORE
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#else // <-- Including MONO
#endif

namespace SclSharp7
{
    public partial class S7Client
	{
        // S7 Nck Data Item
        public struct S7NckDataItem
		{
			public int NckArea;
			public int NckUnit;
			public int NckModule;
			public int WordLen;
			public int Result;
			public int ParameterNumber;
			public int Start;
			public int Amount;
			public IntPtr pData;
		}
	}
}
