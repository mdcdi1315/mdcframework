// Add the required includes.

using System;
using System.Text;
using System.Security.Cryptography;
using System.Runtime.Serialization;

namespace ExternalHashCaculators 
{
	// NullFX original CRC hashing algorithm.
	
	// License comment is included as well:
	
	/*
	 Author:
	       steve whitley <steve@nullfx.com>
	
	 Copyright (c) 2017 
	
	 Permission is hereby granted, free of charge, to any person obtaining a copy
	 of this software and associated documentation files (the "Software"), to deal
	 in the Software without restriction, including without limitation the rights
	 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	 copies of the Software, and to permit persons to whom the Software is
	 furnished to do so, subject to the following conditions:
	
	 The above copyright notice and this permission notice shall be included in
	 all copies or substantial portions of the Software.
	
	 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	 THE SOFTWARE.
	*/
	
	public class NullFXCRC 
	{
		
		public enum Crc16Algorithm 
		{
			/// <summary>
			/// Performs CRC 16 using x^16 + x^15 + x^2 + 1 polynomial with an initial CRC value of 0
			/// </summary>
			Standard,
			/// <summary>
			/// A CRC 16 CCITT Utility using x^16 + x^15 + x^2 + 1 polynomial with an initial CRC value of 0 (used in XMODEM, Bluetooth PACTOR, SD, DigRF and other communication)
			/// </summary>
			Ccitt,
			/// <summary>
			/// Performs CRC 16 CCITT using a reversed x^16 + x^15 + x^2 + 1 polynomial with an initial CRC value of 0
			/// </summary>
			CcittKermit,
			/// <summary>
			/// Performs CRC 16 CCITT using x^16 + x^15 + x^2 + 1 polynomial with an initial CRC value of 0xffff
			/// </summary>
			CcittInitialValue0xFFFF,
			/// <summary>
			/// Performs CRC 16 CCITT using x^16 + x^15 + x^2 + 1 polynomial with an initial CRC value of 0x1D0F
			/// </summary>
			CcittInitialValue0x1D0F,
			/// <summary>
			/// Performs CRC 16 using reversed x^16 + x^13 + x^12 + x^11 + x^10 + x^8 + x^6 + x^5 + x^2 + 1 (0xA6BC) with an initial CRC value of 0 (used in Distributed Network Protocol communication)
			/// </summary>
			Dnp,
			/// <summary>
			/// Performs CRC 16 using x^16 + x^15 + x^2 + 1 polynomial with an initial CRC value of 0xffff (used in Modbus communication)
			/// </summary>
			Modbus
		}
		
		[Serializable]
		public class UnknownAlgorithmException : Exception 
		{
			public Crc16Algorithm Algorithm { get; set; }

			public UnknownAlgorithmException ( ) {
			}

			public UnknownAlgorithmException ( Crc16Algorithm algorithm ) {
				this.Algorithm = algorithm;
			}

			public UnknownAlgorithmException ( string message ) : base ( message ) {
			}

			public UnknownAlgorithmException ( string message, Exception innerException ) : base ( message, innerException ) {
			}

			protected UnknownAlgorithmException ( SerializationInfo info, StreamingContext context ) : base ( info, context ) {
			}
		}
		
		public static class CRC8 
		{
			static byte[] table = { 0x00, 0xD5, 0x7F, 0xAA, 0xFE, 0x2B, 0x81, 0x54, 0x29, 0xFC, 0x56, 0x83, 0xD7, 0x02, 0xA8, 0x7D, 0x52, 0x87, 0x2D, 0xF8, 0xAC, 0x79, 0xD3, 0x06, 0x7B, 0xAE, 0x04, 0xD1, 0x85, 0x50, 0xFA, 0x2F, 0xA4, 0x71, 0xDB, 0x0E, 0x5A, 0x8F, 0x25, 0xF0, 0x8D, 0x58, 0xF2, 0x27, 0x73, 0xA6, 0x0C, 0xD9, 0xF6, 0x23, 0x89, 0x5C, 0x08, 0xDD, 0x77, 0xA2, 0xDF, 0x0A, 0xA0, 0x75, 0x21, 0xF4, 0x5E, 0x8B, 0x9D, 0x48, 0xE2, 0x37, 0x63, 0xB6, 0x1C, 0xC9, 0xB4, 0x61, 0xCB, 0x1E, 0x4A, 0x9F, 0x35, 0xE0, 0xCF, 0x1A, 0xB0, 0x65, 0x31, 0xE4, 0x4E, 0x9B, 0xE6, 0x33, 0x99, 0x4C, 0x18, 0xCD, 0x67, 0xB2, 0x39, 0xEC, 0x46, 0x93, 0xC7, 0x12, 0xB8, 0x6D, 0x10, 0xC5, 0x6F, 0xBA, 0xEE, 0x3B, 0x91, 0x44, 0x6B, 0xBE, 0x14, 0xC1, 0x95, 0x40, 0xEA, 0x3F, 0x42, 0x97, 0x3D, 0xE8, 0xBC, 0x69, 0xC3, 0x16, 0xEF, 0x3A, 0x90, 0x45, 0x11, 0xC4, 0x6E, 0xBB, 0xC6, 0x13, 0xB9, 0x6C, 0x38, 0xED, 0x47, 0x92, 0xBD, 0x68, 0xC2, 0x17, 0x43, 0x96, 0x3C, 0xE9, 0x94, 0x41, 0xEB, 0x3E, 0x6A, 0xBF, 0x15, 0xC0, 0x4B, 0x9E, 0x34, 0xE1, 0xB5, 0x60, 0xCA, 0x1F, 0x62, 0xB7, 0x1D, 0xC8, 0x9C, 0x49, 0xE3, 0x36, 0x19, 0xCC, 0x66, 0xB3, 0xE7, 0x32, 0x98, 0x4D, 0x30, 0xE5, 0x4F, 0x9A, 0xCE, 0x1B, 0xB1, 0x64, 0x72, 0xA7, 0x0D, 0xD8, 0x8C, 0x59, 0xF3, 0x26, 0x5B, 0x8E, 0x24, 0xF1, 0xA5, 0x70, 0xDA, 0x0F, 0x20, 0xF5, 0x5F, 0x8A, 0xDE, 0x0B, 0xA1, 0x74, 0x09, 0xDC, 0x76, 0xA3, 0xF7, 0x22, 0x88, 0x5D, 0xD6, 0x03, 0xA9, 0x7C, 0x28, 0xFD, 0x57, 0x82, 0xFF, 0x2A, 0x80, 0x55, 0x01, 0xD4, 0x7E, 0xAB, 0x84, 0x51, 0xFB, 0x2E, 0x7A, 0xAF, 0x05, 0xD0, 0xAD, 0x78, 0xD2, 0x07, 0x53, 0x86, 0x2C, 0xF9 };
			private static byte InitialValue = byte.MinValue;
			/// <summary>
			/// Computes the CRC 8 checksum of the specified bytes
			/// </summary>
			/// <param name="bytes">The buffer to compute the CRC upon</param>
			/// <returns>The specified CRC</returns>
			public static byte ComputeChecksum ( params byte[] bytes ) {
				return ComputeChecksum ( bytes, 0, bytes?.Length ?? 0 );
			}

			/// <summary>
			/// Computes the CRC 8 of the specified byte range
			/// </summary>
			/// <param name="bytes">The buffer to compute the CRC upon</param>
			/// <param name="start">The start index upon which to compute the CRC</param>
			/// <param name="length">The length of the buffer upon which to compute the CRC</param>
			/// <returns>The specified CRC</returns>
			public static byte ComputeChecksum ( byte[] bytes, int start, int length ) {
				if ( bytes == null ) { throw new ArgumentNullException ( nameof ( bytes ) ); }
				if ( bytes.Length == 0 ) { throw new ArgumentOutOfRangeException ( nameof ( bytes ) ); }
				if ( start < 0 ) { throw new ArgumentOutOfRangeException ( nameof ( start ) ); }
				if ( start >= bytes.Length && length > 1 ) { throw new ArgumentOutOfRangeException ( nameof ( start ) ); }
				var crc = InitialValue;
				var end = start + length - 1;
				if ( end > bytes.Length ) { throw new ArgumentOutOfRangeException ( nameof ( length ) ); }
				if ( length < 0 ) { throw new ArgumentOutOfRangeException ( nameof ( length ) ); }
				for ( int i = start; i <= end; ++i ) {
					crc = table[crc ^ bytes[i]];
				}
				return crc;
			}
		}
		
		public static class CRC16 
		{
			/// <summary>
			/// A CRC 16 Utility using reversed x^16 + x^15 + x^2 + 1 (0xA001) polynomial with an initial CRC value of 0
			/// </summary>
			internal static class StandardCrc16Impl 
			{
				// pre-computed table using reversed x^16 + x^15 + x^2 + 1 poly / 0xA001
				static ushort[] table = { 0x0000, 0xC0C1, 0xC181, 0x0140, 0xC301, 0x03C0, 0x0280, 0xC241, 0xC601, 0x06C0, 0x0780, 0xC741, 0x0500, 0xC5C1, 0xC481, 0x0440, 0xCC01, 0x0CC0, 0x0D80, 0xCD41, 0x0F00, 0xCFC1, 0xCE81, 0x0E40, 0x0A00, 0xCAC1, 0xCB81, 0x0B40, 0xC901, 0x09C0, 0x0880, 0xC841, 0xD801, 0x18C0, 0x1980, 0xD941, 0x1B00, 0xDBC1, 0xDA81, 0x1A40, 0x1E00, 0xDEC1, 0xDF81, 0x1F40, 0xDD01, 0x1DC0, 0x1C80, 0xDC41, 0x1400, 0xD4C1, 0xD581, 0x1540, 0xD701, 0x17C0, 0x1680, 0xD641, 0xD201, 0x12C0, 0x1380, 0xD341, 0x1100, 0xD1C1, 0xD081, 0x1040, 0xF001, 0x30C0, 0x3180, 0xF141, 0x3300, 0xF3C1, 0xF281, 0x3240, 0x3600, 0xF6C1, 0xF781, 0x3740, 0xF501, 0x35C0, 0x3480, 0xF441, 0x3C00, 0xFCC1, 0xFD81, 0x3D40, 0xFF01, 0x3FC0, 0x3E80, 0xFE41, 0xFA01, 0x3AC0, 0x3B80, 0xFB41, 0x3900, 0xF9C1, 0xF881, 0x3840, 0x2800, 0xE8C1, 0xE981, 0x2940, 0xEB01, 0x2BC0, 0x2A80, 0xEA41, 0xEE01, 0x2EC0, 0x2F80, 0xEF41, 0x2D00, 0xEDC1, 0xEC81, 0x2C40, 0xE401, 0x24C0, 0x2580, 0xE541, 0x2700, 0xE7C1, 0xE681, 0x2640, 0x2200, 0xE2C1, 0xE381, 0x2340, 0xE101, 0x21C0, 0x2080, 0xE041, 0xA001, 0x60C0, 0x6180, 0xA141, 0x6300, 0xA3C1, 0xA281, 0x6240, 0x6600, 0xA6C1, 0xA781, 0x6740, 0xA501, 0x65C0, 0x6480, 0xA441, 0x6C00, 0xACC1, 0xAD81, 0x6D40, 0xAF01, 0x6FC0, 0x6E80, 0xAE41, 0xAA01, 0x6AC0, 0x6B80, 0xAB41, 0x6900, 0xA9C1, 0xA881, 0x6840, 0x7800, 0xB8C1, 0xB981, 0x7940, 0xBB01, 0x7BC0, 0x7A80, 0xBA41, 0xBE01, 0x7EC0, 0x7F80, 0xBF41, 0x7D00, 0xBDC1, 0xBC81, 0x7C40, 0xB401, 0x74C0, 0x7580, 0xB541, 0x7700, 0xB7C1, 0xB681, 0x7640, 0x7200, 0xB2C1, 0xB381, 0x7340, 0xB101, 0x71C0, 0x7080, 0xB041, 0x5000, 0x90C1, 0x9181, 0x5140, 0x9301, 0x53C0, 0x5280, 0x9241, 0x9601, 0x56C0, 0x5780, 0x9741, 0x5500, 0x95C1, 0x9481, 0x5440, 0x9C01, 0x5CC0, 0x5D80, 0x9D41, 0x5F00, 0x9FC1, 0x9E81, 0x5E40, 0x5A00, 0x9AC1, 0x9B81, 0x5B40, 0x9901, 0x59C0, 0x5880, 0x9841, 0x8801, 0x48C0, 0x4980, 0x8941, 0x4B00, 0x8BC1, 0x8A81, 0x4A40, 0x4E00, 0x8EC1, 0x8F81, 0x4F40, 0x8D01, 0x4DC0, 0x4C80, 0x8C41, 0x4400, 0x84C1, 0x8581, 0x4540, 0x8701, 0x47C0, 0x4680, 0x8641, 0x8201, 0x42C0, 0x4380, 0x8341, 0x4100, 0x81C1, 0x8081, 0x4040 };
				private const ushort StandardInitialValue = ushort.MinValue;
				/// <summary>
				/// Computes a CRC 16 of the provided bytes
				/// </summary>
				/// <param name="bytes">bytes to crc</param>
				/// <returns>the CRC 16 of the provided bytes</returns>
				internal static ushort ComputeChecksumInternal ( params byte[] bytes ) {
					return ComputeChecksumInternal ( bytes, 0, bytes?.Length ?? 0 );
				}

				/// <summary>
				/// Computes a CRC 16 of the provided bytes
				/// </summary>
				/// <param name="bytes">The buffer to compute the CRC upon</param>
				/// <param name="start">The start index upon which to compute the CRC</param>
				/// <param name="length">The length of the buffer upon which to compute the CRC</param>
				/// <returns>The specified CRC</returns>
				internal static ushort ComputeChecksumInternal ( byte[] bytes, int start, int length, ushort initialValue = StandardInitialValue ) {
					if ( bytes == null ) { throw new ArgumentNullException ( nameof ( bytes ) ); }
					if ( bytes.Length == 0 ) { throw new ArgumentOutOfRangeException ( nameof ( bytes ) ); }
					if ( start < 0 ) { throw new ArgumentOutOfRangeException ( nameof ( start ) ); }
					if ( start >= bytes.Length && length > 1 ) { throw new ArgumentOutOfRangeException ( nameof ( start ) ); }
					var crc = initialValue;
					var end = start + length - 1;
					if ( end > bytes.Length ) { throw new ArgumentOutOfRangeException ( nameof ( length ) ); }
					if ( length < 0 ) { throw new ArgumentOutOfRangeException ( nameof ( length ) ); }
					for ( int i = start; i <= end; ++i ) {
						crc = ( ushort )( ( crc >> 8 ) ^ table[( byte )( crc ^ bytes[i] )] );
					}
					return crc;
				}
			}

			/// <summary>
			/// A CRC 16 Distributed Network Protocol (DNP) utility using reversed x^16 + x^13 + x^12 + x^11 + x^10 + x^8 + x^6 + x^5 + x^2 + 1 (0xA6BC) polynomial with an initial CRC value of 0
			/// </summary>
			internal static class DnpCrc16Impl 
			{
				// pre-computed table using reversed x^16 + x^13 + x^12 + x^11 + x^10 + x^8 + x^6 + x^5 + x^2 + 1 poly / 0xA6BC
				static ushort[] table = { 0x0000, 0x365E, 0x6CBC, 0x5AE2, 0xD978, 0xEF26, 0xB5C4, 0x839A, 0xFF89, 0xC9D7, 0x9335, 0xA56B, 0x26F1, 0x10AF, 0x4A4D, 0x7C13, 0xB26B, 0x8435, 0xDED7, 0xE889, 0x6B13, 0x5D4D, 0x07AF, 0x31F1, 0x4DE2, 0x7BBC, 0x215E, 0x1700, 0x949A, 0xA2C4, 0xF826, 0xCE78, 0x29AF, 0x1FF1, 0x4513, 0x734D, 0xF0D7, 0xC689, 0x9C6B, 0xAA35, 0xD626, 0xE078, 0xBA9A, 0x8CC4, 0x0F5E, 0x3900, 0x63E2, 0x55BC, 0x9BC4, 0xAD9A, 0xF778, 0xC126, 0x42BC, 0x74E2, 0x2E00, 0x185E, 0x644D, 0x5213, 0x08F1, 0x3EAF, 0xBD35, 0x8B6B, 0xD189, 0xE7D7, 0x535E, 0x6500, 0x3FE2, 0x09BC, 0x8A26, 0xBC78, 0xE69A, 0xD0C4, 0xACD7, 0x9A89, 0xC06B, 0xF635, 0x75AF, 0x43F1, 0x1913, 0x2F4D, 0xE135, 0xD76B, 0x8D89, 0xBBD7, 0x384D, 0x0E13, 0x54F1, 0x62AF, 0x1EBC, 0x28E2, 0x7200, 0x445E, 0xC7C4, 0xF19A, 0xAB78, 0x9D26, 0x7AF1, 0x4CAF, 0x164D, 0x2013, 0xA389, 0x95D7, 0xCF35, 0xF96B, 0x8578, 0xB326, 0xE9C4, 0xDF9A, 0x5C00, 0x6A5E, 0x30BC, 0x06E2, 0xC89A, 0xFEC4, 0xA426, 0x9278, 0x11E2, 0x27BC, 0x7D5E, 0x4B00, 0x3713, 0x014D, 0x5BAF, 0x6DF1, 0xEE6B, 0xD835, 0x82D7, 0xB489, 0xA6BC, 0x90E2, 0xCA00, 0xFC5E, 0x7FC4, 0x499A, 0x1378, 0x2526, 0x5935, 0x6F6B, 0x3589, 0x03D7, 0x804D, 0xB613, 0xECF1, 0xDAAF, 0x14D7, 0x2289, 0x786B, 0x4E35, 0xCDAF, 0xFBF1, 0xA113, 0x974D, 0xEB5E, 0xDD00, 0x87E2, 0xB1BC, 0x3226, 0x0478, 0x5E9A, 0x68C4, 0x8F13, 0xB94D, 0xE3AF, 0xD5F1, 0x566B, 0x6035, 0x3AD7, 0x0C89, 0x709A, 0x46C4, 0x1C26, 0x2A78, 0xA9E2, 0x9FBC, 0xC55E, 0xF300, 0x3D78, 0x0B26, 0x51C4, 0x679A, 0xE400, 0xD25E, 0x88BC, 0xBEE2, 0xC2F1, 0xF4AF, 0xAE4D, 0x9813, 0x1B89, 0x2DD7, 0x7735, 0x416B, 0xF5E2, 0xC3BC, 0x995E, 0xAF00, 0x2C9A, 0x1AC4, 0x4026, 0x7678, 0x0A6B, 0x3C35, 0x66D7, 0x5089, 0xD313, 0xE54D, 0xBFAF, 0x89F1, 0x4789, 0x71D7, 0x2B35, 0x1D6B, 0x9EF1, 0xA8AF, 0xF24D, 0xC413, 0xB800, 0x8E5E, 0xD4BC, 0xE2E2, 0x6178, 0x5726, 0x0DC4, 0x3B9A, 0xDC4D, 0xEA13, 0xB0F1, 0x86AF, 0x0535, 0x336B, 0x6989, 0x5FD7, 0x23C4, 0x159A, 0x4F78, 0x7926, 0xFABC, 0xCCE2, 0x9600, 0xA05E, 0x6E26, 0x5878, 0x029A, 0x34C4, 0xB75E, 0x8100, 0xDBE2, 0xEDBC, 0x91AF, 0xA7F1, 0xFD13, 0xCB4D, 0x48D7, 0x7E89, 0x246B, 0x1235 };
				private const ushort LocalInitialValue = ushort.MinValue;
				/// <summary>
				/// Computes a CRC 16 Distributed Network Protocol (DNP) utility using reversed x^16 + x^13 + x^12 + x^11 + x^10 + x^8 + x^6 + x^5 + x^2 + 1 (0xA6BC) polynomial with an initial CRC value of 0
				/// </summary>
				/// <param name="bytes">bytes to crc</param>
				/// <returns>the CRC 16 of the provided bytes</returns>
				internal static ushort ComputeChecksumInternal ( params byte[] bytes ) {
					return ComputeChecksumInternal ( bytes, 0, bytes?.Length ?? 0 );
				}

				/// <summary>
				/// Computes a CRC 16 Distributed Network Protocol (DNP) utility using reversed x^16 + x^13 + x^12 + x^11 + x^10 + x^8 + x^6 + x^5 + x^2 + 1 (0xA6BC) polynomial with an initial CRC value of 0
				/// </summary>
				/// <param name="bytes">The buffer to compute the CRC upon</param>
				/// <param name="start">The start index upon which to compute the CRC</param>
				/// <param name="length">The length of the buffer upon which to compute the CRC</param>
				/// <returns>The specified CRC</returns>
				internal static ushort ComputeChecksumInternal ( byte[] bytes, int start, int length ) {
					if ( bytes == null ) { throw new ArgumentNullException ( nameof ( bytes ) ); }
					if ( bytes.Length == 0 ) { throw new ArgumentOutOfRangeException ( nameof ( bytes ) ); }
					if ( start < 0 ) { throw new ArgumentOutOfRangeException ( nameof ( start ) ); }
					if ( start >= bytes.Length && length > 1 ) { throw new ArgumentOutOfRangeException ( nameof ( start ) ); }
					var crc = LocalInitialValue;
					var end = start + length - 1;
					if ( end > bytes.Length ) { throw new ArgumentOutOfRangeException ( nameof ( length ) ); }
					if ( length < 0 ) { throw new ArgumentOutOfRangeException ( nameof ( length ) ); }
					for ( int i = start; i <= end; ++i ) {
						crc = ( ushort )( ( crc >> 8 ) ^ table[( byte )( crc ^ bytes[i] )] );
					}
					return (ushort)(~crc);
				}
			}

			/// <summary>
			/// A CRC 16 CCITT Utility using a reversed x^16 + x^12 + x^8 + 1 polynomial (0x8408) with an initial CRC value of 0
			/// </summary>
			internal static class Crc16CcittKermitImpl 
			{
				// pre-computed table using reversed x^16 + x^15 + x^2 + 1 poly / 0x8408 
				static ushort[] table = { 0x0000, 0x1189, 0x2312, 0x329B, 0x4624, 0x57AD, 0x6536, 0x74BF, 0x8C48, 0x9DC1, 0xAF5A, 0xBED3, 0xCA6C, 0xDBE5, 0xE97E, 0xF8F7, 0x1081, 0x0108, 0x3393, 0x221A, 0x56A5, 0x472C, 0x75B7, 0x643E, 0x9CC9, 0x8D40, 0xBFDB, 0xAE52, 0xDAED, 0xCB64, 0xF9FF, 0xE876, 0x2102, 0x308B, 0x0210, 0x1399, 0x6726, 0x76AF, 0x4434, 0x55BD, 0xAD4A, 0xBCC3, 0x8E58, 0x9FD1, 0xEB6E, 0xFAE7, 0xC87C, 0xD9F5, 0x3183, 0x200A, 0x1291, 0x0318, 0x77A7, 0x662E, 0x54B5, 0x453C, 0xBDCB, 0xAC42, 0x9ED9, 0x8F50, 0xFBEF, 0xEA66, 0xD8FD, 0xC974, 0x4204, 0x538D, 0x6116, 0x709F, 0x0420, 0x15A9, 0x2732, 0x36BB, 0xCE4C, 0xDFC5, 0xED5E, 0xFCD7, 0x8868, 0x99E1, 0xAB7A, 0xBAF3, 0x5285, 0x430C, 0x7197, 0x601E, 0x14A1, 0x0528, 0x37B3, 0x263A, 0xDECD, 0xCF44, 0xFDDF, 0xEC56, 0x98E9, 0x8960, 0xBBFB, 0xAA72, 0x6306, 0x728F, 0x4014, 0x519D, 0x2522, 0x34AB, 0x0630, 0x17B9, 0xEF4E, 0xFEC7, 0xCC5C, 0xDDD5, 0xA96A, 0xB8E3, 0x8A78, 0x9BF1, 0x7387, 0x620E, 0x5095, 0x411C, 0x35A3, 0x242A, 0x16B1, 0x0738, 0xFFCF, 0xEE46, 0xDCDD, 0xCD54, 0xB9EB, 0xA862, 0x9AF9, 0x8B70, 0x8408, 0x9581, 0xA71A, 0xB693, 0xC22C, 0xD3A5, 0xE13E, 0xF0B7, 0x0840, 0x19C9, 0x2B52, 0x3ADB, 0x4E64, 0x5FED, 0x6D76, 0x7CFF, 0x9489, 0x8500, 0xB79B, 0xA612, 0xD2AD, 0xC324, 0xF1BF, 0xE036, 0x18C1, 0x0948, 0x3BD3, 0x2A5A, 0x5EE5, 0x4F6C, 0x7DF7, 0x6C7E, 0xA50A, 0xB483, 0x8618, 0x9791, 0xE32E, 0xF2A7, 0xC03C, 0xD1B5, 0x2942, 0x38CB, 0x0A50, 0x1BD9, 0x6F66, 0x7EEF, 0x4C74, 0x5DFD, 0xB58B, 0xA402, 0x9699, 0x8710, 0xF3AF, 0xE226, 0xD0BD, 0xC134, 0x39C3, 0x284A, 0x1AD1, 0x0B58, 0x7FE7, 0x6E6E, 0x5CF5, 0x4D7C, 0xC60C, 0xD785, 0xE51E, 0xF497, 0x8028, 0x91A1, 0xA33A, 0xB2B3, 0x4A44, 0x5BCD, 0x6956, 0x78DF, 0x0C60, 0x1DE9, 0x2F72, 0x3EFB, 0xD68D, 0xC704, 0xF59F, 0xE416, 0x90A9, 0x8120, 0xB3BB, 0xA232, 0x5AC5, 0x4B4C, 0x79D7, 0x685E, 0x1CE1, 0x0D68, 0x3FF3, 0x2E7A, 0xE70E, 0xF687, 0xC41C, 0xD595, 0xA12A, 0xB0A3, 0x8238, 0x93B1, 0x6B46, 0x7ACF, 0x4854, 0x59DD, 0x2D62, 0x3CEB, 0x0E70, 0x1FF9, 0xF78F, 0xE606, 0xD49D, 0xC514, 0xB1AB, 0xA022, 0x92B9, 0x8330, 0x7BC7, 0x6A4E, 0x58D5, 0x495C, 0x3DE3, 0x2C6A, 0x1EF1, 0x0F78 };
				private const ushort InitialValue = ushort.MinValue;
				/// <summary>
				/// Computes a CRC 16 CCITT Kermit of the provided bytes using an initial value of 0
				/// </summary>
				/// <param name="bytes">bytes to crc</param>
				/// <returns>the CRC 16 of the provided bytes</returns>
				internal static ushort ComputeChecksumInternal ( params byte[] bytes ) {
					return ComputeChecksumInternal ( bytes, 0, bytes?.Length ?? 0 );
				}

				/// <summary>
				/// Computes a CRC 16 CCITT Kermit of the provided bytes using an initial value of 0
				/// </summary>
				/// <param name="bytes">The buffer to compute the CRC upon</param>
				/// <param name="start">The start index upon which to compute the CRC</param>
				/// <param name="length">The length of the buffer upon which to compute the CRC</param>
				/// <returns>The specified CRC</returns>
				internal static ushort ComputeChecksumInternal ( byte[] bytes, int start, int length ) {
					if ( bytes == null ) { throw new ArgumentNullException ( nameof ( bytes ) ); }
					if ( bytes.Length == 0 ) { throw new ArgumentOutOfRangeException ( nameof ( bytes ) ); }
					if ( start < 0 ) { throw new ArgumentOutOfRangeException ( nameof ( start ) ); }
					if ( start >= bytes.Length && length > 1 ) { throw new ArgumentOutOfRangeException ( nameof ( start ) ); }
					var crc = InitialValue;
					var end = start + length - 1;
					if ( end > bytes.Length ) { throw new ArgumentOutOfRangeException ( nameof ( length ) ); }
					if ( length < 0 ) { throw new ArgumentOutOfRangeException ( nameof ( length ) ); }
					for ( int i = start; i <= end; ++i ) {
						crc = ( ushort )( ( crc >> 8 ) ^ table[( byte )( crc ^ bytes[i] )] );
					}
					return crc;
				}
			}

			/// <summary>
			/// A CRC 16 CCITT Utility using x^16 + x^15 + x^2 + 1 polynomial with an initial CRC value of 0
			/// </summary>
			internal static class Crc16CcittInitialZeroImpl 
			{
				private const ushort LocalInitialValue = ushort.MinValue;
				/// <summary>
				/// Computes a CRC 16 CCITT of the provided bytes using an initial value of 0
				/// </summary>
				/// <param name="bytes">bytes to crc</param>
				/// <returns>the CRC 16 CCITT of the provided bytes</returns>
				public static ushort ComputeChecksumInternal ( params byte[] bytes ) {
					return Crc16CcittBaseImpl.ComputeChecksumInternal ( LocalInitialValue, bytes );
				}

				/// <summary>
				/// Computes a CRC 16 CCITT of the provided bytes using an initial value of 0
				/// </summary>
				/// <param name="bytes">The buffer to compute the CRC upon</param>
				/// <param name="start">The start index upon which to compute the CRC</param>
				/// <param name="length">The length of the buffer upon which to compute the CRC</param>
				/// <returns>The specified CRC</returns>
				public static ushort ComputeChecksumInternal ( byte[] bytes, int start, int length ) {
					return Crc16CcittBaseImpl.ComputeChecksumInternal ( LocalInitialValue, bytes, start, length );
				}
			}

			/// <summary>
			/// A CRC 16 CCITT Utility using x^16 + x^15 + x^2 + 1 polynomial with an initial CRC value of 0xFFFF
			/// </summary>
			internal static class Crc16CcittInitial0XFfffImpl 
			{
				private const ushort LocalInitialValue = ushort.MaxValue;
				/// <summary>
				/// Computes a CRC 16 CCITT of the provided bytes using an initial value of 0xFFFF
				/// </summary>
				/// <param name="bytes">bytes to crc</param>
				/// <returns>the CRC 16 CCITT of the provided bytes</returns>
				public static ushort ComputeChecksumInternal ( params byte[] bytes ) {
					return Crc16CcittBaseImpl.ComputeChecksumInternal ( LocalInitialValue, bytes );
				}

				/// <summary>
				/// Computes a CRC 16 CCITT of the provided bytes using an initial value of 0xFFFF
				/// </summary>
				/// <param name="bytes">The buffer to compute the CRC upon</param>
				/// <param name="start">The start index upon which to compute the CRC</param>
				/// <param name="length">The length of the buffer upon which to compute the CRC</param>
				/// <returns>The specified CRC</returns>
				public static ushort ComputeChecksumInternal ( byte[] bytes, int start, int length ) {
					return Crc16CcittBaseImpl.ComputeChecksumInternal ( LocalInitialValue, bytes, start, length );
				}
			}

			/// <summary>
			/// A CRC 16 CCITT Utility using x^16 + x^15 + x^2 + 1 polynomial with an initial CRC value of 0x1D0F
			/// </summary>
			internal static class Crc16CcittInitial0X1D0FImpl 
			{
				private const ushort LocalInitialValue = 0x1D0F;
				/// <summary>
				/// Computes a CRC 16 CCITT of the provided bytes using an initial value of 0x1D0F
				/// </summary>
				/// <param name="bytes">bytes to crc</param>
				/// <returns>the CRC 16 CCITT of the provided bytes</returns>
				public static ushort ComputeChecksumInternal ( params byte[] bytes ) {
					return Crc16CcittBaseImpl.ComputeChecksumInternal ( LocalInitialValue, bytes );
				}

				/// <summary>
				/// Computes a CRC 16 CCITT of the provided bytes using an initial value of 0x1D0F
				/// </summary>
				/// <param name="bytes">The buffer to compute the CRC upon</param>
				/// <param name="start">The start index upon which to compute the CRC</param>
				/// <param name="length">The length of the buffer upon which to compute the CRC</param>
				/// <returns>The specified CRC</returns>
				public static ushort ComputeChecksumInternal ( byte[] bytes, int start, int length ) {
					return Crc16CcittBaseImpl.ComputeChecksumInternal ( LocalInitialValue, bytes, start, length );
				}
			}
			// Base CRC 16 CCITT implementation
			internal static class Crc16CcittBaseImpl 
			{
				// pre-computed table using x^16 + x^15 + x^2 + 1 polynomial / 0x1021
				internal static ushort[] Table { get; private set; } = { 0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50A5, 0x60C6, 0x70E7, 0x8108, 0x9129, 0xA14A, 0xB16B, 0xC18C, 0xD1AD, 0xE1CE, 0xF1EF, 0x1231, 0x0210, 0x3273, 0x2252, 0x52B5, 0x4294, 0x72F7, 0x62D6, 0x9339, 0x8318, 0xB37B, 0xA35A, 0xD3BD, 0xC39C, 0xF3FF, 0xE3DE, 0x2462, 0x3443, 0x0420, 0x1401, 0x64E6, 0x74C7, 0x44A4, 0x5485, 0xA56A, 0xB54B, 0x8528, 0x9509, 0xE5EE, 0xF5CF, 0xC5AC, 0xD58D, 0x3653, 0x2672, 0x1611, 0x0630, 0x76D7, 0x66F6, 0x5695, 0x46B4, 0xB75B, 0xA77A, 0x9719, 0x8738, 0xF7DF, 0xE7FE, 0xD79D, 0xC7BC, 0x48C4, 0x58E5, 0x6886, 0x78A7, 0x0840, 0x1861, 0x2802, 0x3823, 0xC9CC, 0xD9ED, 0xE98E, 0xF9AF, 0x8948, 0x9969, 0xA90A, 0xB92B, 0x5AF5, 0x4AD4, 0x7AB7, 0x6A96, 0x1A71, 0x0A50, 0x3A33, 0x2A12, 0xDBFD, 0xCBDC, 0xFBBF, 0xEB9E, 0x9B79, 0x8B58, 0xBB3B, 0xAB1A, 0x6CA6, 0x7C87, 0x4CE4, 0x5CC5, 0x2C22, 0x3C03, 0x0C60, 0x1C41, 0xEDAE, 0xFD8F, 0xCDEC, 0xDDCD, 0xAD2A, 0xBD0B, 0x8D68, 0x9D49, 0x7E97, 0x6EB6, 0x5ED5, 0x4EF4, 0x3E13, 0x2E32, 0x1E51, 0x0E70, 0xFF9F, 0xEFBE, 0xDFDD, 0xCFFC, 0xBF1B, 0xAF3A, 0x9F59, 0x8F78, 0x9188, 0x81A9, 0xB1CA, 0xA1EB, 0xD10C, 0xC12D, 0xF14E, 0xE16F, 0x1080, 0x00A1, 0x30C2, 0x20E3, 0x5004, 0x4025, 0x7046, 0x6067, 0x83B9, 0x9398, 0xA3FB, 0xB3DA, 0xC33D, 0xD31C, 0xE37F, 0xF35E, 0x02B1, 0x1290, 0x22F3, 0x32D2, 0x4235, 0x5214, 0x6277, 0x7256, 0xB5EA, 0xA5CB, 0x95A8, 0x8589, 0xF56E, 0xE54F, 0xD52C, 0xC50D, 0x34E2, 0x24C3, 0x14A0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405, 0xA7DB, 0xB7FA, 0x8799, 0x97B8, 0xE75F, 0xF77E, 0xC71D, 0xD73C, 0x26D3, 0x36F2, 0x0691, 0x16B0, 0x6657, 0x7676, 0x4615, 0x5634, 0xD94C, 0xC96D, 0xF90E, 0xE92F, 0x99C8, 0x89E9, 0xB98A, 0xA9AB, 0x5844, 0x4865, 0x7806, 0x6827, 0x18C0, 0x08E1, 0x3882, 0x28A3, 0xCB7D, 0xDB5C, 0xEB3F, 0xFB1E, 0x8BF9, 0x9BD8, 0xABBB, 0xBB9A, 0x4A75, 0x5A54, 0x6A37, 0x7A16, 0x0AF1, 0x1AD0, 0x2AB3, 0x3A92, 0xFD2E, 0xED0F, 0xDD6C, 0xCD4D, 0xBDAA, 0xAD8B, 0x9DE8, 0x8DC9, 0x7C26, 0x6C07, 0x5C64, 0x4C45, 0x3CA2, 0x2C83, 0x1CE0, 0x0CC1, 0xEF1F, 0xFF3E, 0xCF5D, 0xDF7C, 0xAF9B, 0xBFBA, 0x8FD9, 0x9FF8, 0x6E17, 0x7E36, 0x4E55, 0x5E74, 0x2E93, 0x3EB2, 0x0ED1, 0x1EF0 };
				// base compute checksum code
				internal static ushort ComputeChecksumInternal ( ushort initialValue, params byte[] bytes ) {
					return ComputeChecksumInternal ( initialValue, bytes, 0, bytes?.Length ?? 0 );
				}
				internal static ushort ComputeChecksumInternal ( ushort initialValue, byte[] bytes, int start, int length ) {
					if ( bytes == null ) { throw new ArgumentNullException ( nameof ( bytes ) ); }
					if ( bytes.Length == 0 ) { throw new ArgumentOutOfRangeException ( nameof ( bytes ) ); }
					if ( start < 0 ) { throw new ArgumentOutOfRangeException ( nameof ( start ) ); }
					if ( start >= bytes.Length && length > 1 ) { throw new ArgumentOutOfRangeException ( nameof ( start ) ); }
					var crc = initialValue;
					var end = start + length - 1;
					if ( end > bytes.Length ) { throw new ArgumentOutOfRangeException ( nameof ( length ) ); }
					if ( length < 0 ) { throw new ArgumentOutOfRangeException ( nameof ( length ) ); }
					for ( int i = start; i <= end; ++i ) {
						crc = ( ushort )( ( crc << 8 ) ^ Table[( ( crc >> 8 ) ^ ( 0xff & bytes[i] ) )] );
					}
					return crc;
				}
			}

			/// <summary>
			/// Computes a CRC 16 checksum of the specified bytes using the given algorithm
			/// </summary>
			/// <param name="algorithm">The CRC 16 Algorithm to use</param>
			/// <param name="bytes">The buffer to compute the CRC upon</param>
			/// <returns>The specified CRC</returns>
			public static ushort ComputeChecksum ( Crc16Algorithm algorithm, params byte[] bytes ) 
			{
				return ComputeChecksum ( algorithm, bytes, 0, bytes?.Length ?? 0 );
			}

			/// <summary>
			/// Computes a CRC 16 checksum of the specified bytes using the given algorithm
			/// </summary>
			/// <param name="algorithm">The CRC 16 Algorithm to use</param>
			/// <param name="bytes">The buffer to compute the CRC upon</param>
			/// <param name="start">The start index upon which to compute the CRC</param>
			/// <param name="length">The length of the buffer upon which to compute the CRC</param>
			/// <returns>The specified CRC</returns>
			public static ushort ComputeChecksum ( Crc16Algorithm algorithm, byte[] bytes, int start, int length ) 
			{
				switch ( algorithm ) {
					case Crc16Algorithm.Standard:
					return StandardCrc16Impl.ComputeChecksumInternal ( bytes, start, length );
					case Crc16Algorithm.Ccitt:
					return Crc16CcittInitialZeroImpl.ComputeChecksumInternal ( bytes, start, length );
					case Crc16Algorithm.CcittKermit:
					return Crc16CcittKermitImpl.ComputeChecksumInternal ( bytes, start, length );
					case Crc16Algorithm.CcittInitialValue0xFFFF:
					return Crc16CcittInitial0XFfffImpl.ComputeChecksumInternal ( bytes, start, length );
					case Crc16Algorithm.CcittInitialValue0x1D0F:
					return Crc16CcittInitial0X1D0FImpl.ComputeChecksumInternal ( bytes, start, length );
					case Crc16Algorithm.Dnp:
					return DnpCrc16Impl.ComputeChecksumInternal ( bytes, start, length );
					case Crc16Algorithm.Modbus:
					return StandardCrc16Impl.ComputeChecksumInternal ( bytes, start, length, ushort.MaxValue );
				}
				throw new UnknownAlgorithmException ( "Unknown Algorithm" );
			}
		}
		
		public static class CRC32 
		{
			static uint[] table = { 0x00000000, 0x77073096, 0xEE0E612C, 0x990951BA, 0x076DC419, 0x706AF48F, 0xE963A535, 0x9E6495A3, 0x0EDB8832, 0x79DCB8A4, 0xE0D5E91E, 0x97D2D988, 0x09B64C2B, 0x7EB17CBD, 0xE7B82D07, 0x90BF1D91, 0x1DB71064, 0x6AB020F2, 0xF3B97148, 0x84BE41DE, 0x1ADAD47D, 0x6DDDE4EB, 0xF4D4B551, 0x83D385C7, 0x136C9856, 0x646BA8C0, 0xFD62F97A, 0x8A65C9EC, 0x14015C4F, 0x63066CD9, 0xFA0F3D63, 0x8D080DF5, 0x3B6E20C8, 0x4C69105E, 0xD56041E4, 0xA2677172, 0x3C03E4D1, 0x4B04D447, 0xD20D85FD, 0xA50AB56B, 0x35B5A8FA, 0x42B2986C, 0xDBBBC9D6, 0xACBCF940, 0x32D86CE3, 0x45DF5C75, 0xDCD60DCF, 0xABD13D59, 0x26D930AC, 0x51DE003A, 0xC8D75180, 0xBFD06116, 0x21B4F4B5, 0x56B3C423, 0xCFBA9599, 0xB8BDA50F, 0x2802B89E, 0x5F058808, 0xC60CD9B2, 0xB10BE924, 0x2F6F7C87, 0x58684C11, 0xC1611DAB, 0xB6662D3D, 0x76DC4190, 0x01DB7106, 0x98D220BC, 0xEFD5102A, 0x71B18589, 0x06B6B51F, 0x9FBFE4A5, 0xE8B8D433, 0x7807C9A2, 0x0F00F934, 0x9609A88E, 0xE10E9818, 0x7F6A0DBB, 0x086D3D2D, 0x91646C97, 0xE6635C01, 0x6B6B51F4, 0x1C6C6162, 0x856530D8, 0xF262004E, 0x6C0695ED, 0x1B01A57B, 0x8208F4C1, 0xF50FC457, 0x65B0D9C6, 0x12B7E950, 0x8BBEB8EA, 0xFCB9887C, 0x62DD1DDF, 0x15DA2D49, 0x8CD37CF3, 0xFBD44C65, 0x4DB26158, 0x3AB551CE, 0xA3BC0074, 0xD4BB30E2, 0x4ADFA541, 0x3DD895D7, 0xA4D1C46D, 0xD3D6F4FB, 0x4369E96A, 0x346ED9FC, 0xAD678846, 0xDA60B8D0, 0x44042D73, 0x33031DE5, 0xAA0A4C5F, 0xDD0D7CC9, 0x5005713C, 0x270241AA, 0xBE0B1010, 0xC90C2086, 0x5768B525, 0x206F85B3, 0xB966D409, 0xCE61E49F, 0x5EDEF90E, 0x29D9C998, 0xB0D09822, 0xC7D7A8B4, 0x59B33D17, 0x2EB40D81, 0xB7BD5C3B, 0xC0BA6CAD, 0xEDB88320, 0x9ABFB3B6, 0x03B6E20C, 0x74B1D29A, 0xEAD54739, 0x9DD277AF, 0x04DB2615, 0x73DC1683, 0xE3630B12, 0x94643B84, 0x0D6D6A3E, 0x7A6A5AA8, 0xE40ECF0B, 0x9309FF9D, 0x0A00AE27, 0x7D079EB1, 0xF00F9344, 0x8708A3D2, 0x1E01F268, 0x6906C2FE, 0xF762575D, 0x806567CB, 0x196C3671, 0x6E6B06E7, 0xFED41B76, 0x89D32BE0, 0x10DA7A5A, 0x67DD4ACC, 0xF9B9DF6F, 0x8EBEEFF9, 0x17B7BE43, 0x60B08ED5, 0xD6D6A3E8, 0xA1D1937E, 0x38D8C2C4, 0x4FDFF252, 0xD1BB67F1, 0xA6BC5767, 0x3FB506DD, 0x48B2364B, 0xD80D2BDA, 0xAF0A1B4C, 0x36034AF6, 0x41047A60, 0xDF60EFC3, 0xA867DF55, 0x316E8EEF, 0x4669BE79, 0xCB61B38C, 0xBC66831A, 0x256FD2A0, 0x5268E236, 0xCC0C7795, 0xBB0B4703, 0x220216B9, 0x5505262F, 0xC5BA3BBE, 0xB2BD0B28, 0x2BB45A92, 0x5CB36A04, 0xC2D7FFA7, 0xB5D0CF31, 0x2CD99E8B, 0x5BDEAE1D, 0x9B64C2B0, 0xEC63F226, 0x756AA39C, 0x026D930A, 0x9C0906A9, 0xEB0E363F, 0x72076785, 0x05005713, 0x95BF4A82, 0xE2B87A14, 0x7BB12BAE, 0x0CB61B38, 0x92D28E9B, 0xE5D5BE0D, 0x7CDCEFB7, 0x0BDBDF21, 0x86D3D2D4, 0xF1D4E242, 0x68DDB3F8, 0x1FDA836E, 0x81BE16CD, 0xF6B9265B, 0x6FB077E1, 0x18B74777, 0x88085AE6, 0xFF0F6A70, 0x66063BCA, 0x11010B5C, 0x8F659EFF, 0xF862AE69, 0x616BFFD3, 0x166CCF45, 0xA00AE278, 0xD70DD2EE, 0x4E048354, 0x3903B3C2, 0xA7672661, 0xD06016F7, 0x4969474D, 0x3E6E77DB, 0xAED16A4A, 0xD9D65ADC, 0x40DF0B66, 0x37D83BF0, 0xA9BCAE53, 0xDEBB9EC5, 0x47B2CF7F, 0x30B5FFE9, 0xBDBDF21C, 0xCABAC28A, 0x53B39330, 0x24B4A3A6, 0xBAD03605, 0xCDD70693, 0x54DE5729, 0x23D967BF, 0xB3667A2E, 0xC4614AB8, 0x5D681B02, 0x2A6F2B94, 0xB40BBE37, 0xC30C8EA1, 0x5A05DF1B, 0x2D02EF8D };
			const uint InitialValue = uint.MaxValue;

			/// <summary>
			/// Computes the CRC 32 checksum of the specified bytes using an initial value of 0xFFFFFFFF
			/// </summary>
			/// <param name="bytes">The buffer to compute the CRC upon</param>
			/// <returns>The specified CRC</returns>
			public static uint ComputeChecksum ( params byte[] bytes ) 
			{
				return ComputeChecksum ( bytes, 0, bytes?.Length ?? 0 );
			}

			/// <summary>
			/// Computes the CRC 32 checksum of the specified bytes using an initial value of 0xFFFFFFFF
			/// </summary>
			/// <param name="bytes">The buffer to compute the CRC upon</param>
			/// <param name="start">The start index upon which to compute the CRC</param>
			/// /// <param name="length">The length of the buffer upon which to compute the CRC</param>
			/// <returns>The specified CRC</returns>
			public static uint ComputeChecksum( byte[] bytes, int start, int length ) 
			{
				if ( bytes == null ) { throw new ArgumentNullException ( nameof ( bytes ) ); }
				if ( bytes.Length == 0 ) { throw new ArgumentOutOfRangeException ( nameof ( bytes ) ); }
				if ( start < 0 ) { throw new ArgumentOutOfRangeException ( nameof ( start ) ); }
				if ( start >= bytes.Length && length > 1 ) { throw new ArgumentOutOfRangeException ( nameof ( start ) ); }
				var crc = InitialValue;
				var end = start + length - 1;
				if ( end > bytes.Length ) { throw new ArgumentOutOfRangeException ( nameof ( length ) ); }
				if ( length < 0 ) { throw new ArgumentOutOfRangeException ( nameof ( length ) ); }
				for ( int i = start; i <= end; ++i ) {
					crc = ( uint )( ( crc >> 8 ) ^ table[( byte )( ( ( crc ) & 0xff ) ^ bytes[i] )] );
				}
				return ~crc;
			}
		}
	}
	
	namespace BLAKE2S
	{
		// Copyright notices:
		/*
		// ---
		 Originally Written in 2012 by Christian Winnerlein  <codesinchaos@gmail.com>
		 Rewritten Fall 2014 (for the Blake2s flavor instead of the Blake2b flavor) 
		 by Dustin Sparks <sparkdustjoe@gmail.com>

		 To the extent possible under law, the author(s) have dedicated all copyright
		 and related and neighboring rights to this software to the public domain
		 worldwide. This software is distributed without any warranty.

		 You should have received a copy of the CC0 Public Domain Dedication along with
		 this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
		 ---
		 Based on BlakeSharp
		 by Dominik Reichl <dominik.reichl@t-online.de>
		 Web: http://www.dominik-reichl.de/
		 If you're using this class, it would be nice if you'd mention
		 me somewhere in the documentation of your program, but it's
		 not required.
		 BLAKE was designed by Jean-Philippe Aumasson, Luca Henzen,
		 Willi Meier and Raphael C.-W. Phan.
		 BlakeSharp was derived from the reference C implementation.
		 ---
		 This implementation is based on: https://github.com/SparkDustJoe/miniLockManaged
		 Reason:
		 - Have a single nuget package for Blake2s
		 - Added libsodium-net similar interface for the hash functions. 
		*/
		// Core-Unrolled.cs file: <--
		public sealed partial class Blake2sCore
		{
			partial void Compress(byte[] block, int start)
			{
				if (BitConverter.IsLittleEndian)
				{
					Buffer.BlockCopy(block, start, _m, 0, BlockSizeInBytes);
				}
				else
				{
					for (var i = 0; i < 16; i += 4)
						_m[i] = BytesToUInt32(block, start + i); //
				}

				var m0 = _m[0];
				var m1 = _m[1];
				var m2 = _m[2];
				var m3 = _m[3];
				var m4 = _m[4];
				var m5 = _m[5];
				var m6 = _m[6];
				var m7 = _m[7];
				var m8 = _m[8];
				var m9 = _m[9];
				var m10 = _m[10];
				var m11 = _m[11];
				var m12 = _m[12];
				var m13 = _m[13];
				var m14 = _m[14];
				var m15 = _m[15]; //*/

				var v0 = _h[0];
				var v1 = _h[1];
				var v2 = _h[2];
				var v3 = _h[3];
				var v4 = _h[4];
				var v5 = _h[5];
				var v6 = _h[6];
				var v7 = _h[7];

				var v8 = IV0;
				var v9 = IV1;
				var v10 = IV2;
				var v11 = IV3;
				var v12 = IV4 ^ _counter0;
				var v13 = IV5 ^ _counter1;
				var v14 = IV6 ^ _finalizationFlag0;
				var v15 = IV7 ^ _finalizationFlag1;

				// Rounds
			//*
				// Round 1.
				v0 += m0;
				v0 += v4;
				v12 ^= v0;
				v12 = v12 << (32 - 16) | v12 >> 16;
				v8 += v12;
				v4 ^= v8;
				v4 = v4 << (32 - 12) | v4 >> 12;
				v1 += m2;
				v1 += v5;
				v13 ^= v1;
				v13 = v13 << (32 - 16) | v13 >> 16;
				v9 += v13;
				v5 ^= v9;
				v5 = v5 << (32 - 12) | v5 >> 12;
				v2 += m4;
				v2 += v6;
				v14 ^= v2;
				v14 = v14 << (32 - 16) | v14 >> 16;
				v10 += v14;
				v6 ^= v10;
				v6 = v6 << (32 - 12) | v6 >> 12;
				v3 += m6;
				v3 += v7;
				v15 ^= v3;
				v15 = v15 << (32 - 16) | v15 >> 16;
				v11 += v15;
				v7 ^= v11;
				v7 = v7 << (32 - 12) | v7 >> 12;
				v2 += m5;
				v2 += v6;
				v14 ^= v2;
				v14 = v14 << (32 - 8) | v14 >> 8;
				v10 += v14;
				v6 ^= v10;
				v6 = v6 << (32 - 7) | v6 >> 7;
				v3 += m7;
				v3 += v7;
				v15 ^= v3;
				v15 = v15 << (32 - 8) | v15 >> 8;
				v11 += v15;
				v7 ^= v11;
				v7 = v7 << (32 - 7) | v7 >> 7;
				v1 += m3;
				v1 += v5;
				v13 ^= v1;
				v13 = v13 << (32 - 8) | v13 >> 8;
				v9 += v13;
				v5 ^= v9;
				v5 = v5 << (32 - 7) | v5 >> 7;
				v0 += m1;
				v0 += v4;
				v12 ^= v0;
				v12 = v12 << (32 - 8) | v12 >> 8;
				v8 += v12;
				v4 ^= v8;
				v4 = v4 << (32 - 7) | v4 >> 7;
				v0 += m8;
				v0 += v5;
				v15 ^= v0;
				v15 = v15 << (32 - 16) | v15 >> 16;
				v10 += v15;
				v5 ^= v10;
				v5 = v5 << (32 - 12) | v5 >> 12;
				v1 += m10;
				v1 += v6;
				v12 ^= v1;
				v12 = v12 << (32 - 16) | v12 >> 16;
				v11 += v12;
				v6 ^= v11;
				v6 = v6 << (32 - 12) | v6 >> 12;
				v2 += m12;
				v2 += v7;
				v13 ^= v2;
				v13 = v13 << (32 - 16) | v13 >> 16;
				v8 += v13;
				v7 ^= v8;
				v7 = v7 << (32 - 12) | v7 >> 12;
				v3 += m14;
				v3 += v4;
				v14 ^= v3;
				v14 = v14 << (32 - 16) | v14 >> 16;
				v9 += v14;
				v4 ^= v9;
				v4 = v4 << (32 - 12) | v4 >> 12;
				v2 += m13;
				v2 += v7;
				v13 ^= v2;
				v13 = v13 << (32 - 8) | v13 >> 8;
				v8 += v13;
				v7 ^= v8;
				v7 = v7 << (32 - 7) | v7 >> 7;
				v3 += m15;
				v3 += v4;
				v14 ^= v3;
				v14 = v14 << (32 - 8) | v14 >> 8;
				v9 += v14;
				v4 ^= v9;
				v4 = v4 << (32 - 7) | v4 >> 7;
				v1 += m11;
				v1 += v6;
				v12 ^= v1;
				v12 = v12 << (32 - 8) | v12 >> 8;
				v11 += v12;
				v6 ^= v11;
				v6 = v6 << (32 - 7) | v6 >> 7;
				v0 += m9;
				v0 += v5;
				v15 ^= v0;
				v15 = v15 << (32 - 8) | v15 >> 8;
				v10 += v15;
				v5 ^= v10;
				v5 = v5 << (32 - 7) | v5 >> 7;

				// Round 2.
				v0 += m14;
				v0 += v4;
				v12 ^= v0;
				v12 = v12 << (32 - 16) | v12 >> 16;
				v8 += v12;
				v4 ^= v8;
				v4 = v4 << (32 - 12) | v4 >> 12;
				v1 += m4;
				v1 += v5;
				v13 ^= v1;
				v13 = v13 << (32 - 16) | v13 >> 16;
				v9 += v13;
				v5 ^= v9;
				v5 = v5 << (32 - 12) | v5 >> 12;
				v2 += m9;
				v2 += v6;
				v14 ^= v2;
				v14 = v14 << (32 - 16) | v14 >> 16;
				v10 += v14;
				v6 ^= v10;
				v6 = v6 << (32 - 12) | v6 >> 12;
				v3 += m13;
				v3 += v7;
				v15 ^= v3;
				v15 = v15 << (32 - 16) | v15 >> 16;
				v11 += v15;
				v7 ^= v11;
				v7 = v7 << (32 - 12) | v7 >> 12;
				v2 += m15;
				v2 += v6;
				v14 ^= v2;
				v14 = v14 << (32 - 8) | v14 >> 8;
				v10 += v14;
				v6 ^= v10;
				v6 = v6 << (32 - 7) | v6 >> 7;
				v3 += m6;
				v3 += v7;
				v15 ^= v3;
				v15 = v15 << (32 - 8) | v15 >> 8;
				v11 += v15;
				v7 ^= v11;
				v7 = v7 << (32 - 7) | v7 >> 7;
				v1 += m8;
				v1 += v5;
				v13 ^= v1;
				v13 = v13 << (32 - 8) | v13 >> 8;
				v9 += v13;
				v5 ^= v9;
				v5 = v5 << (32 - 7) | v5 >> 7;
				v0 += m10;
				v0 += v4;
				v12 ^= v0;
				v12 = v12 << (32 - 8) | v12 >> 8;
				v8 += v12;
				v4 ^= v8;
				v4 = v4 << (32 - 7) | v4 >> 7;
				v0 += m1;
				v0 += v5;
				v15 ^= v0;
				v15 = v15 << (32 - 16) | v15 >> 16;
				v10 += v15;
				v5 ^= v10;
				v5 = v5 << (32 - 12) | v5 >> 12;
				v1 += m0;
				v1 += v6;
				v12 ^= v1;
				v12 = v12 << (32 - 16) | v12 >> 16;
				v11 += v12;
				v6 ^= v11;
				v6 = v6 << (32 - 12) | v6 >> 12;
				v2 += m11;
				v2 += v7;
				v13 ^= v2;
				v13 = v13 << (32 - 16) | v13 >> 16;
				v8 += v13;
				v7 ^= v8;
				v7 = v7 << (32 - 12) | v7 >> 12;
				v3 += m5;
				v3 += v4;
				v14 ^= v3;
				v14 = v14 << (32 - 16) | v14 >> 16;
				v9 += v14;
				v4 ^= v9;
				v4 = v4 << (32 - 12) | v4 >> 12;
				v2 += m7;
				v2 += v7;
				v13 ^= v2;
				v13 = v13 << (32 - 8) | v13 >> 8;
				v8 += v13;
				v7 ^= v8;
				v7 = v7 << (32 - 7) | v7 >> 7;
				v3 += m3;
				v3 += v4;
				v14 ^= v3;
				v14 = v14 << (32 - 8) | v14 >> 8;
				v9 += v14;
				v4 ^= v9;
				v4 = v4 << (32 - 7) | v4 >> 7;
				v1 += m2;
				v1 += v6;
				v12 ^= v1;
				v12 = v12 << (32 - 8) | v12 >> 8;
				v11 += v12;
				v6 ^= v11;
				v6 = v6 << (32 - 7) | v6 >> 7;
				v0 += m12;
				v0 += v5;
				v15 ^= v0;
				v15 = v15 << (32 - 8) | v15 >> 8;
				v10 += v15;
				v5 ^= v10;
				v5 = v5 << (32 - 7) | v5 >> 7;

				// Round 3.
				v0 += m11;
				v0 += v4;
				v12 ^= v0;
				v12 = v12 << (32 - 16) | v12 >> 16;
				v8 += v12;
				v4 ^= v8;
				v4 = v4 << (32 - 12) | v4 >> 12;
				v1 += m12;
				v1 += v5;
				v13 ^= v1;
				v13 = v13 << (32 - 16) | v13 >> 16;
				v9 += v13;
				v5 ^= v9;
				v5 = v5 << (32 - 12) | v5 >> 12;
				v2 += m5;
				v2 += v6;
				v14 ^= v2;
				v14 = v14 << (32 - 16) | v14 >> 16;
				v10 += v14;
				v6 ^= v10;
				v6 = v6 << (32 - 12) | v6 >> 12;
				v3 += m15;
				v3 += v7;
				v15 ^= v3;
				v15 = v15 << (32 - 16) | v15 >> 16;
				v11 += v15;
				v7 ^= v11;
				v7 = v7 << (32 - 12) | v7 >> 12;
				v2 += m2;
				v2 += v6;
				v14 ^= v2;
				v14 = v14 << (32 - 8) | v14 >> 8;
				v10 += v14;
				v6 ^= v10;
				v6 = v6 << (32 - 7) | v6 >> 7;
				v3 += m13;
				v3 += v7;
				v15 ^= v3;
				v15 = v15 << (32 - 8) | v15 >> 8;
				v11 += v15;
				v7 ^= v11;
				v7 = v7 << (32 - 7) | v7 >> 7;
				v1 += m0;
				v1 += v5;
				v13 ^= v1;
				v13 = v13 << (32 - 8) | v13 >> 8;
				v9 += v13;
				v5 ^= v9;
				v5 = v5 << (32 - 7) | v5 >> 7;
				v0 += m8;
				v0 += v4;
				v12 ^= v0;
				v12 = v12 << (32 - 8) | v12 >> 8;
				v8 += v12;
				v4 ^= v8;
				v4 = v4 << (32 - 7) | v4 >> 7;
				v0 += m10;
				v0 += v5;
				v15 ^= v0;
				v15 = v15 << (32 - 16) | v15 >> 16;
				v10 += v15;
				v5 ^= v10;
				v5 = v5 << (32 - 12) | v5 >> 12;
				v1 += m3;
				v1 += v6;
				v12 ^= v1;
				v12 = v12 << (32 - 16) | v12 >> 16;
				v11 += v12;
				v6 ^= v11;
				v6 = v6 << (32 - 12) | v6 >> 12;
				v2 += m7;
				v2 += v7;
				v13 ^= v2;
				v13 = v13 << (32 - 16) | v13 >> 16;
				v8 += v13;
				v7 ^= v8;
				v7 = v7 << (32 - 12) | v7 >> 12;
				v3 += m9;
				v3 += v4;
				v14 ^= v3;
				v14 = v14 << (32 - 16) | v14 >> 16;
				v9 += v14;
				v4 ^= v9;
				v4 = v4 << (32 - 12) | v4 >> 12;
				v2 += m1;
				v2 += v7;
				v13 ^= v2;
				v13 = v13 << (32 - 8) | v13 >> 8;
				v8 += v13;
				v7 ^= v8;
				v7 = v7 << (32 - 7) | v7 >> 7;
				v3 += m4;
				v3 += v4;
				v14 ^= v3;
				v14 = v14 << (32 - 8) | v14 >> 8;
				v9 += v14;
				v4 ^= v9;
				v4 = v4 << (32 - 7) | v4 >> 7;
				v1 += m6;
				v1 += v6;
				v12 ^= v1;
				v12 = v12 << (32 - 8) | v12 >> 8;
				v11 += v12;
				v6 ^= v11;
				v6 = v6 << (32 - 7) | v6 >> 7;
				v0 += m14;
				v0 += v5;
				v15 ^= v0;
				v15 = v15 << (32 - 8) | v15 >> 8;
				v10 += v15;
				v5 ^= v10;
				v5 = v5 << (32 - 7) | v5 >> 7;

				// Round 4.
				v0 += m7;
				v0 += v4;
				v12 ^= v0;
				v12 = v12 << (32 - 16) | v12 >> 16;
				v8 += v12;
				v4 ^= v8;
				v4 = v4 << (32 - 12) | v4 >> 12;
				v1 += m3;
				v1 += v5;
				v13 ^= v1;
				v13 = v13 << (32 - 16) | v13 >> 16;
				v9 += v13;
				v5 ^= v9;
				v5 = v5 << (32 - 12) | v5 >> 12;
				v2 += m13;
				v2 += v6;
				v14 ^= v2;
				v14 = v14 << (32 - 16) | v14 >> 16;
				v10 += v14;
				v6 ^= v10;
				v6 = v6 << (32 - 12) | v6 >> 12;
				v3 += m11;
				v3 += v7;
				v15 ^= v3;
				v15 = v15 << (32 - 16) | v15 >> 16;
				v11 += v15;
				v7 ^= v11;
				v7 = v7 << (32 - 12) | v7 >> 12;
				v2 += m12;
				v2 += v6;
				v14 ^= v2;
				v14 = v14 << (32 - 8) | v14 >> 8;
				v10 += v14;
				v6 ^= v10;
				v6 = v6 << (32 - 7) | v6 >> 7;
				v3 += m14;
				v3 += v7;
				v15 ^= v3;
				v15 = v15 << (32 - 8) | v15 >> 8;
				v11 += v15;
				v7 ^= v11;
				v7 = v7 << (32 - 7) | v7 >> 7;
				v1 += m1;
				v1 += v5;
				v13 ^= v1;
				v13 = v13 << (32 - 8) | v13 >> 8;
				v9 += v13;
				v5 ^= v9;
				v5 = v5 << (32 - 7) | v5 >> 7;
				v0 += m9;
				v0 += v4;
				v12 ^= v0;
				v12 = v12 << (32 - 8) | v12 >> 8;
				v8 += v12;
				v4 ^= v8;
				v4 = v4 << (32 - 7) | v4 >> 7;
				v0 += m2;
				v0 += v5;
				v15 ^= v0;
				v15 = v15 << (32 - 16) | v15 >> 16;
				v10 += v15;
				v5 ^= v10;
				v5 = v5 << (32 - 12) | v5 >> 12;
				v1 += m5;
				v1 += v6;
				v12 ^= v1;
				v12 = v12 << (32 - 16) | v12 >> 16;
				v11 += v12;
				v6 ^= v11;
				v6 = v6 << (32 - 12) | v6 >> 12;
				v2 += m4;
				v2 += v7;
				v13 ^= v2;
				v13 = v13 << (32 - 16) | v13 >> 16;
				v8 += v13;
				v7 ^= v8;
				v7 = v7 << (32 - 12) | v7 >> 12;
				v3 += m15;
				v3 += v4;
				v14 ^= v3;
				v14 = v14 << (32 - 16) | v14 >> 16;
				v9 += v14;
				v4 ^= v9;
				v4 = v4 << (32 - 12) | v4 >> 12;
				v2 += m0;
				v2 += v7;
				v13 ^= v2;
				v13 = v13 << (32 - 8) | v13 >> 8;
				v8 += v13;
				v7 ^= v8;
				v7 = v7 << (32 - 7) | v7 >> 7;
				v3 += m8;
				v3 += v4;
				v14 ^= v3;
				v14 = v14 << (32 - 8) | v14 >> 8;
				v9 += v14;
				v4 ^= v9;
				v4 = v4 << (32 - 7) | v4 >> 7;
				v1 += m10;
				v1 += v6;
				v12 ^= v1;
				v12 = v12 << (32 - 8) | v12 >> 8;
				v11 += v12;
				v6 ^= v11;
				v6 = v6 << (32 - 7) | v6 >> 7;
				v0 += m6;
				v0 += v5;
				v15 ^= v0;
				v15 = v15 << (32 - 8) | v15 >> 8;
				v10 += v15;
				v5 ^= v10;
				v5 = v5 << (32 - 7) | v5 >> 7;

				// Round 5.
				v0 += m9;
				v0 += v4;
				v12 ^= v0;
				v12 = v12 << (32 - 16) | v12 >> 16;
				v8 += v12;
				v4 ^= v8;
				v4 = v4 << (32 - 12) | v4 >> 12;
				v1 += m5;
				v1 += v5;
				v13 ^= v1;
				v13 = v13 << (32 - 16) | v13 >> 16;
				v9 += v13;
				v5 ^= v9;
				v5 = v5 << (32 - 12) | v5 >> 12;
				v2 += m2;
				v2 += v6;
				v14 ^= v2;
				v14 = v14 << (32 - 16) | v14 >> 16;
				v10 += v14;
				v6 ^= v10;
				v6 = v6 << (32 - 12) | v6 >> 12;
				v3 += m10;
				v3 += v7;
				v15 ^= v3;
				v15 = v15 << (32 - 16) | v15 >> 16;
				v11 += v15;
				v7 ^= v11;
				v7 = v7 << (32 - 12) | v7 >> 12;
				v2 += m4;
				v2 += v6;
				v14 ^= v2;
				v14 = v14 << (32 - 8) | v14 >> 8;
				v10 += v14;
				v6 ^= v10;
				v6 = v6 << (32 - 7) | v6 >> 7;
				v3 += m15;
				v3 += v7;
				v15 ^= v3;
				v15 = v15 << (32 - 8) | v15 >> 8;
				v11 += v15;
				v7 ^= v11;
				v7 = v7 << (32 - 7) | v7 >> 7;
				v1 += m7;
				v1 += v5;
				v13 ^= v1;
				v13 = v13 << (32 - 8) | v13 >> 8;
				v9 += v13;
				v5 ^= v9;
				v5 = v5 << (32 - 7) | v5 >> 7;
				v0 += m0;
				v0 += v4;
				v12 ^= v0;
				v12 = v12 << (32 - 8) | v12 >> 8;
				v8 += v12;
				v4 ^= v8;
				v4 = v4 << (32 - 7) | v4 >> 7;
				v0 += m14;
				v0 += v5;
				v15 ^= v0;
				v15 = v15 << (32 - 16) | v15 >> 16;
				v10 += v15;
				v5 ^= v10;
				v5 = v5 << (32 - 12) | v5 >> 12;
				v1 += m11;
				v1 += v6;
				v12 ^= v1;
				v12 = v12 << (32 - 16) | v12 >> 16;
				v11 += v12;
				v6 ^= v11;
				v6 = v6 << (32 - 12) | v6 >> 12;
				v2 += m6;
				v2 += v7;
				v13 ^= v2;
				v13 = v13 << (32 - 16) | v13 >> 16;
				v8 += v13;
				v7 ^= v8;
				v7 = v7 << (32 - 12) | v7 >> 12;
				v3 += m3;
				v3 += v4;
				v14 ^= v3;
				v14 = v14 << (32 - 16) | v14 >> 16;
				v9 += v14;
				v4 ^= v9;
				v4 = v4 << (32 - 12) | v4 >> 12;
				v2 += m8;
				v2 += v7;
				v13 ^= v2;
				v13 = v13 << (32 - 8) | v13 >> 8;
				v8 += v13;
				v7 ^= v8;
				v7 = v7 << (32 - 7) | v7 >> 7;
				v3 += m13;
				v3 += v4;
				v14 ^= v3;
				v14 = v14 << (32 - 8) | v14 >> 8;
				v9 += v14;
				v4 ^= v9;
				v4 = v4 << (32 - 7) | v4 >> 7;
				v1 += m12;
				v1 += v6;
				v12 ^= v1;
				v12 = v12 << (32 - 8) | v12 >> 8;
				v11 += v12;
				v6 ^= v11;
				v6 = v6 << (32 - 7) | v6 >> 7;
				v0 += m1;
				v0 += v5;
				v15 ^= v0;
				v15 = v15 << (32 - 8) | v15 >> 8;
				v10 += v15;
				v5 ^= v10;
				v5 = v5 << (32 - 7) | v5 >> 7;

				// Round 6.
				v0 += m2;
				v0 += v4;
				v12 ^= v0;
				v12 = v12 << (32 - 16) | v12 >> 16;
				v8 += v12;
				v4 ^= v8;
				v4 = v4 << (32 - 12) | v4 >> 12;
				v1 += m6;
				v1 += v5;
				v13 ^= v1;
				v13 = v13 << (32 - 16) | v13 >> 16;
				v9 += v13;
				v5 ^= v9;
				v5 = v5 << (32 - 12) | v5 >> 12;
				v2 += m0;
				v2 += v6;
				v14 ^= v2;
				v14 = v14 << (32 - 16) | v14 >> 16;
				v10 += v14;
				v6 ^= v10;
				v6 = v6 << (32 - 12) | v6 >> 12;
				v3 += m8;
				v3 += v7;
				v15 ^= v3;
				v15 = v15 << (32 - 16) | v15 >> 16;
				v11 += v15;
				v7 ^= v11;
				v7 = v7 << (32 - 12) | v7 >> 12;
				v2 += m11;
				v2 += v6;
				v14 ^= v2;
				v14 = v14 << (32 - 8) | v14 >> 8;
				v10 += v14;
				v6 ^= v10;
				v6 = v6 << (32 - 7) | v6 >> 7;
				v3 += m3;
				v3 += v7;
				v15 ^= v3;
				v15 = v15 << (32 - 8) | v15 >> 8;
				v11 += v15;
				v7 ^= v11;
				v7 = v7 << (32 - 7) | v7 >> 7;
				v1 += m10;
				v1 += v5;
				v13 ^= v1;
				v13 = v13 << (32 - 8) | v13 >> 8;
				v9 += v13;
				v5 ^= v9;
				v5 = v5 << (32 - 7) | v5 >> 7;
				v0 += m12;
				v0 += v4;
				v12 ^= v0;
				v12 = v12 << (32 - 8) | v12 >> 8;
				v8 += v12;
				v4 ^= v8;
				v4 = v4 << (32 - 7) | v4 >> 7;
				v0 += m4;
				v0 += v5;
				v15 ^= v0;
				v15 = v15 << (32 - 16) | v15 >> 16;
				v10 += v15;
				v5 ^= v10;
				v5 = v5 << (32 - 12) | v5 >> 12;
				v1 += m7;
				v1 += v6;
				v12 ^= v1;
				v12 = v12 << (32 - 16) | v12 >> 16;
				v11 += v12;
				v6 ^= v11;
				v6 = v6 << (32 - 12) | v6 >> 12;
				v2 += m15;
				v2 += v7;
				v13 ^= v2;
				v13 = v13 << (32 - 16) | v13 >> 16;
				v8 += v13;
				v7 ^= v8;
				v7 = v7 << (32 - 12) | v7 >> 12;
				v3 += m1;
				v3 += v4;
				v14 ^= v3;
				v14 = v14 << (32 - 16) | v14 >> 16;
				v9 += v14;
				v4 ^= v9;
				v4 = v4 << (32 - 12) | v4 >> 12;
				v2 += m14;
				v2 += v7;
				v13 ^= v2;
				v13 = v13 << (32 - 8) | v13 >> 8;
				v8 += v13;
				v7 ^= v8;
				v7 = v7 << (32 - 7) | v7 >> 7;
				v3 += m9;
				v3 += v4;
				v14 ^= v3;
				v14 = v14 << (32 - 8) | v14 >> 8;
				v9 += v14;
				v4 ^= v9;
				v4 = v4 << (32 - 7) | v4 >> 7;
				v1 += m5;
				v1 += v6;
				v12 ^= v1;
				v12 = v12 << (32 - 8) | v12 >> 8;
				v11 += v12;
				v6 ^= v11;
				v6 = v6 << (32 - 7) | v6 >> 7;
				v0 += m13;
				v0 += v5;
				v15 ^= v0;
				v15 = v15 << (32 - 8) | v15 >> 8;
				v10 += v15;
				v5 ^= v10;
				v5 = v5 << (32 - 7) | v5 >> 7;

				// Round 7.
				v0 += m12;
				v0 += v4;
				v12 ^= v0;
				v12 = v12 << (32 - 16) | v12 >> 16;
				v8 += v12;
				v4 ^= v8;
				v4 = v4 << (32 - 12) | v4 >> 12;
				v1 += m1;
				v1 += v5;
				v13 ^= v1;
				v13 = v13 << (32 - 16) | v13 >> 16;
				v9 += v13;
				v5 ^= v9;
				v5 = v5 << (32 - 12) | v5 >> 12;
				v2 += m14;
				v2 += v6;
				v14 ^= v2;
				v14 = v14 << (32 - 16) | v14 >> 16;
				v10 += v14;
				v6 ^= v10;
				v6 = v6 << (32 - 12) | v6 >> 12;
				v3 += m4;
				v3 += v7;
				v15 ^= v3;
				v15 = v15 << (32 - 16) | v15 >> 16;
				v11 += v15;
				v7 ^= v11;
				v7 = v7 << (32 - 12) | v7 >> 12;
				v2 += m13;
				v2 += v6;
				v14 ^= v2;
				v14 = v14 << (32 - 8) | v14 >> 8;
				v10 += v14;
				v6 ^= v10;
				v6 = v6 << (32 - 7) | v6 >> 7;
				v3 += m10;
				v3 += v7;
				v15 ^= v3;
				v15 = v15 << (32 - 8) | v15 >> 8;
				v11 += v15;
				v7 ^= v11;
				v7 = v7 << (32 - 7) | v7 >> 7;
				v1 += m15;
				v1 += v5;
				v13 ^= v1;
				v13 = v13 << (32 - 8) | v13 >> 8;
				v9 += v13;
				v5 ^= v9;
				v5 = v5 << (32 - 7) | v5 >> 7;
				v0 += m5;
				v0 += v4;
				v12 ^= v0;
				v12 = v12 << (32 - 8) | v12 >> 8;
				v8 += v12;
				v4 ^= v8;
				v4 = v4 << (32 - 7) | v4 >> 7;
				v0 += m0;
				v0 += v5;
				v15 ^= v0;
				v15 = v15 << (32 - 16) | v15 >> 16;
				v10 += v15;
				v5 ^= v10;
				v5 = v5 << (32 - 12) | v5 >> 12;
				v1 += m6;
				v1 += v6;
				v12 ^= v1;
				v12 = v12 << (32 - 16) | v12 >> 16;
				v11 += v12;
				v6 ^= v11;
				v6 = v6 << (32 - 12) | v6 >> 12;
				v2 += m9;
				v2 += v7;
				v13 ^= v2;
				v13 = v13 << (32 - 16) | v13 >> 16;
				v8 += v13;
				v7 ^= v8;
				v7 = v7 << (32 - 12) | v7 >> 12;
				v3 += m8;
				v3 += v4;
				v14 ^= v3;
				v14 = v14 << (32 - 16) | v14 >> 16;
				v9 += v14;
				v4 ^= v9;
				v4 = v4 << (32 - 12) | v4 >> 12;
				v2 += m2;
				v2 += v7;
				v13 ^= v2;
				v13 = v13 << (32 - 8) | v13 >> 8;
				v8 += v13;
				v7 ^= v8;
				v7 = v7 << (32 - 7) | v7 >> 7;
				v3 += m11;
				v3 += v4;
				v14 ^= v3;
				v14 = v14 << (32 - 8) | v14 >> 8;
				v9 += v14;
				v4 ^= v9;
				v4 = v4 << (32 - 7) | v4 >> 7;
				v1 += m3;
				v1 += v6;
				v12 ^= v1;
				v12 = v12 << (32 - 8) | v12 >> 8;
				v11 += v12;
				v6 ^= v11;
				v6 = v6 << (32 - 7) | v6 >> 7;
				v0 += m7;
				v0 += v5;
				v15 ^= v0;
				v15 = v15 << (32 - 8) | v15 >> 8;
				v10 += v15;
				v5 ^= v10;
				v5 = v5 << (32 - 7) | v5 >> 7;

				// Round 8.
				v0 += m13;
				v0 += v4;
				v12 ^= v0;
				v12 = v12 << (32 - 16) | v12 >> 16;
				v8 += v12;
				v4 ^= v8;
				v4 = v4 << (32 - 12) | v4 >> 12;
				v1 += m7;
				v1 += v5;
				v13 ^= v1;
				v13 = v13 << (32 - 16) | v13 >> 16;
				v9 += v13;
				v5 ^= v9;
				v5 = v5 << (32 - 12) | v5 >> 12;
				v2 += m12;
				v2 += v6;
				v14 ^= v2;
				v14 = v14 << (32 - 16) | v14 >> 16;
				v10 += v14;
				v6 ^= v10;
				v6 = v6 << (32 - 12) | v6 >> 12;
				v3 += m3;
				v3 += v7;
				v15 ^= v3;
				v15 = v15 << (32 - 16) | v15 >> 16;
				v11 += v15;
				v7 ^= v11;
				v7 = v7 << (32 - 12) | v7 >> 12;
				v2 += m1;
				v2 += v6;
				v14 ^= v2;
				v14 = v14 << (32 - 8) | v14 >> 8;
				v10 += v14;
				v6 ^= v10;
				v6 = v6 << (32 - 7) | v6 >> 7;
				v3 += m9;
				v3 += v7;
				v15 ^= v3;
				v15 = v15 << (32 - 8) | v15 >> 8;
				v11 += v15;
				v7 ^= v11;
				v7 = v7 << (32 - 7) | v7 >> 7;
				v1 += m14;
				v1 += v5;
				v13 ^= v1;
				v13 = v13 << (32 - 8) | v13 >> 8;
				v9 += v13;
				v5 ^= v9;
				v5 = v5 << (32 - 7) | v5 >> 7;
				v0 += m11;
				v0 += v4;
				v12 ^= v0;
				v12 = v12 << (32 - 8) | v12 >> 8;
				v8 += v12;
				v4 ^= v8;
				v4 = v4 << (32 - 7) | v4 >> 7;
				v0 += m5;
				v0 += v5;
				v15 ^= v0;
				v15 = v15 << (32 - 16) | v15 >> 16;
				v10 += v15;
				v5 ^= v10;
				v5 = v5 << (32 - 12) | v5 >> 12;
				v1 += m15;
				v1 += v6;
				v12 ^= v1;
				v12 = v12 << (32 - 16) | v12 >> 16;
				v11 += v12;
				v6 ^= v11;
				v6 = v6 << (32 - 12) | v6 >> 12;
				v2 += m8;
				v2 += v7;
				v13 ^= v2;
				v13 = v13 << (32 - 16) | v13 >> 16;
				v8 += v13;
				v7 ^= v8;
				v7 = v7 << (32 - 12) | v7 >> 12;
				v3 += m2;
				v3 += v4;
				v14 ^= v3;
				v14 = v14 << (32 - 16) | v14 >> 16;
				v9 += v14;
				v4 ^= v9;
				v4 = v4 << (32 - 12) | v4 >> 12;
				v2 += m6;
				v2 += v7;
				v13 ^= v2;
				v13 = v13 << (32 - 8) | v13 >> 8;
				v8 += v13;
				v7 ^= v8;
				v7 = v7 << (32 - 7) | v7 >> 7;
				v3 += m10;
				v3 += v4;
				v14 ^= v3;
				v14 = v14 << (32 - 8) | v14 >> 8;
				v9 += v14;
				v4 ^= v9;
				v4 = v4 << (32 - 7) | v4 >> 7;
				v1 += m4;
				v1 += v6;
				v12 ^= v1;
				v12 = v12 << (32 - 8) | v12 >> 8;
				v11 += v12;
				v6 ^= v11;
				v6 = v6 << (32 - 7) | v6 >> 7;
				v0 += m0;
				v0 += v5;
				v15 ^= v0;
				v15 = v15 << (32 - 8) | v15 >> 8;
				v10 += v15;
				v5 ^= v10;
				v5 = v5 << (32 - 7) | v5 >> 7;

				// Round 9.
				v0 += m6;
				v0 += v4;
				v12 ^= v0;
				v12 = v12 << (32 - 16) | v12 >> 16;
				v8 += v12;
				v4 ^= v8;
				v4 = v4 << (32 - 12) | v4 >> 12;
				v1 += m14;
				v1 += v5;
				v13 ^= v1;
				v13 = v13 << (32 - 16) | v13 >> 16;
				v9 += v13;
				v5 ^= v9;
				v5 = v5 << (32 - 12) | v5 >> 12;
				v2 += m11;
				v2 += v6;
				v14 ^= v2;
				v14 = v14 << (32 - 16) | v14 >> 16;
				v10 += v14;
				v6 ^= v10;
				v6 = v6 << (32 - 12) | v6 >> 12;
				v3 += m0;
				v3 += v7;
				v15 ^= v3;
				v15 = v15 << (32 - 16) | v15 >> 16;
				v11 += v15;
				v7 ^= v11;
				v7 = v7 << (32 - 12) | v7 >> 12;
				v2 += m3;
				v2 += v6;
				v14 ^= v2;
				v14 = v14 << (32 - 8) | v14 >> 8;
				v10 += v14;
				v6 ^= v10;
				v6 = v6 << (32 - 7) | v6 >> 7;
				v3 += m8;
				v3 += v7;
				v15 ^= v3;
				v15 = v15 << (32 - 8) | v15 >> 8;
				v11 += v15;
				v7 ^= v11;
				v7 = v7 << (32 - 7) | v7 >> 7;
				v1 += m9;
				v1 += v5;
				v13 ^= v1;
				v13 = v13 << (32 - 8) | v13 >> 8;
				v9 += v13;
				v5 ^= v9;
				v5 = v5 << (32 - 7) | v5 >> 7;
				v0 += m15;
				v0 += v4;
				v12 ^= v0;
				v12 = v12 << (32 - 8) | v12 >> 8;
				v8 += v12;
				v4 ^= v8;
				v4 = v4 << (32 - 7) | v4 >> 7;
				v0 += m12;
				v0 += v5;
				v15 ^= v0;
				v15 = v15 << (32 - 16) | v15 >> 16;
				v10 += v15;
				v5 ^= v10;
				v5 = v5 << (32 - 12) | v5 >> 12;
				v1 += m13;
				v1 += v6;
				v12 ^= v1;
				v12 = v12 << (32 - 16) | v12 >> 16;
				v11 += v12;
				v6 ^= v11;
				v6 = v6 << (32 - 12) | v6 >> 12;
				v2 += m1;
				v2 += v7;
				v13 ^= v2;
				v13 = v13 << (32 - 16) | v13 >> 16;
				v8 += v13;
				v7 ^= v8;
				v7 = v7 << (32 - 12) | v7 >> 12;
				v3 += m10;
				v3 += v4;
				v14 ^= v3;
				v14 = v14 << (32 - 16) | v14 >> 16;
				v9 += v14;
				v4 ^= v9;
				v4 = v4 << (32 - 12) | v4 >> 12;
				v2 += m4;
				v2 += v7;
				v13 ^= v2;
				v13 = v13 << (32 - 8) | v13 >> 8;
				v8 += v13;
				v7 ^= v8;
				v7 = v7 << (32 - 7) | v7 >> 7;
				v3 += m5;
				v3 += v4;
				v14 ^= v3;
				v14 = v14 << (32 - 8) | v14 >> 8;
				v9 += v14;
				v4 ^= v9;
				v4 = v4 << (32 - 7) | v4 >> 7;
				v1 += m7;
				v1 += v6;
				v12 ^= v1;
				v12 = v12 << (32 - 8) | v12 >> 8;
				v11 += v12;
				v6 ^= v11;
				v6 = v6 << (32 - 7) | v6 >> 7;
				v0 += m2;
				v0 += v5;
				v15 ^= v0;
				v15 = v15 << (32 - 8) | v15 >> 8;
				v10 += v15;
				v5 ^= v10;
				v5 = v5 << (32 - 7) | v5 >> 7;

				// Round 10.
				v0 += m10;
				v0 += v4;
				v12 ^= v0;
				v12 = v12 << (32 - 16) | v12 >> 16;
				v8 += v12;
				v4 ^= v8;
				v4 = v4 << (32 - 12) | v4 >> 12;
				v1 += m8;
				v1 += v5;
				v13 ^= v1;
				v13 = v13 << (32 - 16) | v13 >> 16;
				v9 += v13;
				v5 ^= v9;
				v5 = v5 << (32 - 12) | v5 >> 12;
				v2 += m7;
				v2 += v6;
				v14 ^= v2;
				v14 = v14 << (32 - 16) | v14 >> 16;
				v10 += v14;
				v6 ^= v10;
				v6 = v6 << (32 - 12) | v6 >> 12;
				v3 += m1;
				v3 += v7;
				v15 ^= v3;
				v15 = v15 << (32 - 16) | v15 >> 16;
				v11 += v15;
				v7 ^= v11;
				v7 = v7 << (32 - 12) | v7 >> 12;
				v2 += m6;
				v2 += v6;
				v14 ^= v2;
				v14 = v14 << (32 - 8) | v14 >> 8;
				v10 += v14;
				v6 ^= v10;
				v6 = v6 << (32 - 7) | v6 >> 7;
				v3 += m5;
				v3 += v7;
				v15 ^= v3;
				v15 = v15 << (32 - 8) | v15 >> 8;
				v11 += v15;
				v7 ^= v11;
				v7 = v7 << (32 - 7) | v7 >> 7;
				v1 += m4;
				v1 += v5;
				v13 ^= v1;
				v13 = v13 << (32 - 8) | v13 >> 8;
				v9 += v13;
				v5 ^= v9;
				v5 = v5 << (32 - 7) | v5 >> 7;
				v0 += m2;
				v0 += v4;
				v12 ^= v0;
				v12 = v12 << (32 - 8) | v12 >> 8;
				v8 += v12;
				v4 ^= v8;
				v4 = v4 << (32 - 7) | v4 >> 7;
				v0 += m15;
				v0 += v5;
				v15 ^= v0;
				v15 = v15 << (32 - 16) | v15 >> 16;
				v10 += v15;
				v5 ^= v10;
				v5 = v5 << (32 - 12) | v5 >> 12;
				v1 += m9;
				v1 += v6;
				v12 ^= v1;
				v12 = v12 << (32 - 16) | v12 >> 16;
				v11 += v12;
				v6 ^= v11;
				v6 = v6 << (32 - 12) | v6 >> 12;
				v2 += m3;
				v2 += v7;
				v13 ^= v2;
				v13 = v13 << (32 - 16) | v13 >> 16;
				v8 += v13;
				v7 ^= v8;
				v7 = v7 << (32 - 12) | v7 >> 12;
				v3 += m13;
				v3 += v4;
				v14 ^= v3;
				v14 = v14 << (32 - 16) | v14 >> 16;
				v9 += v14;
				v4 ^= v9;
				v4 = v4 << (32 - 12) | v4 >> 12;
				v2 += m12;
				v2 += v7;
				v13 ^= v2;
				v13 = v13 << (32 - 8) | v13 >> 8;
				v8 += v13;
				v7 ^= v8;
				v7 = v7 << (32 - 7) | v7 >> 7;
				v3 += m0;
				v3 += v4;
				v14 ^= v3;
				v14 = v14 << (32 - 8) | v14 >> 8;
				v9 += v14;
				v4 ^= v9;
				v4 = v4 << (32 - 7) | v4 >> 7;
				v1 += m14;
				v1 += v6;
				v12 ^= v1;
				v12 = v12 << (32 - 8) | v12 >> 8;
				v11 += v12;
				v6 ^= v11;
				v6 = (v6 << (32 - 7)) | (v6 >> 7);
				v0 += m11;
				v0 += v5;
				v15 ^= v0;
				v15 = (v15 << (32 - 8)) | (v15 >> 8);
				v10 += v15;
				v5 ^= v10;
				v5 = (v5 << (32 - 7)) | (v5 >> 7);
		//*/
				//Finalization
				_h[0] ^= v0 ^ v8;
				_h[1] ^= v1 ^ v9;
				_h[2] ^= v2 ^ v10;
				_h[3] ^= v3 ^ v11;
				_h[4] ^= v4 ^ v12;
				_h[5] ^= v5 ^ v13;
				_h[6] ^= v6 ^ v14;
				_h[7] ^= v7 ^ v15;
			}
		}
		// Ends here: -->
		
		// Core.cs file: <--
		public sealed partial class Blake2sCore
		{
			private bool _isInitialized = false;

			private int _bufferFilled;
			private byte[] _buf = new byte[64]; //

			private UInt32[] _m = new UInt32[16]; 
			private UInt32[] _h = new UInt32[8]; // stays the same
			private UInt32 _counter0;
			private UInt32 _counter1;
			private UInt32 _finalizationFlag0;
			private UInt32 _finalizationFlag1;

			//private const int NumberOfRounds = 10; //
			public const int BlockSizeInBytes = 64; //

			const UInt32 IV0 = 0x6A09E667U; //
			const UInt32 IV1 = 0xBB67AE85U; //
			const UInt32 IV2 = 0x3C6EF372U; //
			const UInt32 IV3 = 0xA54FF53AU; //
			const UInt32 IV4 = 0x510E527FU; //
			const UInt32 IV5 = 0x9B05688CU; //
			const UInt32 IV6 = 0x1F83D9ABU; //
			const UInt32 IV7 = 0x5BE0CD19U; //

			/*private static readonly int[] Sigma = new int[NumberOfRounds * 16] {
				0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
				14, 10, 4, 8, 9, 15, 13, 6, 1, 12, 0, 2, 11, 7, 5, 3,
				11, 8, 12, 0, 5, 2, 15, 13, 10, 14, 3, 6, 7, 1, 9, 4,
				7, 9, 3, 1, 13, 12, 11, 14, 2, 6, 5, 10, 4, 0, 15, 8,
				9, 0, 5, 7, 2, 4, 10, 15, 14, 1, 11, 12, 6, 8, 3, 13,
				2, 12, 6, 10, 0, 11, 8, 3, 4, 13, 7, 5, 15, 14, 1, 9,
				12, 5, 1, 15, 14, 13, 4, 10, 0, 7, 6, 3, 9, 2, 8, 11,
				13, 11, 7, 14, 12, 1, 3, 9, 5, 0, 15, 4, 8, 6, 2, 10,
				6, 15, 14, 9, 11, 3, 0, 8, 12, 2, 13, 7, 1, 4, 10, 5,
				10, 2, 8, 4, 7, 6, 1, 5, 15, 11, 9, 14, 3, 12, 13, 0,
			}; //*/

			internal static UInt32 BytesToUInt32(byte[] buf, int offset) //
			{
				return
					(
					 ((UInt32)buf[offset + 3] << 24) +  //
					 ((UInt32)buf[offset + 2] << 16) + //
					 ((UInt32)buf[offset + 1] << 8) + //
					  (UInt32)buf[offset]
					); //
			}

			private static void UInt32ToBytes(UInt32 value, byte[] buf, int offset) //
			{
				buf[offset + 3] = (byte)(value >> 24); //
				buf[offset + 2] = (byte)(value >> 16); //
				buf[offset + 1] = (byte)(value >> 8); //
				buf[offset] = (byte)value;
			}

			partial void Compress(byte[] block, int start);

			public void Initialize(UInt32[] salt) //
			{
				if (salt == null)
					throw new ArgumentNullException("salt");
				if (salt.Length != 8)
					throw new ArgumentException("salt length must be 8 words");
				_isInitialized = true;

				_h[0] = IV0;
				_h[1] = IV1;
				_h[2] = IV2;
				_h[3] = IV3;
				_h[4] = IV4;
				_h[5] = IV5;
				_h[6] = IV6;
				_h[7] = IV7;

				_counter0 = 0;
				_counter1 = 0;
				_finalizationFlag0 = 0;
				_finalizationFlag1 = 0;

				_bufferFilled = 0;

				Array.Clear(_buf, 0, _buf.Length);

				for (int i = 0; i < _h.Length; i++)
					_h[i] ^= salt[i];
			}

			public void HashCore(byte[] array, int start, int count)
			{
				if (!_isInitialized)
					throw new InvalidOperationException("Not initialized");
				if (array == null)
					throw new ArgumentNullException("array");
				if (start < 0)
					throw new ArgumentOutOfRangeException("start");
				if (count < 0)
					throw new ArgumentOutOfRangeException("count");
				if ((long)start + (long)count > array.Length)
					throw new ArgumentOutOfRangeException("start+count");
				int offset = start;
				int bufferRemaining = BlockSizeInBytes - _bufferFilled;

				if ((_bufferFilled > 0) && (count > bufferRemaining))
				{
					Array.Copy(array, offset, _buf, _bufferFilled, bufferRemaining);
					_counter0 += BlockSizeInBytes;
					if (_counter0 == 0)
						_counter1++;
					Compress(_buf, 0);
					offset += bufferRemaining;
					count -= bufferRemaining;
					_bufferFilled = 0;
				}

				while (count > BlockSizeInBytes)
				{
					_counter0 += BlockSizeInBytes;
					if (_counter0 == 0)
						_counter1++;
					Compress(array, offset);
					offset += BlockSizeInBytes;
					count -= BlockSizeInBytes;
				}

				if (count > 0)
				{
					Array.Copy(array, offset, _buf, _bufferFilled, count);
					_bufferFilled += count;
				}
			}

			public byte[] HashFinal()
			{
				return HashFinal(false);
			}

			public byte[] HashFinal(bool isEndOfLayer)
			{
				if (!_isInitialized)
					throw new InvalidOperationException("Not initialized");
				_isInitialized = false;

				//Last compression
				_counter0 += (uint)_bufferFilled;
				_finalizationFlag0 = UInt32.MaxValue; //
				//if (isEndOfLayer) // tree mode
				//    _finalizationFlag1 = UInt32.MaxValue; //
				for (int i = _bufferFilled; i < _buf.Length; i++)
					_buf[i] = 0;
				Compress(_buf, 0);

				//Output
				byte[] hash = new byte[32]; //
				for (int i = 0; i < 8; i++) //
					UInt32ToBytes(_h[i], hash, i * 4); //
				return hash;
			}
		}
		// Ends here: -->
		
		
		// Hasher.cs file: <--
		internal class Blake2sHasher : Hasher
		{
			private Blake2sCore core = new Blake2sCore();
			private UInt32[] rawConfig = null; // no longer read only
			private byte[] key = null;
			private int outputSizeInBytes;
			private static readonly Blake2sConfig DefaultConfig = new Blake2sConfig();

			public override void Init()
			{
				core.Initialize(rawConfig);
				if (key != null)
				{
					core.HashCore(key, 0, key.Length);
				}
			}

			public override byte[] Finish()
			{
				byte[] fullResult = core.HashFinal();
				if (outputSizeInBytes != fullResult.Length)
				{
					byte[] result = new byte[outputSizeInBytes];
					Array.Copy(fullResult, result, result.Length);
					return result;
				}
				else 
					return fullResult;
			}

			public Blake2sHasher(Blake2sConfig config)
			{
				if (config == null)
					config = DefaultConfig;
				rawConfig = Blake2sIvBuilder.ConfigS(config); //, null); no tree config;
				if (config.Key != null && config.Key.Length != 0)
				{
					key = new byte[Blake2sCore.BlockSizeInBytes]; //  DOES THIS NEED TO BE THE BLOCK LENGTH?!
					Array.Copy(config.Key, key, config.Key.Length);
				}
				outputSizeInBytes = config.OutputSizeInBytes;
				Init();
			}

			public override void Update(byte[] data, int start, int count)
			{
				core.HashCore(data, start, count);
			}
		}

		public abstract class Hasher
		{
			public abstract void Init();
			public abstract byte[] Finish();
			public abstract void Update(byte[] data, int start, int count);

			public void Update(byte[] data)
			{
				Update(data, 0, data.Length);
			}

			public HashAlgorithm AsHashAlgorithm()
			{
				return new HashAlgorithmAdapter(this);
			}

			internal class HashAlgorithmAdapter : HashAlgorithm
			{
				private readonly Hasher _hasher;

				protected override void HashCore(byte[] array, int ibStart, int cbSize)
				{
					_hasher.Update(array, ibStart, cbSize);
				}

				protected override byte[] HashFinal()
				{
					return _hasher.Finish();
				}

				public override void Initialize()
				{
					_hasher.Init();
				}

				public HashAlgorithmAdapter(Hasher hasher)
				{
					_hasher = hasher;
				}
			}
		}
		// Ends here: -->
		
		// IvBuilder.cs file: <--
		
		/// <summary>
		/// Class to build the IV.
		/// </summary>
		internal static class Blake2sIvBuilder
		{
			/// <summary>
			/// </summary>
			/// <param name="config">A valid Blake2sConfig.</param>
			/// <returns></returns>
			/// <exception cref="ArgumentOutOfRangeException"></exception>
			/// <exception cref="ArgumentException"></exception>
			public static uint[] ConfigS(Blake2sConfig config)
			{
				var rawConfig = new uint[8];
				//digest length
				if (config.OutputSizeInBytes <= 0 | config.OutputSizeInBytes > 32) //
					throw new ArgumentOutOfRangeException("config.OutputSize");
				rawConfig[0] |= (uint) config.OutputSizeInBytes; //

				//Key length
				if (config.Key != null)
				{
					if (config.Key.Length > 32) //
						throw new ArgumentException("config.Key", "Key too long");
					rawConfig[0] |= (uint) (config.Key.Length << 8); //
				}
				// Fan Out =1 and Max Height / Depth = 1
				rawConfig[0] |= 1 << 16;
				rawConfig[0] |= 1 << 24;
				// Leaf Length and Inner Length 0, no need to worry about them
				// Salt
				if (config.Salt != null)
				{
					if (config.Salt.Length != 8)
						throw new ArgumentException("config.Salt has invalid length");
					rawConfig[4] = Blake2sCore.BytesToUInt32(config.Salt, 0);
					rawConfig[5] = Blake2sCore.BytesToUInt32(config.Salt, 4);
				}
				// Personalization
				if (config.Personalization != null)
				{
					if (config.Personalization.Length != 8)
						throw new ArgumentException("config.Personalization has invalid length");
					rawConfig[6] = Blake2sCore.BytesToUInt32(config.Personalization, 0);
					rawConfig[7] = Blake2sCore.BytesToUInt32(config.Personalization, 4);
				}

				return rawConfig;
			}
		}
		// Ends here: -->
		
		// Config.cs file: <--
		public sealed class Blake2sConfig : ICloneable
		{
			public byte[] Personalization { get; set; }
			public byte[] Salt { get; set; }
			public byte[] Key { get; set; }
			public int OutputSizeInBytes { get; set; }
			public int OutputSizeInBits
			{
				get { return OutputSizeInBytes * 8; }
				set
				{
					if (value % 8 == 0)
						throw new ArgumentException("Output size must be a multiple of 8 bits");
					OutputSizeInBytes = value / 8;
				}
			}

			public Blake2sConfig()
			{
				OutputSizeInBytes = 32;
			}

			public Blake2sConfig Clone()
			{
				var result = new Blake2sConfig();
				result.OutputSizeInBytes = OutputSizeInBytes;
				if (Key != null)
					result.Key = (byte[])Key.Clone();
				if (Personalization != null)
					result.Personalization = (byte[])Personalization.Clone();
				if (Salt != null)
					result.Salt = (byte[])Salt.Clone();
				return result;
			}

			object ICloneable.Clone()
			{
				return Clone();
			}
		}
		// Ends here: -->
		
		// Blake2s.cs file: <--
		
		/// <summary>
		/// BLAKE2s hash function.
		/// </summary>
		public static class Blake2S
		{
			private const int BytesMin = 1;
			private const int BytesMax = 64;
			private const int KeyBytesMin = 16;
			private const int KeyBytesMax = 64;
			private const int OutBytes = 64;
			private const int SaltBytes = 16;
			private const int PersonalBytes = 16;

			public static Hasher Create()
			{
				return Create(new Blake2sConfig());
			}

			public static Hasher Create(Blake2sConfig config)
			{
				return new Blake2sHasher(config);
			}

			public static byte[] ComputeHash(byte[] data, int start, int count)
			{
				return ComputeHash(data, start, count, null);
			}

			public static byte[] ComputeHash(byte[] data)
			{
				return ComputeHash(data, 0, data.Length, null);
			}

			public static byte[] ComputeHash(byte[] data, Blake2sConfig config)
			{
				return ComputeHash(data, 0, data.Length, config);
			}

			public static byte[] ComputeHash(byte[] data, int start, int count, Blake2sConfig config)
			{
				var hasher = Create(config);
				hasher.Update(data, start, count);
				return hasher.Finish();
			}

			/// <summary>Hashes a message, with an optional key, using the BLAKE2s primitive.</summary>
			/// <param name="message">The message to be hashed.</param>
			/// <param name="key">The key; may be null, otherwise between 16 and 64 bytes.</param>
			/// <param name="bytes">The size (in bytes) of the desired result.</param>
			/// <returns>Returns a byte array.</returns>
			/// <exception cref="KeyOutOfRangeException"></exception>
			/// <exception cref="BytesOutOfRangeException"></exception>
			/// <exception cref="OverflowException"></exception>
			public static byte[] Hash(string message, string key, int bytes)
			{
				return Hash(message, Encoding.UTF8.GetBytes(key), bytes);
			}

			/// <summary>Hashes a message, with an optional key, using the BLAKE2s primitive.</summary>
			/// <param name="message">The message to be hashed.</param>
			/// <param name="key">The key; may be null, otherwise between 16 and 64 bytes.</param>
			/// <param name="bytes">The size (in bytes) of the desired result.</param>
			/// <returns>Returns a byte array.</returns>
			/// <exception cref="KeyOutOfRangeException"></exception>
			/// <exception cref="BytesOutOfRangeException"></exception>
			/// <exception cref="OverflowException"></exception>
			public static byte[] Hash(string message, byte[] key, int bytes)
			{
				return Hash(Encoding.UTF8.GetBytes(message), key, bytes);
			}

			/// <summary>Hashes a message, with an optional key, using the BLAKE2s primitive.</summary>
			/// <param name="message">The message to be hashed.</param>
			/// <param name="key">The key; may be null, otherwise between 16 and 64 bytes.</param>
			/// <param name="bytes">The size (in bytes) of the desired result.</param>
			/// <returns>Returns a byte array.</returns>
			/// <exception cref="KeyOutOfRangeException"></exception>
			/// <exception cref="BytesOutOfRangeException"></exception>
			/// <exception cref="OverflowException"></exception>
			public static byte[] Hash(byte[] message, byte[] key, int bytes)
			{
				//validate the length of the key
				if (key != null)
				{
					if (key.Length > KeyBytesMax || key.Length < KeyBytesMin)
					{
						throw new KeyOutOfRangeException(string.Format("key must be between {0} and {1} bytes in length.",
							KeyBytesMin, KeyBytesMax));
					}
				}
				else
				{
					key = new byte[0];
				}

				//validate output length
				if (bytes > BytesMax || bytes < BytesMin)
					throw new BytesOutOfRangeException("bytes", bytes,
						string.Format("bytes must be between {0} and {1} bytes in length.", BytesMin, BytesMax));

				var config = new Blake2sConfig
				{
					Key = key,
					OutputSizeInBytes = bytes
				};

				if (message == null)
				{
					message = new byte[0];
				}

				return ComputeHash(message, 0, message.Length, config);
			}

			/// <summary>Generates a hash based on a key, salt and personal strings</summary>
			/// <returns><c>byte</c> hashed message</returns>
			/// <param name="message">Message.</param>
			/// <param name="key">Key.</param>
			/// <param name="salt">Salt.</param>
			/// <param name="personal">Personal.</param>
			/// <param name="bytes">The size (in bytes) of the desired result.</param>
			/// <exception cref="ArgumentNullException"></exception>
			/// <exception cref="KeyOutOfRangeException"></exception>
			/// <exception cref="SaltOutOfRangeException"></exception>
			/// <exception cref="PersonalOutOfRangeException"></exception>
			/// <exception cref="BytesOutOfRangeException"></exception>
			public static byte[] HashSaltPersonal(string message, string key, string salt, string personal, int bytes = OutBytes)
			{
				return HashSaltPersonal(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(salt), Encoding.UTF8.GetBytes(personal), bytes);
			}

			/// <summary>Generates a hash based on a key, salt and personal bytes</summary>
			/// <returns><c>byte</c> hashed message</returns>
			/// <param name="message">Message.</param>
			/// <param name="key">Key.</param>
			/// <param name="salt">Salt.</param>
			/// <param name="personal">Personal string.</param>
			/// <param name="bytes">The size (in bytes) of the desired result.</param>
			/// <exception cref="ArgumentNullException"></exception>
			/// <exception cref="KeyOutOfRangeException"></exception>
			/// <exception cref="SaltOutOfRangeException"></exception>
			/// <exception cref="PersonalOutOfRangeException"></exception>
			/// <exception cref="BytesOutOfRangeException"></exception>
			public static byte[] HashSaltPersonal(byte[] message, byte[] key, byte[] salt, byte[] personal, int bytes = OutBytes)
			{
				if (message == null)
					throw new ArgumentNullException("message", "Message cannot be null");

				if (salt == null)
					throw new ArgumentNullException("salt", "Salt cannot be null");

				if (personal == null)
					throw new ArgumentNullException("personal", "Personal string cannot be null");

				if (key != null && (key.Length > KeyBytesMax || key.Length < KeyBytesMin))
					throw new KeyOutOfRangeException(string.Format("key must be between {0} and {1} bytes in length.", KeyBytesMin, KeyBytesMax));

				if (key == null)
					key = new byte[0];

				if (salt.Length != SaltBytes)
					throw new SaltOutOfRangeException(string.Format("Salt must be {0} bytes in length.", SaltBytes));

				if (personal.Length != PersonalBytes)
					throw new PersonalOutOfRangeException(string.Format("Personal bytes must be {0} bytes in length.", PersonalBytes));

				//validate output length
				if (bytes > BytesMax || bytes < BytesMin)
					throw new BytesOutOfRangeException("bytes", bytes,
					  string.Format("bytes must be between {0} and {1} bytes in length.", BytesMin, BytesMax));

				var config = new Blake2sConfig
				{
					Key = key,
					OutputSizeInBytes = bytes,
					Personalization = personal,
					Salt = salt
				};

				return ComputeHash(message, 0, message.Length, config);
			}
		}
		// Ends here: -->
		
		
		// Exceptions\BytesOutOfRangeException.cs file: <--
		public class BytesOutOfRangeException : ArgumentOutOfRangeException
		{
			public BytesOutOfRangeException()
			{
			}

			public BytesOutOfRangeException(string message)
				: base(message)
			{
			}

			public BytesOutOfRangeException(string message, Exception inner)
				: base(message, inner)
			{
			}

			public BytesOutOfRangeException(string paramName, object actualValue, string message)
				: base(paramName, actualValue, message)
			{
			}
		}
		// Ends here: -->
		
		// Exceptions\KeyOutOfRangeException.cs file: <--
		public class KeyOutOfRangeException : ArgumentOutOfRangeException
		{
			public KeyOutOfRangeException()
			{
			}

			public KeyOutOfRangeException(string message)
				: base(message)
			{
			}

			public KeyOutOfRangeException(string message, Exception inner)
				: base(message, inner)
			{
			}

			public KeyOutOfRangeException(string paramName, object actualValue, string message)
				: base(paramName, actualValue, message)
			{
			}
		}
		// Ends here: -->
		
		// Exceptions\PersonalOutOfRangeException.cs file: <--
		public class PersonalOutOfRangeException : ArgumentOutOfRangeException
		{
			public PersonalOutOfRangeException()
			{
			}

			public PersonalOutOfRangeException(string message)
				: base(message)
			{
			}

			public PersonalOutOfRangeException(string message, Exception inner)
				: base(message, inner)
			{
			}

			public PersonalOutOfRangeException(string paramName, object actualValue, string message)
				: base(paramName, actualValue, message)
			{
			}
		}
		// Ends here: -->
		
		// Exceptions\SaltOutOfRangeException.cs file: <--
		public class SaltOutOfRangeException : ArgumentOutOfRangeException
		{
			public SaltOutOfRangeException()
			{
			}

			public SaltOutOfRangeException(string message)
				: base(message)
			{
			}

			public SaltOutOfRangeException(string message, Exception inner)
				: base(message, inner)
			{
			}

			public SaltOutOfRangeException(string paramName, object actualValue, string message)
				: base(paramName, actualValue, message)
			{
			}
		}
		// Ends here: -->
	}
	
}
