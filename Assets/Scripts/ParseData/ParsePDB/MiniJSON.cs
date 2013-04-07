/// @file MiniJSON.cs
/// @brief Details to be specified
/// @author FvNano/LBT team
/// @author Marc Baaden <baaden@smplinux.de>
/// @date   2013-4
///
/// Copyright Centre National de la Recherche Scientifique (CNRS)
///
/// contributors :
/// FvNano/LBT team, 2010-13
/// Marc Baaden, 2010-13
///
/// baaden@smplinux.de
/// http://www.baaden.ibpc.fr
///
/// This software is a computer program based on the Unity3D game engine.
/// It is part of UnityMol, a general framework whose purpose is to provide
/// a prototype for developing molecular graphics and scientific
/// visualisation applications. More details about UnityMol are provided at
/// the following URL: "http://unitymol.sourceforge.net". Parts of this
/// source code are heavily inspired from the advice provided on the Unity3D
/// forums and the Internet.
///
/// This software is governed by the CeCILL-C license under French law and
/// abiding by the rules of distribution of free software. You can use,
/// modify and/or redistribute the software under the terms of the CeCILL-C
/// license as circulated by CEA, CNRS and INRIA at the following URL:
/// "http://www.cecill.info".
/// 
/// As a counterpart to the access to the source code and rights to copy, 
/// modify and redistribute granted by the license, users are provided only 
/// with a limited warranty and the software's author, the holder of the 
/// economic rights, and the successive licensors have only limited 
/// liability.
///
/// In this respect, the user's attention is drawn to the risks associated 
/// with loading, using, modifying and/or developing or reproducing the 
/// software by the user in light of its specific status of free software, 
/// that may mean that it is complicated to manipulate, and that also 
/// therefore means that it is reserved for developers and experienced 
/// professionals having in-depth computer knowledge. Users are therefore 
/// encouraged to load and test the software's suitability as regards their 
/// requirements in conditions enabling the security of their systems and/or 
/// data to be ensured and, more generally, to use and operate it in the 
/// same conditions as regards security.
///
/// The fact that you are presently reading this means that you have had 
/// knowledge of the CeCILL-C license and that you accept its terms.
///
/// $Id: MiniJSON.cs 227 2013-04-07 15:21:09Z baaden $
///
/// References : 
/// If you use this code, please cite the following reference : 	
/// Z. Lv, A. Tek, F. Da Silva, C. Empereur-mot, M. Chavent and M. Baaden:
/// "Game on, Science - how video game technology may help biologists tackle
/// visualization challenges" (2013), PLoS ONE 8(3):e57990.
/// doi:10.1371/journal.pone.0057990
///
/// If you use the HyperBalls visualization metaphor, please also cite the
/// following reference : M. Chavent, A. Vanel, A. Tek, B. Levy, S. Robert,
/// B. Raffin and M. Baaden: "GPU-accelerated atom and dynamic bond visualization
/// using HyperBalls, a unified algorithm for balls, sticks and hyperboloids",
/// J. Comput. Chem., 2011, 32, 2924
///

// using UnityEngine;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
/* Based on the JSON parser from 
 * http://techblog.procurios.nl/k/618/news/view/14605/14863/How-do-I-write-my-own-parser-for-JSON.html
 * 
 * I simplified it so that it doesn't throw exceptions
 * and can be used in Unity iPhone with maximum code stripping.
 */
/// <summary>
/// This class encodes and decodes JSON strings.
/// Spec. details, see http://www.json.org/
/// 
/// JSON uses Arrays and Objects. These correspond here to the datatypes ArrayList and Hashtable.
/// All numbers are parsed to doubles.
/// </summary>
public class MiniJSON
{
	public const int TOKEN_NONE = 0; 
	public const int TOKEN_CURLY_OPEN = 1;
	public const int TOKEN_CURLY_CLOSE = 2;
	public const int TOKEN_SQUARED_OPEN = 3;
	public const int TOKEN_SQUARED_CLOSE = 4;
	public const int TOKEN_COLON = 5;
	public const int TOKEN_COMMA = 6;
	public const int TOKEN_STRING = 7;
	public const int TOKEN_NUMBER = 8;
	public const int TOKEN_TRUE = 9;
	public const int TOKEN_FALSE = 10;
	public const int TOKEN_NULL = 11;

	private const int BUILDER_CAPACITY = 2000;

	protected static MiniJSON instance = new MiniJSON();

	/// <summary>
	/// On decoding, this value holds the position at which the parse failed (-1 = no error).
	/// </summary>
	protected int lastErrorIndex = -1;
	protected string lastDecode = "";

	/// <summary>
	/// Parses the string json into a value
	/// </summary>
	/// <param name="json">A JSON string.</param>
	/// <returns>An ArrayList, a Hashtable, a double, a string, null, true, or false</returns>
	public static object JsonDecode(string json)
	{
		// save the string for debug information
		MiniJSON.instance.lastDecode = json;

		if (json != null) 
		{
            char[] charArray = json.ToCharArray();
//            for(int jj=charArray.Length-20;jj<charArray.Length;jj++)
//            {
//            	Debug.Log(charArray[jj]);
//            }

            int index = 0;
			bool success = true;
			object value = MiniJSON.instance.ParseValue(charArray, ref index, ref success);
			//Hashtable FieldLines = (Hashtable)value ;
			Debug.Log("object value =");
			
			if (success) 
			{
				MiniJSON.instance.lastErrorIndex = -1;
			} 
			else 
			{
				MiniJSON.instance.lastErrorIndex = index;
			}
//			Debug.Log((string)value);
			return value;
        }
        else 
        {
            return null;
        }
	}

	/// <summary>
	/// Converts a Hashtable / ArrayList object into a JSON string
	/// </summary>
	/// <param name="json">A Hashtable / ArrayList</param>
	/// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
	public static string JsonEncode(object json)
	{
		StringBuilder builder = new StringBuilder(BUILDER_CAPACITY);
		bool success = MiniJSON.instance.SerializeValue(json, builder);
		return (success ? builder.ToString() : null);
	}

	/// <summary>
	/// On decoding, this function returns the position at which the parse failed (-1 = no error).
	/// </summary>
	/// <returns></returns>
	public static bool LastDecodeSuccessful()
	{
		return (MiniJSON.instance.lastErrorIndex == -1);
	}

	/// <summary>
	/// On decoding, this function returns the position at which the parse failed (-1 = no error).
	/// </summary>
	/// <returns></returns>
	public static int GetLastErrorIndex()
	{
		return MiniJSON.instance.lastErrorIndex;
	}

	/// <summary>
	/// If a decoding error occurred, this function returns a piece of the JSON string 
	/// at which the error took place. To ease debugging.
	/// </summary>
	/// <returns></returns>
	public static string GetLastErrorSnippet()
	{
		if (MiniJSON.instance.lastErrorIndex == -1) {
			return "";
		} else {
			int startIndex = MiniJSON.instance.lastErrorIndex - 5;
			int endIndex = MiniJSON.instance.lastErrorIndex + 15;
			if (startIndex < 0) {
				startIndex = 0;
			}
			if (endIndex >= MiniJSON.instance.lastDecode.Length) {
				endIndex = MiniJSON.instance.lastDecode.Length - 1;
			}

			return MiniJSON.instance.lastDecode.Substring(startIndex, endIndex - startIndex + 1);
		}
	}

	protected Hashtable ParseObject(char[] json, ref int index)
	{
		Hashtable table = new Hashtable();
		int token;

		// {
		NextToken(json, ref index);

		bool done = false;
		while (!done) {
			token = LookAhead(json, index);
			if (token == MiniJSON.TOKEN_NONE) {
				return null;
			} else if (token == MiniJSON.TOKEN_COMMA) {
				NextToken(json, ref index);
			} else if (token == MiniJSON.TOKEN_CURLY_CLOSE) {
				NextToken(json, ref index);
				return table;
			} else {

				// name
				string name = ParseString(json, ref index);
				if (name == null) {
					return null;
				}

				// :
				token = NextToken(json, ref index);
				if (token != MiniJSON.TOKEN_COLON) {
					return null;
				}

				// value
				bool success = true;
				object value = ParseValue(json, ref index, ref success);
				if (!success) {
					return null;
				}

				table[name] = value;
			}
		}

		return table;
	}

	protected ArrayList ParseArray(char[] json, ref int index)
	{
		ArrayList array = new ArrayList();

		// [
		NextToken(json, ref index);

		bool done = false;
		while (!done) {
			int token = LookAhead(json, index);
			if (token == MiniJSON.TOKEN_NONE) {
				return null;
			} else if (token == MiniJSON.TOKEN_COMMA) {
				NextToken(json, ref index);
			} else if (token == MiniJSON.TOKEN_SQUARED_CLOSE) {
				NextToken(json, ref index);
				break;
			} else {
				bool success = true;
				object value = ParseValue(json, ref index, ref success);
				if (!success) {
					return null;
				}

				array.Add(value);
			}
		}

		return array;
	}

	protected object ParseValue(char[] json, ref int index, ref bool success)
	{
		switch (LookAhead(json, index)) {
			case MiniJSON.TOKEN_STRING:
//				Debug.Log("TOKEN_STRING");
				return ParseString(json, ref index);
			case MiniJSON.TOKEN_NUMBER:
//				Debug.Log("TOKEN_NUMBER");
				return ParseNumber(json, ref index);
			case MiniJSON.TOKEN_CURLY_OPEN:
//				Debug.Log("TOKEN_CURLY_OPEN");
				return ParseObject(json, ref index);
			case MiniJSON.TOKEN_SQUARED_OPEN:
//				Debug.Log("TOKEN_SQUARED_OPEN");
				return ParseArray(json, ref index);
			case MiniJSON.TOKEN_TRUE:
//				Debug.Log("TOKEN_TRUE");
				NextToken(json, ref index);
				return Boolean.Parse("TRUE");
			case MiniJSON.TOKEN_FALSE:
//				Debug.Log("TOKEN_FALSE");
				NextToken(json, ref index);
				return Boolean.Parse("FALSE");
			case MiniJSON.TOKEN_NULL:
//				Debug.Log("TOKEN_NULL");
				NextToken(json, ref index);
				return null;
			case MiniJSON.TOKEN_NONE:
//				Debug.Log("TOKEN_NONE");
				break;
		}

		success = false;
		return null;
	}

	protected string ParseString(char[] json, ref int index)
	{
		string s = "";
		char c;

		EatWhitespace(json, ref index);
		
		// "
		c = json[index++];

		bool complete = false;
		while (!complete) {

			if (index == json.Length) {
				break;
			}

			c = json[index++];
			if (c == '"') {
				complete = true;
				break;
			} else if (c == '\\') {

				if (index == json.Length) {
					break;
				}
				c = json[index++];
				if (c == '"') {
					s += '"';
				} else if (c == '\\') {
					s += '\\';
				} else if (c == '/') {
					s += '/';
				} else if (c == 'b') {
					s += '\b';
				} else if (c == 'f') {
					s += '\f';
				} else if (c == 'n') {
					s += '\n';
				} else if (c == 'r') {
					s += '\r';
				} else if (c == 't') {
					s += '\t';
				} else if (c == 'u') {
					int remainingLength = json.Length - index;
					if (remainingLength >= 4) {
						char[] unicodeCharArray = new char[4];
						Array.Copy(json, index, unicodeCharArray, 0, 4);

						// Drop in the HTML markup for the unicode character
						s += "&#x" + new string(unicodeCharArray) + ";";
						
						/*
						uint codePoint = UInt32.Parse(new string(unicodeCharArray), NumberStyles.HexNumber);
						// convert the integer codepoint to a unicode char and add to string
						s += Char.ConvertFromUtf32((int)codePoint);
						*/
						
						// skip 4 chars
						index += 4;
					} else {
						break;
					}					
				}
			} else {
				s += c;
			}

		}

		if (!complete) {
			return null;
		}

		return s;
	}

	protected double ParseNumber(char[] json, ref int index)
	{
		EatWhitespace(json, ref index);

		int lastIndex = GetLastIndexOfNumber(json, index);
		int charLength = (lastIndex - index) + 1;
		char[] numberCharArray = new char[charLength];

		Array.Copy(json, index, numberCharArray, 0, charLength);
		index = lastIndex + 1;
		return Double.Parse(new string(numberCharArray)); // , CultureInfo.InvariantCulture);
	}

	protected int GetLastIndexOfNumber(char[] json, int index)
	{
		int lastIndex;
		for (lastIndex = index; lastIndex < json.Length; lastIndex++) {
			if ("0123456789+-.eE".IndexOf(json[lastIndex]) == -1) {
				break;
			}
		}
		return lastIndex - 1;
	}

	protected void EatWhitespace(char[] json, ref int index)
	{
		for (; index < json.Length; index++) {
			if (" \t\n\r".IndexOf(json[index]) == -1) {
				break;
			}
		}
	}

	protected int LookAhead(char[] json, int index)
	{
		int saveIndex = index;
		return NextToken(json, ref saveIndex);
	}

	protected int NextToken(char[] json, ref int index)
	{
		EatWhitespace(json, ref index);

		if (index == json.Length) {
			return MiniJSON.TOKEN_NONE;
		}
		
		char c = json[index];
		index++;
		switch (c) {
			case '{':
				return MiniJSON.TOKEN_CURLY_OPEN;
			case '}':
				return MiniJSON.TOKEN_CURLY_CLOSE;
			case '[':
				return MiniJSON.TOKEN_SQUARED_OPEN;
			case ']':
				return MiniJSON.TOKEN_SQUARED_CLOSE;
			case ',':
				return MiniJSON.TOKEN_COMMA;
			case '"':
				return MiniJSON.TOKEN_STRING;
			case '0': case '1': case '2': case '3': case '4': 
			case '5': case '6': case '7': case '8': case '9':
			case '-': 
				return MiniJSON.TOKEN_NUMBER;
			case ':':
				return MiniJSON.TOKEN_COLON;
		}
		index--;

		int remainingLength = json.Length - index;

		// false
		if (remainingLength >= 5) {
			if (json[index] == 'f' &&
				json[index + 1] == 'a' &&
				json[index + 2] == 'l' &&
				json[index + 3] == 's' &&
				json[index + 4] == 'e') {
				index += 5;
				return MiniJSON.TOKEN_FALSE;
			}
		}

		// true
		if (remainingLength >= 4) {
			if (json[index] == 't' &&
				json[index + 1] == 'r' &&
				json[index + 2] == 'u' &&
				json[index + 3] == 'e') {
				index += 4;
				return MiniJSON.TOKEN_TRUE;
			}
		}

		// null
		if (remainingLength >= 4) {
			if (json[index] == 'n' &&
				json[index + 1] == 'u' &&
				json[index + 2] == 'l' &&
				json[index + 3] == 'l') {
				index += 4;
				return MiniJSON.TOKEN_NULL;
			}
		}

		return MiniJSON.TOKEN_NONE;
	}

	protected bool SerializeObjectOrArray(object objectOrArray, StringBuilder builder)
	{
		if (objectOrArray is Hashtable) {
			return SerializeObject((Hashtable)objectOrArray, builder);
		} else if (objectOrArray is ArrayList) {
			return SerializeArray((ArrayList)objectOrArray, builder);
		} else {
			return false;
		}
	}

	protected bool SerializeObject(Hashtable anObject, StringBuilder builder)
	{
		builder.Append("{");

		IDictionaryEnumerator e = anObject.GetEnumerator();
		bool first = true;
		while (e.MoveNext()) {
			string key = e.Key.ToString();
			object value = e.Value;

			if (!first) {
				builder.Append(", ");
			}

			SerializeString(key, builder);
			builder.Append(":");
			if (!SerializeValue(value, builder)) {
				return false;
			}

			first = false;
		}

		builder.Append("}");
		return true;
	}

	protected bool SerializeArray(ArrayList anArray, StringBuilder builder)
	{
		builder.Append("[");

		bool first = true;
		for (int i = 0; i < anArray.Count; i++) {
			object value = anArray[i];

			if (!first) {
				builder.Append(", ");
			}

			if (!SerializeValue(value, builder)) {
				return false;
			}

			first = false;
		}

		builder.Append("]");
		return true;
	}

	protected bool SerializeValue(object value, StringBuilder builder)
	{
		
		// Type t = value.GetType();
		
		// Debug.Log("type: " + t.ToString() + " isArray: " + t.IsArray);
		
		if (value.GetType().IsArray) {
			SerializeArray(new ArrayList((ICollection) value), builder);
		} else if (value is string) {
			SerializeString((string)value, builder);
		} else if (value is Char) {			
			SerializeString(System.Convert.ToString((char) value), builder);
		} else if (value is Hashtable) {
			SerializeObject((Hashtable)value, builder);
		} else if (value is ArrayList) {
			SerializeArray((ArrayList)value, builder);
		} 
		else if ((value is Boolean) && ((Boolean)value == true)) {
			builder.Append("true");
		} else if ((value is Boolean) && ((Boolean)value == false)) {
			builder.Append("false");
		}
		else if (value.GetType().IsPrimitive) {
			SerializeNumber(System.Convert.ToDouble(value), builder);			
		} else if (value == null) {
			builder.Append("null");
		} else {
			return false;
		}
		return true;
	}

	protected void SerializeString(string aString, StringBuilder builder)
	{
		builder.Append("\"");

		char[] charArray = aString.ToCharArray();
		for (int i = 0; i < charArray.Length; i++) {
			char c = charArray[i];
			if (c == '"') {
				builder.Append("\\\"");
			} else if (c == '\\') {
				builder.Append("\\\\");
			} else if (c == '\b') {
				builder.Append("\\b");
			} else if (c == '\f') {
				builder.Append("\\f");
			} else if (c == '\n') {
				builder.Append("\\n");
			} else if (c == '\r') {
				builder.Append("\\r");
			} else if (c == '\t') {
				builder.Append("\\t");
			} else {
				int codepoint = System.Convert.ToInt32(c);
				if ((codepoint >= 32) && (codepoint <= 126)) {
					builder.Append(c);
				} else {
					builder.Append("\\u" + System.Convert.ToString(codepoint, 16).PadLeft(4, '0'));
				}
			}
		}

		builder.Append("\"");
	}

	protected void SerializeNumber(double number, StringBuilder builder)
	{
		builder.Append(System.Convert.ToString(number)); // , CultureInfo.InvariantCulture));
	}

	/*
	/// <summary>
	/// Determines if a given object is numeric in any way
	/// (can be integer, double, etc). C# has no pretty way to do this.
	/// </summary>
	protected bool IsNumeric(object o)
	{
		try {
			Double.Parse(o.ToString());
		} catch (Exception e) {
				                // Something went wrong, so lets get information about it.
						Debug.Log(e.ToString());
			return false;
		}
		return true;
	}
	*/
}
