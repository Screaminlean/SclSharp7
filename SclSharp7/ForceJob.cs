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
        public class ForceJob
		{
			public string FullAdress
			{
				get
				{
					if (BitAdress == null)
					{
						return $"{ForceType} {ByteAdress}";
					}
					else
					{
						return $"{ForceType} {ByteAdress}.{BitAdress}";
					}
				}
			}
			public int ForceValue { get; set; }
			public string ForceType { get; set; }
			public int ByteAdress { get; set; }
			public int? BitAdress { get; set; }
			public string Symbol { get; set; }
			public string Comment { get; set; }

		}

        #region forcejob

        internal int GetActiveForces(List<ForceJob> forces, byte[] forceframe)
        {


            // sending second package only if there are force jobs active 
            SendPacket(forceframe);
            var length = RecvIsoPacket();

            switch (WordFromByteArr(PDU, 27))
            {
                default:
                    _LastError = S7Consts.errTCPDataReceive;
                    break;
                case 0x000:

                    // creating byte [] with length of useful data (first 67 bytes aren't useful data )
                    byte[] forceData = new byte[length - 67];
                    // copy pdu to other byte[] and remove the unused data 
                    Array.Copy(PDU, 67, forceData, 0, length - 67);
                    // check array transition definition > value's 
                    byte[] splitDefData = new byte[] { 0x00, 0x09, 0x00 };
                    int Splitposition = 0;
                    for (int x = 0; x < forceData.Length - 3; x = x + 6)
                    {
                        // checking when the definitions go to data (the data starts with split definition data and the amount of bytes before should always be a plural of 6)
                        if (forceData[x] == splitDefData[0] && forceData[x + 1] == splitDefData[1] && forceData[x + 2] == splitDefData[2] && x % 6 == 0)
                        {
                            Splitposition = x;
                            break;
                        }

                    }
                    // calculating amount of forces 
                    int amountForces = Splitposition / 6;
                    // setting first byte from data
                    int dataposition = Splitposition;
                    for (int x = 0; x < amountForces; x++)
                    {
                        ForceJob force = new ForceJob
                        {
                            // bit value
                            BitAdress = (forceData[(1 + (6 * x))]),

                            // byte value

                            ByteAdress = ((forceData[(4 + (6 * x))]) * 256) + (forceData[(5 + (6 * x))])
                        };
                        // foce identity
                        switch (forceData[0 + (6 * x)])
                        {

                            case 0x0:
                                force.ForceType = "M";
                                break;

                            case 0x1:
                                force.ForceType = "MB";
                                force.BitAdress = null;
                                break;
                            case 0x2:
                                force.ForceType = "MW";
                                force.BitAdress = null;
                                break;
                            case 0x3:
                                force.ForceType = "MD";
                                force.BitAdress = null;
                                break;

                            case 0x10:
                                force.ForceType = "I";
                                break;

                            case 0x11:
                                force.ForceType = "IB";
                                force.BitAdress = null;
                                break;

                            case 0x12:
                                force.ForceType = "IW";
                                force.BitAdress = null;
                                break;

                            case 0x13:
                                force.ForceType = "ID";
                                force.BitAdress = null;
                                break;


                            case 0x20:
                                force.ForceType = "Q";
                                break;

                            case 0x21:
                                force.ForceType = "QB";
                                force.BitAdress = null;
                                break;

                            case 0x22:
                                force.ForceType = "QW";
                                force.BitAdress = null;
                                break;

                            case 0x23:
                                force.ForceType = "QD";
                                force.BitAdress = null;
                                break;

                            // if you get this code You can add it in the list above.
                            default:
                                force.ForceType = forceData[0 + (6 * x)].ToString() + " unknown";
                                break;
                        }

                        // setting force value depending on the data length
                        switch (forceData[dataposition + 3])// Data length from force
                        {

                            case 0x01:
                                force.ForceValue = forceData[dataposition + 4];
                                break;

                            case 0x02:
                                force.ForceValue = WordFromByteArr(forceData, dataposition + 4);
                                break;

                            case 0x04:
                                force.ForceValue = DoubleFromByteArr(forceData, dataposition + 4);
                                break;

                            default:
                                break;


                        }

                        // calculating when the next force start 

                        var nextForce = 0x04 + (forceData[dataposition + 3]);
                        if (nextForce < 6)
                        {
                            nextForce = 6;
                        }
                        dataposition += nextForce;
                        // adding force to list 
                        forces.Add(force);
                    }
                    break;
            }
            return _LastError;
        }

        public int GetForceValues300(ref S7Forces forces)
        {
            _LastError = 00;
            int Elapsed = Environment.TickCount;
            List<ForceJob> forcedValues = new List<ForceJob>();
            SendPacket(S7_FORCE_VAL1);
            var Length = RecvIsoPacket();

            // when response is 45 there are no force jobs active or no correct response from plc

            switch (WordFromByteArr(PDU, 27))
            {
                case 0x0000:// no error code

                    if (WordFromByteArr(PDU, 31) >= 16)
                    {
                        _LastError = GetActiveForces(forcedValues, S7_FORCE_VAL300);
                    }
                    break;

                default:
                    _LastError = S7Consts.errTCPDataReceive;
                    break;
            }

            forces.Forces = forcedValues;

            Time_ms = Environment.TickCount - Elapsed;
            return _LastError;
        }

        public int GetForceValues400(ref S7Forces forces)
        {
            _LastError = 00;
            int Elapsed = Environment.TickCount;
            List<ForceJob> forcedValues = new List<ForceJob>();
            SendPacket(S7_FORCE_VAL1);
            var Length = RecvIsoPacket();

            // when response is 45 there are no force jobs active or no correct response from PLC

            switch (WordFromByteArr(PDU, 27))
            {
                case 0x0000:

                    if (WordFromByteArr(PDU, 31) >= 12)
                    {
                        _LastError = GetActiveForces(forcedValues, S7_FORCE_VAL400);
                    }
                    break;

                default:
                    _LastError = S7Consts.errTCPDataReceive;
                    break;
            }

            forces.Forces = forcedValues;
            Time_ms = Environment.TickCount - Elapsed;
            return _LastError;
        }

        public int WordFromByteArr(byte[] data, int position)
        {
            int result = Convert.ToInt32((data[position] << 8) + data[position + 1]);
            return result;
        }

        public int DoubleFromByteArr(byte[] data, int position)
        {
            int result = Convert.ToInt32((data[position] << 24) + (data[position + 1] << 16) + (data[position + 2] << 8) + (data[position + 3]));
            return result;
        }


        // S7 Get Force Values frame 1
        byte[] S7_FORCE_VAL1 = {
            0x03, 0x00, 0x00, 0x3d,
            0x02, 0xf0 ,0x80, 0x32,
            0x07, 0x00, 0x00, 0x07,
            0x00, 0x00, 0x0c, 0x00,
            0x20, 0x00, 0x01, 0x12,
            0x08, 0x12, 0x41, 0x10,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0xff, 0x09, 0x00,
            0x1c, 0x00, 0x14, 0x00,
            0x04, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x01, 0x00,
            0x00, 0x00, 0x01, 0x00,
            0x01, 0x00, 0x01, 0x00,
            0x01, 0x00, 0x01, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00

        };

        // S7 Get Force Values frame 2 (300 series )
        byte[] S7_FORCE_VAL300 = {
            0x03, 0x00, 0x00, 0x3b,
            0x02, 0xf0, 0x80, 0x32,
            0x07, 0x00, 0x00, 0x0c,
            0x00, 0x00, 0x0c, 0x00,
            0x1e, 0x00, 0x01, 0x12,
            0x08, 0x12, 0x41, 0x11,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0xff, 0x09, 0x00,
            0x1a, 0x00, 0x14, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x01, 0x00,
            0x00, 0x00, 0x01, 0x00,
            0x01, 0x00, 0x01, 0x00,
            0x01, 0x00, 0x01, 0x00,
            0x00, 0x09, 0x03

        };

        // S7 Get Force Values frame 2 (400 series )
        byte[] S7_FORCE_VAL400 = {
            0x03, 0x00, 0x00, 0x3b,
            0x02, 0xf0, 0x80, 0x32,
            0x07, 0x00, 0x00, 0x0c,
            0x00, 0x00, 0x0c, 0x00,
            0x1e, 0x00, 0x01, 0x12,
            0x08, 0x12, 0x41, 0x11,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0xff, 0x09, 0x00,
            0x1a, 0x00, 0x14, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x01, 0x00,
            0x00, 0x00, 0x01, 0x00,
            0x01, 0x00, 0x01, 0x00,
            0x01, 0x00, 0x01, 0x00,
            0x00, 0x09, 0x05
        };

        #endregion
    }
}
