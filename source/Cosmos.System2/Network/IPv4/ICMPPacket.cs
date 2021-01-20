﻿/*
* PROJECT:          Aura Operating System Development
* CONTENT:          ICMP Packet (to ping for exemple)
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System;

namespace Cosmos.System.Network.IPv4
{
    /// <summary>
    /// ICMPPacket class. See also: <seealso cref="IPPacket"/>.
    /// </summary>
    public class ICMPPacket : IPPacket
    {
        /// <summary>
        /// Packet type.
        /// </summary>
        protected byte icmpType;
        /// <summary>
        /// Packet code.
        /// </summary>
        protected byte icmpCode;
        /// <summary>
        /// Packet CRC.
        /// </summary>
        protected ushort icmpCRC;
        /// <summary>
        /// Received reply.
        /// </summary>

        /// <summary>
        /// Create new inctanse of the <see cref="ICMPPacket"/> class.
        /// </summary>
        /// <param name="packetData">Packet data.</param>
        /// <exception cref="ArgumentException">Thrown if packetData is invalid.</exception>
        internal static void ICMPHandler(byte[] packetData)
        {
            Global.mDebugger.Send("ICMP Handler called");
            ICMPPacket icmp_packet = new ICMPPacket(packetData);
            switch (icmp_packet.ICMP_Type)
            {
                case 0:
                    ICMPClient receiver = ICMPClient.Client(icmp_packet.SourceIP.Hash);
                    if (receiver != null)
                    {
                        receiver.receiveData(icmp_packet);
                    }
                    Global.mDebugger.Send("Received ICMP Echo reply from " + icmp_packet.SourceIP.ToString());
                    break;
                case 8:
                    ICMPEchoRequest request = new ICMPEchoRequest(packetData);
                    ICMPEchoReply reply = new ICMPEchoReply(request);
                    Global.mDebugger.Send("Sending ICMP Echo reply to " + reply.DestinationIP.ToString());
                    OutgoingBuffer.AddPacket(reply);
                    NetworkStack.Update();
                    break;
            }
        }

        /// <summary>
        /// Create new inctanse of the <see cref="ICMPPacket"/> class.
        /// </summary>
        internal ICMPPacket()
            : base()
        { }

        /// <summary>
        /// Create new inctanse of the <see cref="ICMPPacket"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        internal ICMPPacket(byte[] rawData)
            : base(rawData)
        {
        }

        /// <summary>
        /// Init ICMPPacket fields.1
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        protected override void initFields()
        {
            //Sys.Console.WriteLine("ICMPPacket.initFields() called;");
            base.initFields();
            icmpType = RawData[DataOffset];
            icmpCode = RawData[DataOffset + 1];
            icmpCRC = (ushort)((RawData[DataOffset + 2] << 8) | RawData[DataOffset + 3]);
        }

        /// <summary>
        /// Create new inctanse of the <see cref="ICMPPacket"/> class.
        /// </summary>
        /// <param name="source">Source address.</param>
        /// <param name="dest">Destination address.</param>
        /// <param name="type">Type.</param>
        /// <param name="code">Code.</param>
        /// <param name="id">ID.</param>
        /// <param name="seq">SEQ.</param>
        /// <param name="icmpDataSize">Data size.</param>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        internal ICMPPacket(Address source, Address dest, byte type, byte code, ushort id, ushort seq, ushort icmpDataSize)
            : base(icmpDataSize, 1, source, dest, 0x00)
        {
            RawData[DataOffset] = type;
            RawData[DataOffset + 1] = code;
            RawData[DataOffset + 2] = 0x00;
            RawData[DataOffset + 3] = 0x00;
            RawData[DataOffset + 4] = (byte)((id >> 8) & 0xFF);
            RawData[DataOffset + 5] = (byte)((id >> 0) & 0xFF);
            RawData[DataOffset + 6] = (byte)((seq >> 8) & 0xFF);
            RawData[DataOffset + 7] = (byte)((seq >> 0) & 0xFF);

            icmpCRC = CalcICMPCRC((ushort)(icmpDataSize));

            RawData[DataOffset + 2] = (byte)((icmpCRC >> 8) & 0xFF);
            RawData[DataOffset + 3] = (byte)((icmpCRC >> 0) & 0xFF);
            initFields();
        }

        /// <summary>
        /// Calculate ICMP CRC3.
        /// </summary>
        /// <param name="length">Lenght.</param>
        /// <returns></returns>
        protected ushort CalcICMPCRC(ushort length)
        {
            return CalcOcCRC(DataOffset, length);
        }

        /// <summary>
        /// Get ICMP type.
        /// </summary>
        internal byte ICMP_Type
        {
            get { return icmpType; }
        }
        /// <summary>
        /// Get ICMP code.
        /// </summary>
        internal byte ICMP_Code
        {
            get { return icmpCode; }
        }
        /// <summary>
        /// Get ICMP CRC.
        /// </summary>
        internal ushort ICMP_CRC
        {
            get { return icmpCRC; }
        }
        /// <summary>
        /// Get ICMP data length.
        /// </summary>
        internal ushort ICMP_DataLength
        {
            get { return (ushort)(DataLength - 8); }
        }

        /// <summary>
        /// Get ICMP data.
        /// </summary>
        /// <returns>byte array value.</returns>
        internal byte[] GetICMPData()
        {
            byte[] data = new byte[ICMP_DataLength];

            for (int b = 0; b < ICMP_DataLength; b++)
            {
                data[b] = RawData[DataOffset + 8 + b];
            }

            return data;
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>string value.</returns>
        public override string ToString()
        {
            return "ICMP Packet Src=" + SourceIP + ", Dest=" + DestinationIP + ", Type=" + icmpType + ", Code=" + icmpCode;
        }
    }

    /// <summary>
    /// ICMPEchoRequest class. See also: <seealso cref="ICMPPacket"/>.
    /// </summary>
    internal class ICMPEchoRequest : ICMPPacket
    {
        protected ushort icmpID;
        protected ushort icmpSequence;

        /// <summary>
        /// Create new inctanse of the <see cref="ICMPEchoRequest"/> class.
        /// </summary>
        internal ICMPEchoRequest()
            : base()
        { }

        /// <summary>
        /// Create new inctanse of the <see cref="ICMPEchoRequest"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        internal ICMPEchoRequest(byte[] rawData)
            : base(rawData)
        {
        }

        internal ICMPEchoRequest(Address source, Address dest, ushort id, ushort sequence)
            : base(source, dest, 8, 0, id, sequence, 40)
        {
            for (int b = 8; b < ICMP_DataLength; b++)
            {
                RawData[DataOffset + b] = (byte)b;
            }

            RawData[DataOffset + 2] = 0x00;
            RawData[DataOffset + 3] = 0x00;
            icmpCRC = CalcICMPCRC((ushort)(ICMP_DataLength + 8));
            RawData[DataOffset + 2] = (byte)((icmpCRC >> 8) & 0xFF);
            RawData[DataOffset + 3] = (byte)((icmpCRC >> 0) & 0xFF);
        }

        protected override void initFields()
        {
            //Sys.Console.WriteLine("ICMPEchoRequest.initFields() called;");
            base.initFields();
            icmpID = (ushort)((RawData[DataOffset + 4] << 8) | RawData[DataOffset + 5]);
            icmpSequence = (ushort)((RawData[DataOffset + 6] << 8) | RawData[DataOffset + 7]);
        }

        internal ushort ICMP_ID
        {
            get { return icmpID; }
        }
        internal ushort ICMP_Sequence
        {
            get { return icmpSequence; }
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>string value.</returns>
        public override string ToString()
        {
            return "ICMP Echo Request Src=" + SourceIP + ", Dest=" + DestinationIP + ", ID=" + icmpID + ", Sequence=" + icmpSequence;
        }
    }

    /// <summary>
    /// ICMPEchoReply class. See also: <seealso cref="ICMPPacket"/>.
    /// </summary>
    internal class ICMPEchoReply : ICMPPacket
    {
        protected ushort icmpID;
        protected ushort icmpSequence;

        /// <summary>
        /// Create new inctanse of the <see cref="ICMPEchoReply"/> class.
        /// </summary>
        internal ICMPEchoReply()
            : base()
        { }

        /// <summary>
        /// Create new inctanse of the <see cref="ICMPEchoReply"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        internal ICMPEchoReply(byte[] rawData)
            : base(rawData)
        {
        }

        /// <summary>
        /// Init ICMPEchoReply fields.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        protected override void initFields()
        {
            //Sys.Console.WriteLine("ICMPEchoReply.initFields() called;");
            base.initFields();
            icmpID = (ushort)((RawData[DataOffset + 4] << 8) | RawData[DataOffset + 5]);
            icmpSequence = (ushort)((RawData[DataOffset + 6] << 8) | RawData[DataOffset + 7]);
        }

        /// <summary>
        /// Create new inctanse of the <see cref="ICMPEchoReply"/> class.
        /// </summary>
        /// <param name="request">ICMP echo request.</param>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        internal ICMPEchoReply(ICMPEchoRequest request)
            : base(request.DestinationIP, request.SourceIP, 0, 0, request.ICMP_ID, request.ICMP_Sequence, (ushort)(request.ICMP_DataLength))
        {
            for (int b = 0; b < ICMP_DataLength; b++)
            {
                RawData[DataOffset + 8 + b] = request.RawData[DataOffset + 8 + b];
            }

            RawData[DataOffset + 2] = 0x00;
            RawData[DataOffset + 3] = 0x00;
            icmpCRC = CalcICMPCRC((ushort)(ICMP_DataLength + 8));
            RawData[DataOffset + 2] = (byte)((icmpCRC >> 8) & 0xFF);
            RawData[DataOffset + 3] = (byte)((icmpCRC >> 0) & 0xFF);
        }

        /// <summary>
        /// Get ICMP ID.
        /// </summary>
        internal ushort ICMP_ID
        {
            get { return icmpID; }
        }
        /// <summary>
        /// Get ICMP sequence.
        /// </summary>
        internal ushort ICMP_Sequence
        {
            get { return icmpSequence; }
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>string value.</returns>
        public override string ToString()
        {
            return "ICMP Echo Reply Src=" + SourceIP + ", Dest=" + DestinationIP + ", ID=" + icmpID + ", Sequence=" + icmpSequence;
        }
    }
}
