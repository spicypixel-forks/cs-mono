//
// HMACSHA512Test.cs - NUnit Test Cases for HMACSHA512
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2006, 2007 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography {

	public class HS512 : HMACSHA512 {

		public int BlockSize {
			get { return base.BlockSizeValue; }
			set { base.BlockSizeValue = value; }
		}
	}

	public class SelectableHmacSha512: HMAC {

		// Legacy parameter explanation:
		// http://blogs.msdn.com/shawnfa/archive/2007/01/31/please-do-not-use-the-net-2-0-hmacsha512-and-hmacsha384-classes.aspx

		public SelectableHmacSha512 (byte[] key, bool legacy)
		{
			HashName = "SHA512";
			HashSizeValue = 512;
			BlockSizeValue = legacy ? 64 : 128;
			Key = key;
		}
	}

	// References:
	// a.	Identifiers and Test Vectors for HMAC-SHA-224, HMAC-SHA-256, HMAC-SHA-384, and HMAC-SHA-512
	//	http://www.ietf.org/rfc/rfc4231.txt

	[TestFixture]
	public class HMACSHA512Test : KeyedHashAlgorithmTest {

		protected HMACSHA512 algo;

		[SetUp]
		public override void SetUp () 
		{
			algo = new HMACSHA512 ();
			algo.Key = new byte [8];
			hash = algo;
		}

		// the hash algorithm only exists as a managed implementation
		public override bool ManagedHashImplementation {
			get { return true; }
		}

		[Test]
		public void Constructors () 
		{
			algo = new HMACSHA512 ();
			Assert.IsNotNull (algo, "HMACSHA512 ()");

			byte[] key = new byte [8];
			algo = new HMACSHA512 (key);
			Assert.IsNotNull (algo, "HMACSHA512 (key)");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor_Null () 
		{
			new HMACSHA512 (null);
		}

		[Test]
		public void Invariants () 
		{
			algo = new HMACSHA512 ();
			Assert.IsTrue (algo.CanReuseTransform, "HMACSHA512.CanReuseTransform");
			Assert.IsTrue (algo.CanTransformMultipleBlocks, "HMACSHA512.CanTransformMultipleBlocks");
			Assert.AreEqual ("SHA512", algo.HashName, "HMACSHA512.HashName");
			Assert.AreEqual (512, algo.HashSize, "HMACSHA512.HashSize");
			Assert.AreEqual (1, algo.InputBlockSize, "HMACSHA512.InputBlockSize");
			Assert.AreEqual (1, algo.OutputBlockSize, "HMACSHA512.OutputBlockSize");
			Assert.AreEqual ("System.Security.Cryptography.HMACSHA512", algo.ToString (), "HMACSHA512.ToString()");
		}

		// some test case truncate the result
		private void Compare (byte[] expected, byte[] actual, string msg)
		{
			if (expected.Length == actual.Length) {
				Assert.AreEqual (expected, actual, msg);
			} else {
				byte[] data = new byte [expected.Length];
				Array.Copy (actual, data, data.Length);
				Assert.AreEqual (expected, data, msg);
			}
		}

		public void Check (string testName, HMAC algo, byte[] data, byte[] result)
		{
			CheckA (testName, algo, data, result);
			CheckB (testName, algo, data, result);
			CheckC (testName, algo, data, result);
			CheckD (testName, algo, data, result);
			CheckE (testName, algo, data, result);
		}

		public void CheckA (string testName, HMAC algo, byte[] data, byte[] result)
		{
			byte[] hmac = algo.ComputeHash (data);
			Compare (result, hmac, testName + "a1");
			Compare (result, algo.Hash, testName + "a2");
		}

		public void CheckB (string testName, HMAC algo, byte[] data, byte[] result)
		{
			byte[] hmac = algo.ComputeHash (data, 0, data.Length);
			Compare (result, hmac, testName + "b1");
			Compare (result, algo.Hash, testName + "b2");
		}

		public void CheckC (string testName, HMAC algo, byte[] data, byte[] result)
		{
			using (MemoryStream ms = new MemoryStream (data)) {
				byte[] hmac = algo.ComputeHash (ms);
				Compare (result, hmac, testName + "c1");
				Compare (result, algo.Hash, testName + "c2");
			}
		}

		public void CheckD (string testName, HMAC algo, byte[] data, byte[] result)
		{
			algo.TransformFinalBlock (data, 0, data.Length);
			Compare (result, algo.Hash, testName + "d");
			algo.Initialize ();
		}

		public void CheckE (string testName, HMAC algo, byte[] data, byte[] result)
		{
			byte[] copy = new byte[data.Length];
			for (int i = 0; i < data.Length - 1; i++)
				algo.TransformBlock (data, i, 1, copy, i);
			algo.TransformFinalBlock (data, data.Length - 1, 1);
			Compare (result, algo.Hash, testName + "e");
			algo.Initialize ();
		}

		[Test]
		public void RFC4231_TC1_Normal ()
		{
			byte[] key = { 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b };
			byte[] data = Encoding.Default.GetBytes ("Hi There");
			byte[] digest = { 0x87, 0xaa, 0x7c, 0xde, 0xa5, 0xef, 0x61, 0x9d, 0x4f, 0xf0, 0xb4, 0x24, 0x1a, 0x1d, 0x6c, 0xb0,
				0x23, 0x79, 0xf4, 0xe2, 0xce, 0x4e, 0xc2, 0x78, 0x7a, 0xd0, 0xb3, 0x05, 0x45, 0xe1, 0x7c, 0xde,
				0xda, 0xa8, 0x33, 0xb7, 0xd6, 0xb8, 0xa7, 0x02, 0x03, 0x8b, 0x27, 0x4e, 0xae, 0xa3, 0xf4, 0xe4,
				0xbe, 0x9d, 0x91, 0x4e, 0xeb, 0x61, 0xf1, 0x70, 0x2e, 0x69, 0x6c, 0x20, 0x3a, 0x12, 0x68, 0x54 };
			HMAC hmac = new SelectableHmacSha512 (key, false);
			Check ("HMACSHA512-N-RFC4231-TC1", hmac, data, digest);
		}

		[Test]
		// Test with a key shorter than the length of the HMAC output.
		public void RFC4231_TC2_Normal ()
		{
			byte[] key = Encoding.Default.GetBytes ("Jefe");
			byte[] data = Encoding.Default.GetBytes ("what do ya want for nothing?");
			byte[] digest = { 0x16, 0x4b, 0x7a, 0x7b, 0xfc, 0xf8, 0x19, 0xe2, 0xe3, 0x95, 0xfb, 0xe7, 0x3b, 0x56, 0xe0, 0xa3,
				0x87, 0xbd, 0x64, 0x22, 0x2e, 0x83, 0x1f, 0xd6, 0x10, 0x27, 0x0c, 0xd7, 0xea, 0x25, 0x05, 0x54,
				0x97, 0x58, 0xbf, 0x75, 0xc0, 0x5a, 0x99, 0x4a, 0x6d, 0x03, 0x4f, 0x65, 0xf8, 0xf0, 0xe6, 0xfd,
				0xca, 0xea, 0xb1, 0xa3, 0x4d, 0x4a, 0x6b, 0x4b, 0x63, 0x6e, 0x07, 0x0a, 0x38, 0xbc, 0xe7, 0x37 };
			HMAC hmac = new SelectableHmacSha512 (key, false);
			Check ("HMACSHA512-N-RFC4231-TC2", hmac, data, digest);
		}

		[Test]
		// Test with a combined length of key and data that is larger than 64 bytes (= block-size of SHA-224 and SHA-256).
		public void RFC4231_TC3_Normal ()
		{
			byte[] key = { 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa };
			byte[] data = { 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd,
				0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd,
				0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd,
				0xdd, 0xdd };
			byte[] digest = { 0xfa, 0x73, 0xb0, 0x08, 0x9d, 0x56, 0xa2, 0x84, 0xef, 0xb0, 0xf0, 0x75, 0x6c, 0x89, 0x0b, 0xe9,
				0xb1, 0xb5, 0xdb, 0xdd, 0x8e, 0xe8, 0x1a, 0x36, 0x55, 0xf8, 0x3e, 0x33, 0xb2, 0x27, 0x9d, 0x39,
				0xbf, 0x3e, 0x84, 0x82, 0x79, 0xa7, 0x22, 0xc8, 0x06, 0xb4, 0x85, 0xa4, 0x7e, 0x67, 0xc8, 0x07,
				0xb9, 0x46, 0xa3, 0x37, 0xbe, 0xe8, 0x94, 0x26, 0x74, 0x27, 0x88, 0x59, 0xe1, 0x32, 0x92, 0xfb };
			HMAC hmac = new SelectableHmacSha512 (key, false);
			Check ("HMACSHA512-N-RFC4231-TC3", hmac, data, digest);
		}

		[Test]
		// Test with a combined length of key and data that is larger than 64 bytes (= block-size of SHA-224 and SHA-256).
		public void RFC4231_TC4_Normal ()
		{
			byte[] key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10,
				0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19 };
			byte[] data = { 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd,
				0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd,
				0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd,
				0xcd, 0xcd };
			byte[] digest = { 0xb0, 0xba, 0x46, 0x56, 0x37, 0x45, 0x8c, 0x69, 0x90, 0xe5, 0xa8, 0xc5, 0xf6, 0x1d, 0x4a, 0xf7,
				0xe5, 0x76, 0xd9, 0x7f, 0xf9, 0x4b, 0x87, 0x2d, 0xe7, 0x6f, 0x80, 0x50, 0x36, 0x1e, 0xe3, 0xdb,
				0xa9, 0x1c, 0xa5, 0xc1, 0x1a, 0xa2, 0x5e, 0xb4, 0xd6, 0x79, 0x27, 0x5c, 0xc5, 0x78, 0x80, 0x63,
				0xa5, 0xf1, 0x97, 0x41, 0x12, 0x0c, 0x4f, 0x2d, 0xe2, 0xad, 0xeb, 0xeb, 0x10, 0xa2, 0x98, 0xdd };
			HMAC hmac = new SelectableHmacSha512 (key, false);
			Check ("HMACSHA512-N-RFC4231-TC4", hmac, data, digest);
		}

		[Test]
		// Test with a truncation of output to 128 bits.
		public void RFC4231_TC5_Normal ()
		{
			byte[] key = { 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c,
				0x0c, 0x0c, 0x0c, 0x0c };
			byte[] data = Encoding.Default.GetBytes ("Test With Truncation");
			byte[] digest = { 0x41, 0x5f, 0xad, 0x62, 0x71, 0x58, 0x0a, 0x53, 0x1d, 0x41, 0x79, 0xbc, 0x89, 0x1d, 0x87, 0xa6 };
			HMAC hmac = new SelectableHmacSha512 (key, false);
			Check ("HMACSHA512-N-RFC4231-TC5", hmac, data, digest);
		}

		[Test]
		// Test with a key larger than 128 bytes (= block-size of SHA-384 and SHA-512).
		public void RFC4231_TC6_Normal ()
		{
			byte[] key = { 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa };
			byte[] data = Encoding.Default.GetBytes ("Test Using Larger Than Block-Size Key - Hash Key First");
			byte[] digest = { 0x80, 0xb2, 0x42, 0x63, 0xc7, 0xc1, 0xa3, 0xeb, 0xb7, 0x14, 0x93, 0xc1, 0xdd, 0x7b, 0xe8, 0xb4,
				0x9b, 0x46, 0xd1, 0xf4, 0x1b, 0x4a, 0xee, 0xc1, 0x12, 0x1b, 0x01, 0x37, 0x83, 0xf8, 0xf3, 0x52,
				0x6b, 0x56, 0xd0, 0x37, 0xe0, 0x5f, 0x25, 0x98, 0xbd, 0x0f, 0xd2, 0x21, 0x5d, 0x6a, 0x1e, 0x52,
				0x95, 0xe6, 0x4f, 0x73, 0xf6, 0x3f, 0x0a, 0xec, 0x8b, 0x91, 0x5a, 0x98, 0x5d, 0x78, 0x65, 0x98 };
			HMAC hmac = new SelectableHmacSha512 (key, false);
			Check ("HMACSHA512-N-RFC4231-TC6", hmac, data, digest);
		}

		[Test]
		// Test with a key and data that is larger than 128 bytes (= block-size of SHA-384 and SHA-512).
		public void RFC4231_TC7_Normal ()
		{
			byte[] key = { 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa };
			byte[] data = Encoding.Default.GetBytes ("This is a test using a larger than block-size key and a larger than block-size data. The key needs to be hashed before being used by the HMAC algorithm.");
			byte[] digest = { 0xe3, 0x7b, 0x6a, 0x77, 0x5d, 0xc8, 0x7d, 0xba, 0xa4, 0xdf, 0xa9, 0xf9, 0x6e, 0x5e, 0x3f, 0xfd,
				0xde, 0xbd, 0x71, 0xf8, 0x86, 0x72, 0x89, 0x86, 0x5d, 0xf5, 0xa3, 0x2d, 0x20, 0xcd, 0xc9, 0x44,
				0xb6, 0x02, 0x2c, 0xac, 0x3c, 0x49, 0x82, 0xb1, 0x0d, 0x5e, 0xeb, 0x55, 0xc3, 0xe4, 0xde, 0x15,
				0x13, 0x46, 0x76, 0xfb, 0x6d, 0xe0, 0x44, 0x60, 0x65, 0xc9, 0x74, 0x40, 0xfa, 0x8c, 0x6a, 0x58 };
			HMAC hmac = new SelectableHmacSha512 (key, false);
			Check ("HMACSHA512-N-RFC4231-TC7", hmac, data, digest);
		}

		[Test]
		public void RFC4231_TC1_Legacy ()
		{
			byte[] key = { 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b };
			byte[] data = Encoding.Default.GetBytes ("Hi There");
			byte[] digest = { 0x96, 0x56, 0x97, 0x5E, 0xE5, 0xDE, 0x55, 0xE7, 0x5F, 0x29, 0x76, 0xEC, 0xCE, 0x9A, 0x04, 0x50, 
				0x10, 0x60, 0xB9, 0xDC, 0x22, 0xA6, 0xED, 0xA2, 0xEA, 0xEF, 0x63, 0x89, 0x66, 0x28, 0x01, 0x82,
				0x47, 0x7F, 0xE0, 0x9F, 0x08, 0x0B, 0x2B, 0xF5, 0x64, 0x64, 0x9C, 0xAD, 0x42, 0xAF, 0x86, 0x07,
				0xA2, 0xBD, 0x8D, 0x02, 0x97, 0x9D, 0xF3, 0xA9, 0x80, 0xF1, 0x5E, 0x23, 0x26, 0xA0, 0xA2, 0x2A };
			HMAC hmac = new SelectableHmacSha512 (key, true);
			Check ("HMACSHA512-L-RFC4231-TC1", hmac, data, digest);
		}

		[Test]
		// Test with a key shorter than the length of the HMAC output.
		public void RFC4231_TC2_Legacy ()
		{
			byte[] key = Encoding.Default.GetBytes ("Jefe");
			byte[] data = Encoding.Default.GetBytes ("what do ya want for nothing?");
			byte[] digest = { 0xD2, 0x76, 0x6E, 0xCA, 0x33, 0xFE, 0x85, 0x2B, 0xD6, 0x29, 0x25, 0x3F, 0xE0, 0x1C, 0x63, 0x51, 
				0x9E, 0xB2, 0x45, 0x9B, 0xDB, 0x0A, 0xF2, 0x54, 0xBD, 0x43, 0x41, 0x74, 0x0B, 0x4D, 0x0E, 0xA7,
				0x23, 0xC6, 0xA2, 0xA4, 0xDF, 0xC3, 0x42, 0x52, 0x89, 0x1C, 0x14, 0xF2, 0x65, 0x80, 0x55, 0x23,
				0x7A, 0xA3, 0xF6, 0x49, 0x62, 0xD4, 0xD4, 0xDE, 0x21, 0x70, 0xAE, 0x18, 0xFD, 0x91, 0x60, 0xAA };
			HMAC hmac = new SelectableHmacSha512 (key, true);
			Check ("HMACSHA512-L-RFC4231-TC2", hmac, data, digest);
		}

		[Test]
		// Test with a combined length of key and data that is larger than 64 bytes (= block-size of SHA-224 and SHA-256).
		public void RFC4231_TC3_Legacy ()
		{
			byte[] key = { 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa };
			byte[] data = { 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd,
				0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd,
				0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd, 0xdd,
				0xdd, 0xdd };
			byte[] digest = { 0x57, 0xD1, 0xE0, 0xB1, 0x1E, 0xE0, 0x8D, 0xF8, 0x9E, 0x3C, 0xDC, 0xD4, 0xB7, 0xD3, 0xA9, 0x2C, 
				0x42, 0x30, 0x75, 0x24, 0xDB, 0x89, 0xD0, 0xC3, 0x1A, 0xCA, 0xBD, 0x41, 0x58, 0xF5, 0x19, 0xAA, 
				0xC2, 0x2E, 0x0C, 0xA5, 0xBF, 0x05, 0x37, 0xF1, 0x0B, 0xD8, 0x3B, 0xDE, 0x43, 0x07, 0xD6, 0x4D, 
				0xE1, 0x91, 0xA2, 0xCF, 0x12, 0x01, 0x8F, 0x49, 0x95, 0x1C, 0xF5, 0x99, 0xFF, 0x8A, 0xD1, 0x68 };
			HMAC hmac = new SelectableHmacSha512 (key, true);
			Check ("HMACSHA512-L-RFC4231-TC3", hmac, data, digest);
		}

		[Test]
		// Test with a combined length of key and data that is larger than 64 bytes (= block-size of SHA-224 and SHA-256).
		public void RFC4231_TC4_Legacy ()
		{
			byte[] key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10,
				0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19 };
			byte[] data = { 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd,
				0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd,
				0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd,
				0xcd, 0xcd };
			byte[] digest = { 0xF9, 0x82, 0x79, 0x3A, 0x2E, 0x24, 0xF6, 0xEB, 0x82, 0xF5, 0x2D, 0x10, 0x85, 0x2C, 0x13, 0x50,
				0x69, 0xA5, 0x81, 0x99, 0xF8, 0x1C, 0x2E, 0x09, 0x4F, 0xED, 0x96, 0x4E, 0x59, 0xD5, 0x57, 0xE4, 
				0xE1, 0x02, 0x9A, 0x62, 0x9A, 0xF3, 0x45, 0x8C, 0xA8, 0xEE, 0x8A, 0xB4, 0x39, 0x99, 0x32, 0xE8, 
				0xC7, 0x94, 0x4B, 0x37, 0xB5, 0x5E, 0x3C, 0xE8, 0xF5, 0x6D, 0x31, 0xA5, 0x25, 0x11, 0x97, 0xFB };
			HMAC hmac = new SelectableHmacSha512 (key, true);
			Check ("HMACSHA512-L-RFC4231-TC4", hmac, data, digest);
		}

		[Test]
		// Test with a truncation of output to 128 bits.
		public void RFC4231_TC5_Legacy ()
		{
			byte[] key = { 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c,
				0x0c, 0x0c, 0x0c, 0x0c };
			byte[] data = Encoding.Default.GetBytes ("Test With Truncation");
			byte[] digest = { 0xBC, 0x59, 0x5A, 0x3F, 0x08, 0x9B, 0x2C, 0x5E, 0x25, 0x9B, 0x94, 0xAD, 0x7C, 0x48, 0xBA, 0xF3 };
			HMAC hmac = new SelectableHmacSha512 (key, true);
			Check ("HMACSHA512-L-RFC4231-TC5", hmac, data, digest);
		}

		[Test]
		// Test with a key larger than 128 bytes (= block-size of SHA-384 and SHA-512).
		public void RFC4231_TC6_Legacy ()
		{
			byte[] key = { 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa };
			byte[] data = Encoding.Default.GetBytes ("Test Using Larger Than Block-Size Key - Hash Key First");
			byte[] digest = { 0x6A, 0x2D, 0x1C, 0x4B, 0x7B, 0x7A, 0x88, 0x15, 0xBA, 0xC4, 0xEB, 0x5D, 0x41, 0xB6, 0x6F, 0x7F,
				0x55, 0x5D, 0x4A, 0xDF, 0x00, 0xD2, 0x83, 0x3F, 0xDF, 0xCD, 0x2B, 0x55, 0xF4, 0xC5, 0x3D, 0xCA, 
				0xEB, 0x11, 0x86, 0xEE, 0xB6, 0x46, 0x67, 0xB2, 0x54, 0x48, 0x42, 0x94, 0x41, 0x83, 0x57, 0x8E, 
				0x47, 0xCB, 0x73, 0x32, 0x32, 0x0B, 0x35, 0x4F, 0xC0, 0xD5, 0x19, 0xFF, 0x4E, 0x5D, 0x90, 0xAD };
			HMAC hmac = new SelectableHmacSha512 (key, true);
			Check ("HMACSHA512-L-RFC4231-TC6", hmac, data, digest);
		}

		[Test]
		// Test with a key and data that is larger than 128 bytes (= block-size of SHA-384 and SHA-512).
		public void RFC4231_TC7_Legacy ()
		{
			byte[] key = { 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa,
				0xaa, 0xaa, 0xaa };
			byte[] data = Encoding.Default.GetBytes ("This is a test using a larger than block-size key and a larger than block-size data. The key needs to be hashed before being used by the HMAC algorithm.");
			byte[] digest = { 0x18, 0xDC, 0x82, 0xB6, 0x9D, 0x9A, 0xF8, 0xA5, 0x28, 0x46, 0x8C, 0x38, 0x3A, 0x99, 0xAB, 0x2E, 
				0xD4, 0x11, 0x14, 0xA6, 0xBB, 0x24, 0x00, 0x61, 0x13, 0xAA, 0xD2, 0x44, 0x00, 0x5F, 0xA9, 0xC3, 
				0xAC, 0xBE, 0x53, 0x77, 0xB9, 0x3B, 0x4C, 0x5D, 0x17, 0x16, 0x8C, 0xAA, 0x85, 0x77, 0x52, 0x72, 
				0xFF, 0xF4, 0x5A, 0xFC, 0x68, 0xF8, 0x90, 0x27, 0x2F, 0x2E, 0x12, 0x9D, 0x81, 0xB6, 0x48, 0x49 };
			HMAC hmac = new SelectableHmacSha512 (key, true);
			Check ("HMACSHA512-L-RFC4231-TC7", hmac, data, digest);
		}

		[Test]
		public void Bug6510a ()
		{
			byte[] key = Encoding.UTF8.GetBytes ("CA61A777DC1041B2FDCC354820F7F83CE0530C0E019A29BF576F175D314A6D891B35F");
			byte[] data = Encoding.UTF8.GetBytes ("123456789");
			byte[] digest = { 147, 50, 79, 211, 70, 90, 205, 252, 100, 25, 50, 209, 254, 157,
				154, 12, 132, 205, 113, 59, 97, 216, 252, 94, 43, 126, 207, 140, 137, 60, 227,
				82, 82, 240, 9, 15, 166, 167, 110, 85, 217, 245, 250, 169, 189, 138, 55, 197,
				146, 192, 96, 20, 249, 95, 130, 163, 147, 82, 71, 244, 139, 76, 45, 75 };
			HMAC hmac = new SelectableHmacSha512 (key, false);
			Check ("HMACSHA512-BUG-6510", hmac, data, digest);
		}

		[Test]
		public void Bug6510b ()
		{
			byte[] key = Encoding.UTF8.GetBytes ("CA61A777DC1041B2FDCC354820F7F83CE0530C0E019A29BF576F175D314A6D891");
			byte[] data = Encoding.UTF8.GetBytes ("123456789");
			byte[] digest = { 254, 159, 56, 218, 66, 110, 74, 135, 209, 81, 166, 150, 241, 194,
				171, 228, 143, 228, 16, 198, 171, 53, 40, 211, 229, 160, 158, 2, 102, 99, 11,
				49, 70, 53, 70, 32, 32, 219, 119, 14, 2, 149, 197, 220, 166, 38, 128, 25, 255,
				227, 25, 20, 229, 54, 217, 57, 143, 224, 251, 1, 202, 24, 245, 133 };
			HMAC hmac = new SelectableHmacSha512 (key, false);
			Check ("HMACSHA512-BUG-6510", hmac, data, digest);
		}
	}
}