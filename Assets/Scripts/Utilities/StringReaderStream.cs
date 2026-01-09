/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Xavier Martinez, 2017-2022
        Hubert Santuz, 2022-2026
        Marc Baaden, 2010-2026
        unitymol@gmail.com
        https://unity.mol3d.tech/

        This file is part of UnityMol, a general framework whose purpose is to provide
        a prototype for developing molecular graphics and scientific
        visualisation applications based on the Unity3D game engine.
        More details about UnityMol are provided at the following URL: https://unity.mol3d.tech/

        This program is free software: you can redistribute it and/or modify
        it under the terms of the GNU General Public License as published by
        the Free Software Foundation, either version 3 of the License, or
        (at your option) any later version.

        This program is distributed in the hope that it will be useful,
        but WITHOUT ANY WARRANTY; without even the implied warranty of
        MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
        GNU General Public License for more details.

        You should have received a copy of the GNU General Public License
        along with this program. If not, see <https://www.gnu.org/licenses/>.

        To help us with UnityMol development, we ask that you cite
        the research papers listed at https://unity.mol3d.tech/cite-us/.
    ================================================================================
*/
using System;
using System.IO;
using System.Text;

public class StringReaderStream : Stream
{
	private string input;
	private readonly Encoding encoding;
	private int maxBytesPerChar;
	private int inputLength;
	private int inputPosition;
	private readonly long length;
	private long position;

	public StringReaderStream(string input)
		: this(input, Encoding.UTF8)
	{ }

	public StringReaderStream(string input, Encoding encoding)
	{
		this.encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
		this.input = input;
		inputLength = input == null ? 0 : input.Length;
		if (!string.IsNullOrEmpty(input))
			length = encoding.GetByteCount(input);
		maxBytesPerChar = encoding == Encoding.ASCII ? 1 : encoding.GetMaxByteCount(1);
	}

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length => length;

	public override long Position
	{
		get => position;
		set => throw new NotImplementedException();
	}

	public override void Flush()
	{
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (inputPosition >= inputLength)
			return 0;
		if (count < maxBytesPerChar)
			throw new ArgumentException("count has to be greater or equal to max encoding byte count per char");
		int charCount = Math.Min(inputLength - inputPosition, count / maxBytesPerChar);
		int byteCount = encoding.GetBytes(input, inputPosition, charCount, buffer, offset);
		inputPosition += charCount;
		position += byteCount;
		return byteCount;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotImplementedException();
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotImplementedException();
	}
}
